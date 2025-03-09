using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Services.BallControllers;
using Services.Provides.Balls;
using UnityEngine;
using Logic.Zuma.Balls;
using Zenject;

namespace Logic.Zuma.Players
{
    public class BallIntersectionTracker : MonoBehaviour
    {
        private float _thresholdToPoint = 0.35f;
        private float _bufferOutOfScreen = 0.25f;
        
        private Ball _ball;
        private Coroutine _intersectionTrackerCoroutine;
        
        private List<Vector3> _intersectionPoints = new();
        
        private IBallProvider _ballProvider;
        private IBallChainController _ballChainController;

        [Inject]
        private void Constructor(
            IBallProvider ballProvider, 
            IBallChainController ballChainController)
        {
            _ballProvider = ballProvider;
            _ballChainController = ballChainController;
        }

        public void Initialize(Ball ball)
        {
            _ball = ball;
        }

        public void StartTracker(List<Vector3> intersectionPoints)
        {
            _intersectionTrackerCoroutine ??= StartCoroutine(IntersectionTrackerRoutine(intersectionPoints));
        }

        public void StopTracker()
        {
            if (_intersectionTrackerCoroutine != null)
            {
                StopCoroutine(_intersectionTrackerCoroutine);
                _intersectionTrackerCoroutine = null;
            }
        }

        private IEnumerator IntersectionTrackerRoutine(List<Vector3> intersectionPoints)
        {
            intersectionPoints = intersectionPoints.OrderBy(point => Vector3.Distance(transform.position, point))
                .ToList();
            
            if (intersectionPoints.Count == 0)
            {
                Debug.LogError("No intersection points found.");

                while (true)
                {
                    if (IsOutOfScreen())
                    {
                        _ballProvider.ReturnBall(_ball);
                        yield break;
                    }

                    yield return null;
                }
            }

            int currentIndex = 0;

            while (true)
            {
                if (currentIndex < intersectionPoints.Count)
                {
                    Vector3 targetPoint = intersectionPoints[currentIndex];
                    float distanceToPoint = Vector3.Distance(transform.position, targetPoint);

                    if (distanceToPoint <= _thresholdToPoint)
                    {
                        Debug.Log($"Point {currentIndex} reached, moving to the next one.");
                        currentIndex++;
                        _ballChainController.TryAttachBall(_ball);
                    }
                }

                if (IsOutOfScreen())
                {
                    _ball.SetInteractive(false);
                    _ballProvider.ReturnBall(_ball);
                    yield break;
                }

                yield return null;
            }
        }
        
        private bool IsOutOfScreen()
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            return screenPoint.x < -_bufferOutOfScreen || screenPoint.x > 1 + _bufferOutOfScreen ||
                   screenPoint.y < -_bufferOutOfScreen || screenPoint.y > 1 + _bufferOutOfScreen;
        }
    }
}