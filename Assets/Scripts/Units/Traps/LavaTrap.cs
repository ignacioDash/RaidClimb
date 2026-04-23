using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Units.UnitTypes;
using UnityEngine;

namespace Units.Traps
{
    public class LavaTrap : BaseTrap
    {
        private List<BaseUnit> _unitsInTrap;
        private Dictionary<BaseUnit, GameObject> _burnParticles;
        private CancellationTokenSource _cts;
        private Task _trapLoop;
        private float _nextAttackTime;

        public override void Init(string playerId)
        {
            _unitsInTrap = new List<BaseUnit>();
            _burnParticles = new Dictionary<BaseUnit, GameObject>();
            trapType = TrapTypes.Lava;

            base.Init(playerId);

            _trapLoop = null;
            ChangeState(TrapState.Active);
        }

        private async Task LavaTrapLoop()
        {
            _cts = new CancellationTokenSource();

            while (CurrentTrapState == TrapState.Active && !_cts.Token.IsCancellationRequested)
            {
                if (Time.time >= _nextAttackTime)
                {
                    _nextAttackTime = Time.time + trapConfig.AttackSpeed;

                    foreach (var unit in _unitsInTrap)
                        unit.TakeDamage(trapConfig.Damage);
                }

                await Task.Yield();
            }
        }

        protected override void OnTrapActivated()
        {
            if (_trapLoop == null)
                _trapLoop = LavaTrapLoop();
        }

        protected override void OnTrapDisabled()
        {
            _cts?.Cancel();
            _trapLoop = null;
        }

        protected override void OnTrapDestroyed()
        {
            _cts?.Cancel();
            _trapLoop = null;
        }

        protected override void OnEnemyUnitEnteredTrap(BaseUnit unit)
        {
            if (CurrentTrapState != TrapState.Active) return;
            if (_unitsInTrap.Contains(unit)) return;

            _unitsInTrap.Add(unit);

            if (trapConfig.ProjectilePrefab != null)
            {
                var particles = Instantiate(trapConfig.ProjectilePrefab, unit.transform);
                particles.transform.localPosition = Vector3.zero;
                _burnParticles[unit] = particles;
            }
        }

        protected override void OnEnemyUnitExitedTrap(BaseUnit unit)
        {
            if (!_unitsInTrap.Contains(unit)) return;

            _unitsInTrap.Remove(unit);

            if (_burnParticles.TryGetValue(unit, out var particles))
            {
                if (particles) Destroy(particles);
                _burnParticles.Remove(unit);
            }
        }

        public override void CleanUp()
        {
            _unitsInTrap.Clear();

            foreach (var p in _burnParticles.Values)
                if (p) Destroy(p);

            _burnParticles.Clear();
            _cts?.Cancel();
            _trapLoop = null;
        }
    }
}
