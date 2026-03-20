using System;
using Config;
using Units.UnitTypes;
using UnityEngine;

namespace Units.Traps
{
    [RequireComponent(typeof(Collider))]
    public abstract class BaseTrap : MonoBehaviour
    {
        public enum TrapTypes
        {
            None,
            Spikes,
            ThornHedge
        }

        public enum TrapState
        {
            Idle,
            Active,
            Destroyed
        }

        [SerializeField] protected TrapTypes trapType;
        [SerializeField] protected UnitBaseConfig trapConfig;
        
        private Collider _trapCollider;
        private string _playerId;

        protected TrapState _trapState;

        private void OnEnable()
        {
            _trapCollider = GetComponent<Collider>();
        }

        public virtual void Init(string playerId)
        {
            _playerId = playerId;

            ChangeState(TrapState.Idle);
        }

        protected void ChangeState(TrapState trapState)
        {
            if (trapState == _trapState)
                return;

            _trapState = trapState;
            
            switch (_trapState)
            {
                case TrapState.Idle:
                    OnTrapDisabled();
                    break;
                case TrapState.Active:
                    OnTrapActivated();
                    break;
                case TrapState.Destroyed:
                    OnTrapDestroyed();
                    break;
            }
        }

        protected abstract void OnTrapDisabled();

        protected abstract void OnTrapActivated();

        protected abstract void OnTrapDestroyed();

        protected abstract void OnEnemyUnitEnteredTrap(BaseUnit unit);

        protected abstract void OnEnemyUnitExitedTrap(BaseUnit unit);

        public abstract void CleanUp();

        private void OnTriggerEnter(Collider other)
        {
            var unit = other.GetComponentInParent<BaseUnit>();
            if (unit && unit.PlayerId != _playerId)
            {
                OnEnemyUnitEnteredTrap(unit);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var unit = other.GetComponentInParent<BaseUnit>();
            if (unit && unit.PlayerId != _playerId)
            {
                OnEnemyUnitExitedTrap(unit);
            }
        }
    }
}