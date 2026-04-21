using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class RaiderUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Raider;
            Attack = Animator.StringToHash("Sword");
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}
