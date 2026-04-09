using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class TankUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Tank;
            Attack = Animator.StringToHash("Sword");
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}