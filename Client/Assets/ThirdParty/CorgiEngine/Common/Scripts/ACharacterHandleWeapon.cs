using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using UnityEngine.EventSystems;
using  DG.Tweening;
using MoreMountains.InventoryEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using UnityEngine.UI;


public class ACharacterHandleWeapon : CharacterHandleWeapon
{
  private GameObject photoCanvas;
  private bool isUsing = false;
  private bool touch = false;

  protected override void Initialization () 
  {
    base.Initialization();
    photoCanvas = GameObject.Find("/Canvas/PhotoGraphPanel");
    photoCanvas.SetActive(false);
  }
  
  public float timer = 2.0f;
  protected override void HandleInput ()
  {
    timer -= Time.deltaTime;
    if (timer <= 0) {
      if (!photoCanvas.activeSelf)
        MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 1f, 0f, false, 0f, true);
      timer = 2.0f;
    }

    bool canMove = false;
    #if UNITY_ANDROID || UNITY_IPHONE
      canMove = _inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown;
    #else
      canMove = _inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown;
    #endif

    if ( canMove && isUsing)
    {
      if (PropMgr.Instance.PicObj)
      {
        foreach (Transform child in PropMgr.Instance.PicObj.transform)
        {
          MovingPlatform sc1 = child.gameObject.GetComponent<MovingPlatform>();
          if (sc1)
          {
            sc1.enabled = true;
            sc1.PathElements[1].PathElementPosition.x = sc1.PathElements[1].PathElementPosition.x * (-1);
            sc1.PathElements[0].PathElementPosition.x = sc1.PathElements[1].PathElementPosition.x * (-1);
          }
          Collider2D c2d = child.gameObject.GetComponent<Collider2D>();
          if (c2d)
          {
            c2d.enabled = true;
          }
          
          if(child.gameObject.tag != "nogravity")
          {
            child.gameObject.AddComponent<Rigidbody2D>();
          }
          
          Prop prop = child.gameObject.GetComponent<Prop>();
          if (prop)
          {
            prop.putdown = true;
          }
        }
        #if UNITY_ANDROID || UNITY_IPHONE
          GameObject de = GameObject.FindGameObjectWithTag("DebugTag");
          de.GetComponent<Text>().text = "Jump";
          this.GetComponent<CharacterJump>().enabled = true;
        #endif
      }
      PropMgr.Instance.PicObj = null;
      isUsing = false;
    }
    if (PropMgr.Instance.PicObj)
    {
      if (!isUsing)
      {
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        PropMgr.Instance.PicObj = GameObject.Instantiate(PropMgr.Instance.PicObj);

        foreach (Transform child in PropMgr.Instance.PicObj.transform)
        {
          Sequence sequence = DOTween.Sequence();
          if (child.gameObject.GetComponent<Renderer>() == null )
          {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            PropMgr.Instance.PicObj = null;
            isUsing = false;
          }
          Material ma = child.gameObject.GetComponent<Renderer>().material;
          Color cl = ma.color;
          child.gameObject.GetComponent<Renderer>().material.color =  new Color(cl.r, cl.g, cl.b, 0f);
          sequence.Append(ma.DOColor(new Color(cl.r, cl.g, cl.b, 1f), 1f));
          sequence.onComplete = () => {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
          };
          MovingPlatform sc1 = child.gameObject.GetComponent<MovingPlatform>();
          if (sc1)
          {
            sc1.enabled = false;
          }
          Collider2D c2d = child.gameObject.GetComponent<Collider2D>();
          if (c2d)
          {
            c2d.enabled = false;
          }
        }
        isUsing = true;

        
      #if UNITY_ANDROID || UNITY_IPHONE
        PropMgr.Instance.PicObj.transform.position = new Vector3(transform.position.x, transform.position.y + 2, PropMgr.Instance.PicObj.transform.position.z);
      #endif
      }

      #if UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
          Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition; 

          PropMgr.Instance.PicObj.transform.Translate(touchDeltaPosition.x * 0.01f, touchDeltaPosition.y * 0.01f, PropMgr.Instance.PicObj.transform.position.z);
        }
        GameObject de = GameObject.FindGameObjectWithTag("DebugTag");
        de.GetComponent<Text>().text = "放置";
        this.GetComponent<CharacterJump>().enabled = false;
      #else
        Vector3 pos = Camera.main.WorldToScreenPoint(PropMgr.Instance.PicObj.transform.position);
        Vector3 m_MousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pos.z);
        PropMgr.Instance.PicObj.transform.position = Camera.main.ScreenToWorldPoint(m_MousePos);
      #endif
    }
  }
}
