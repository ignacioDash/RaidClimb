using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using JetBrains.Annotations;
using Units;
using UnityEngine;

namespace Managers
{
    public class UnitManager : MonoBehaviour, IManager
    {
        [Header("Debug")]
        [SerializeField] private BaseUnit testTarget;
        
        [SerializeField] private Transform unitsContainer;
        
        [Header("Settings")]
        [SerializeField] private List<UnitReference> unitReferences;
        
        private readonly List<BaseUnit> _registeredUnits = new();
        
        public async Task Init(object[] args)
        {
            
        }

        [CanBeNull]
        public BaseUnit SpawnUnit(BaseUnit.UnitTypes unitType, Vector3 spawnPoint, string playerId)
        {
            var unitPrefab = unitReferences.FirstOrDefault(u => u.UnitType == unitType)?.UnitPrefab;
            if (unitPrefab == null)
            {
                Debug.LogError($"Could not find unit of type {unitType.ToString()}");
                return null;
            }

            var isPlayerUnit = playerId == Keys.PLAYER_ID;
            var unit = Instantiate(unitPrefab, spawnPoint,
                isPlayerUnit ? Quaternion.identity : Quaternion.Euler(new Vector3(0, 180, 0)), unitsContainer);
            
            return unit;
        }
        
        public void RegisterUnit(BaseUnit unit, string playerId)
        {
            _registeredUnits.Add(unit);
            
            unit.Init(playerId, () => UnRegisterUnit(unit));
            
            _ = DelayedTarget(unit);
        }

        private async Task DelayedTarget(BaseUnit unit)
        {
            await Task.Delay(2000);
            
            unit.SetUnitTarget(testTarget);
        }

        private void UnRegisterUnit(BaseUnit unit)
        {
            if (_registeredUnits.Contains(unit))
                _registeredUnits.Remove(unit);

            Destroy(unit.gameObject);
        }

        public void Cleanup()
        {
            foreach (var unit in _registeredUnits)
            {
                Destroy(unit.gameObject);
            }
            
            _registeredUnits.Clear();
        }
    }

    [Serializable]
    public class UnitReference
    {
        [SerializeField] private BaseUnit.UnitTypes unitType;
        [SerializeField] private BaseUnit unitPrefab;

        public BaseUnit.UnitTypes UnitType => unitType;
        public BaseUnit UnitPrefab => unitPrefab;
    }
}