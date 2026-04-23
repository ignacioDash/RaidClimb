using System.Threading;
using System.Threading.Tasks;
using Units.UnitTypes;
using UnityEngine;

namespace Units.Traps
{
    public class PusherTrap : BaseTrap
    {
        [SerializeField] private Transform visual;
        [SerializeField] private float pushDuration = 0.3f;
        [SerializeField] private float detectionRadius = 2f;

        private const float PUSH_VISUAL_OFFSET = 0.8f;

        private readonly Collider[] _overlapBuffer = new Collider[20];
        private CancellationTokenSource _cts;
        private Task _trapLoop;
        private Vector3 _visualStartLocalPos;
        private string _playerId;

        public override void Init(string playerId)
        {
            _playerId = playerId;
            _visualStartLocalPos = visual ? visual.localPosition : Vector3.zero;
            trapType = TrapTypes.Pusher;

            base.Init(playerId);

            _trapLoop = null;
            ChangeState(TrapState.Active);
        }

        private async Task PusherTrapLoop()
        {
            _cts = new CancellationTokenSource();

            while (CurrentTrapState == TrapState.Active && !_cts.Token.IsCancellationRequested)
            {
                await Task.Delay((int)(trapConfig.AttackSpeed * 1000), _cts.Token);

                if (_cts.Token.IsCancellationRequested) break;

                await AnimatePush(_cts.Token);
            }
        }

        private async Task AnimatePush(CancellationToken token)
        {
            if (!visual) return;

            var extendedLocalPos = _visualStartLocalPos + Vector3.up * PUSH_VISUAL_OFFSET;

            var t = 0f;
            while (t < 1f && !token.IsCancellationRequested)
            {
                t += Time.deltaTime / pushDuration;
                visual.localPosition = Vector3.Lerp(_visualStartLocalPos, extendedLocalPos, Mathf.Clamp01(t));
                await Task.Yield();
            }

            if (token.IsCancellationRequested) return;

            PushNearbyClimbingUnits();

            t = 0f;
            while (t < 1f && !token.IsCancellationRequested)
            {
                t += Time.deltaTime / pushDuration;
                visual.localPosition = Vector3.Lerp(extendedLocalPos, _visualStartLocalPos, Mathf.Clamp01(t));
                await Task.Yield();
            }

            if (!token.IsCancellationRequested)
                visual.localPosition = _visualStartLocalPos;
        }

        private void PushNearbyClimbingUnits()
        {
            var count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, _overlapBuffer);
            var pushForce = transform.forward * trapConfig.Damage;

            for (var i = 0; i < count; i++)
            {
                var unit = _overlapBuffer[i].GetComponentInParent<BaseUnit>();
                if (unit && unit.PlayerId != _playerId && unit.UnitCurrentState == BaseUnit.UnitState.Climbing)
                    unit.InterruptClimb(pushForce);
            }
        }

        protected override void OnTrapActivated()
        {
            if (_trapLoop == null)
                _trapLoop = PusherTrapLoop();
        }

        protected override void OnTrapDisabled()
        {
            _cts?.Cancel();
            _trapLoop = null;
            if (visual) visual.localPosition = _visualStartLocalPos;
        }

        protected override void OnTrapDestroyed()
        {
            _cts?.Cancel();
            _trapLoop = null;
            if (visual) visual.localPosition = _visualStartLocalPos;
        }

        protected override void OnEnemyUnitEnteredTrap(BaseUnit unit) { }

        protected override void OnEnemyUnitExitedTrap(BaseUnit unit) { }

        public override void CleanUp()
        {
            _cts?.Cancel();
            _trapLoop = null;
            if (visual) visual.localPosition = _visualStartLocalPos;
        }
    }
}
