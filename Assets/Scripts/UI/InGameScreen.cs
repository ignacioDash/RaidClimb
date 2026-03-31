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
        [SerializeField] private Slider unitSlider1;
        [SerializeField] private Image fillImage;
        [SerializeField] private Button pauseButton, unPauseButton, exitButton;
        [SerializeField] private GameObject pauseMenu;

        [Header("Settings")]
        [SerializeField] private HoldRanges[] holdRanges;

        private UnitManager _unitManager;
        private InputManager _inputManager;
        private BaseUnit _unitToSpawn;
        private Vector3 _lastValidUnitPosition;
        
        // ranges
        private float _totalDuration;
        private float[] _thresholds; // normalized thresholds [0..1]
        private int _currentRange = -1;

        private const float MIN_Y_HEIGHT = 7f;

        protected override void OnEnable()
        {
            base.OnEnable();

            unitSlider1.value = 0;

            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _inputManager = GameManager.Instance.GetManager<InputManager>();

            _inputManager.OnHold += OnHoldInput;
            _inputManager.OnRelease += OnReleaseInput;

            pauseButton.onClick.AddListener(OnPaused);
            unPauseButton.onClick.AddListener(OnUnPause);
            exitButton.onClick.AddListener(OnExitButton);

            pauseMenu.gameObject.SetActive(false);

            fillImage.sprite = holdRanges[0].RangeSprite;
            
            _thresholds = new float[holdRanges.Length];

            _totalDuration = 0f;
            foreach (var t in holdRanges)
                _totalDuration += t.RangeDuration;

            var acc = 0f;
            for (var i = 0; i < holdRanges.Length; i++)
            {
                acc += holdRanges[i].RangeDuration;
                _thresholds[i] = acc / _totalDuration;
            }
            
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
                Mathf.FloorToInt(unitSlider1.value / sectionSize),
                holdRanges.Length - 1);

            var targetValue = (sectionIndex + 1) * sectionSize;
            var speed = sectionSize / holdRanges[sectionIndex].RangeDuration;

            unitSlider1.value = Mathf.MoveTowards(
                unitSlider1.value,
                targetValue,
                speed * Time.deltaTime);

            var newRange = Mathf.Min(
                Mathf.FloorToInt(unitSlider1.value / sectionSize),
                holdRanges.Length - 1);

            if (unitSlider1.value >= targetValue)
                newRange = sectionIndex + 1;

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

            unitSlider1.value = 0f;
            _currentRange = -1;
        }
        
        private void OnReachedRange(int index)
        {
            if (index < 0 || index >= holdRanges.Length)
                return;
            
            var rangeSprite = holdRanges[index].RangeSprite;
            fillImage.sprite = rangeSprite;
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
                1 => BaseUnit.UnitTypes.Melee,
                2 => BaseUnit.UnitTypes.Melee,
                3 => BaseUnit.UnitTypes.Melee,
                4 => BaseUnit.UnitTypes.Melee,
                _ => BaseUnit.UnitTypes.Melee
            };
        }
        
        private int GetRangeIndex()
        {
            var sectionSize = 1f / holdRanges.Length;
            return Mathf.FloorToInt(unitSlider1.value / sectionSize);
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

    [Serializable]
    public class HoldRanges
    {
        public float RangeDuration;
        public Sprite RangeSprite;
    }
}