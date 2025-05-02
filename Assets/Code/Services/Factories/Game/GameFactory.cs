using Zenject;

namespace Code.Services.Factories.Game
{
    public class GameFactory : Factory, IGameFactory
    {
        public GameFactory(IInstantiator instantiator) : base(instantiator)
        {
            
        }

    }
}