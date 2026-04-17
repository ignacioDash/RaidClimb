using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Config/EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        public int trophiesPerWin = 30;
        public int trophiesPerLoss = 20;
        public int tutorialBonusCoins = 200;
        public int newPlayerShieldGames = 10;
        public int demotionShieldChargesOnPromotion = 1;
        public List<ArenaRewardTier> arenaRewardTiers;
        public List<int> arenaTrophyThresholds;
    }

    [Serializable]
    public class ArenaRewardTier
    {
        public int minTrophies;
        public int coinsPerWin;
    }
}
