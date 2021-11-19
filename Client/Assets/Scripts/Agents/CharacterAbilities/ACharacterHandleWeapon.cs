using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using UnityEngine.EventSystems;
using  DG.Tweening;
using MoreMountains.InventoryEngine;
using MoreMountains.CorgiEngine;


public class ACharacterHandleWeapon : CharacterHandleWeapon
{
  private GameObject photoCanvas;
  private bool isUsing = false;

  protected override void Initialization () 
  {
    base.Initialization();
    photoCanvas = GameObject.Find("/Canvas/PhotoGraphPanel");
    photoCanvas.SetActive(false);
  }

  protected override void HandleInput ()
  {
    if (Input.GetMouseButtonDown(1))
    {
      photoCanvas.GetComponent<PhotoGraphPanel>().Show(false);
    }

    if (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
    {
      if (PropMgr.Instance.PicObj)
      {
        foreach (Transform child in PropMgr.Instance.PicObj.transform)
        {
          Collider2D c2d = child.gameObject.GetComponent<Collider2D>();
          if (c2d)
          {
            c2d.enabled = true;
          }
          
          if(child.gameObject.tag != "nogravity")
          {
            child.gameObject.AddComponent<Rigidbody2D>();
          }
        }
      }
      else
      {
        GameObject btn = EventSystem.current.currentSelectedGameObject;
        GameObject InventoryCanvas = GameObject.Find("/UICamera/InventoryCanvas");
        InventoryInputManager script = InventoryCanvas.GetComponent<InventoryInputManager>();
        if (btn == null && !photoCanvas.activeSelf && !script.InventoryIsOpen)
        {
          photoCanvas.GetComponent<PhotoGraphPanel>().Show(true);
        }
      }
      PropMgr.Instance.PicObj = null;
      isUsing = false;
    }
    if (PropMgr.Instance.PicObj)
    {
      if (!isUsing)
      {
        PropMgr.Instance.PicObj = GameObject.Instantiate(PropMgr.Instance.PicObj);

        foreach (Transform child in PropMgr.Instance.PicObj.transform)
        {
          Collider2D c2d = child.gameObject.GetComponent<Collider2D>();
          if (c2d)
          {
            c2d.enabled = false;
          }
        }
        isUsing = true;
      }
      Vector3 pos = Camera.main.WorldToScreenPoint(PropMgr.Instance.PicObj.transform.position);
      Vector3 m_MousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pos.z);
      PropMgr.Instance.PicObj.transform.position = Camera.main.ScreenToWorldPoint(m_MousePos);
    }
  }
}
