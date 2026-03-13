using System;

namespace Units.UnitTypes
{
    public class MeleeUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Melee;
            base.Init(playerId, startState, onUnitDeath);
        }
    }
}