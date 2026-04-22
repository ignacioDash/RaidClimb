using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ButtonPointerComponent : Button, IPointerUpHandler, IPointerDownHandler
    {
        public Action PointerDown;
        public Action PointerUp;

        protected override void Awake()
        {
            base.Awake();
            transition = Transition.ColorTint;
            var c = colors;
            c.normalColor = Color.white;
            c.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors = c;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (interactable) PointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (interactable) PointerUp?.Invoke();
        }
    }
}