using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameEndScreen : BaseScreen
    {
        [SerializeField] private TextMeshProUGUI youWinText;
        [SerializeField] private Button closeButton;

        private const string YOU_WIN_TEXT = "You Win!";
        private const string YOU_LOSE_TEXT = "You Lose!";

        protected override void OnEnable()
        {
            base.OnEnable();

            closeButton.onClick.AddListener(OnQuit);

            closeButton.interactable = true;
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(OnQuit);
        }

        public override async Task OpenScreen(object[] args)
        {
            if (args is { Length: > 0 })
            {
                var playerWon = (bool)args[0];

                youWinText.text = playerWon ? YOU_WIN_TEXT : YOU_LOSE_TEXT;
            }
            
            await base.OpenScreen(args);
        }

        private async void OnQuit()
        {
            closeButton.interactable = false;
            await GameManager.Instance.FinishGame();
        }
    }
}