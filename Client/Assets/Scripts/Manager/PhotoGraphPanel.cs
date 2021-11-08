using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PhotoGraphPanel: MonoBehaviour
{
    public RectTransform root;
    private float timer = 0;
    bool isstart = false;
    public RawImage image;
    public Transform cameraScope;
    public RectTransform cameraAperture;
    private float screenWight, screenHight;
    private Texture2D screenShot;
    private void Start()
    {
        image.enabled = false;
        root = transform.GetComponent<RectTransform>();
        screenShot = new Texture2D(256, 256, TextureFormat.RGB24, false);
        screenWight = Screen.width/ root.rect.width;
        screenHight = Screen.height/ root.rect.height;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))//检测按下回车截图
        {
            isstart = true;
            image.enabled = true;
            image.texture = ScreenShot(Camera.main, cameraAperture);
        }
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
#if UNITY_EDITOR_OSX 
        string filename = Application.dataPath + "/Resources/ScreenShot/screenshot.png";//存放路径
#elif UNITY_EDITOR_WIN
        string filename = Application.dataPath + "/Resources/ScreenShot/screenshot.png";//存放路径
#endif
        System.IO.File.WriteAllBytes(filename, bytes);//根据上边的类型以及路径写入文件夹中去
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();//刷新，这步很关键，否则后面调用图片时没有。
#endif
        return screenShot;
    }
}


