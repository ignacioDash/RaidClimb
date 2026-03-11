using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castles;
using Managers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private CastleManager playerCastle, opponentCastle;

    private GameStateManager _gameStateManager;
    private DataManager _dataManager;

    private readonly Dictionary<Type, IManager> _managers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(Instance);

        Instance = this;
        
        Application.targetFrameRate = 60;
    }

    private async void Start()
    {
        await SetUp();
    }

    public T GetManager<T>()
    {
        return _managers.ContainsKey(typeof(T)) ? (T)_managers[typeof(T)] : default;
    }

    // user presses play button
    public async Task StartGame()
    {
        await uiManager.NavigateTo(UIManager.Screens.InGameScreen);

        ((OpponentCastleManager)opponentCastle).SetUpOpponent(CastleDataByLevel.GetCastleDataForLevel(1));
        
        _gameStateManager.StartGame();
    }

    // user leaves / finishes the game
    public async Task FinishGame()
    {
        await uiManager.NavigateTo(UIManager.Screens.MainScreen);
        
        CleanUp();
    }

    private async Task SetUp()
    {
        _gameStateManager = new GameStateManager();
        _dataManager = new DataManager();
        
        _managers.Add(typeof(DataManager), _dataManager);
        _managers.Add(typeof(UIManager), uiManager);
        _managers.Add(typeof(UnitManager), unitManager);
        _managers.Add(typeof(GameStateManager), _gameStateManager);
        _managers.Add(typeof(PlayerCastleManager), playerCastle);
        _managers.Add(typeof(OpponentCastleManager), opponentCastle);
        
        await _dataManager.Init(null);

        var managersInit = new List<Task>
        {
            uiManager.Init(null),
            unitManager.Init(null),
            _gameStateManager.Init(null),
            playerCastle.Init(null),
            opponentCastle.Init(null)
        };

        await Task.WhenAll(managersInit);
        await uiManager.NavigateTo(UIManager.Screens.MainScreen);
    }

    private void CleanUp()
    {
        foreach (var (_, manager) in _managers)
        {
            manager.Cleanup();
        }
    }
}
