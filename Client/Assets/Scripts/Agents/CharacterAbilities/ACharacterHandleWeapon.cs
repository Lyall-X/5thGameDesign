using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

public class ACharacterHandleWeapon : CharacterHandleWeapon
{
  private GameObject photoCanvas;

  protected override void Initialization () 
  {
    base.Initialization();
    photoCanvas = GameObject.Find("/Canvas/PhotoGraphPanel");
    photoCanvas.SetActive(false);
  }

  protected override void HandleInput ()
  {
    if (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
    {
      PropMgr.Instance.PicObj = null;
      PropMgr.Instance.UsingObj = null;
    }
    if (PropMgr.Instance.PicObj)
    {
      if (!PropMgr.Instance.UsingObj)
      {
        PropMgr.Instance.UsingObj = GameObject.Instantiate(PropMgr.Instance.PicObj);
      }
      Vector3 pos = Camera.main.WorldToScreenPoint(PropMgr.Instance.UsingObj.transform.position);
      Vector3 m_MousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pos.z);
      PropMgr.Instance.UsingObj.transform.position = Camera.main.ScreenToWorldPoint(m_MousePos);
    }
  }
}
