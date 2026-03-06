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

        private void OnHoldInput(Vector3 worldPos, bool canDrop)
        {
            // todo: use canDrop to set material
            unitSlider.value = Mathf.MoveTowards(unitSlider.value, 1f, holdFillSpeed * Time.deltaTime);

            if (_unitToSpawn == null)
                _unitToSpawn = _unitManager.SpawnUnit(BaseUnit.UnitTypes.Melee, worldPos, Keys.PLAYER_ID);

            if (_unitToSpawn != null)
            {
                _unitToSpawn.SetVisuals(canDrop);
                
                var unitPosition = GetUnitPosition(worldPos);
                if (canDrop)
                    _lastValidUnitPosition = unitPosition;
                
                _unitToSpawn.transform.position = unitPosition;
            }
        }

        private void OnReleaseInput(Vector3 worldPos)
        {
            unitSlider.value = 0;
            
            if (_unitToSpawn != null)
            {
                _unitToSpawn.SetVisuals(true);
                _unitToSpawn.transform.position = _lastValidUnitPosition;
                _unitManager.RegisterUnit(_unitToSpawn, Keys.PLAYER_ID);
            }

            _unitToSpawn = null;
        }

        private Vector3 GetUnitPosition(Vector3 worldPos)
        {
            var camForward = _cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            var visualOffset = -camForward * MIN_Y_HEIGHT;
            return worldPos + visualOffset + Vector3.up * MIN_Y_HEIGHT;
        }
    }
}