using System;
using System.Threading;
using System.Threading.Tasks;
using Config;
using UnityEngine;
using UnityEngine.AI;

namespace Units.UnitTypes
{
    public abstract partial class BaseUnit : MonoBehaviour
    {
        public enum UnitState
        {
            Idle, // doing nothing
            Moving, // walking towards the closest enemy
            Climbing, // climbing towards the closest enemy
            Defending, // waiting for a trigger to move/attack
            Attacking, // actively attacking the closest enemy
            Dead,
            Won,
            Lost
        }

        public enum UnitTypes
        {
            None,
            Melee,
            Ranged,
            Tank,
            Swat,
            Sapper,
            King,
            Defender,
            // todo: other defender types?
        }
        
        [SerializeField] private UnitState unitCurrentState;
        
        [Header("Settings")]
        [SerializeField] protected UnitTypes unitType;
        [SerializeField] protected UnitBaseConfig unitConfig;

        [Header("References")]
        [SerializeField] protected UnitRangeCollider rangeCollider;
        [SerializeField] protected UnitHealthController healthController;
        [SerializeField] protected UnitVisualsController unitVisualsController;
        [SerializeField] private UnitTargetController unitTargetController;
        [SerializeField] private NavMeshAgent navMeshAgent;

        public string PlayerId { get; private set; }
        public UnitTargetInfo TargetInfo { get; private set; } // who is targeting this unit
        public UnitState UnitCurrentState => unitCurrentState;
        public UnitTypes UnitType => unitType;
        public bool IsDefender => unitType == UnitTypes.Defender;

        // common
        protected BaseUnit _target;
        private Rigidbody _rigidbody;
        
        //movement
        private Transform _moveTarget;
        
        // climbing
        private Vector3 _wallNormal, _lastTargetPos;
        private Collider _climbWall;
        private CancellationTokenSource _climbCts;
        private Task _climbTask;
        
        // attacking
        private float _nextAttackTime;
        
        // death
        private Action _onUnitDeath;

        private const float RETARGET_DISTANCE = 0.1f;

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public virtual void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            PlayerId = playerId;
            ChangeUnitStateTo(startState);
            
            TargetInfo = new UnitTargetInfo();
            _onUnitDeath += onUnitDeath;
            
            if (navMeshAgent)
                navMeshAgent.enabled = false;
            
            rangeCollider.Init(this);
            healthController.Init(unitConfig, () => ChangeUnitStateTo(UnitState.Dead));
        }

        public void SetUnitTarget(BaseUnit target, UnitState state)
        {
            _target = target;
            _moveTarget = target.unitTargetController.GetRandomTarget(transform.position);

            if (_target.unitType == UnitTypes.King)
                state = UnitState.Moving;
            
            ChangeUnitStateTo(state);
            
            if (navMeshAgent && navMeshAgent.enabled)
                navMeshAgent.SetDestination(_moveTarget.position);
        }

        public void ChangeUnitStateTo(UnitState newState)
        {
            if (unitCurrentState == newState)
                return;
            
            unitCurrentState = newState;
            OnStateChanged();
        }

        public void TakeDamage(float damage)
        {
            healthController.OnUnitHealthChanged(-damage);
        }

        private void OnStateChanged()
        {
            switch (unitCurrentState)
            {
                case UnitState.Idle:
                    // todo: play idle animation
                    break;
                case UnitState.Moving:
                    // todo: play moving animation
                    OnStartMovement();
                    break;
                case UnitState.Climbing:
                    // todo: play climbing animation
                    OnClimbWall();
                    break;
                case UnitState.Defending:
                    // todo: play defending animation
                    OnTriggerDefending();
                    break;
                case UnitState.Attacking:
                    // todo: play attacking animation
                    OnTriggerAttacking();
                    break;
                case UnitState.Dead:
                    _target.unitTargetController.ReturnTarget(_moveTarget);
                    // todo: await play death animation
                    _onUnitDeath?.Invoke();
                    break;
                case UnitState.Won:
                    // todo: play victory animation
                    OnGameEnded();
                    break;
                case UnitState.Lost:
                    // todo: play lose animation
                    OnGameEnded();
                    break;
                default:
                    break;
            }
        }
        
        private void OnGameEnded()
        {
            if (navMeshAgent && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }

            if (_rigidbody)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }
            
            _climbCts?.Cancel();
            _climbCts?.Dispose();
        }

        private void OnStartMovement()
        {
            if (!_rigidbody.isKinematic)
                _rigidbody.linearVelocity = Vector3.zero;
            
            _rigidbody.freezeRotation = false;
            
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
        }

        private void OnClimbWall()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;

            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _rigidbody.freezeRotation = true;

            _climbCts?.Cancel();
            _climbCts?.Dispose();
            _climbCts = new CancellationTokenSource();

            _climbTask = ClimbWallLoop(_climbCts.Token);
        }
        
        private async Task ClimbWallLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _climbWall && unitCurrentState == UnitState.Climbing)
            {
                transform.position += Vector3.up * (unitConfig.ClimbSpeed * Time.deltaTime);
                await Task.Yield();
            }
        }
        
        private async Task DelayedTarget()
        {
            await Task.Delay(1000);
            
            unitCurrentState = UnitState.Moving;
            OnStateChanged();
            
            navMeshAgent.SetDestination(_moveTarget.transform.position);
        }
        
        protected void AttackTarget()
        {
            if (!_target)
                return;
            
            _nextAttackTime = Time.time + unitConfig.AttackSpeed;

            _target.TakeDamage(unitConfig.Damage);
        }

        public void CleanUp()
        {
            _climbCts?.Cancel();
            _climbCts?.Dispose();
            _climbCts = null;
        }
    }
}