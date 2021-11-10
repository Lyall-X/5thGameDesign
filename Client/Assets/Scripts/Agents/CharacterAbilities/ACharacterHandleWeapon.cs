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
    // if (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown && !photoCanvas.activeSelf)
    // {
    //   PhotoGraphPanel script = photoCanvas.GetComponent<PhotoGraphPanel>();
    //   script.Show();
    // }
    if (_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown && !photoCanvas.activeSelf)
    {
      PhotoGraphPanel script = photoCanvas.GetComponent<PhotoGraphPanel>();
      script.Show();
    }
    
  }
}
