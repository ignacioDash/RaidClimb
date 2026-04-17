using System.Threading.Tasks;
using Config;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class CurrencyManager : MonoBehaviour, IManager
    {
        [SerializeField] private TextMeshProUGUI coinsAmount;
        [SerializeField] private TextMeshProUGUI trophiesAmount;
        [SerializeField] private EconomyConfig economyConfig;

        private DataManager _dataManager;
        private int _coinsAmount, _trophiesAmount;
        
        public void AddTrophies(int trophies)
        {
            _trophiesAmount += trophies;
            _dataManager.PlayerData.UserData.trophies = _trophiesAmount;
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

        public (int coins, int trophies) AwardWinRewards()
        {
            var userData = _dataManager.PlayerData.UserData;
            userData.gamesPlayed++;

            var arenaBeforeWin = GetArenaForTrophies(userData.trophies);
            AddTrophies(economyConfig.trophiesPerWin);

            if (GetArenaForTrophies(userData.trophies) > arenaBeforeWin)
                userData.demotionShieldCharges = economyConfig.demotionShieldChargesOnPromotion;

            var coins = GetCoinsForTrophies(userData.trophies);

            if (userData.isFirstWin)
            {
                coins += economyConfig.tutorialBonusCoins;
                userData.isFirstWin = false;
            }

            AddCoins(coins);
            return (coins, economyConfig.trophiesPerWin);
        }

        public int HandleDefeat()
        {
            var userData = _dataManager.PlayerData.UserData;
            userData.gamesPlayed++;

            if (userData.gamesPlayed <= economyConfig.newPlayerShieldGames)
                return 0;

            if (userData.demotionShieldCharges > 0)
            {
                userData.demotionShieldCharges--;
                return 0;
            }

            AddTrophies(-economyConfig.trophiesPerLoss);
            return -economyConfig.trophiesPerLoss;
        }

        public int GetArenaForTrophies(int trophies)
        {
            var arena = 1;
            foreach (var threshold in economyConfig.arenaTrophyThresholds)
            {
                if (trophies >= threshold)
                    arena++;
                else
                    break;
            }
            return arena;
        }

        private int GetCoinsForTrophies(int trophies)
        {
            var reward = economyConfig.arenaRewardTiers[0].coinsPerWin;
            foreach (var tier in economyConfig.arenaRewardTiers)
            {
                if (trophies >= tier.minTrophies)
                    reward = tier.coinsPerWin;
                else
                    break;
            }
            return reward;
        }

        public void Cleanup()
        {

        }
    }
}