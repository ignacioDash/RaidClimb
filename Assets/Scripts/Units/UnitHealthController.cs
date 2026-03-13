using System;
using Config;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class UnitHealthController : MonoBehaviour
    {
        [SerializeField] private Canvas worldCanvas;
        [SerializeField] private Image healthBar;
        [SerializeField] private CanvasGroup healthCanvasGroup;
        
        private Action _onDeath;
        private UnitBaseConfig _config;
        private Camera _mainCamera;
        
        public Canvas WorldCanvas => worldCanvas;
        public float UnitHealth { get; private set; }
        
        public void Init(UnitBaseConfig config, Action onDeath)
        {
            _config = config;
            UnitHealth = _config.Health;

            _mainCamera = Camera.main;
            worldCanvas.worldCamera = _mainCamera;
            
            healthBar.fillAmount = 1;
            _onDeath += onDeath;
        }

        public void OnUnitHealthChanged(float healthDelta)
        {
            UnitHealth += healthDelta;

            var sliderValue = UnitHealth / _config.Health;
            healthBar.fillAmount = Mathf.Clamp(sliderValue, 0f, 1f);

            if (UnitHealth <= 0)
            {
                healthCanvasGroup.DOFade(0, 0.4f).SetLink(gameObject);
                _onDeath?.Invoke();
            }
        }

        private void LateUpdate()
        {
            worldCanvas.transform.forward = _mainCamera.transform.forward;
        }
    }
}