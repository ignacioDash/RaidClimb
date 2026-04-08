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
    }
}