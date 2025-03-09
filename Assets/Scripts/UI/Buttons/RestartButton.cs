using Infrastructure.StateMachine.Game.States;
using Infrastructure.StateMachine.MonoBehaviours;
using UnityEngine.SceneManagement;

namespace UI.Buttons
{
    public class RestartButton : LoadStateButton<IGameState>
    {
        protected override void LoadState(StateLoader<IGameState> stateMachine)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            stateMachine.LoadState<LoadLevelState, string>(currentSceneName);
        }
    }
}