using Infrastructure.StateMachine.Game.States;
using UnityEngine;
using Zenject;

namespace Infrastructure.StateMachine.Game
{
    public class GameStateMachine : StateMachine<IGameState>, ITickable
    {
        public GameStateMachine(GameStateFactory gameStateFactory) : base(gameStateFactory)
        {
        }

        public void Tick()
        {
            Update();
        }
    }
}