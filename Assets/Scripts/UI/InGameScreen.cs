using System;
using System.Threading.Tasks;
using Constants;
using Input;
using Managers;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameScreen : BaseScreen
    {
        [SerializeField] private Button pauseButton, unPauseButton, exitButton;
        [SerializeField] private Image[] rangeImages;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Image pointerImage;
        [SerializeField] private RectTransform pointerStart, pointerEnd;

        [Header("Settings")]
        [SerializeField] private HoldRanges[] holdRanges;

        private UnitManager _unitManager;
        private InputManager _inputManager;
        private BaseUnit _unitToSpawn;
        
        // ranges
        private float _holdProgress;
        private int _currentRange = -1;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _inputManager = GameManager.Instance.GetManager<InputManager>();

            _inputManager.OnHold += OnHoldInput;
            _inputManager.OnRelease += OnReleaseInput;

            pauseButton.onClick.AddListener(OnPaused);
            unPauseButton.onClick.AddListener(OnUnPause);
            exitButton.onClick.AddListener(OnExitButton);

            pauseMenu.gameObject.SetActive(false);

            _holdProgress = 0f;
            _currentRange = -1;
            UpdateRangeVisuals(-1);
            UpdatePointerVisual();
            
            SetButtons(true);
        }

        private void OnDisable()
        {
            _inputManager.OnHold -= OnHoldInput;
            _inputManager.OnRelease -= OnReleaseInput;
            
            pauseButton.onClick.RemoveListener(OnPaused);
            unPauseButton.onClick.RemoveListener(OnUnPause);
            exitButton.onClick.RemoveListener(OnExitButton);
        }

        private void SetButtons(bool on)
        {
            pauseButton.interactable = on;
            unPauseButton.interactable = on;
            exitButton.interactable = on;
        }

        private async void OnExitButton()
        {
            Time.timeScale = 1;
            
            SetButtons(false);
            
            await GameManager.Instance.FinishGame();
        }

        private void OnUnPause()
        {
            pauseMenu.SetActive(false);

            Time.timeScale = 1;
        }

        private void OnPaused()
        {
            Time.timeScale = 0;

            pauseMenu.SetActive(true);
        }

        private void OnHoldInput()
        {
            if (holdRanges == null || holdRanges.Length == 0)
                return;

            var sectionSize = 1f / holdRanges.Length;

            var sectionIndex = Mathf.Min(
                Mathf.FloorToInt(_holdProgress / sectionSize),
                holdRanges.Length - 1);

            var targetValue = (sectionIndex + 1) * sectionSize;
            var speed = sectionSize / holdRanges[sectionIndex].RangeDuration;

            _holdProgress = Mathf.MoveTowards(
                _holdProgress,
                targetValue,
                speed * Time.deltaTime);

            UpdatePointerVisual();

            var newRange = Mathf.Min(
                Mathf.FloorToInt(_holdProgress / sectionSize),
                holdRanges.Length - 1);

            if (_holdProgress >= targetValue)
                newRange = Mathf.Min(sectionIndex + 1, holdRanges.Length - 1);

            if (newRange != _currentRange)
            {
                _currentRange = newRange;
                OnReachedRange(_currentRange);
            }
        }

        private void OnReleaseInput(Vector3 worldPos)
        {
            var range = GetRangeIndex();
            OnReleasedRange(range, worldPos);

            _holdProgress = 0f;
            _currentRange = -1;
            UpdateRangeVisuals(-1);
            UpdatePointerVisual();
        }
        
        private void OnReachedRange(int index)
        {
            UpdateRangeVisuals(index);
        }

        private void OnReleasedRange(int index, Vector3 worldPos)
        {
            var unitType = GetUnitTypeForRange(index);
            if (unitType == BaseUnit.UnitTypes.None)
                return;
            
            var unitPosition = GetUnitPosition(worldPos);
            _unitToSpawn = _unitManager.SpawnUnit(unitType, unitPosition, Keys.PLAYER_ID);
            
            if (_unitToSpawn)
            {
                _unitToSpawn.transform.position = unitPosition;
                _ = SetDelayedTarget();
            }
        }

        private BaseUnit.UnitTypes GetUnitTypeForRange(int range)
        {
            // todo
            return range switch
            {
                0 => BaseUnit.UnitTypes.Melee,
                1 => BaseUnit.UnitTypes.Ranged,
                2 => BaseUnit.UnitTypes.Tank,
                3 => BaseUnit.UnitTypes.Melee,
                _ => BaseUnit.UnitTypes.Melee
            };
        }
        
        private int GetRangeIndex()
        {
            if (holdRanges == null || holdRanges.Length == 0)
                return -1;

            var sectionSize = 1f / holdRanges.Length;
            return Mathf.Min(Mathf.FloorToInt(_holdProgress / sectionSize), holdRanges.Length - 1);
        }

        private void UpdateRangeVisuals(int activeIndex)
        {
            if (rangeImages == null)
                return;

            for (var i = 0; i < rangeImages.Length; i++)
            {
                if (!rangeImages[i])
                    continue;

                rangeImages[i].gameObject.SetActive(i == activeIndex);
            }
        }

        private void UpdatePointerVisual()
        {
            if (!pointerImage || !pointerStart || !pointerEnd)
                return;

            pointerImage.rectTransform.position = Vector3.Lerp(pointerStart.position, pointerEnd.position, _holdProgress);
        }

        private async Task SetDelayedTarget()
        {
            await Task.Delay(1500);
            
            _unitManager.FindNewTargetFor(_unitToSpawn);
            
            _unitToSpawn = null;
        }

        private Vector3 GetUnitPosition(Vector3 worldPos)
        {
            return new Vector3(worldPos.x, Values.UNIT_SPAWN_Y, worldPos.z);
        }
    }

    [Serializable]
    public class HoldRanges
    {
        public float RangeDuration;
    }
}