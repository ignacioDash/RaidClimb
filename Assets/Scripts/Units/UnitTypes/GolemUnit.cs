using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class GolemUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Golem;
            Attack = Animator.StringToHash("Sword");
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}
