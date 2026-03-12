using System;
using System.Threading.Tasks;
using Constants;
using Input;
using Managers;
using Units;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameScreen : BaseScreen
    {
        [SerializeField] private Slider unitSlider;
        
        [Header("Settings")]
        [SerializeField] private float holdFillSpeed = 1f;

        private UnitManager _unitManager;
        private BaseUnit _unitToSpawn;
        private Camera _cam;
        private Vector3 _lastValidUnitPosition;

        private const float MIN_Y_HEIGHT = 7f;

        protected override void OnEnable()
        {
            base.OnEnable();

            unitSlider.value = 0;

            _cam = Camera.main;
            _unitManager = GameManager.Instance.GetManager<UnitManager>();

            InputManager.Instance.OnHold += OnHoldInput;
            InputManager.Instance.OnRelease += OnReleaseInput;
        }

        private void OnDisable()
        {
            InputManager.Instance.OnHold -= OnHoldInput;
            InputManager.Instance.OnRelease -= OnReleaseInput;
        }

        private void OnHoldInput()
        {
            unitSlider.value = Mathf.MoveTowards(unitSlider.value, 1f, holdFillSpeed * Time.deltaTime);
        }

        private void OnReleaseInput(Vector3 worldPos)
        {
            if (unitSlider.value >= 1) // todo: change with ranges
            {
                var unitPosition = GetUnitPosition(worldPos);
                _unitToSpawn = _unitManager.SpawnUnit(BaseUnit.UnitTypes.Melee, unitPosition, Keys.PLAYER_ID);
            
                if (_unitToSpawn)
                {
                    _unitToSpawn.transform.position = unitPosition;
                    _ = SetDelayedTarget();
                }
            }

            unitSlider.value = 0f;
        }

        private async Task SetDelayedTarget()
        {
            await Task.Delay(1500);
            
            _unitManager.FindNewTargetFor(_unitToSpawn);
            
            _unitToSpawn = null;
        }

        private Vector3 GetUnitPosition(Vector3 worldPos)
        {
            return new Vector3(worldPos.x, worldPos.y + MIN_Y_HEIGHT, worldPos.z);
        }
    }
}