using Infrastructure.StateMachine.Game.States;

namespace Infrastructure.StateMachine
{
    public interface IStateFactory
    {
        T GetState<T>() where T : class, IExitable;
    }
}