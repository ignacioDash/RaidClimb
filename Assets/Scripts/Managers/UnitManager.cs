using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Config;
using Constants;
using Data;
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

        public readonly List<BaseUnit> PlayerUnits = new();
        public readonly List<BaseUnit> OpponentUnits = new();
        
        public async Task Init(object[] args)
        {
            
        }

        public void OnGameEnded(string winnerId)
        {
            var playerWon = winnerId == Keys.PLAYER_ID;
            
            foreach (var unit in PlayerUnits)
            {
                unit.ChangeUnitStateTo(playerWon ? BaseUnit.UnitState.Won : BaseUnit.UnitState.Lost);
            }

            foreach (var unit in OpponentUnits)
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
            
            if (isPlayerUnit) PlayerUnits.Add(unit);
            else OpponentUnits.Add(unit);
            
            unit.Init(playerId, startState, () => OnUnitDeath(unit));
            
            return unit;
        }

        public void FindNewTargetFor(BaseUnit unit)
        {
            var isPlayerUnit = unit.PlayerId == Keys.PLAYER_ID;
            var enemyUnits = isPlayerUnit ? OpponentUnits : PlayerUnits;

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

        public List<BaseUnit.UnitTypes> GetUnitsUnlockingAtArena(int arena) =>
            unitReferences.GetUnitsUnlockingAtArena(arena);

        public string GetUnitDisplayName(BaseUnit.UnitTypes unitType) =>
            unitReferences.GetDisplayName(unitType);

        public int GetUnitSquadCost(BaseUnit.UnitTypes unitType) =>
            unitReferences.GetSquadCost(unitType);

        public float GetPlayerKingDistance()
        {
            return GetNormalizedDistanceToOpponentKing(PlayerUnits, OpponentUnits);
        }

        public float GetOpponentKingDistance()
        {
            return GetNormalizedDistanceToOpponentKing(OpponentUnits, PlayerUnits);
        }

        private static float GetNormalizedDistanceToOpponentKing(List<BaseUnit> sourceUnits, List<BaseUnit> targetUnits)
        {
            var targetKing = targetUnits.FirstOrDefault(u => u && u.UnitType == BaseUnit.UnitTypes.King);
            if (!targetKing)
                return 1f;

            var closestDistance = float.MaxValue;

            foreach (var unit in sourceUnits)
            {
                if (!unit)
                    continue;

                var distance = Vector3.Distance(unit.transform.position, targetKing.transform.position);
                if (distance < closestDistance)
                    closestDistance = distance;
            }

            if (Math.Abs(closestDistance - float.MaxValue) < 0.01f)
                return 1f;
            
            return Mathf.Clamp01(closestDistance / 50f);
        }

        private void UnRegisterUnit(BaseUnit unit)
        {
            unit.CleanUp();
            
            if (unit.PlayerId == Keys.PLAYER_ID)
            {
                if (PlayerUnits.Contains(unit))
                    PlayerUnits.Remove(unit);
            }
            else
            {
                if (OpponentUnits.Contains(unit))
                    OpponentUnits.Remove(unit);
            }

            if (unit) Destroy(unit.gameObject);
        }

        public void Cleanup()
        {
            foreach (var unit in PlayerUnits.Where(unit => unit))
            {
                unit.CleanUp();
                Destroy(unit.gameObject);
            }

            foreach (var unit in OpponentUnits.Where(unit => unit))
            {
                unit.CleanUp();
                Destroy(unit.gameObject);
            }

            PlayerUnits.Clear();
            OpponentUnits.Clear();
        }

        private void OnUnitDeath(BaseUnit unit)
        {
            UnRegisterUnit(unit);

            foreach (var attacker in unit.TargetInfo.TargetedBy)
            {
                if (!attacker)continue;
                
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
        [SerializeField] private UnitBaseConfig unitConfig;

        public BaseUnit.UnitTypes UnitType => unitType;
        public BaseUnit UnitPrefab => unitPrefab;
        public UnitBaseConfig Config => unitConfig;
    }
}