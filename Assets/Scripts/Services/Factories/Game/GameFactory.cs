using Zenject;

namespace Services.Factories.Game
{
    public class GameFactory : Factory, IGameFactory
    {
        public GameFactory(IInstantiator instantiator) : base(instantiator)
        {
            
        }

    }
}