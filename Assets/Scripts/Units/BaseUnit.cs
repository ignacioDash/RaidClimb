using System;
using System.Threading.Tasks;
using Config;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Units
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public abstract class BaseUnit : MonoBehaviour
    {
        public enum UnitState
        {
            Idle, // doing nothing
            Moving, // walking towards the closest enemy
            Climbing, // climbing towards the closest enemy
            Defending, // waiting for a trigger to move/attack
            Attacking, // actively attacking the closest enemy
            Dead
        }

        public enum UnitTypes
        {
            None,
            Melee,
            Ranged,
            Tank,
            Swat,
            Sapper,
            King
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
        public UnitTargetController UnitTargetController => unitTargetController;
        public Rigidbody Rigidbody { get; private set; }
        public Collider UnitCollider { get; private set; }

        private Transform _target;
        private Vector3 _wallNormal, _lastTargetPos;
        private Collider _climbWall;
        private Action _onUnitDeath;

        private const float RETARGET_DISTANCE = 0.2f;

        private void OnEnable()
        {
            Rigidbody = GetComponent<Rigidbody>();
            UnitCollider = GetComponent<Collider>();
        }

        public virtual void Init(string playerId, Action onUnitDeath)
        {
            PlayerId = playerId;
            unitCurrentState = UnitState.Idle;
            _onUnitDeath += onUnitDeath;
            
            navMeshAgent.enabled = false;
            
            rangeCollider.Init(this);
                healthController.Init(unitConfig, () => ChangeUnitStateTo(UnitState.Dead));
        }

        public void SetUnitTarget(BaseUnit target)
        {
            Rigidbody.useGravity = false;
            
            _target = target.unitTargetController.GetRandomTarget(transform.position);
            unitCurrentState = UnitState.Moving;
            OnStateChanged();
            
            navMeshAgent.SetDestination(_target.transform.position);
        }

        public void ChangeUnitStateTo(UnitState newState)
        {
            unitCurrentState = newState;
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            Debug.Log($"Change state to {unitCurrentState}");
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
                    // todo: play idle animation
                    Rigidbody.isKinematic = false;
                    break;
                case UnitState.Attacking:
                    // todo: play attacking animation
                    Rigidbody.isKinematic = false;
                    break;
                case UnitState.Dead:
                    // todo: await play death animation
                    
                    _onUnitDeath?.Invoke();
                    break;
                default:
                    break;
            }
        }

        private void OnStartMovement()
        {
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
            
            Rigidbody.linearVelocity = Vector3.zero;
        }

        private void OnClimbWall()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
            Rigidbody.freezeRotation = true;

            Rigidbody.linearVelocity = Vector3.up * unitConfig.ClimbSpeed;
        }
        
        private async Task DelayedTarget()
        {
            await Task.Delay(500);
            
            Rigidbody.freezeRotation = false;
            
            unitCurrentState = UnitState.Moving;
            OnStateChanged();
            
            navMeshAgent.SetDestination(_target.transform.position);
        }
        
        private void HandleMoveToTarget()
        {
            if (!_target)
                return;

            var xDistance = Mathf.Abs(_target.transform.position.x - transform.position.x);

            if (!(xDistance <= unitConfig.Range))
                return;
            
            navMeshAgent.isStopped = true;
            ChangeUnitStateTo(UnitState.Attacking);
        }

        private void HandleAttackTarget()
        {
            if (!_target)
                return;

            var xDistance = Mathf.Abs(_target.transform.position.x - transform.position.x);

            if (!(xDistance > unitConfig.Range))
                return;
            
            ChangeUnitStateTo(UnitState.Moving);
        }

        private void HandleTargetMovements()
        {
            if (!_target || !navMeshAgent.enabled)
                return;

            _lastTargetPos = _target.position;

            if (Vector3.Distance(_lastTargetPos, _target.position) < RETARGET_DISTANCE)
                return;

            navMeshAgent.SetDestination(_lastTargetPos);
        }
        
        #region UNITY
        
        private void Update()
        {
            HandleTargetMovements();
            
            switch (unitCurrentState)
            {
                case UnitState.Moving:
                    HandleMoveToTarget();
                    break;
                case UnitState.Attacking:
                    HandleAttackTarget();
                    break;
            }
        }
        
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("CastleWall") && unitCurrentState == UnitState.Moving)
            {
                if (_target && _target.transform.position.y > transform.position.y)
                {
                    _climbWall = other.GetComponent<Collider>();
                    
                    var position = transform.position;
                    var closestPoint = other.ClosestPoint(position);
                    _wallNormal = (position - closestPoint).normalized;

                    ChangeUnitStateTo(UnitState.Climbing);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other == _climbWall && unitCurrentState == UnitState.Climbing)
            {
                var velocity = _wallNormal * unitConfig.ClimbSpeed * -2f;

                Rigidbody.linearVelocity = velocity;
                Rigidbody.useGravity = true;

                _climbWall = null;

                _ = DelayedTarget();
            }
        }
        
        #endregion
    }
}