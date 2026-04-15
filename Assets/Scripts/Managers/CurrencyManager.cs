using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class CurrencyManager : MonoBehaviour, IManager
    {
        [SerializeField] private TextMeshProUGUI coinsAmount;
        [SerializeField] private TextMeshProUGUI trophiesAmount;

        private DataManager _dataManager;
        private int _coinsAmount, _trophiesAmount;
        
        public void AddTrophies(int trophies)
        {
            _trophiesAmount += trophies;
            _dataManager.PlayerData.UserData.coins = _trophiesAmount;
            trophiesAmount.text = _trophiesAmount.ToString();
        }
        
        public void AddCoins(int coins)
        {
            _coinsAmount += coins;
            _dataManager.PlayerData.UserData.coins = _coinsAmount;
            coinsAmount.text = _coinsAmount.ToString();
        }

        private void SetTrophiesAmount(int amount)
        {
            _trophiesAmount = amount;
            trophiesAmount.text = _trophiesAmount.ToString();
        }

        private void SetCoinsAmount(int amount)
        {
            _coinsAmount = amount;
            coinsAmount.text = _coinsAmount.ToString();
        }
        
        public async Task Init(object[] args)
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();
            
            SetCoinsAmount(_dataManager.PlayerData.UserData.coins);
            SetTrophiesAmount(_dataManager.PlayerData.UserData.trophies);
        }

        public void Cleanup()
        {
            
        }
    }
}