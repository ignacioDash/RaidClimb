using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;

namespace Managers
{
    public class UIManager : MonoBehaviour, IManager
    {
        public enum Screens
        {
            None,
            MainScreen,
            InGameScreen,
            SettingsScreen,
            TowerScreen,
            GameEndScreen,
            SquadScreen
        }
        
        [SerializeField] private MainScreenUI mainScreen;
        [SerializeField] private InGameScreen inGameScreen;
        [SerializeField] private SettingsScreen settingsScreen;
        [SerializeField] private TowerScreen towerScreen;
        [SerializeField] private GameEndScreen gameEndScreen;
        [SerializeField] private SquadScreenUI squadScreen;

        private readonly Dictionary<Screens, BaseScreen> _screensDictionary = new();

        private Screens _currentScreen;

        public async Task Init(object[] args)
        {
            _currentScreen = Screens.None;
            
            mainScreen.gameObject.SetActive(false);
            inGameScreen.gameObject.SetActive(false);
            settingsScreen.gameObject.SetActive(false);
            towerScreen.gameObject.SetActive(false);
            gameEndScreen.gameObject.SetActive(false);
            
            _screensDictionary.Add(Screens.MainScreen, mainScreen);
            _screensDictionary.Add(Screens.InGameScreen, inGameScreen);
            _screensDictionary.Add(Screens.SettingsScreen, settingsScreen);
            _screensDictionary.Add(Screens.TowerScreen, towerScreen);
            _screensDictionary.Add(Screens.GameEndScreen, gameEndScreen);
            _screensDictionary.Add(Screens.SquadScreen, squadScreen);
        }

        public async Task NavigateTo(Screens targetScreen, bool asPopUp = false, object[] args = null)
        {
            var transitions = new List<Task>();
            Action onCompleted = null;
            
            if (!asPopUp)
            {
                var hasCurrent = _screensDictionary.TryGetValue(_currentScreen, out var currentScreen);
                if (hasCurrent)
                {
                    var fadeOut = currentScreen.CloseScreen();
                    transitions.Add(fadeOut);
                    onCompleted += () => currentScreen.gameObject.SetActive(false);
                }
            }

            var hasNext = _screensDictionary.TryGetValue(targetScreen, out var nextScreen);
            if (hasNext)
            {
                nextScreen.gameObject.SetActive(true);
                var fadeIn = nextScreen.OpenScreen(args);
                transitions.Add(fadeIn);

                _currentScreen = targetScreen;
            }

            await Task.WhenAll(transitions);
            
            onCompleted?.Invoke();
        }

        public void Cleanup()
        {
            
        }
    }
}