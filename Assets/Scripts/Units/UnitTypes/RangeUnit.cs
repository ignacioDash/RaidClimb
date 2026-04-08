using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class RangeUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Ranged;
            Attack = Animator.StringToHash("Arrow");
            base.Init(playerId, startState, onUnitDeath);

            _attackRotationOffset = new Vector3(0, 60, 0);
        }
    }
}