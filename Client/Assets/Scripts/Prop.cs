using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using  DG.Tweening;

public class Prop : MonoBehaviour
{
public float timer = 10.0f;
public bool putdown = false;
    void Update() {
      if (putdown)
      {
        timer -= Time.deltaTime;
        if (timer - 1 <= 0) {
            Sequence sequence = DOTween.Sequence();
            Material ma = gameObject.GetComponent<Renderer>().material;
            Color cl = ma.color;
            sequence.Append(ma.DOColor(new Color(cl.r, cl.g, cl.b, 0), 1f));
            sequence.onComplete = () => {
              GameObject.Destroy(gameObject);
            };
        }
      }
    }

    public void Init()
    {
    }

    private void ShowHightLight()
    {
        //TODO-- 高光效果
        // transform.localScale = Vector3.one * 1.5f;
    }

    public bool CheckBeFound(Vector3 targetPos,float wight,float hight,Rect rect)
    {
        //--检查是否被发现 --即是否在拍照范围
        Vector3 ptViewport = Camera.main.WorldToViewportPoint(transform.position);
 
        //设置锚点位置，锚点在左下角
        ptViewport = new Vector3(ptViewport.x * rect.width, ptViewport.y * rect.height, 0);
        // Debug.Log("ptViewport" + ptViewport);

        if (ptViewport.x + 10> targetPos.x + wight*0.5f)
        {
            return false;
        }
        else if (ptViewport.x - 10 < targetPos.x - wight*0.5f)
        {
            return false;
        }
        else if (ptViewport.y + 10 > targetPos.y + hight*0.5f)
        {
            return false;
        }
        else if (ptViewport.y - 10 < targetPos.y - hight*0.5f)
        {
            return false;
        }

        ShowHightLight();
        return true;
    }
    
    

}
