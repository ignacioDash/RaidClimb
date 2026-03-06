using System.Threading.Tasks;

namespace Managers
{
    public class GameStateManager: IManager
    {
        public enum GameState
        {
            NotStarted,
            InGame,
            GameEnded,
        }

        private GameState _currentState;

        public void StartGame()
        {
            _currentState = GameState.InGame;
        }
        
        public async Task Init(object[] args)
        {
            _currentState = GameState.NotStarted;
        }

        public void Cleanup()
        {
            _currentState = GameState.NotStarted;
        }
    }
}