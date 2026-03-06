using System;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(Collider))]
    public class UnitRangeCollider : MonoBehaviour
    {
        public Collider RangeCollider { get; private set; }
        private BaseUnit _baseUnit;

        public void Init(BaseUnit baseUnit)
        {
            _baseUnit = baseUnit;
        }

        private void OnEnable()
        {
            RangeCollider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var unitOnRange = other.GetComponent<BaseUnit>();
            if (unitOnRange)
            {
                if (_baseUnit.Target == unitOnRange)
                {
                    // stop moving, trigger attacking
                    if (_baseUnit.UnitCurrentState == BaseUnit.UnitState.Moving)
                        _baseUnit.ChangeUnitStateTo(BaseUnit.UnitState.Attacking);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var unitOnRange = other.GetComponent<BaseUnit>();
            if (unitOnRange)
            {
                if (_baseUnit.Target == unitOnRange)
                {
                    // stop attacking, trigger moving
                    if (_baseUnit.UnitCurrentState == BaseUnit.UnitState.Attacking)
                        _baseUnit.ChangeUnitStateTo(BaseUnit.UnitState.Moving);
                }
            }
        }
    }
}