using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public class DragCheck : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform root;
    private RectTransform rt;
    Vector3 globalMousePos;
    private float min_x, max_x, min_y, max_y;
    // Use this for initialization
    void Start ()
    {
        rt = gameObject.GetComponent<RectTransform>();
        min_x = root.position.x - (root.rect.width * 0.5f - rt.rect.width * 0.5f);
        max_x = root.position.x + (root.rect.width * 0.5f - rt.rect.width * 0.5f);
        min_y = root.position.y - (root.rect.height * 0.5f - rt.rect.height * 0.5f);
        max_y = root.position.y + (root.rect.height * 0.5f - rt.rect.height * 0.5f);
    }
 
    // begin dragging
    public void OnBeginDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }
 
    // during dragging
    public void OnDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }
 
    // end dragging
    public void OnEndDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }
 
    /// <summary>
    /// set position of the dragged game object
    /// </summary>
    /// <param name="eventData"></param>
    private void SetDraggedPosition(PointerEventData eventData)
    {
        // transform the screen point to world point int rectangle
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out globalMousePos))
        {
            float x = globalMousePos.x, y = globalMousePos.y;
            rt.position = CheckPos(x, y);
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);
        }
    }
 
 
    Vector2 CheckPos(float x, float y)
    {
        if (x < min_x)
            x = min_x;
        else if (x > max_x)
            x = max_x;
        if (y > max_y)
            y = max_y;
        else if (y < min_y)
            y = min_y;
        return new Vector2(x, y);
    }
 
}