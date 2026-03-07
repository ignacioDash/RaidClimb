using System;
using Config;
using UnityEngine;
using UnityEngine.Serialization;

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

        [Header("Settings")]
        [SerializeField] protected UnitTypes unitType;
        [SerializeField] protected UnitBaseConfig unitConfig;
        
        [Header("References")]
        [SerializeField] protected UnitRangeCollider rangeCollider;
        [SerializeField] protected UnitHealthController healthController;
        [SerializeField] protected UnitVisualsController unitVisualsController;

        public string PlayerId { get; private set; }
        public BaseUnit Target { get; private set; }
        public UnitState UnitCurrentState { get; private set; }
        
        private Rigidbody _rigidbody;
        private Collider _unitCollider;
        private Collider _climbWall;

        private Action _onUnitDeath;

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _unitCollider = GetComponent<Collider>();
        }

        public virtual void Init(string playerId, Action onUnitDeath)
        {
            PlayerId = playerId;
            UnitCurrentState = UnitState.Idle;
            _onUnitDeath += onUnitDeath;

            rangeCollider.Init(this);
                healthController.Init(unitConfig, () => ChangeUnitStateTo(UnitState.Dead));
        }

        public void SetVisuals(bool on)
        {
            unitVisualsController.SetUnitVisuals(on);
        }

        public void SetColliders(bool on)
        {
            _rigidbody.useGravity = on;
            _unitCollider.enabled = on;
            rangeCollider.RangeCollider.enabled = on;
        }

        public void SetUnitTarget(BaseUnit target)
        {
            Target = target;
        }

        public void ChangeUnitStateTo(UnitState newState)
        {
            UnitCurrentState = newState;
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            Debug.Log($"Change state to {UnitCurrentState}");
            switch (UnitCurrentState)
            {
                case UnitState.Idle:
                    // todo: play idle animation
                    break;
                case UnitState.Moving:
                    // todo: play moving animation
                    break;
                case UnitState.Climbing:
                    // todo: play climbing animation
                    break;
                case UnitState.Defending:
                    // todo: play idle animation
                    break;
                case UnitState.Attacking:
                    // todo: play attacking animation
                    break;
                case UnitState.Dead:
                    // todo: await play death animation
                    
                    _onUnitDeath?.Invoke();
                    break;
                default:
                    break;
            }
        }

        private void FixedUpdate()
        {
            switch (UnitCurrentState)
            {
                case UnitState.Idle:
                    break;
                case UnitState.Climbing:
                case UnitState.Moving:
                    HandleUnitMovement();
                    break;
                case UnitState.Defending:
                    break;
                case UnitState.Attacking:
                    break;
                default:
                    break;
            }
        }

        private void HandleUnitMovement()
        {
            switch (UnitCurrentState)
            {
                case UnitState.Climbing:
                    _rigidbody.linearVelocity = new Vector3(0f, unitConfig.ClimbSpeed, 0f);
                    break;
                case UnitState.Moving:
                    var to = Target.transform.position - transform.position;
                    to.y = 0f;

                    var dir = to.normalized;
                    _rigidbody.linearVelocity = new Vector3(dir.x, 0, dir.z) * unitConfig.MovementSpeed;
                    break;
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("CastleWall") && UnitCurrentState == UnitState.Moving)
            {
                if (Target && Target.transform.position.y > transform.position.y)
                {
                    _climbWall = collision.collider;

                    _rigidbody.useGravity = false;
                    _rigidbody.linearVelocity = Vector3.zero;
                    
                    ChangeUnitStateTo(UnitState.Climbing);
                }
            }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (collision.collider == _climbWall && UnitCurrentState == UnitState.Climbing)
            {
                _climbWall = null;

                _rigidbody.useGravity = true;
                
                ChangeUnitStateTo(UnitState.Moving);
            }
        }
    }
}