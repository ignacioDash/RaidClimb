using System;
using System.Collections.Generic;
using Units.UnitTypes;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "BotDifficultyConfig", menuName = "Config/BotDifficultyConfig")]
    public class BotDifficultyConfig : ScriptableObject
    {
        public List<BotDifficultyTier> tiers;

        public BotDifficultyTier GetTierForArena(int arena)
        {
            var result = tiers[0];
            foreach (var tier in tiers)
            {
                if (arena >= tier.minArena)
                    result = tier;
                else
                    break;
            }
            return result;
        }
    }

    [Serializable]
    public class BotDifficultyTier
    {
        public int minArena;
        public int minSpawnIntervalMs;
        public int maxSpawnIntervalMs;
        public List<BaseUnit.UnitTypes> availableUnits;
    }
}
