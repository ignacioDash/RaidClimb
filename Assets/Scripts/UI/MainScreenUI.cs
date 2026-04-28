using System.Threading.Tasks;
using Castles;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class MainScreenUI : BaseScreen
    {
        [SerializeField] private Button playButton, settingsButton, towerButton, squadButton;
        [SerializeField] private TextMeshProUGUI trophiesText;
        [SerializeField] private TextMeshProUGUI arenaText;
        [SerializeField] private Slider trophiesSlider;
        [SerializeField] private GameObject matchmakingContainer;

        private UIManager _uiManager;

        protected override void OnEnable()
        {
            SetButtons(false);
            base.OnEnable();
            
            _uiManager = GameManager.Instance.GetManager<UIManager>();
            
            playButton.onClick.AddListener(OnPlayButton);
            settingsButton.onClick.AddListener(OnSettingsButton);
            towerButton.onClick.AddListener(OnCastleButton);
            squadButton.onClick.AddListener(OnSquadButton);
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButton);
            settingsButton.onClick.RemoveListener(OnSettingsButton);
            towerButton.onClick.RemoveListener(OnCastleButton);
            squadButton.onClick.RemoveListener(OnSquadButton);
        }

        public override async Task OpenScreen(object[] args)
        {
            await base.OpenScreen(args);

            var currencyManager = GameManager.Instance.GetManager<CurrencyManager>();
            var trophies = GameManager.Instance.GetManager<DataManager>().PlayerData.UserData.trophies;
            var arena = currencyManager.GetArenaForTrophies(trophies);
            var progress = currencyManager.GetTrophyProgress(trophies);

            arenaText.text = $"{arena}";
            trophiesText.text = $"{progress.current}/{progress.next}";
            trophiesSlider.value = progress.next > 0 ? (float)progress.current / progress.next : 1f;

            GameManager.Instance.GetManager<PlayerCastleManager>().RefreshDefenses();
            matchmakingContainer.SetActive(false);
            SetButtons(true);
        }

        private async void OnPlayButton()
        {
            SetButtons(false);
            await OnMatchmakingStarted();
            await GameManager.Instance.StartGame();
        }

        private async Task OnMatchmakingStarted()
        {
            matchmakingContainer.SetActive(true);
            await Task.Delay(Random.Range(3000, 5001));
            matchmakingContainer.SetActive(false);
        }

        private async void OnSettingsButton()
        {
            SetButtons(false);
            await _uiManager.NavigateTo(UIManager.Screens.SettingsScreen, true);
            SetButtons(true);
        }
        
        private async void OnCastleButton()
        {
            SetButtons(false);
            var cameraTransition = GameManager.Instance.GetManager<CameraManager>()
                .SetCameraAt(CameraManager.CameraPosition.Castle);
            
            var screenTransition = _uiManager.NavigateTo(UIManager.Screens.TowerScreen);
            
            GameManager.Instance.GetManager<PlayerCastleManager>().OnCastleScreenOpened();

            await Task.WhenAll(cameraTransition, screenTransition);
        }

        private async void OnSquadButton()
        {
            SetButtons(false);
            await _uiManager.NavigateTo(UIManager.Screens.SquadScreen);
        }

        private void SetButtons(bool on)
        {
            towerButton.interactable = on;
            settingsButton.interactable = on;
            playButton.interactable = on;
            squadButton.interactable = on;
        }
    }
}