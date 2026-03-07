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
        
        public float UnitHealth { get; private set; }
        
        public void Init(UnitBaseConfig config, Action onDeath)
        {
            UnitHealth = config.Health;
            _onDeath += onDeath;
        }

        public void OnUnitHealthChanged(float healthDelta)
        {
            UnitHealth += healthDelta;
            
            // todo: update UI
            
            if (UnitHealth <= 0)
            {
                // fade out health bar
                _onDeath?.Invoke();
            }
        }
    }
}