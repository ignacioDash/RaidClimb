using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ButtonPointerComponent : Button, IPointerUpHandler, IPointerDownHandler
    {
        public Action PointerDown;
        public Action PointerUp;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke();
        }
    }
}