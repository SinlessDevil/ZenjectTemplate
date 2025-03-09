using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Logic;
using Logic.Zuma.Balls;
using PathCreation;

namespace Services.BallControllers
{
    public interface IBallChainController
    {
        public List<Ball> Balls { get; }
        public List<Item> ActiveItems { get; }
        void Initialize(PathCreator pathCreator, BallChainDTO ballChainDTO);
        public void Update();
        public void StartBallSpawning();
        public void StopBallSpawning();
        public void TryAttachBall(Ball newBall);
        public UniTask MoveParticleAlongPathAsync(ParticleSystemHolder particle);
    }
}