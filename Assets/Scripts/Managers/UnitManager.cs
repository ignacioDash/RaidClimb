using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Config;
using Constants;
using JetBrains.Annotations;
using Units.UnitTypes;
using UnityEngine;

namespace Managers
{
    public class UnitManager : MonoBehaviour, IManager
    {
        [SerializeField] private Transform unitsContainer;
        
        [Header("Settings")]
        [SerializeField] private UnitReferences unitReferences;
        
        private readonly List<BaseUnit> _playerUnits = new();
        private readonly List<BaseUnit> _opponentUnits = new();
        private readonly Dictionary<BaseUnit, float> _idleStateStartTimes = new();

        private const float MAX_IDLE_STATE_TIME = 10f;
        
        public async Task Init(object[] args)
        {
            
        }

        private void Update()
        {
            CheckBuggedIdleUnits(_playerUnits);
            CheckBuggedIdleUnits(_opponentUnits);
        }

        private void CheckBuggedIdleUnits(List<BaseUnit> units)
        {
            for (var i = units.Count - 1; i >= 0; i--)
            {
                var unit = units[i];
                if (!unit)
                    continue;

                if (unit.UnitCurrentState != BaseUnit.UnitState.Idle)
                {
                    _idleStateStartTimes.Remove(unit);
                    continue;
                }

                if (!_idleStateStartTimes.TryGetValue(unit, out var idleStartTime))
                {
                    _idleStateStartTimes[unit] = Time.time;
                    continue;
                }

                if (Time.time - idleStartTime < MAX_IDLE_STATE_TIME)
                    continue;

                Debug.LogWarning($"Destroying bugged idle unit: {unit.name}");
                UnRegisterUnit(unit);
                _idleStateStartTimes.Remove(unit);
            }
        }

        public void OnGameEnded(string winnerId)
        {
            var playerWon = winnerId == Keys.PLAYER_ID;
            
            foreach (var unit in _playerUnits)
            {
                unit.ChangeUnitStateTo(playerWon ? BaseUnit.UnitState.Won : BaseUnit.UnitState.Lost);
            }

            foreach (var unit in _opponentUnits)
            {
                unit.ChangeUnitStateTo(!playerWon ? BaseUnit.UnitState.Won : BaseUnit.UnitState.Lost);
            }
        }

        [CanBeNull]
        public BaseUnit SpawnUnit(BaseUnit.UnitTypes unitType, Vector3 spawnPoint, string playerId, BaseUnit.UnitState startState = BaseUnit.UnitState.Idle)
        {
            var unitPrefab = unitReferences.GetUnit(unitType);
            if (unitPrefab == null)
            {
                Debug.LogError($"Could not find unit of type {unitType.ToString()}");
                return null;
            }

            var isPlayerUnit = playerId == Keys.PLAYER_ID;
            var unitRotation = isPlayerUnit
                ? Quaternion.Euler(new Vector3(0, 90, 0))
                : Quaternion.Euler(new Vector3(0, -90, 0));
            
            var unit = Instantiate(unitPrefab, spawnPoint, unitRotation, unitsContainer);
            
            var unitRenderer = unit.GetComponent<Renderer>();
            if (unitRenderer)
            {
                var transform1 = unit.transform;
                var pos = transform1.position;
                pos.y += unitRenderer.bounds.extents.y;
                transform1.position = pos;
            }
            
            if (isPlayerUnit) _playerUnits.Add(unit);
            else _opponentUnits.Add(unit);
            
            unit.Init(playerId, startState, () => OnUnitDeath(unit));
            
            if (startState == BaseUnit.UnitState.Idle)
                _idleStateStartTimes[unit] = Time.time;
            
            return unit;
        }

        public void FindNewTargetFor(BaseUnit unit)
        {
            var isPlayerUnit = unit.PlayerId == Keys.PLAYER_ID;
            var enemyUnits = isPlayerUnit ? _opponentUnits : _playerUnits;

            BaseUnit closestEnemy = null;
            var closestDistance = float.MaxValue;

            foreach (var enemy in enemyUnits)
            {
                if (!enemy || !unit)
                    continue;

                if (!enemy.IsDefender && enemy.UnitType != BaseUnit.UnitTypes.King)
                    continue;

                var distance = (enemy.transform.position - unit.transform.position).sqrMagnitude;
                if (!(distance < closestDistance))
                    continue;
                
                closestDistance = distance;
                closestEnemy = enemy;
            }

            if (closestEnemy)
            {
                closestEnemy.TargetInfo.AddTargeter(unit);
                
                var newState = unit.UnitCurrentState == BaseUnit.UnitState.Climbing
                    ? BaseUnit.UnitState.Climbing
                    : BaseUnit.UnitState.Moving;
                
                unit.SetUnitTarget(closestEnemy, newState);
            }
            else
            {
                Debug.LogError("Couldnt find closest enemy??");
            }
        }

        private void UnRegisterUnit(BaseUnit unit)
        {
            _idleStateStartTimes.Remove(unit);
            unit.CleanUp();
            
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

            if (unit) Destroy(unit.gameObject);
        }

        public void Cleanup()
        {
            foreach (var unit in _playerUnits.Where(unit => unit))
            {
                unit.CleanUp();
                Destroy(unit.gameObject);
            }

            foreach (var unit in _opponentUnits.Where(unit => unit))
            {
                unit.CleanUp();
                Destroy(unit.gameObject);
            }

            _idleStateStartTimes.Clear();
            _playerUnits.Clear();
            _opponentUnits.Clear();
        }

        private void OnUnitDeath(BaseUnit unit)
        {
            UnRegisterUnit(unit);

            foreach (var attacker in unit.TargetInfo.TargetedBy)
            {
                if (attacker.IsDefender)
                {
                    // keep defending
                    attacker.ChangeUnitStateTo(BaseUnit.UnitState.Defending);
                    continue;
                }

                var canAttackAgain = attacker.UnitCurrentState != BaseUnit.UnitState.Dead;

                if (canAttackAgain)
                    FindNewTargetFor(attacker);
            }

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