using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class PhotoManager : MonoBehaviour
{
	private BoxCollider2D _collider2D;
  private SpriteRenderer _sprRend;
  public Vector2 photoSize;
  
  public GameObject[] photoObjs;

  protected void Awake()
  {
    Initialization();
  }
  protected void Initialization()
  {
    _collider2D = GetComponent<BoxCollider2D>();
    _sprRend = GetComponent<SpriteRenderer>();

  }

  void Update()
  {
    _collider2D.size = photoSize;
    _sprRend.size = photoSize;
  }

  protected virtual void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.CompareTag("Player"))
    {
      return;
    }
  }

  protected virtual void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.CompareTag("Player"))
    {
      return;
    }		
  }
}
