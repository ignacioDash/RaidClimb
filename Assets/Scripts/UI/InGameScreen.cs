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
        [SerializeField] private Slider unitSlider;
        [SerializeField] private Button pauseButton, unPauseButton, exitButton;
        [SerializeField] private GameObject pauseMenu;
        
        [Header("Settings")]
        [SerializeField] private float holdFillSpeed = 1f;

        private UnitManager _unitManager;
        private InputManager _inputManager;
        private BaseUnit _unitToSpawn;
        private Camera _cam;
        private Vector3 _lastValidUnitPosition;

        private const float MIN_Y_HEIGHT = 7f;

        protected override async void OnEnable()
        {
            base.OnEnable();

            unitSlider.value = 0;

            _cam = Camera.main;
            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _inputManager = GameManager.Instance.GetManager<InputManager>();

            _inputManager.OnHold += OnHoldInput;
            _inputManager.OnRelease += OnReleaseInput;

            pauseButton.onClick.AddListener(OnPaused);
            unPauseButton.onClick.AddListener(OnUnPause);
            exitButton.onClick.AddListener(OnExitButton);

            pauseMenu.gameObject.SetActive(false);
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