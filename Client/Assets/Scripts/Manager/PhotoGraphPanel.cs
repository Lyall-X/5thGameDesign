using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using  DG.Tweening;
using MoreMountains.InventoryEngine;

public class PhotoGraphPanel: MonoBehaviour
{
    public Transform photo;
    public Button yesBtn, colseBtn, photoBtn, cameraBtn;
    public RawImage image;
    public Transform cameraScope;
    public RectTransform cameraAperture;
    
    private RectTransform root;
    private float screenWight, screenHight;

    private void Start()
    {
        photo.gameObject.SetActive(false);
        root = transform.GetComponent<RectTransform>();
        screenWight = Screen.width/ root.rect.width;
        screenHight = Screen.height/ root.rect.height;
        yesBtn.onClick.AddListener(TakePicture);
        colseBtn.onClick.AddListener(Close);
        photoBtn.onClick.AddListener(ShowPhotos);
        cameraBtn.onClick.AddListener(showCanmra);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        photo.gameObject.SetActive(false);
        cameraAperture.localPosition = Vector3.zero;
    }

    private void showCanmra()
    {
        //TODO  相机图片按钮
    }

    private void ShowPhotos()
    {
      
      GameObject InventoryCanvas = GameObject.Find("/UICamera/InventoryCanvas");
      InventoryInputManager script = InventoryCanvas.GetComponent<InventoryInputManager>();
      script.ToggleInventory();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void TakePicture()
    {
        //--TODO  判断拍照区域是否有道具
        
        photo.localScale = Vector3.one;
        photo.localPosition = Vector3.zero;
        photo.localEulerAngles = Vector3.zero;
        photo.gameObject.SetActive(true);
        image.texture = ScreenShot(Camera.main, cameraAperture);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(photo.DORotate(new Vector3(0,0,-15), 0.1f));
        sequence.Insert(1f,photo.DOMove(photoBtn.transform.position, 0.5f));
        sequence.Insert(1f, photo.DOScale(Vector3.one * 0.2f, 0.5f));
        sequence.onComplete = () => { photo.gameObject.SetActive(false); };
    }
    

    public Texture2D ScreenShot(Camera camera,RectTransform cameraAperture)
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);//渲染一张图
        camera.targetTexture = rt;
        camera.Render();
       
        RenderTexture.active = rt;
        Vector2 centerPos = new Vector2(cameraAperture.position.x * screenWight, cameraAperture.position.y * screenHight);
        Rect rect = new Rect(centerPos.x - cameraAperture.rect.width*0.5f,centerPos.y-cameraAperture.rect.height*0.5f, cameraAperture.rect.width, cameraAperture.rect.height);
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        
        //颜色纹理格式，每个通道8位。
        screenShot.ReadPixels(rect, 0, 0);
        // screenShot.ReadPixels(rect, 0, 0);//从屏幕读取像素到保存的纹理数据中。
        screenShot.Apply();//实际上应用所有以前的SetPixel和SetPixels更改。
        camera.targetTexture = null;
        RenderTexture.active = null;
        GameObject.Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();//设置文件类型
        string filename = "";
#if UNITY_EDITOR_OSX 
        filename = Application.dataPath + "/Resources/ScreenShot/screenshot.png";//存放路径
        System.IO.File.WriteAllBytes(filename, bytes);//根据上边的类型以及路径写入文件夹中去
#elif UNITY_EDITOR_WIN
        filename = Application.dataPath + "/Resources/ScreenShot/screenshot.png";//存放路径
        System.IO.File.WriteAllBytes(filename, bytes);//根据上边的类型以及路径写入文件夹中去
#endif
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();//刷新，这步很关键，否则后面调用图片时没有。
#endif
        return screenShot;
    }
}


