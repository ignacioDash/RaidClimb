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
            base.OnEnable();
            
            _uiManager = GameManager.Instance.GetManager<UIManager>();
            
            playButton.onClick.AddListener(OnPlayButton);
            settingsButton.onClick.AddListener(OnSettingsButton);
            leaderboardButton.onClick.AddListener(OnLeaderboardButton);
            collectionButton.onClick.AddListener(OnCollectionButton);
            towerButton.onClick.AddListener(OnTowerButton);
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButton);
            settingsButton.onClick.RemoveListener(OnSettingsButton);
            leaderboardButton.onClick.RemoveListener(OnLeaderboardButton);
            collectionButton.onClick.RemoveListener(OnCollectionButton);
            towerButton.onClick.RemoveListener(OnTowerButton);
        }

        private async void OnPlayButton()
        {
            playButton.interactable = false;

            await GameManager.Instance.StartGame();
        }

        private async void OnSettingsButton()
        {
            settingsButton.interactable = false;
            await _uiManager.NavigateTo(UIManager.Screens.SettingsScreen, true);
            settingsButton.interactable = true;
        }

        private async void OnLeaderboardButton()
        {
            leaderboardButton.interactable = false;
            await _uiManager.NavigateTo(UIManager.Screens.LeaderboardScreen);
        }

        private async void OnCollectionButton()
        {
            collectionButton.interactable = false;
            await _uiManager.NavigateTo(UIManager.Screens.CollectionScreen);
        }

        private async void OnTowerButton()
        {
            towerButton.interactable = false;
            await _uiManager.NavigateTo(UIManager.Screens.TowerScreen);
        }
    }
}