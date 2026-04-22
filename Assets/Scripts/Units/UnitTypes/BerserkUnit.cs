using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class BerserkUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Berserk;
            Attack = Animator.StringToHash("Sword");
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}
