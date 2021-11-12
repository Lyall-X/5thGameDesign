using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using  DG.Tweening;
using MoreMountains.InventoryEngine;
using MoreMountains.CorgiEngine;

public class PhotoGraphPanel: MonoBehaviour
{
    public Transform photo;
    public Button yesBtn, colseBtn, photoBtn, cameraBtn;
    public RawImage image;
    public Transform cameraScope;
    public RectTransform cameraAperture;

    public GameObject mainCam;
    
    private RectTransform root;

    private void Start()
    {
        photo.gameObject.SetActive(false);
        root = transform.GetComponent<RectTransform>();
        yesBtn.onClick.AddListener(TakePicture);
        colseBtn.onClick.AddListener(Close);
        photoBtn.onClick.AddListener(ShowPhotos);
        cameraBtn.onClick.AddListener(showCanmra);
    }

    private void Update() 
    {
      if (Input.GetKeyDown (KeyCode.F))  
      {  
          TakePicture();
      }  
    }

    public void Show(bool status)
    {
      GameObject InventoryCanvas = GameObject.Find("/UICamera/InventoryCanvas");
      InventoryInputManager script = InventoryCanvas.GetComponent<InventoryInputManager>();
      //       GameObject InventoryCanvas = GameObject.Find("/UICamera/InventoryCanvas");
      // InventoryInputManager script = InventoryCanvas.GetComponent<InventoryInputManager>();
      if (script.InventoryIsOpen)
      {
          script.CloseInventory();
      }
      gameObject.SetActive(status);
      photo.gameObject.SetActive(false);
      cameraAperture.localPosition = Vector3.zero;
      
      if (status)
      {
        CameraController sc = mainCam.GetComponent<CameraController>();
        sc.CameraOffset = new Vector3(0,0,0);
        InputManager.Instance.InputDetectionActive = false;
      }
      else
      {
        CameraController sc = mainCam.GetComponent<CameraController>();
        sc.CameraOffset = new Vector3(0,3,0);
        InputManager.Instance.InputDetectionActive = true;
      }
    }

    private void showCanmra()
    {
      this.Show(!gameObject.activeSelf);
    }

    private void ShowPhotos()
    {
      GameObject InventoryCanvas = GameObject.Find("/UICamera/InventoryCanvas");
      InventoryInputManager script = InventoryCanvas.GetComponent<InventoryInputManager>();
      script.ToggleInventory();
      gameObject.SetActive(false);
    }

    private void Close()
    {
        Show(false);
    }

    public void TakePicture()
    {
        photo.localScale = Vector3.one;
        photo.localPosition = Vector3.zero;
        photo.localEulerAngles = Vector3.zero;
        photo.gameObject.SetActive(true);
        //TODO 照片命名
        GameObject pic = PropMgr.Instance.CheckProposBeFound(cameraAperture.position,cameraAperture.rect.width,cameraAperture.rect.height,root.rect);
        string picname = pic != null ? pic.name : "photo";
        Texture2D texture = ScreenShot(Camera.main, cameraAperture, picname);
        image.texture = texture;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(photo.DORotate(new Vector3(0,0,-15), 0.1f));
        sequence.Insert(1f,photo.DOMove(photoBtn.transform.position, 0.5f));
        sequence.Insert(1f, photo.DOScale(Vector3.one * 0.2f, 0.5f));
        sequence.onComplete = () => {
          Show(false);
        };
        
        //--TODO  判断拍照区域是否有道具 -- 可在此处获得当前拍照发现的道具ID
        InventoryPicutre Item = new InventoryPicutre();
        Item.ItemID = picname;
        Item.Icon = Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0, 0));
        GameObject mainInventoryTmp = GameObject.Find("MainInventory");
				if (mainInventoryTmp != null) 
        { 
          Inventory MainInventory = mainInventoryTmp.GetComponent<Inventory> (); 
          MainInventory.AddItem(Item, 1);
        }	
    }
    

    public Texture2D ScreenShot(Camera camera,RectTransform cameraAperture,string name = "screenshot")
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);//渲染一张图
        camera.targetTexture = rt;
        camera.Render();
       
        RenderTexture.active = rt;
        Rect rect = new Rect(cameraAperture.position.x - cameraAperture.rect.width*0.5f, Screen.height - cameraAperture.position.y - cameraAperture.rect.height*0.5f, cameraAperture.rect.width, cameraAperture.rect.height);
        
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        //颜色纹理格式，每个通道8位。
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();//实际上应用所有以前的SetPixel和SetPixels更改。
        camera.targetTexture = null;
        RenderTexture.active = null;
        GameObject.Destroy(rt);
        return screenShot;
    }
}


