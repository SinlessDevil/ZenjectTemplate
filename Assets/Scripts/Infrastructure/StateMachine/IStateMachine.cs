using System;
using Infrastructure.StateMachine.Game.States;
using Zenject;

namespace Infrastructure.StateMachine
{
    public interface IStateMachine<TBaseState>
    {
        Type ActiveStateType { get; }
        TState Enter<TState>() where TState : class, TBaseState, IState;
        TState Enter<TState, TPayload>(TPayload payload) where TState : class, TBaseState, IPayloadedState<TPayload>;
        bool Back();
    }
}