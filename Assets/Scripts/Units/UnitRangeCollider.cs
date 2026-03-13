using System;
using Units.UnitTypes;
using UnityEngine;

namespace Units
{
    public class UnitRangeCollider : MonoBehaviour
    {
        private BaseUnit _baseUnit;

        public void Init(BaseUnit baseUnit)
        {
            _baseUnit = baseUnit;
        }
    }
}