using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Input;
using Managers;
using Units;
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
        [SerializeField] private RectTransform unitPreviewContainer;

        [Header("Progress bar")] 
        [SerializeField] private RectTransform playerCastleTarget;
        [SerializeField] private RectTransform opponentCastleTarget;
        [SerializeField] private RectTransform playerUnit, opponentUnit;

        [Header("Settings")]
        [SerializeField] private HoldRanges[] holdRanges;
        [SerializeField] private List<UnitPreviewReferences> previewReferences;

        private UnitManager _unitManager;
        private InputManager _inputManager;
        
        private IDissolve _currentDefenderToSpawn;
        private bool _currentDissolveCompleted;
        private Vector3 _playerUnitStartPosition;
        private Vector3 _opponentUnitStartPosition;
        
        // ranges
        private float _holdProgress;
        private float _holdStartTime;
        private const float MIN_HOLD = 0.25f;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _inputManager = GameManager.Instance.GetManager<InputManager>();

            _inputManager.OnHoldRight += OnHoldAttack;
            _inputManager.OnReleaseRight += OnReleaseAttack;

            pauseButton.onClick.AddListener(OnPaused);
            unPauseButton.onClick.AddListener(OnUnPause);
            exitButton.onClick.AddListener(OnExitButton);

            pauseMenu.gameObject.SetActive(false);

            if (playerUnit)
                _playerUnitStartPosition = playerUnit.position;

            if (opponentUnit)
                _opponentUnitStartPosition = opponentUnit.position;

            _holdProgress = 0f;
            UpdateRangeVisuals();
            if (unitPreviewContainer)
                unitPreviewContainer.gameObject.SetActive(false);
            UpdateUnitPreview(BaseUnit.UnitTypes.None);
            
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

        private void OnHoldAttack(Vector2 screenPos)
        {
            if (holdRanges == null || holdRanges.Length == 0)
                return;
            if (unitPreviewContainer)
            {
                unitPreviewContainer.gameObject.SetActive(true);
                unitPreviewContainer.position = screenPos;
            }

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
            
            UpdateRangeVisuals();
            UpdateUnitPreview(GetUnitTypeForRange(GetRangeIndex()));
        }

        private void OnReleaseAttack(Vector3 worldPos)
        {
            if (unitPreviewContainer)
                unitPreviewContainer.gameObject.SetActive(false);
            if (_holdStartTime <= 0f || Time.time - _holdStartTime < MIN_HOLD + 0.05f)
            {
                _holdStartTime = 0f;
                _holdProgress = 0f;
                UpdateRangeVisuals();
                UpdateUnitPreview(BaseUnit.UnitTypes.None);
                return;
            }

            var range = GetRangeIndex();
            OnReleasedRange(range, worldPos);

            _holdProgress = 0f;
            _holdStartTime = 0f;
            UpdateRangeVisuals();
            UpdateUnitPreview(BaseUnit.UnitTypes.None);
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

        private void UpdateRangeVisuals()
        {
            if (rangeImages == null || rangeImages.Length == 0)
                return;

            var filledCount = Mathf.Clamp(Mathf.FloorToInt(_holdProgress * rangeImages.Length), 0, rangeImages.Length);

            for (var i = 0; i < rangeImages.Length; i++)
            {
                if (!rangeImages[i])
                    continue;

                rangeImages[i].gameObject.SetActive(i < filledCount);
            }
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
        
        private void UpdateUnitPreview(BaseUnit.UnitTypes unitType)
        {
            if (previewReferences == null || previewReferences.Count == 0)
                return;

            foreach (var preview in previewReferences.Where(preview => preview != null && preview.unitReference))
            {
                preview.unitReference.SetActive(preview.unitType == unitType && unitType != BaseUnit.UnitTypes.None);
            }
        }
        
        private void FixedUpdate()
        {
            var playerDistance = _unitManager.GetPlayerKingDistance();
            var opponentDistance = _unitManager.GetOpponentKingDistance();

            if (playerUnit && playerCastleTarget)
                playerUnit.position = Vector3.Lerp(playerCastleTarget.position, _playerUnitStartPosition, playerDistance);

            if (opponentUnit && opponentCastleTarget)
                opponentUnit.position = Vector3.Lerp(opponentCastleTarget.position, _opponentUnitStartPosition, opponentDistance);
        }
    }

    [Serializable]
    public class HoldRanges
    {
        public float RangeDuration;
    }

    [Serializable]
    public class UnitPreviewReferences
    {
        public BaseUnit.UnitTypes unitType;
        public GameObject unitReference;
    }
}