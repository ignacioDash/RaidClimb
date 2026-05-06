using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Units.UnitTypes;
using UnityEngine;

namespace Units.Traps
{
    public class SawsTrap : BaseTrap
    {
        [SerializeField] private Transform saw1;
        [SerializeField] private Transform saw2;
        [SerializeField] private Transform saw3;

        private const float SawRange = 0.1f;
        private const float SawSpeed = 2f;

        private float _saw1OriginZ;
        private float _saw2OriginZ;
        private float _saw3OriginZ;

        private List<BaseUnit> _unitsInTrap;
        private CancellationTokenSource _cts;
        private Task _trapLoop;
        private float _nextAttackTime;

        public override void Init(string playerId)
        {
            if (saw1) _saw1OriginZ = saw1.localPosition.z;
            if (saw2) _saw2OriginZ = saw2.localPosition.z;
            if (saw3) _saw3OriginZ = saw3.localPosition.z;

            _unitsInTrap = new List<BaseUnit>();
            trapType = TrapTypes.Saws;

            base.Init(playerId);

            _trapLoop = null;
            ChangeState(TrapState.Active);
        }

        private async Task SawsTrapLoop()
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
                        PlayParticlesAtXZ(unit.transform.position.x, unit.transform.position.z);
                    }
                }

                await Task.Yield();
            }
        }

        protected override void OnTrapDisabled()
        {
            _cts?.Cancel();
            _trapLoop = null;
            StopParticles();
        }

        protected override void OnTrapActivated()
        {
            if (_trapLoop == null)
                _trapLoop = SawsTrapLoop();
        }

        protected override void OnTrapDestroyed()
        {
            _cts?.Cancel();
            _trapLoop = null;
            StopParticles();
        }

        protected override void OnEnemyUnitEnteredTrap(BaseUnit unit)
        {
            if (CurrentTrapState != TrapState.Active)
                return;

            if (!_unitsInTrap.Contains(unit))
            {
                _unitsInTrap.Add(unit);
                unit.OnDeath += () => OnEnemyUnitExitedTrap(unit);
            }
        }

        protected override void OnEnemyUnitExitedTrap(BaseUnit unit)
        {
            _unitsInTrap.Remove(unit);
        }

        private void Update()
        {
            if (CurrentTrapState == TrapState.Destroyed) return;

            var offset = Mathf.Sin(Time.time * SawSpeed) * SawRange;
            if (saw1) saw1.localPosition = new Vector3(saw1.localPosition.x, saw1.localPosition.y, _saw1OriginZ + offset);
            if (saw2) saw2.localPosition = new Vector3(saw2.localPosition.x, saw2.localPosition.y, _saw2OriginZ - offset);
            if (saw3) saw3.localPosition = new Vector3(saw3.localPosition.x, saw3.localPosition.y, _saw3OriginZ + offset);
        }

        public override void CleanUp()
        {
            _unitsInTrap.Clear();
            _cts?.Cancel();
            _trapLoop = null;
        }
    }
}
