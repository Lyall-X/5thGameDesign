using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

public class KeyDoor: MonoBehaviour
{
  public UnityEvent KeyAction;
  public UnityEvent KeyAction2;

    private void Start()
    {
    }

    private void Update() 
    {
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
      if ((other.gameObject.layer == 8  || other.gameObject.layer == 9)&& KeyAction != null)
      {
				KeyAction.Invoke ();
      }
    }

    private void OnTriggerExit2D(Collider2D other) {
      if ((other.gameObject.layer == 8  || other.gameObject.layer == 9) && KeyAction != null)
      {
				KeyAction2.Invoke ();
      }
    }
}


