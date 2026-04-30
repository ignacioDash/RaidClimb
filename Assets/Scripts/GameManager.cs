using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Units.UnitTypes;
using Castles;
using Constants;
using Input;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private TrapsManager trapsManager;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private OpponentManager opponentManager;
    [SerializeField] private CurrencyManager currencyManager;
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
        
        EnhancedTouchSupport.Enable();
    }

    private async void Start()
    {
        await cameraManager.SetCameraAt(CameraManager.CameraPosition.Default);
        
        await SetUp();
    }

    public T GetManager<T>()
    {
        return _managers.ContainsKey(typeof(T)) ? (T)_managers[typeof(T)] : default;
    }

    // user presses play button
    public async Task StartGame()
    {
        var cameraAnimation = cameraManager.SetCameraAt(CameraManager.CameraPosition.Opponent);
        var screenChange = uiManager.NavigateTo(UIManager.Screens.InGameScreen);

        await Task.WhenAll(cameraAnimation, screenChange);
        
        unitManager.Cleanup();
        trapsManager.CleanupPlayerTraps();

        playerCastle.OnGameStarted();

        var trophies = _dataManager.PlayerData.UserData.trophies;
        var opponentLevel = Mathf.Clamp(trophies / 50, 1, 20);
        ((OpponentCastleManager)opponentCastle).SetUpOpponent(CastleDataByLevel.GetCastleDataForLevel(opponentLevel, trophies));
        currencyManager.SetContainerActive(false);
        
        _gameStateManager.StartGame();
        
        opponentManager.OnGameStarted();
    }

    // game end
    public async Task GameEnded(string winnerId)
    {
        var playerWon = winnerId == Keys.PLAYER_ID;

        int coinsEarned, trophiesEarned;
        List<BaseUnit.UnitTypes> newlyUnlocked;
        if (playerWon)
        {
            var rewards = currencyManager.AwardWinRewards();
            coinsEarned = rewards.coins;
            trophiesEarned = rewards.trophies;
            newlyUnlocked = rewards.newlyUnlocked;
        }
        else
        {
            coinsEarned = 0;
            trophiesEarned = currencyManager.HandleDefeat();
            newlyUnlocked = new List<BaseUnit.UnitTypes>();
        }

        await _dataManager.Save();

        var cameraAnimation =
            cameraManager.SetCameraAt(playerWon ? CameraManager.CameraPosition.Win : CameraManager.CameraPosition.Lose);

        opponentManager.OnGameEnded();

        await Task.WhenAll(cameraAnimation);

        unitManager.OnGameEnded(winnerId);

        currencyManager.SetContainerActive(true);
        await uiManager.NavigateTo(UIManager.Screens.GameEndScreen, args: new object[] { playerWon, coinsEarned, trophiesEarned, newlyUnlocked });
    }

    // user leaves / finishes the game
    public async Task FinishGame()
    {
        currencyManager.SetContainerActive(true);
        CleanUp();

        var cameraAnimation = cameraManager.SetCameraAt(CameraManager.CameraPosition.Default);
        var screenChange = uiManager.NavigateTo(UIManager.Screens.MainScreen);

        await Task.WhenAll(cameraAnimation, screenChange);
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
        _managers.Add(typeof(TrapsManager), trapsManager);
        _managers.Add(typeof(CameraManager), cameraManager);
        _managers.Add(typeof(OpponentManager), opponentManager);
        _managers.Add(typeof(CurrencyManager), currencyManager);
        
        await _dataManager.Init(null);

        var managersInit = new List<Task>
        {
            uiManager.Init(null),
            unitManager.Init(null),
            _gameStateManager.Init(null),
            playerCastle.Init(null),
            opponentCastle.Init(null),
            trapsManager.Init(null),
            cameraManager.Init(null),
            opponentManager.Init(null),
            currencyManager.Init(null),
        };

        await Task.WhenAll(managersInit);

        if (_dataManager.PlayerData.UserData.gamesPlayed == 0)
            await StartGame();
        else
            await uiManager.NavigateTo(UIManager.Screens.MainScreen);
    }

    private void CleanUp()
    {
        foreach (var (_, manager) in _managers)
        {
            manager.Cleanup();
        }
    }
    
    private void OnDestroy()
    {
        CleanUp();
    }
}
