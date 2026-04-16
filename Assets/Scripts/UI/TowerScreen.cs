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
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            exitButton.onClick.AddListener(OnExit);

            exitButton.interactable = true;
        }

        private void OnDisable()
        {
            exitButton.onClick.RemoveListener(OnExit);
        }

        private async void OnExit()
        {
            exitButton.interactable = false;
            
            GameManager.Instance.GetManager<PlayerCastleManager>().OnCastleScreenClosed();

            var cameraTransition = GameManager.Instance.GetManager<CameraManager>()
                .SetCameraAt(CameraManager.CameraPosition.Default);

            var screenTransition =
                GameManager.Instance.GetManager<UIManager>().NavigateTo(UIManager.Screens.MainScreen);
            
            GameManager.Instance.GetManager<UnitManager>().Cleanup();
            GameManager.Instance.GetManager<TrapsManager>().Cleanup();

            await Task.WhenAll(cameraTransition, screenTransition);
        }
    }
}