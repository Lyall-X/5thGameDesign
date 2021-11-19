using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Prop : MonoBehaviour
{

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
