using System;
using Config;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class UnitHealthController : MonoBehaviour
    {
        [SerializeField] private Slider healthBar;
        
        private Action _onDeath;
        private float _unitHealth;
        
        public void Init(UnitBaseConfig config, Action onDeath)
        {
            _unitHealth = config.Health;
            _onDeath += onDeath;
        }

        public void OnUnitHealthChanged(float healthDelta)
        {
            _unitHealth += healthDelta;
            
            // todo: update UI
            
            if (_unitHealth <= 0)
            {
                // fade out health bar
                _onDeath?.Invoke();
            }
        }
    }
}