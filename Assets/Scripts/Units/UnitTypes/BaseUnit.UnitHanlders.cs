using UnityEngine;

namespace Units.UnitTypes
{
    public abstract partial class BaseUnit
    {
        private float _lastClimbTime;
        private const float CLIMB_END_PUSH_X = 4f;
        private const float CLIMB_END_PUSH_Y = 6f;
        private const float CLIMB_CD = 2f;
        
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
                if (_moveTarget && _moveTarget.transform.position.y > transform.position.y)
                {
                    if (Time.time - _lastClimbTime < CLIMB_CD)
                        return;

                    _lastClimbTime = Time.time;
                    
                    _climbWall = other.GetComponent<Collider>();
                    
                    var position = transform.position;
                    var closestPoint = other.ClosestPoint(position);
                    _wallNormal = (position - closestPoint).normalized;
                    
                    transform.rotation = Quaternion.LookRotation(-_wallNormal);

                    ChangeUnitStateTo(UnitState.Climbing);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
        }

        protected virtual void OnTriggerAttacking()
        {
            _target.TargetInfo.AddTargeter(this);
        }
        
        protected virtual void OnTriggerDefending()
        {
            
        }
                
        protected virtual void HandleMoveToTarget()
        {
            if (!_moveTarget || _target.unitType == UnitTypes.King)
                return;

            var position = transform.position;
            var a = new Vector2(position.x, position.z);
            var targetPosition = _moveTarget.position;
            var b = new Vector2(targetPosition.x, targetPosition.z);

            var distance = Vector2.Distance(a, b);
            
            if (distance > unitConfig.Range)
                return;
            
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.isStopped = true;
            ChangeUnitStateTo(UnitState.Attacking);
        }

        protected virtual void HandleAttackTarget()
        {
            var target = _moveTarget ? _moveTarget : _target ? _target.transform : null;
            
            if (!target)
                return;

            var xDistance = Mathf.Abs(target.transform.position.x - transform.position.x);

            // defenders are handled via enter/exit
            if (!IsDefender && xDistance > unitConfig.Range)
            {
                ChangeUnitStateTo(UnitState.Moving);
                return;
            }
            
            if (Time.time < _nextAttackTime)
                return;
            
            AttackTarget();
        }

        protected virtual void HandleTargetMovements() // did the target moved more than retarget? if so refresh target's position
        {
            if (!_moveTarget || !navMeshAgent.enabled)
                return;

            _lastTargetPos = _moveTarget.position;

            if (Vector3.Distance(_lastTargetPos, _moveTarget.position) < RETARGET_DISTANCE)
                return;

            navMeshAgent.SetDestination(_lastTargetPos);
        }
    }
}