using System;
using System.Threading;
using System.Threading.Tasks;
using Config;
using Constants;
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
        [SerializeField] protected UnitHealthController healthController;
        [SerializeField] protected UnitVisualsController unitVisualsController;
        [SerializeField] private UnitTargetController unitTargetController;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] protected Animator animatorController;

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
        protected Vector3 _attackRotationOffset;
        
        // death
        private Action _onUnitDeath;
        
        private static readonly int Falling = Animator.StringToHash("Falling");
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int Victory = Animator.StringToHash("Dancing");
        private static readonly int Defeat = Animator.StringToHash("Defeat");
        private static readonly int Climb = Animator.StringToHash("Climb");
        protected static int Attack = Animator.StringToHash("");
        protected static int Empty = Animator.StringToHash("Empty");

        private const float RETARGET_DISTANCE = 0.1f;

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public virtual void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            _attackRotationOffset = Vector3.zero;
            PlayerId = playerId;
            
            unitVisualsController.Init(PlayerId);
            ChangeUnitStateTo(startState);
            
            TargetInfo = new UnitTargetInfo();
            _onUnitDeath += onUnitDeath;
            
            if (navMeshAgent)
                navMeshAgent.enabled = false;

            var useAttackCamera = (PlayerId == Keys.PLAYER_ID && !IsDefender) ||
                                  (PlayerId == Keys.OPPONENT_ID && IsDefender);
            
            healthController.Init(unitConfig, () => ChangeUnitStateTo(UnitState.Dead), useAttackCamera);
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
            if (unitCurrentState == newState || unitCurrentState == UnitState.Lost || unitCurrentState == UnitState.Won)
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
                    if (animatorController && unitType != UnitTypes.Defender)
                        animatorController.SetTrigger(Falling);
                    break;
                case UnitState.Moving:
                    if (animatorController)
                        animatorController.SetTrigger(Walking);
                    OnStartMovement();
                    break;
                case UnitState.Climbing:
                    if (animatorController)
                        animatorController.SetTrigger(Climb);
                    OnClimbWall();
                    break;
                case UnitState.Defending:
                    // todo: play defending animation
                    OnTriggerDefending();
                    break;
                case UnitState.Attacking:
                    if (gameObject && _target)
                    {
                        var dir = _target.transform.position - transform.position;
                        dir.y = 0f;

                        if (dir.sqrMagnitude > 0.001f)
                        {
                            transform.rotation = Quaternion.LookRotation(dir);
                            transform.Rotate(_attackRotationOffset);
                        }
                    }
                    
                    if (animatorController)
                        animatorController.SetTrigger(Attack);
                    
                    OnTriggerAttacking();
                    break;
                case UnitState.Dead:
                    if (animatorController && animatorController.runtimeAnimatorController)
                        animatorController.SetTrigger(Death);
                    
                    if (_target && _target.unitTargetController)
                        _target.unitTargetController.ReturnTarget(_moveTarget);
                    
                    DelayedDeath();
                    break;
                case UnitState.Won:
                    if (animatorController)
                        animatorController.SetTrigger(Victory);

                    healthController.gameObject.SetActive(false);
                    OnGameEnded();
                    break;
                case UnitState.Lost:
                    if (animatorController)
                        animatorController.SetTrigger(Defeat);
                    
                    healthController.gameObject.SetActive(false);
                    OnGameEnded();
                    break;
                default:
                    break;
            }
        }

        private async void DelayedDeath()
        {
            await Task.Delay(2000);
            _onUnitDeath?.Invoke();
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

            _nextAttackTime = Time.time + (1f / unitConfig.AttackSpeed);

            _target.TakeDamage(unitConfig.Damage);
        }

        public virtual void CleanUp()
        {
            _climbCts?.Cancel();
            _climbCts = null;
        }
    }
}