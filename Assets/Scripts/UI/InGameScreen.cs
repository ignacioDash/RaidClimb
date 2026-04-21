using System;
using System.Collections;
using Constants;
using Managers;
using TMPro;
using Units;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class InGameScreen : BaseScreen
    {
        [SerializeField] private Button pauseButton, unPauseButton, exitButton;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private RectTransform unitPreviewContainer;

        [SerializeField] private ButtonPointerComponent unit1Pointer, unit2Pointer, unit3Pointer;

        [SerializeField] private TextMeshProUGUI unitName1, unitName2, unitName3;
        [SerializeField] private UnitCamerasController unitCamerasController;
        [SerializeField] private Image[] rangeImages;
        [SerializeField] private Image[] range2Images;
        [SerializeField] private Image[] range3Images;

        [Header("Squad Meter")]
        [SerializeField] private Image[] squadMeterImages;
        [SerializeField] private TextMeshProUGUI unitCost1, unitCost2, unitCost3;

        [Header("Progress bar")]
        [SerializeField] private RectTransform playerCastleTarget;
        [SerializeField] private RectTransform opponentCastleTarget;
        [SerializeField] private RectTransform playerUnit, opponentUnit;

        [Header("Settings")]
        [SerializeField] private HoldRanges[] holdRanges;
        [SerializeField] private Collider playerSpawnArea;

        private UnitManager _unitManager;

        private Vector3 _playerUnitStartPosition;
        private Vector3 _opponentUnitStartPosition;

        private int _activeButtonIndex = -1;
        private float _fillProgress;
        private float _fillStartTime;
        private int _squadUsed;

        private Action _onUnit1Down, _onUnit1Up;
        private Action _onUnit2Down, _onUnit2Up;
        private Action _onUnit3Down, _onUnit3Up;

        private void Awake()
        {
            // Cache lambdas so OnDisable can unsubscribe the same instances
            _onUnit1Down = () => OnButtonPressed(0); _onUnit1Up = () => OnButtonReleased(0);
            _onUnit2Down = () => OnButtonPressed(1); _onUnit2Up = () => OnButtonReleased(1);
            _onUnit3Down = () => OnButtonPressed(2); _onUnit3Up = () => OnButtonReleased(2);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _unitManager = GameManager.Instance.GetManager<UnitManager>();

            unit1Pointer.PointerDown += _onUnit1Down;
            unit1Pointer.PointerUp   += _onUnit1Up;
            unit2Pointer.PointerDown += _onUnit2Down;
            unit2Pointer.PointerUp   += _onUnit2Up;
            unit3Pointer.PointerDown += _onUnit3Down;
            unit3Pointer.PointerUp   += _onUnit3Up;

            pauseButton.onClick.AddListener(OnPaused);
            unPauseButton.onClick.AddListener(OnUnPause);
            exitButton.onClick.AddListener(OnExitButton);

            pauseMenu.SetActive(false);

            if (playerUnit)
                _playerUnitStartPosition = playerUnit.position;

            if (opponentUnit)
                _opponentUnitStartPosition = opponentUnit.position;

            _activeButtonIndex = -1;
            _fillProgress = 0f;
            _squadUsed = 0;
            UpdateRangeVisuals();
            UpdateSquadMeter();

            if (unitPreviewContainer)
                unitPreviewContainer.gameObject.SetActive(false);

            UpdateUnitNames();
            SetButtons(true);
        }

        private void OnDisable()
        {
            unit1Pointer.PointerDown -= _onUnit1Down;
            unit1Pointer.PointerUp   -= _onUnit1Up;
            unit2Pointer.PointerDown -= _onUnit2Down;
            unit2Pointer.PointerUp   -= _onUnit2Up;
            unit3Pointer.PointerDown -= _onUnit3Down;
            unit3Pointer.PointerUp   -= _onUnit3Up;

            pauseButton.onClick.RemoveListener(OnPaused);
            unPauseButton.onClick.RemoveListener(OnUnPause);
            exitButton.onClick.RemoveListener(OnExitButton);
            StopAllCoroutines();
        }

        private void Update()
        {
            if (_activeButtonIndex < 0) return;

            _fillProgress = Mathf.Clamp01((Time.time - _fillStartTime) / holdRanges[_activeButtonIndex].RangeDuration);

            UpdateRangeVisuals();

            if (_fillProgress >= 1f)
                OnFillCompleted(_activeButtonIndex);
        }

        private void OnButtonPressed(int index)
        {
            _activeButtonIndex = index;
            _fillStartTime = Time.time;
            _fillProgress = 0f;
            UpdateRangeVisuals();
        }

        private void OnButtonReleased(int index)
        {
            if (_activeButtonIndex != index) return;
            _activeButtonIndex = -1;
            _fillProgress = 0f;
            UpdateRangeVisuals();
        }

        private void OnFillCompleted(int buttonIndex)
        {
            var unitType = GetUnitTypeForButton(buttonIndex);
            if (unitType != BaseUnit.UnitTypes.None)
            {
                var cost = _unitManager.GetUnitSquadCost(unitType);
                if (_squadUsed + cost <= squadMeterImages.Length)
                {
                    var unitPosition = GetRandomSpawnPosition();
                    var unit = _unitManager.SpawnUnit(unitType, unitPosition, Keys.PLAYER_ID);
                    if (unit)
                    {
                        _squadUsed += cost;
                        unit.OnDeath += () => FreeSquadCost(cost);
                        UpdateSquadMeter();
                        StartCoroutine(SetDelayedTarget(unit, 1.5f));
                    }
                }
            }

            _fillStartTime = Time.time;
            _fillProgress = 0f;
            UpdateRangeVisuals();
        }

        private BaseUnit.UnitTypes GetUnitTypeForButton(int index)
        {
            var equipped = GameManager.Instance.GetManager<DataManager>()
                .PlayerData.SquadData.EquippedUnits;
            return index < equipped.Count ? equipped[index] : BaseUnit.UnitTypes.None;
        }

        private void UpdateUnitNames()
        {
            var equipped = GameManager.Instance.GetManager<DataManager>()
                .PlayerData.SquadData.EquippedUnits;
            var nameTexts = new[] { unitName1, unitName2, unitName3 };
            var costTexts = new[] { unitCost1, unitCost2, unitCost3 };

            for (var i = 0; i < nameTexts.Length; i++)
            {
                var unitType = i < equipped.Count ? equipped[i] : BaseUnit.UnitTypes.None;

                if (nameTexts[i] != null)
                    nameTexts[i].text = unitType != BaseUnit.UnitTypes.None
                        ? _unitManager.GetUnitDisplayName(unitType)
                        : string.Empty;

                if (costTexts[i] != null)
                    costTexts[i].text = unitType != BaseUnit.UnitTypes.None
                        ? _unitManager.GetUnitSquadCost(unitType).ToString()
                        : string.Empty;
            }

            unitCamerasController.Init(equipped);
        }

        private void FreeSquadCost(int cost)
        {
            _squadUsed = Mathf.Max(0, _squadUsed - cost);
            UpdateSquadMeter();
        }

        private void UpdateSquadMeter()
        {
            if (squadMeterImages == null) return;
            for (var i = 0; i < squadMeterImages.Length; i++)
                if (squadMeterImages[i]) squadMeterImages[i].gameObject.SetActive(i < _squadUsed);
        }

        private void UpdateRangeVisuals()
        {
            UpdateImageArray(rangeImages,  _activeButtonIndex == 0 ? _fillProgress : 0f);
            UpdateImageArray(range2Images, _activeButtonIndex == 1 ? _fillProgress : 0f);
            UpdateImageArray(range3Images, _activeButtonIndex == 2 ? _fillProgress : 0f);
        }

        private void UpdateImageArray(Image[] images, float progress)
        {
            if (images == null || images.Length == 0) return;
            var filled = Mathf.Clamp(Mathf.FloorToInt(progress * images.Length), 0, images.Length);
            for (var i = 0; i < images.Length; i++)
                if (images[i]) images[i].gameObject.SetActive(i < filled);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            var box = playerSpawnArea as BoxCollider;
            if (box != null)
            {
                var localPoint = new Vector3(
                    (Random.value - 0.5f) * box.size.x + box.center.x,
                    box.center.y,
                    (Random.value - 0.5f) * box.size.z + box.center.z
                );
                var worldPoint = box.transform.TransformPoint(localPoint);
                return new Vector3(worldPoint.x, Values.UNIT_SPAWN_Y, worldPoint.z);
            }

            var bounds = playerSpawnArea.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Values.UNIT_SPAWN_Y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        private IEnumerator SetDelayedTarget(BaseUnit unit, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (unit) _unitManager.FindNewTargetFor(unit);
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
}
