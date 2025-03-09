using StaticData;
using UnityEngine;
using Logic.Zuma.Balls;
using Logic.Zuma.Level;
using Logic.Zuma.Players;

namespace Services.Factories.Game
{
    public interface IGameFactory
    {
        Player Player { get; }
        
        Ball CreateBall(Vector3 position, Quaternion rotation);
        Player CreatePlayer(Vector3 position, Quaternion rotation);
        public LevelHolder CreateLevelHolder(LevelStaticData levelStaticData);
    }
}