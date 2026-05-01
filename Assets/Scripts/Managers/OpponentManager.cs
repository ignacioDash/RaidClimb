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
        [SerializeField] private BotDifficultyConfig botDifficultyConfig;

        private DataManager _dataManager;
        private UnitManager _unitManager;
        private CurrencyManager _currencyManager;
        private CancellationTokenSource _cts;

        private bool _isPlaying;
        private BotDifficultyTier _currentTier;
        private float _matchStartTime;

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
            _currencyManager = GameManager.Instance.GetManager<CurrencyManager>();

            var arena = _currencyManager.GetArenaForTrophies(_dataManager.PlayerData.UserData.trophies);
            _currentTier = botDifficultyConfig.GetTierForArena(arena);

            _matchStartTime = Time.time;
            _isPlaying = true;
            _ = PlayingLoop();
        }

        public void OnGameEnded()
        {
            _cts?.Cancel();
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

                await Task.Delay(GetCurrentSpawnInterval(), _cts.Token);
            }
        }
        
        private async Task SetDelayedTarget(BaseUnit unit)
        {
            await Task.Delay(1500, _cts.Token);
            
            if (!_cts.IsCancellationRequested)
                _unitManager.FindNewTargetFor(unit);
        }

        private int GetCurrentSpawnInterval()
        {
            var min = _currentTier.minSpawnIntervalMs;
            var max = _currentTier.maxSpawnIntervalMs;

            if (_currentTier.rampIntervalSeconds > 0 && _currentTier.intervalReductionPerRampMs > 0)
            {
                var elapsed = Time.time - _matchStartTime;
                var steps = Mathf.FloorToInt(elapsed / _currentTier.rampIntervalSeconds);
                var reduction = steps * _currentTier.intervalReductionPerRampMs;
                var floor = _currentTier.spawnIntervalFloorMs;
                min = Mathf.Max(min - reduction, floor);
                max = Mathf.Max(max - reduction, floor);
            }

            return Random.Range(min, max);
        }

        private int GetDelayForUnit(BaseUnit.UnitTypes unitType)
        {
            return unitType switch
            {
                BaseUnit.UnitTypes.Melee => 2000,
                BaseUnit.UnitTypes.Raider => 2000,
                BaseUnit.UnitTypes.Berserk => 2000,
                BaseUnit.UnitTypes.Ranged => 4000,
                BaseUnit.UnitTypes.Hunter => 4000,
                BaseUnit.UnitTypes.Deadeye => 4000,
                BaseUnit.UnitTypes.Tank => 6000,
                BaseUnit.UnitTypes.Ogre => 6000,
                BaseUnit.UnitTypes.Golem => 6000,
                _ => 2000
            };
        }

        private BaseUnit.UnitTypes GetRandomUnit()
        {
            return _currentTier.availableUnits[Random.Range(0, _currentTier.availableUnits.Count)];
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            var box = spawnArea as BoxCollider;
            if (box != null)
            {
                var localPoint = new Vector3(
                    (Random.value - 0.5f) * box.size.x + box.center.x,
                    box.center.y,
                    (Random.value - 0.5f) * box.size.z + box.center.z
                );
                var worldPoint = box.transform.TransformPoint(localPoint);
                return new Vector3(worldPoint.x, Values.UNIT_SPAWN_Y, worldPoint.z);
            }

            var bounds = spawnArea.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Values.UNIT_SPAWN_Y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
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