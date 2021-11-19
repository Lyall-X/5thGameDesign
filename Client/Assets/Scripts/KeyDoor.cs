using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

public class KeyDoor: MonoBehaviour
{
  public UnityEvent KeyAction;

    private void Start()
    {
    }

    private void Update() 
    {
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
      if (other.gameObject.layer == 8 && KeyAction != null)
      {
				KeyAction.Invoke ();
      }
    }
}


