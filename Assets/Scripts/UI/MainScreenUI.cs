using System.Threading.Tasks;
using Castles;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainScreenUI : BaseScreen
    {
        [SerializeField] private Button playButton, settingsButton, leaderboardButton, collectionButton, towerButton;

        private UIManager _uiManager;

        protected override void OnEnable()
        {
            SetButtons(false);
            base.OnEnable();
            
            _uiManager = GameManager.Instance.GetManager<UIManager>();
            
            playButton.onClick.AddListener(OnPlayButton);
            settingsButton.onClick.AddListener(OnSettingsButton);
            leaderboardButton.onClick.AddListener(OnLeaderboardButton);
            collectionButton.onClick.AddListener(OnCollectionButton);
            towerButton.onClick.AddListener(OnCastleButton);
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButton);
            settingsButton.onClick.RemoveListener(OnSettingsButton);
            leaderboardButton.onClick.RemoveListener(OnLeaderboardButton);
            collectionButton.onClick.RemoveListener(OnCollectionButton);
            towerButton.onClick.RemoveListener(OnCastleButton);
        }

        public override async Task OpenScreen(object[] args)
        {
            await base.OpenScreen(args);
            
            SetButtons(true);
        }

        private async void OnPlayButton()
        {
            SetButtons(false);
            await GameManager.Instance.StartGame();
        }

        private async void OnSettingsButton()
        {
            SetButtons(false);
            await _uiManager.NavigateTo(UIManager.Screens.SettingsScreen, true);
            SetButtons(true);
        }

        private async void OnLeaderboardButton()
        {
            SetButtons(false);
            await _uiManager.NavigateTo(UIManager.Screens.LeaderboardScreen);
        }

        private async void OnCollectionButton()
        {
            SetButtons(false);
            await _uiManager.NavigateTo(UIManager.Screens.CollectionScreen);
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

        private void SetButtons(bool on)
        {
            towerButton.interactable = on;
            collectionButton.interactable = on;
            leaderboardButton.interactable = on;
            settingsButton.interactable = on;
            playButton.interactable = on;
        }
    }
}