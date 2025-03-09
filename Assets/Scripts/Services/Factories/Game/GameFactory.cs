using Services.Factories.Paths;
using UnityEngine;
using Logic.Zuma.Balls;
using Logic.Zuma.Level;
using Logic.Zuma.Players;
using StaticData;
using Zenject;

namespace Services.Factories.Game
{
    public class GameFactory : Factory, IGameFactory
    {
        public GameFactory(IInstantiator instantiator) : base(instantiator)
        {
            
        }
        public Player Player { get; private set; }
        
        public Ball CreateBall(Vector3 position, Quaternion rotation)
        {
            var ball = Instantiate(ResourcePath.BallPath, position, rotation, null).GetComponent<Ball>();
            return ball;
        }

        public Player CreatePlayer(Vector3 position, Quaternion rotation)
        {
            var player = Instantiate(ResourcePath.PlayerPath, position, rotation, null).GetComponent<Player>();
            Player = player;
            return player;
        }
        
        public LevelHolder CreateLevelHolder(LevelStaticData levelStaticData)
        {
            GameObject levelHolder = Instantiate(levelStaticData.LevelHolder);
            return levelHolder.GetComponent<LevelHolder>();
        }
    }
}