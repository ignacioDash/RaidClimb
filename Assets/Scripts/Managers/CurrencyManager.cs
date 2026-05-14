using System.Collections.Generic;
using System.Threading.Tasks;
using Config;
using TMPro;
using Units.UnitTypes;
using UnityEngine;

namespace Managers
{
    public class CurrencyManager : MonoBehaviour, IManager
    {
        [SerializeField] private TextMeshProUGUI coinsAmount;
        [SerializeField] private EconomyConfig economyConfig;
        [SerializeField] private GameObject container;

        private DataManager _dataManager;
        private int _coinsAmount, _trophiesAmount;
        
        public List<BaseUnit.UnitTypes> AddTrophies(int trophies)
        {
            var arenaBefore = GetArenaForTrophies(_trophiesAmount);
            _trophiesAmount += trophies;
            _dataManager.PlayerData.UserData.trophies = _trophiesAmount;

            var arenaAfter = GetArenaForTrophies(_trophiesAmount);
            if (arenaAfter > arenaBefore)
                return CheckAndUnlockUnitsForArena(arenaAfter);

            return new List<BaseUnit.UnitTypes>();
        }

        private List<BaseUnit.UnitTypes> CheckAndUnlockUnitsForArena(int arena)
        {
            var unitManager = GameManager.Instance.GetManager<UnitManager>();
            var newUnits = unitManager.GetUnitsUnlockingAtArena(arena);
            var squad = _dataManager.PlayerData.SquadData;
            var unlocked = new List<BaseUnit.UnitTypes>();

            foreach (var unit in newUnits)
            {
                if (!squad.UnlockedUnits.Contains(unit))
                {
                    squad.UnlockedUnits.Add(unit);
                    unlocked.Add(unit);
                }
            }

            return unlocked;
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
        }

        private void SetCoinsAmount(int amount)
        {
            _coinsAmount = amount;
            coinsAmount.text = _coinsAmount.ToString();
        }
        
        private void Awake()
        {
            if (!container)
            {
                Debug.LogWarning("CurrencyManager: container reference is not assigned.", this);
                return;
            }
            container.SetActive(true);
        }

        public async Task Init(object[] args)
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();

            SetCoinsAmount(_dataManager.PlayerData.UserData.coins);
            SetTrophiesAmount(_dataManager.PlayerData.UserData.trophies);
        }

        public (int coins, int trophies, List<BaseUnit.UnitTypes> newlyUnlocked) AwardWinRewards()
        {
            var userData = _dataManager.PlayerData.UserData;
            userData.gamesPlayed++;

            var arenaBeforeWin = GetArenaForTrophies(userData.trophies);
            var newlyUnlocked = AddTrophies(economyConfig.trophiesPerWin);

            if (GetArenaForTrophies(userData.trophies) > arenaBeforeWin)
                userData.demotionShieldCharges = economyConfig.demotionShieldChargesOnPromotion;

            var coins = GetCoinsForTrophies(userData.trophies);

            if (userData.isFirstWin)
            {
                coins += economyConfig.tutorialBonusCoins;
                userData.isFirstWin = false;
            }

            AddCoins(coins);
            return (coins, economyConfig.trophiesPerWin, newlyUnlocked);
        }

        public (int coins, int trophies) HandleDefeat()
        {
            var userData = _dataManager.PlayerData.UserData;
            userData.gamesPlayed++;

            var coins = GetCoinsForTrophies(userData.trophies) / 3;
            AddCoins(coins);

            if (userData.gamesPlayed <= economyConfig.newPlayerShieldGames)
                return (coins, 0);

            if (userData.demotionShieldCharges > 0)
            {
                userData.demotionShieldCharges--;
                return (coins, 0);
            }

            AddTrophies(-economyConfig.trophiesPerLoss);
            return (coins, -economyConfig.trophiesPerLoss);
        }

        public int GetArenaForTrophies(int trophies)
        {
            var thresholds = economyConfig.arenaTrophyThresholds;
            var arena = 1;
            foreach (var threshold in thresholds)
            {
                if (trophies >= threshold)
                    arena++;
                else
                    break;
            }

            if (arena == thresholds.Count + 1 && thresholds.Count >= 2)
            {
                var lastGap = thresholds[thresholds.Count - 1] - thresholds[thresholds.Count - 2];
                arena += (trophies - thresholds[thresholds.Count - 1]) / lastGap;
            }

            return arena;
        }

        public (int current, int next) GetTrophyProgress(int trophies)
        {
            var prevThreshold = 0;
            var prevPrevThreshold = 0;
            foreach (var threshold in economyConfig.arenaTrophyThresholds)
            {
                if (trophies < threshold)
                    return (trophies - prevThreshold, threshold - prevThreshold);
                prevPrevThreshold = prevThreshold;
                prevThreshold = threshold;
            }

            var arenaGap = prevThreshold - prevPrevThreshold;
            if (arenaGap <= 0)
                return (trophies - prevThreshold, trophies - prevThreshold);
            return ((trophies - prevThreshold) % arenaGap, arenaGap);
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

        public void SetContainerActive(bool active)
        {
            if (container) container.SetActive(active);
        }

        public void Cleanup()
        {

        }
    }
}