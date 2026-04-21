using System.Collections.Generic;
using Units.UnitTypes;

namespace Units.Traps
{
    public class SpikeTrap : BaseTrap
    {
        private List<BaseUnit> _affectedUnits;
        
        public override void Init(string playerId)
        {
            _affectedUnits = new List<BaseUnit>();
            trapType = TrapTypes.Spikes;
            
            base.Init(playerId);
            ChangeState(TrapState.Active);
        }

        protected override void OnTrapDisabled() {}

        protected override void OnTrapActivated() {}

        protected override void OnTrapDestroyed() {}

        protected override void OnEnemyUnitEnteredTrap(BaseUnit unit)
        {
            if (CurrentTrapState != TrapState.Active)
                return;

            if (!_affectedUnits.Contains(unit))
            {
                unit.TakeDamage(trapConfig.Damage);
                PlayParticlesAtXZ(unit.transform.position.x, unit.transform.position.z);
                _affectedUnits.Add(unit);
            }
        }

        protected override void OnEnemyUnitExitedTrap(BaseUnit unit) { }

        public override void CleanUp()
        {
            _affectedUnits.Clear();
        }
    }
}