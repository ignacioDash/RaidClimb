using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Units.UnitTypes;
using UnityEngine;

namespace Units.Traps
{
    public class ThornHedge : BaseTrap
    {
        private List<BaseUnit> _unitsInTrap;
        private CancellationTokenSource _cts;
        private Task _trapLoop;
        private float _nextAttackTime;
        
        public override void Init(string playerId)
        {
            _unitsInTrap = new List<BaseUnit>();
            trapType = TrapTypes.ThornHedge;
            
            base.Init(playerId);

            _trapLoop = null;
        }

        private async Task ThornHedgeTrapLoop()
        {
            _cts = new CancellationTokenSource();

            while (CurrentTrapState == TrapState.Active && !_cts.Token.IsCancellationRequested)
            {
                if (Time.time >= _nextAttackTime)
                {
                    _nextAttackTime = Time.time + trapConfig.AttackSpeed;

                    foreach (var unit in _unitsInTrap)
                    {
                        unit.TakeDamage(trapConfig.Damage);
                    }
                }

                await Task.Yield(); 
            }
        }

        protected override void OnTrapDisabled()
        {
            _cts?.Cancel();
            _trapLoop = null;
        }

        protected override void OnTrapActivated()
        {
            if (_trapLoop == null)
                _trapLoop = ThornHedgeTrapLoop();
        }

        protected override void OnTrapDestroyed()
        {
            _cts?.Cancel();
            _trapLoop = null;
        }

        protected override void OnEnemyUnitEnteredTrap(BaseUnit unit)
        {
            if (CurrentTrapState != TrapState.Active)
                return;
            
            if (!_unitsInTrap.Contains(unit))
                _unitsInTrap.Add(unit);
        }

        protected override void OnEnemyUnitExitedTrap(BaseUnit unit)
        {
            if (CurrentTrapState != TrapState.Active)
                return;
            
            if (_unitsInTrap.Contains(unit))
                _unitsInTrap.Remove(unit);
        }

        public override void CleanUp()
        {
            _unitsInTrap.Clear();
            _cts?.Cancel();
            _trapLoop = null;
        }
    }
}