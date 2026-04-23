using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Units;
using Units.UnitTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameEndScreen : BaseScreen
    {
        [SerializeField] private TextMeshProUGUI youWinText, coinsWon, trophiesWon;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject unlockPopupPrefab;
        [SerializeField] private Transform popupsContainer;
        [SerializeField] private UnitCamerasController unitCamerasController;

        private readonly List<GameObject> _spawnedPopups = new();

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
            ClearPopups();
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

                var newlyUnlocked = args.Length >= 4 ? args[3] as List<BaseUnit.UnitTypes> : null;
                SpawnUnlockPopups(newlyUnlocked ?? new List<BaseUnit.UnitTypes>());
            }

            await base.OpenScreen(args);
        }

        private void SpawnUnlockPopups(List<BaseUnit.UnitTypes> units)
        {
            ClearPopups();

            popupsContainer.gameObject.SetActive(units.Count > 0);

            foreach (var unitType in units)
            {
                var go = Instantiate(unlockPopupPrefab, popupsContainer);
                var popup = go.GetComponent<UnlockPopup>();
                var renderTexture = unitCamerasController.EnableUnitCamera(unitType);
                popup.Setup(unitType.ToString(), renderTexture, OnPopupClosed);
                _spawnedPopups.Add(go);
            }
        }

        private void OnPopupClosed()
        {
            _spawnedPopups.RemoveAll(p => !p);
            if (_spawnedPopups.Count == 0)
                popupsContainer.gameObject.SetActive(false);
        }

        private void ClearPopups()
        {
            foreach (var popup in _spawnedPopups)
                if (popup) Destroy(popup);
            _spawnedPopups.Clear();
            popupsContainer.gameObject.SetActive(false);
        }

        private async void OnQuit()
        {
            closeButton.interactable = false;
            await GameManager.Instance.FinishGame();
        }
    }
}