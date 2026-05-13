using System;
using System.Threading.Tasks;
using Castles;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TowerScreen : BaseScreen
    {
        [SerializeField] private Button exitButton;
        [SerializeField] private OnboardingScreen onboardingScreen;
        [SerializeField] private GameObject instructionsContainer;

        private const int CastleUpgradeCoinsThreshold = 25;

        public override async Task OpenScreen(object[] args)
        {
            await base.OpenScreen(args);

            var data = GameManager.Instance.GetManager<DataManager>().PlayerData;

            if (instructionsContainer != null)
                instructionsContainer.SetActive(data.OnboardingData.IsStepCompleted(7));

            if (data.UserData.coins >= CastleUpgradeCoinsThreshold)
                onboardingScreen?.ShowTowerSteps();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            exitButton.onClick.AddListener(OnExit);
            exitButton.interactable = true;

            GameManager.Instance.GetManager<PlayerCastleManager>().OnSlotPurchased += OnSlotPurchased;
        }

        private void OnDisable()
        {
            exitButton.onClick.RemoveListener(OnExit);

            var castleManager = GameManager.Instance.GetManager<PlayerCastleManager>();
            if (castleManager != null)
                castleManager.OnSlotPurchased -= OnSlotPurchased;
        }

        private void OnSlotPurchased()
        {
            onboardingScreen?.TryCompleteStep(6);
        }

        private async void OnExit()
        {
            onboardingScreen?.TryCompleteStep(7);
            exitButton.interactable = false;

            GameManager.Instance.GetManager<PlayerCastleManager>().OnCastleScreenClosed();

            var cameraTransition = GameManager.Instance.GetManager<CameraManager>()
                .SetCameraAt(CameraManager.CameraPosition.Default);

            var screenTransition =
                GameManager.Instance.GetManager<UIManager>().NavigateTo(UIManager.Screens.MainScreen);

            await Task.WhenAll(cameraTransition, screenTransition);
        }
    }
}