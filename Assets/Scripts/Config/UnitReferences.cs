using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Managers;
using Units.UnitTypes;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "UnitReferences", menuName = "Config/UnitReferences")]
    public class UnitReferences : ScriptableObject
    {
        [SerializeField] private List<UnitReference> unitReferences;

        [CanBeNull]
        public BaseUnit GetUnit(BaseUnit.UnitTypes unitType) =>
            unitReferences.FirstOrDefault(u => u.UnitType == unitType)?.UnitPrefab;

        public string GetDisplayName(BaseUnit.UnitTypes unitType)
        {
            var entry = unitReferences.FirstOrDefault(u => u.UnitType == unitType);
            return entry?.Config != null ? entry.Config.UnitName : unitType.ToString();
        }

        public int GetSquadCost(BaseUnit.UnitTypes unitType)
        {
            var entry = unitReferences.FirstOrDefault(u => u.UnitType == unitType);
            return entry?.Config != null ? entry.Config.SquadCost : 0;
        }

        public List<BaseUnit.UnitTypes> GetUnitsUnlockingAtArena(int arena) =>
            unitReferences
                .Where(u => u.Config != null && u.Config.ArenaUnlock == arena)
                .Select(u => u.UnitType)
                .ToList();
    }
}