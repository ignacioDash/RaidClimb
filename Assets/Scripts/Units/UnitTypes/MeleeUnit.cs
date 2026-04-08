using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class MeleeUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Melee;
            Attack = Animator.StringToHash("Sword");
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}