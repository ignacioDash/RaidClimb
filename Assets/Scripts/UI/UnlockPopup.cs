using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UnlockPopup : MonoBehaviour
    {
        [SerializeField] private RawImage unitImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI unitNameText;

        private Action _onClosed;

        private void Awake()
        {
            closeButton.onClick.AddListener(OnClose);
        }

        private void OnClose()
        {
            _onClosed?.Invoke();
            Destroy(gameObject);
        }

        public void Setup(string unitName, RenderTexture renderTexture, Action onClosed)
        {
            unitNameText.text = unitName;
            if (renderTexture != null) unitImage.texture = renderTexture;
            _onClosed = onClosed;
        }
    }
}
