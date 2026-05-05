using System.Threading.Tasks;
using Castles;
using Managers;
using Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class MainScreenUI : BaseScreen
    {
        [SerializeField] private Button playButton, settingsButton, towerButton, squadButton;
        [SerializeField] private GameObject castleNewIndicator;
        [SerializeField] private GameObject squadNewIndicator;
        [SerializeField] private OnboardingScreen onboardingScreen;
        [SerializeField] private UnitCamerasController unitCamerasController;
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

            var dataManager = GameManager.Instance.GetManager<DataManager>();
            var coins = dataManager.PlayerData.UserData.coins;
            var onboardingData = dataManager.PlayerData.OnboardingData;

            RefreshNewContentBadges(dataManager, arena);

            var castleStepActive = coins >= 25 && !onboardingData.IsStepCompleted(5);

            if (castleStepActive)
                onboardingScreen?.ShowMainMenuSteps();
            else if (arena >= 2 && !onboardingData.IsStepCompleted(9))
                onboardingScreen?.ShowMainMenuSquadSteps();
            else if (onboardingData.IsStepCompleted(5))
                onboardingScreen?.ShowMainMenuPlaySteps();

        }

        private async void OnPlayButton()
        {
            onboardingScreen?.TryCompleteStep(8);
            SetButtons(false);
            await OnMatchmakingStarted();
            await GameManager.Instance.StartGame();
        }

        private async Task OnMatchmakingStarted()
        {
            matchmakingContainer.SetActive(true);

            var equipped = GameManager.Instance.GetManager<DataManager>().PlayerData.SquadData.EquippedUnits;
            unitCamerasController?.ShowRandomFullBodyUnit(equipped);

            await Task.Delay(Random.Range(3000, 5001));

            unitCamerasController?.HideAllFullBodyUnits();
            matchmakingContainer.SetActive(false);
        }

        private async void OnSettingsButton()
        {
            SetButtons(false);
            await _uiManager.NavigateTo(UIManager.Screens.SettingsScreen, true);
            SetButtons(true);
        }
        
        private static readonly int[] CastleMilestones = { 4, 8, 12 };

        private void RefreshNewContentBadges(DataManager dataManager, int arena)
        {
            var newContent = dataManager.PlayerData.NewContentData;

            var hasNewCastle = false;
            foreach (var milestone in CastleMilestones)
            {
                if (milestone <= arena && !newContent.IsCastleMilestoneSeen(milestone))
                {
                    hasNewCastle = true;
                    break;
                }
            }
            if (castleNewIndicator) castleNewIndicator.SetActive(hasNewCastle);
            if (squadNewIndicator) squadNewIndicator.SetActive(newContent.NewUnitTypes.Count > 0);
        }

        private async void OnCastleButton()
        {
            var dataManager = GameManager.Instance.GetManager<DataManager>();
            var trophies = dataManager.PlayerData.UserData.trophies;
            var arena = GameManager.Instance.GetManager<CurrencyManager>().GetArenaForTrophies(trophies);
            var newContent = dataManager.PlayerData.NewContentData;
            foreach (var milestone in CastleMilestones)
            {
                if (milestone <= arena)
                    newContent.AcknowledgeCastleMilestone(milestone);
            }
            _ = dataManager.Save();
            if (castleNewIndicator) castleNewIndicator.SetActive(false);

            onboardingScreen?.TryCompleteStep(5);
            SetButtons(false);
            var cameraTransition = GameManager.Instance.GetManager<CameraManager>()
                .SetCameraAt(CameraManager.CameraPosition.Castle);
            
            var screenTransition = _uiManager.NavigateTo(UIManager.Screens.TowerScreen);
            
            GameManager.Instance.GetManager<PlayerCastleManager>().OnCastleScreenOpened();

            await Task.WhenAll(cameraTransition, screenTransition);
        }

        private async void OnSquadButton()
        {
            onboardingScreen?.TryCompleteStep(9);
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