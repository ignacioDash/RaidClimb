using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class DeadeyeUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Deadeye;
            Attack = Animator.StringToHash("Arrow");
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}
