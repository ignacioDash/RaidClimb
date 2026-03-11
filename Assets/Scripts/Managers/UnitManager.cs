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
        [SerializeField] private Transform unitsContainer;
        
        [Header("Settings")]
        [SerializeField] private List<UnitReference> unitReferences;
        
        private readonly List<BaseUnit> _playerUnits = new();
        private readonly List<BaseUnit> _opponentUnits = new();
        
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
            
            if (isPlayerUnit) _playerUnits.Add(unit);
            else _opponentUnits.Add(unit);
            
            unit.Init(playerId, () => UnRegisterUnit(unit));
            
            return unit;
        }

        public async Task SetDelayedTarget(BaseUnit unit)
        {
            await Task.Delay(1500);

            var isPlayerUnit = unit.PlayerId == Keys.PLAYER_ID;
            var enemyUnits = isPlayerUnit ? _opponentUnits : _playerUnits;

            BaseUnit closestEnemy = null;
            var closestDistance = float.MaxValue;

            foreach (var enemy in enemyUnits)
            {
                if (!enemy) continue;

                var distance = (enemy.transform.position - unit.transform.position).sqrMagnitude;
                if (!(distance < closestDistance))
                    continue;
                
                closestDistance = distance;
                closestEnemy = enemy;
            }

            unit.SetUnitTarget(closestEnemy);
        }

        private void UnRegisterUnit(BaseUnit unit)
        {
            if (unit.PlayerId == Keys.PLAYER_ID)
            {
                if (_playerUnits.Contains(unit))
                    _playerUnits.Remove(unit);
            }
            else
            {
                if (_opponentUnits.Contains(unit))
                    _opponentUnits.Remove(unit);
            }

            Destroy(unit.gameObject);
        }

        public void Cleanup()
        {
            foreach (var unit in _playerUnits)
            {
                Destroy(unit.gameObject);
            }
            
            foreach (var unit in _opponentUnits)
            {
                Destroy(unit.gameObject);
            }
            
            _playerUnits.Clear();
            _opponentUnits.Clear();
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