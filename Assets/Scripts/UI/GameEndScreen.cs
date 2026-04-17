using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameEndScreen : BaseScreen
    {
        [SerializeField] private TextMeshProUGUI youWinText, coinsWon, trophiesWon;
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
            if (args is { Length: >= 3 })
            {
                var playerWon = (bool)args[0];
                var coinsEarned = (int)args[1];
                var trophiesEarned = (int)args[2];

                youWinText.text = playerWon ? YOU_WIN_TEXT : YOU_LOSE_TEXT;
                coinsWon.text = coinsEarned > 0 ? $"+{coinsEarned}" : coinsEarned < 0 ? $"-{coinsEarned}" : coinsEarned.ToString();
                trophiesWon.text = trophiesEarned > 0 ? $"+{trophiesEarned}" :
                    trophiesEarned < 0 ? $"-{trophiesEarned}" : trophiesEarned.ToString();
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