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

        [SerializeField] private Button unit1Pointer, unit2Pointer, unit3Pointer;

        [SerializeField] private TextMeshProUGUI unitName1, unitName2, unitName3;
        [SerializeField] private UnitCamerasController unitCamerasController;
        [SerializeField] private RawImage unitRawImage1, unitRawImage2, unitRawImage3;

        [Header("Squad Meter")]
        [SerializeField] private Image[] squadMeterImages;
        [SerializeField] private TextMeshProUGUI unitCost1, unitCost2, unitCost3;

        [Header("Progress bar")]
        [SerializeField] private RectTransform playerCastleTarget;
        [SerializeField] private RectTransform opponentCastleTarget;
        [SerializeField] private RectTransform playerUnit, opponentUnit;

        [Header("Settings")]
        [SerializeField] private Collider playerSpawnArea;
        [SerializeField] private OnboardingScreen onboardingScreen;
        [SerializeField] private TextMeshProUGUI opponentNameText;

        [System.Serializable]
        private class OpponentNamesData { public string[] names; }

        private UnitManager _unitManager;

        private Vector3 _playerUnitStartPosition;
        private Vector3 _opponentUnitStartPosition;

        private int _squadMeter;
        private float _refillAccumulator;
        private bool _inGameOnboardingShown;
        private bool _step1Shown;
        private bool _step2Shown;

        private void Awake()
        {
            if (playerUnit) _playerUnitStartPosition = playerUnit.position;
            if (opponentUnit) _opponentUnitStartPosition = opponentUnit.position;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _unitManager.OnPlayerUnitDied += OnPlayerUnitDied;

            unit1Pointer.onClick.AddListener(() => OnUnitButtonPressed(0));
            unit2Pointer.onClick.AddListener(() => OnUnitButtonPressed(1));
            unit3Pointer.onClick.AddListener(() => OnUnitButtonPressed(2));

            pauseButton.onClick.AddListener(OnPaused);
            unPauseButton.onClick.AddListener(OnUnPause);
            exitButton.onClick.AddListener(OnExitButton);

            pauseMenu.SetActive(false);

            if (playerUnit) playerUnit.position = _playerUnitStartPosition;
            if (opponentUnit) opponentUnit.position = _opponentUnitStartPosition;

            _squadMeter = 1;
            _refillAccumulator = 0f;
            _inGameOnboardingShown = true;
            _step1Shown = false;
            _step2Shown = false;
            UpdateSquadMeter();
            StartCoroutine(ShowInGameOnboardingNextFrame());

            if (unitPreviewContainer)
                unitPreviewContainer.gameObject.SetActive(false);

            UpdateUnitNames();
            SetOpponentName();
            SetButtons(true);
        }

        private IEnumerator ShowInGameOnboardingNextFrame()
        {
            yield return new WaitForSeconds(0.4f);
            onboardingScreen?.ShowInGameSteps();
        }

        private void OnDisable()
        {
            _unitManager.OnPlayerUnitDied -= OnPlayerUnitDied;

            unit1Pointer.onClick.RemoveAllListeners();
            unit2Pointer.onClick.RemoveAllListeners();
            unit3Pointer.onClick.RemoveAllListeners();

            pauseButton.onClick.RemoveListener(OnPaused);
            unPauseButton.onClick.RemoveListener(OnUnPause);
            exitButton.onClick.RemoveListener(OnExitButton);
            StopAllCoroutines();
        }

        private void OnUnitButtonPressed(int index)
        {
            var unitType = GetUnitTypeForButton(index);
            if (unitType == BaseUnit.UnitTypes.None) return;

            var cost = _unitManager.GetUnitSquadCost(unitType);
            if (cost > _squadMeter) return;

            var unitPosition = GetRandomSpawnPosition();
            var unit = _unitManager.SpawnUnit(unitType, unitPosition, Keys.PLAYER_ID);
            if (!unit) return;

            _squadMeter -= cost;
            UpdateSquadMeter();
            StartCoroutine(SetDelayedTarget(unit, 1.5f));

            onboardingScreen?.TryCompleteStep(0);
            if (index == 1) onboardingScreen?.TryCompleteStep(1);
            onboardingScreen?.TryCompleteStep(2);
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

            var rawImages = new[] { unitRawImage1, unitRawImage2, unitRawImage3 };
            for (var i = 0; i < rawImages.Length; i++)
            {
                if (!rawImages[i]) continue;
                var unitType = i < equipped.Count ? equipped[i] : BaseUnit.UnitTypes.None;
                rawImages[i].texture = unitType != BaseUnit.UnitTypes.None
                    ? unitCamerasController.GetRenderTexture(unitType)
                    : null;
            }
        }

        private void SetOpponentName()
        {
            if (!opponentNameText) return;
            var asset = Resources.Load<TextAsset>("opponent_names");
            if (asset == null) return;
            var data = JsonUtility.FromJson<OpponentNamesData>(asset.text);
            if (data?.names?.Length > 0)
                opponentNameText.text = data.names[Random.Range(0, data.names.Length)];
        }

        private void OnPlayerUnitDied(BaseUnit unit)
        {
            var max = squadMeterImages != null ? squadMeterImages.Length : 0;
            _squadMeter = Mathf.Min(_squadMeter + _unitManager.GetUnitSquadCost(unit.UnitType), max);
            UpdateSquadMeter();
        }

        private void UpdateSquadMeter()
        {
            if (squadMeterImages == null) return;
            for (var i = 0; i < squadMeterImages.Length; i++)
                if (squadMeterImages[i]) squadMeterImages[i].gameObject.SetActive(i < _squadMeter);
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var pointers = new[] { unit1Pointer, unit2Pointer, unit3Pointer };

            for (var i = 0; i < pointers.Length; i++)
            {
                if (!pointers[i]) continue;
                var unitType = GetUnitTypeForButton(i);
                var cost = unitType != BaseUnit.UnitTypes.None ? _unitManager.GetUnitSquadCost(unitType) : 0;
                pointers[i].interactable = cost > 0 && cost <= _squadMeter;
            }
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

        private float _progressTimer;

        private void Update()
        {
            var max = squadMeterImages != null ? squadMeterImages.Length : 0;
            if (_squadMeter < max)
            {
                _refillAccumulator += Time.deltaTime;
                if (_refillAccumulator >= 3f)
                {
                    _refillAccumulator -= 3f;
                    _squadMeter = Mathf.Min(_squadMeter + 1, max);
                    UpdateSquadMeter();

                    if (_squadMeter == 1 && !_inGameOnboardingShown)
                    {
                        _inGameOnboardingShown = true;
                        onboardingScreen?.ShowInGameSteps();
                    }

                    if (_squadMeter == 3 && !_step1Shown)
                    {
                        _step1Shown = true;
                        onboardingScreen?.ShowInGameStep1();
                    }

                    if (_squadMeter == 5 && !_step2Shown)
                    {
                        _step2Shown = true;
                        onboardingScreen?.ShowInGameStep2();
                    }
                }
            }

            _progressTimer += Time.deltaTime;
            if (_progressTimer >= 0.1f)
            {
                _progressTimer = 0f;
                UpdateProgressBar();
            }
        }

        private void UpdateProgressBar()
        {
            var playerDistance = _unitManager.GetPlayerKingDistance();
            var opponentDistance = _unitManager.GetOpponentKingDistance();

            if (playerUnit && playerCastleTarget)
                playerUnit.position = Vector3.Lerp(playerCastleTarget.position, _playerUnitStartPosition, opponentDistance);

            if (opponentUnit && opponentCastleTarget)
                opponentUnit.position = Vector3.Lerp(opponentCastleTarget.position, _opponentUnitStartPosition, playerDistance);
        }
    }
}
