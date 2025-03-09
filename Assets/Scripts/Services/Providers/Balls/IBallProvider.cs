using UnityEngine;
using Logic.Zuma.Balls;

namespace Services.Provides.Balls
{
    public interface IBallProvider
    {
        public void CreatePoolBall();
        public void CleanupPool();
        public Ball GetBall(Vector3 position, Quaternion rotation);
        public void ReturnBall(Ball ball);
    }   
}