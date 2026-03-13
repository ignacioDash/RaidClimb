using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class KingUnit : BaseUnit
    {
        private SphereCollider _sphereCollider;

        private bool _kingCaptured;
        
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.King;
            _kingCaptured = false;
            _sphereCollider = GetComponent<SphereCollider>();

            healthController.WorldCanvas.gameObject.SetActive(false);
            
            startState = UnitState.Idle;
            
            base.Init(playerId, startState, onUnitDeath);

            _sphereCollider.radius = unitConfig.Range;
        }

        protected override void HandleMoveToTarget() { }

        protected override void HandleTargetMovements() { }

        protected override void HandleAttackTarget() { }

        protected override async void OnTriggerEnter(Collider other)
        {
            if (_kingCaptured)
                return;
            
            if (other.CompareTag("Unit"))
            {
                var entryUnit = other.GetComponent<BaseUnit>();
                if (entryUnit && entryUnit.PlayerId != PlayerId)
                {
                    _kingCaptured = true;
                    await GameManager.Instance.GameEnded(entryUnit.PlayerId);
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            
        }
    }
}