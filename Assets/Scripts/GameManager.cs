using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Constants;
using Managers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UnitManager unitManager;

    private GameStateManager _gameStateManager;

    private readonly Dictionary<Type, IManager> _managers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(Instance);

        Instance = this;
        
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        try
        {
            _ = SetUp();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public T GetManager<T>()
    {
        return _managers.ContainsKey(typeof(T)) ? (T)_managers[typeof(T)] : default;
    }

    // user presses play button
    public async Task StartGame()
    {
        await uiManager.NavigateTo(UIManager.Screens.InGameScreen);
        
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

        var managersInit = new List<Task>
        {
            uiManager.Init(null),
            unitManager.Init(null),
            _gameStateManager.Init(null)
        };

        await Task.WhenAll(managersInit);
        
        _managers.Add(typeof(UIManager), uiManager);
        _managers.Add(typeof(UnitManager), unitManager);
        _managers.Add(typeof(GameStateManager), _gameStateManager);

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
