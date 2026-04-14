using System;
using Config;
using Constants;
using DG.Tweening;
using Managers;
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
        private Camera _playerCamera;
        
        public Canvas WorldCanvas => worldCanvas;
        private float _unitHealth;
        
        public void Init(UnitBaseConfig config, Action onDeath, bool useAttackCamera)
        {
            _config = config;
            _unitHealth = _config.Health;

            var cameraManager = GameManager.Instance.GetManager<CameraManager>();

            _playerCamera = cameraManager.MainCamera;
            
            worldCanvas.worldCamera = _playerCamera;
            
            healthBar.fillAmount = 1;
            _onDeath += onDeath;
        }

        public void OnUnitHealthChanged(float healthDelta)
        {
            _unitHealth += healthDelta;

            var sliderValue = _unitHealth / _config.Health;
            healthBar.fillAmount = Mathf.Clamp(sliderValue, 0f, 1f);

            if (_unitHealth <= 0)
            {
                healthCanvasGroup.DOFade(0, 0.4f).SetLink(gameObject);
                _onDeath?.Invoke();
            }
        }

        private void LateUpdate()
        {
            worldCanvas.transform.forward = _playerCamera.transform.forward;
        }
    }
}