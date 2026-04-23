using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class KingCobraUnit : DefenderUnit
    {
        [SerializeField] private int tickRate;

        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.KingCobra;
            base.Init(playerId, startState, onUnitDeath);
        }

        protected override Action<float> GetHitCallback(BaseUnit target)
        {
            return damage =>
            {
                if (target) target.ApplyPoison(damage / tickRate, tickRate);
            };
        }
    }
}
