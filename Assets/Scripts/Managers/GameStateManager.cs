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

        public GameState CurrentState { get; private set; }

        public void StartGame()
        {
            CurrentState = GameState.InGame;
        }

        public void FinishGame()
        {
            CurrentState = GameState.GameEnded;
        }
        
        public async Task Init(object[] args)
        {
            CurrentState = GameState.NotStarted;
        }

        public void Cleanup()
        {
            CurrentState = GameState.NotStarted;
        }
    }
}