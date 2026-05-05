using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Units.UnitTypes
{
    public class DefenderUnit : BaseUnit
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private ParticleSystem deathParticle;

        private SphereCollider _sphereCollider;
        private List<BaseUnit> _attackers;
        
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Defender;
            _sphereCollider = GetComponent<SphereCollider>();
            startState = UnitState.Defending;
            _attackers = new List<BaseUnit>();
            
            base.Init(playerId, startState, onUnitDeath);

            _sphereCollider.radius = unitConfig.Range;
            ChangeUnitStateTo(UnitState.Defending);
        }

        protected override void OnTriggerDefending()
        {
            base.OnTriggerDefending();

            _attackers.RemoveAll(a => !a || a.UnitCurrentState == UnitState.Dead
                                       || a.UnitCurrentState == UnitState.Won
                                       || a.UnitCurrentState == UnitState.Lost);

            var nextAttacker = _attackers.FirstOrDefault(a => a && a != _target);
            if (nextAttacker)
            {
                _attackers.Remove(nextAttacker);
                _target = nextAttacker;
                ChangeUnitStateTo(UnitState.Attacking);
            }
            else
            {
                _target = null;
            }
        }

        protected override void ApplyAttackRotation()
        {
            if (!_target || !visualRoot) return;
            var dir = _target.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                var worldRotation = Quaternion.LookRotation(dir);
                var localRotation = Quaternion.Inverse(transform.rotation) * worldRotation;
                var localEuler = visualRoot.localEulerAngles;
                visualRoot.localEulerAngles = new Vector3(localEuler.x, localRotation.eulerAngles.y, localEuler.z);
            }
        }

        protected override void OnEnteredDeadState()
        {
            if (deathParticle) deathParticle.Play();
        }

        protected override void HandleMoveToTarget() { }

        protected override void HandleTargetMovements() { }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Unit"))
            {
                var entryUnit = other.GetComponentInParent<BaseUnit>();
                if (entryUnit && entryUnit.PlayerId != PlayerId)
                {
                    if (UnitCurrentState == UnitState.Defending)
                    {
                        _target = entryUnit;
                        ChangeUnitStateTo(UnitState.Attacking);
                    }
                    else
                    {
                        _attackers.Add(entryUnit);
                    }
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            /*if (other.CompareTag("Unit"))
            {
                var exitUnit = other.GetComponent<BaseUnit>();
                if (exitUnit)
                {
                    exitUnit.TargetInfo.RemoveTargeter(this);
                    
                    if (exitUnit == _target)
                    {
                        _target = null;
                        ChangeUnitStateTo(UnitState.Defending);
                    }
                    else if (_attackers.Contains(exitUnit))
                    {
                        _attackers.Remove(exitUnit);
                    }
                }
            }*/
        }
    }
}