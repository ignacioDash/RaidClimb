using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class TeslaCoilUnit : DefenderUnit
    {
        [SerializeField] private float splashRadius;

        private readonly Collider[] _splashBuffer = new Collider[20];

        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.TeslaCoil;
            base.Init(playerId, startState, onUnitDeath);
        }

        protected override Action<float> GetHitCallback(BaseUnit target)
        {
            return damage =>
            {
                if (target) target.TakeDamage(damage);

                var hitPosition = target ? target.transform.position : transform.position;
                var count = Physics.OverlapSphereNonAlloc(hitPosition, splashRadius, _splashBuffer);
                for (var i = 0; i < count; i++)
                {
                    var unit = _splashBuffer[i].GetComponentInParent<BaseUnit>();
                    if (unit && unit != target && unit.PlayerId != PlayerId)
                        unit.TakeDamage(damage);
                }
            };
        }
    }
}
