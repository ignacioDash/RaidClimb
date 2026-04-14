using System;
using System.Collections;
using System.Linq;
using Castles;
using Constants;
using Input;
using Managers;
using Units;
using Units.Traps;
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
        private TrapsManager _trapsManager;
        private InputManager _inputManager;
        
        private IDissolve _currentDefenderToSpawn;
        private bool _currentDissolveCompleted;
        
        // ranges
        private float _holdProgress;
        private int _currentRange = -1;
        private float _holdStartTime;
        private const float MIN_HOLD = 0.25f;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _inputManager = GameManager.Instance.GetManager<InputManager>();
            _trapsManager = GameManager.Instance.GetManager<TrapsManager>();

            _inputManager.OnHoldRight += OnHoldAttack;
            _inputManager.OnReleaseRight += OnReleaseAttack;

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
            _inputManager.OnHoldRight -= OnHoldAttack;
            _inputManager.OnReleaseRight -= OnReleaseAttack;
            
            pauseButton.onClick.RemoveListener(OnPaused);
            unPauseButton.onClick.RemoveListener(OnUnPause);
            exitButton.onClick.RemoveListener(OnExitButton);
            StopAllCoroutines();
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

        private void OnHoldAttack()
        {
            if (holdRanges == null || holdRanges.Length == 0)
                return;

            if (_holdProgress == 0f)
                _holdStartTime = Time.time;

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

            if (Time.time - _holdStartTime < MIN_HOLD)
                return;

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

        private void OnReleaseAttack(Vector3 worldPos)
        {
            if (_holdStartTime <= 0f || Time.time - _holdStartTime < MIN_HOLD + 0.05f)
            {
                _holdStartTime = 0f;
                _holdProgress = 0f;
                _currentRange = -1;
                UpdateRangeVisuals(-1);
                UpdatePointerVisual();
                return;
            }

            var range = GetRangeIndex();
            OnReleasedRange(range, worldPos);

            _holdProgress = 0f;
            _holdStartTime = 0f;
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
            var unitToSpawn = _unitManager.SpawnUnit(unitType, unitPosition, Keys.PLAYER_ID);
            
            if (unitToSpawn)
            {
                unitToSpawn.transform.position = unitPosition;
                StartCoroutine(SetDelayedTarget(unitToSpawn, 1.5f));
            }
        }

        private BaseUnit.UnitTypes GetUnitTypeForRange(int range)
        {
            if (Time.time - _holdStartTime < MIN_HOLD)
                return BaseUnit.UnitTypes.None;

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
            if (Time.time - _holdStartTime < MIN_HOLD)
                activeIndex = -1;

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

        private IEnumerator SetDelayedTarget(BaseUnit unitToSpawn, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (!unitToSpawn)
                yield break;

            _unitManager.FindNewTargetFor(unitToSpawn);
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