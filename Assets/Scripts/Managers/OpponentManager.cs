using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Config;
using Constants;
using Units.UnitTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class OpponentManager : MonoBehaviour, IManager
    {
        [SerializeField] private Collider spawnArea;
        
        [Header("Settings")]
        [SerializeField] private UnitReferences unitReferences;
        
        private DataManager _dataManager;
        private UnitManager _unitManager;
        private CancellationTokenSource _cts;
        
        private bool _isPlaying;
        private int _gameplayLevel; // todo: use it to define the opponent behaviour/difficulty
        
        public async Task Init(object[] args)
        {
            _dataManager = GameManager.Instance.GetManager<DataManager>();

            while (_dataManager is not { Initialized: true })
            {
                await Task.Yield();
            }
            
            _isPlaying = false;
        }

        public void OnGameStarted()
        {
            _unitManager = GameManager.Instance.GetManager<UnitManager>();
            _gameplayLevel = _dataManager.PlayerData.UserData.UserLevel;

            _isPlaying = true;
            //_ = PlayingLoop();
        }

        public void OnGameEnded()
        {
            _cts.Cancel();
            _isPlaying = false;
        }

        private async Task PlayingLoop()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            await Task.Delay(3000, _cts.Token); // initial delay
            
            while (_isPlaying && !_cts.IsCancellationRequested)
            {
                var unitToSpawn = GetRandomUnit();

                var unitPosition = GetRandomSpawnPosition();

                var unitSpawned = _unitManager.SpawnUnit(unitToSpawn, unitPosition, Keys.OPPONENT_ID);
                
                if (unitSpawned)
                {
                    unitSpawned.transform.position = unitPosition;
                    _ = SetDelayedTarget(unitSpawned);
                }

                var delay = GetDelayForUnit(unitToSpawn);
                await Task.Delay(delay, _cts.Token);

                var randomDelay = Random.Range(2000, 5000); // todo based on difficulty
                await Task.Delay(randomDelay, _cts.Token);
            }
        }
        
        private async Task SetDelayedTarget(BaseUnit unit)
        {
            await Task.Delay(1500, _cts.Token);
            
            if (!_cts.IsCancellationRequested)
                _unitManager.FindNewTargetFor(unit);
        }

        private int GetDelayForUnit(BaseUnit.UnitTypes unitType)
        {
            return unitType switch
            {
                BaseUnit.UnitTypes.Melee => 2000,
                _ => 2000
            };
        }

        private BaseUnit.UnitTypes GetRandomUnit()
        {
            // todo:
            var opponentUnits = new List<BaseUnit.UnitTypes> { BaseUnit.UnitTypes.Melee };

            var randomUnit = opponentUnits[Random.Range(0, opponentUnits.Count - 1)];
            return randomUnit;
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            var bounds = spawnArea.bounds;

            var x = Random.Range(bounds.min.x, bounds.max.x);
            var z = Random.Range(bounds.min.z, bounds.max.z);

            return new Vector3(x, Values.UNIT_SPAWN_Y, z);
        }

        public void Cleanup()
        {
            _isPlaying = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void OnDisable()
        {
            Cleanup();
        }
    }
}