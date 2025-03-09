using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Services.Provides.Balls;
using Services.Levels;
using Services.Provides.Widgets;
using Services.Random;
using Services.Finish;
using Services.LocalProgress;
using Services.Input;
using Services.Factories.Game;
using Services.Timer;
using UnityEngine;
using Logic;
using Logic.Zuma.Balls;
using PathCreation;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Services.BallControllers
{
    public class BallChainController : IBallChainController
    {
        private bool _isBoosting = true;
        
        private float _distanceTravelled = 0f;

        private bool _isWin = false;
        private bool _isLose = false;
        
        private List<Color> _colorItems = new();
        private int _countItems = 0;

        private CancellationTokenSource _startBallSpawning;
        private List<Ball> _balls = new();
        private PathCreator _pathCreator;
        private BallChainDTO _ballChainDto;
        
        private readonly IBallProvider _ballProvider;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IRandomService _randomService;
        private readonly ILevelService _levelService;
        private readonly ILevelLocalProgressService _levelLocalProgressService;
        private readonly IFinishService _finishService;
        private readonly IInputService _inputService;
        private readonly IGameFactory _gameFactory;
        private readonly ITimeService _timeService;

        public BallChainController(
            IBallProvider ballProvider,
            IWidgetProvider widgetProvider,
            IRandomService randomService,
            ILevelService levelService,
            ILevelLocalProgressService levelLocalProgressService,
            IFinishService finishService,
            IInputService inputService,
            IGameFactory gameFactory,
            ITimeService timeService)
        {
            _ballProvider = ballProvider;
            _widgetProvider = widgetProvider;
            _randomService = randomService;
            _levelService = levelService;
            _levelLocalProgressService = levelLocalProgressService;
            _finishService = finishService;
            _inputService = inputService;
            _gameFactory = gameFactory;
            _timeService = timeService;
        }

        public List<Ball> Balls => _balls;

        public List<Item> ActiveItems => _balls.Cast<Item>().ToList();

        public void Initialize(PathCreator pathCreator, BallChainDTO ballChainDto)
        {
            _pathCreator = pathCreator;
            _ballChainDto = ballChainDto;
        }
        
        public void Update()
        {
            MoveBalls();
        }
        
        public void StartBallSpawning()
        {
            if (_pathCreator == null)
            {
                Debug.LogError("Path Creator is null!");
                return;
            }
            
            _startBallSpawning?.Cancel();
            _startBallSpawning = new CancellationTokenSource();

            _colorItems = _randomService.GetColorsByLevelRandomConfig();
            _countItems = _levelService.GetCurrentLevelStaticData().LevelConfig.CountItem;

            BoostSpeedAsync(_startBallSpawning.Token).Forget();
            SpawnInitialBallsAsync(_startBallSpawning.Token).Forget();
            
            _timeService.StartTimer();
        }
        
        public void StopBallSpawning()
        {
            _startBallSpawning?.Cancel();
            _pathCreator = null;
            _balls.Clear();
            _colorItems.Clear();
            _countItems = 0;
            _distanceTravelled = 0f;
            _isWin = false;
            _isLose = false;
        }
        
        public void TryAttachBall(Ball newBall)
        {
            List<(Ball ball, float distance, int index)> ballsCollision = new();

            for (int i = 0; i < _balls.Count; i++)
            {
                Ball existingBall = _balls[i];
                float distance = Vector3.Distance(existingBall.transform.position, newBall.transform.position);

                if (distance <= _ballChainDto.CollisionThreshold)
                {
                    ballsCollision.Add((existingBall, distance, i));
                }
            }

            if (ballsCollision.Count == 0)
            {
                return;
            }

            ballsCollision = ballsCollision.OrderBy(b => b.distance).ToList();

            Ball closestBall = ballsCollision.First().ball;
            int closestIndex = ballsCollision.First().index;

            if (closestIndex == 0 || _balls[closestIndex - 1].transform.position.z > closestBall.transform.position.z)
            {
                AttachBallToChain(newBall, closestIndex);
            }
            else
            {
                AttachBallToChain(newBall, closestIndex + 1);
            }
        }

        public async UniTask MoveParticleAlongPathAsync(ParticleSystemHolder particle)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
            
            float distance = _distanceTravelled;
            float pathLength = _pathCreator.path.length;

            if (pathLength <= 0)
            {
                particle.gameObject.SetActive(false);
                return;
            }

            float startProgress = distance / pathLength;
            float currentSpeed = Mathf.Lerp(_ballChainDto.MinParticleSpeed, _ballChainDto.MaxParticleSpeed, startProgress);
            
            float startTime = Time.time;
            float duration = pathLength / currentSpeed;

            while (distance < pathLength)
            {
                float progress = (Time.time - startTime) / duration;
                progress = Mathf.Clamp01(progress);
                distance = Mathf.Lerp(_distanceTravelled, pathLength, progress);

                if(distance == pathLength)
                    break;
                
                particle.transform.position = _pathCreator.path.GetPointAtDistance(distance);
                await UniTask.Yield();
            }

            particle.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InQuart);
            await UniTask.Delay(650);
            particle.gameObject.SetActive(false);
            particle.transform.localScale = Vector3.one;
            particle.Stop();
        }
        
        private async UniTaskVoid SpawnInitialBallsAsync(CancellationToken token)
        {
            for (int i = 0; i < _countItems; i++)
            {
                if (token.IsCancellationRequested)
                    return;

                var color = _colorItems.FirstOrDefault();

                float spawnDistance = i * _ballChainDto.SpacingBalls;
                Ball newBall = _ballProvider.GetBall(_pathCreator.path.GetPointAtDistance(spawnDistance),
                    Quaternion.identity);

                newBall.SetColor(color);

                _colorItems.Remove(color);

                AddBall(newBall);
                newBall.SetIndex(i);
                await UniTask.Delay((int)(_ballChainDto.DurationSpawnBall * 1000), cancellationToken: token);
            }
        }
        
        private async UniTaskVoid BoostSpeedAsync(CancellationToken token)
        {
            float elapsedTime = 0f;
            float startSpeed = _ballChainDto.InitialSpeedMultiplier;
            float endSpeed = _ballChainDto.MoveSpeed;

            _ballChainDto.MoveSpeed = startSpeed;
            
            while (elapsedTime < _ballChainDto.BoostDuration)
            {
                elapsedTime += Time.deltaTime / 2;
                _ballChainDto.MoveSpeed = Mathf.Lerp(startSpeed, endSpeed, elapsedTime);
                await UniTask.Yield(cancellationToken: token);
            }

            _isBoosting = false;
        }
        
        private void MoveBalls()
        {
            if (_balls.Count == 0)
                return;

            float currentSpeed = _isBoosting ? _ballChainDto.MoveSpeed * _ballChainDto.InitialSpeedMultiplier : _ballChainDto.MoveSpeed;
            _distanceTravelled += currentSpeed * Time.deltaTime;
            
            float targetDistance0 = _distanceTravelled;
            Vector3 currentPosition0 = _balls[0].transform.position;
            _balls[0].transform.DOMove(_pathCreator.path.GetPointAtDistance(targetDistance0), _ballChainDto.DurationMovingOffset);
            
            if (_pathCreator.path.GetClosestDistanceAlongPath(currentPosition0) >= _pathCreator.path.length - 0.5f)
            {
                _distanceTravelled -= _ballChainDto.SpacingBalls;
                _balls[0].Deactivate();
                _balls.Remove(_balls[0]);
                TryLose();
            }
            
            for (int i = 1; i < _balls.Count; i++)
            {
                float targetDistance = _distanceTravelled - (i * _ballChainDto.SpacingBalls);
                targetDistance = Mathf.Max(targetDistance, 0);
                
                Vector3 currentPosition = _balls[i].transform.position;
                _balls[i].transform.DOMove(_pathCreator.path.GetPointAtDistance(targetDistance), _ballChainDto.DurationMovingOffset);
                
                if (_pathCreator.path.GetClosestDistanceAlongPath(currentPosition) >= _pathCreator.path.length - 0.5f)
                {
                    _distanceTravelled -= _ballChainDto.SpacingBalls;
                    _balls[i].Deactivate();
                    _balls.Remove(_balls[i]);
                    i--;

                    TryLose();
                }
            }

            if (!_isLose)
                TryUpdateMouthProgress((float)_ballChainDto.PercentToDetectionLose / 100);
        }

        private void AddBall(Ball ball)
        {
            if (_balls.Count == 0)
            {
                ball.transform.position = _pathCreator.path.GetPointAtDistance(_distanceTravelled);
            }
            else
            {
                Vector3 lastBallPosition = _balls[^1].transform.position;
                ball.transform.position = lastBallPosition;
            }

            _balls.Add(ball);
        }
        
        private void AttachBallToChain(Ball newBall, int index)
        {
            newBall.Dispose();
            _balls.Insert(index, newBall);
            ReIndexBalls();
            newBall.transform.DOMove(_balls[index].transform.position, _ballChainDto.DurationMovingOffset).OnComplete(() =>
            {
                CheckAndDestroyMatches(newBall);
            });

            _distanceTravelled += _ballChainDto.SpacingBalls;

            for (int i = index; i < _balls.Count; i++)
            {
                if (_balls[i] == newBall)
                    continue;

                Vector3 targetPos = _pathCreator.path.GetPointAtDistance(_distanceTravelled - (i * _ballChainDto.SpacingBalls));
                _balls[i].transform.DOMove(targetPos, _ballChainDto.DurationMovingOffset);
            }
        }

        private void CheckAndDestroyMatches(Ball insertedBall)
        {
            List<Ball> matchingBalls = new List<Ball> { insertedBall };
            Color matchColor = insertedBall.Color;

            for (int i = insertedBall.Index - 1; i >= 0; i--)
            {
                if (_balls[i].Color == matchColor)
                {
                    matchingBalls.Add(_balls[i]);
                }
                else
                {
                    break;
                }
            }

            for (int i = insertedBall.Index + 1; i < _balls.Count; i++)
            {
                if (_balls[i].Color == matchColor)
                {
                    matchingBalls.Add(_balls[i]);
                }
                else
                {
                    break;
                }
            }

            if (matchingBalls.Count >= _ballChainDto.MatchingCount)
            {
                var count = matchingBalls.Count *
                            _levelService.GetCurrentLevelStaticData().LevelConfig.ScoreConfig.ScorePerItem;
                
                _levelLocalProgressService.AddScore(count);
                
                foreach (var ball in matchingBalls)
                {
                    ball.PlayDestroyAnimation(() =>
                    {
                        insertedBall.SetInteractive(false);
                        
                        _balls.Remove(ball);
                        ball.Deactivate();
                    });
                }

                var totalDistance = matchingBalls.Sum(ball => _pathCreator.path.GetClosestDistanceAlongPath(ball.transform.position));
                var averageDistance = totalDistance / matchingBalls.Count;
                var position = _pathCreator.path.GetPointAtDistance(averageDistance);
                var color = insertedBall.Color;
                var countText = count.ToString();
                SetUpWidget(position, color, countText);
                
                TryWin(matchingBalls);
                
                ReIndexBalls();
            }
            else
            {
                insertedBall.SetInteractive(false);
            }
        }

        private void TryUpdateMouthProgress(float percentage)
        {
            float pathLength = _levelService.GetLevelHolder().PathCreator.path.length;
            float thresholdDistance = pathLength * percentage;
    
            float distanceTravelled = _levelService.GetLevelHolder().PathCreator.path.GetClosestDistanceAlongPath(Balls.First().transform.position);
            float remainingDistance = pathLength - distanceTravelled;

            if (remainingDistance <= thresholdDistance) 
            {
                _levelService.GetLevelHolder().LevelEnd.UpdateMouthProgress(distanceTravelled, pathLength, thresholdDistance);

                if (_levelService.GetLevelHolder().ParticleSystemHolder.IsActive == false)
                {
                    MoveParticleAlongPathAsync(_levelService.GetLevelHolder().ParticleSystemHolder).Forget();   
                }
            }
        }
        
        #region Win
        
        private void TryWin(List<Ball> matchingBalls)
        {
            if (_balls.Count == matchingBalls.Count && !_isWin)
            {
                _isWin = true;
                TriggerWinConditionAsync().Forget();
            }
        }
        
        private async UniTask TriggerWinConditionAsync()
        {
            _timeService.StopTimer();
            _inputService.Cleanup();
            
            MoveParticleAlongPathAsync(_levelService.GetLevelHolder().ParticleSystemHolder).Forget();
            
            float step = _ballChainDto.SetToSpawnWidget;
            float currentDistance = _distanceTravelled;
            float pathEnd = _pathCreator.path.length;
            Color widgetColor = _ballChainDto.BaseColorWidget;
            string widgetText =  "+" + _levelService.GetCurrentLevelStaticData().LevelConfig.ScoreConfig.ScorePerStepPath;
            int index = 1;
            
            while (currentDistance < pathEnd)
            {
                Vector3 position = _pathCreator.path.GetPointAtDistance(currentDistance);
                SetUpWidget(position, widgetColor, widgetText);
                
                index++;
                currentDistance += step;
                
                _levelLocalProgressService.AddScore(_levelService.GetCurrentLevelStaticData().LevelConfig.ScoreConfig.ScorePerStepPath);
                
                await UniTask.Delay(_ballChainDto.TimeToSpawnWidget);
            }
            
            await UniTask.Delay(1000);
            
            _finishService.Win();
        }
        
        #endregion

        #region Defeat
        
        private void TryLose()
        {
            if (_isLose) 
                return;

            _isLose = true;
            
            TriggerDefeatConditionAsync().Forget();
            
            float thresholdDistance = _pathCreator.path.length * 0.2f;
            float distanceTravelled = _pathCreator.path.GetClosestDistanceAlongPath(_balls.Last().transform.position);
            float remainingDistance = _pathCreator.path.length - distanceTravelled;
            float normalizedValue = Mathf.Clamp01(1 - (remainingDistance / thresholdDistance));
            _levelService.GetLevelHolder().LevelEnd.SetMouthProgressionAnimation(normalizedValue);
        }
        
        private async UniTask TriggerDefeatConditionAsync()
        {
            _timeService.StopTimer();
            _inputService.Cleanup();
            
            _gameFactory.Player.PlayerAnimator.PlayLoopRotation();
            _ballChainDto.MoveSpeed = _ballChainDto.BoostSpeedBallForLose;
            await UniTask.WaitUntil(() => _balls.Count == 0);
            _gameFactory.Player.PlayerAnimator.StopRotation();
            await UniTask.Delay(1000);
            
            _finishService.Lose();
        }
        
        #endregion
        
        private void ReIndexBalls()
        {
            for (int i = 0; i < _balls.Count; i++)
            {
                _balls[i].SetIndex(i);
            }
        }

        private void SetUpWidget(Vector3 position, Color color, string text)
        {
            var widget = _widgetProvider.GetWidget(position, Quaternion.identity);
            widget.SetText(text);
            widget.SetColor(color);
            widget.PlayAnimation();
        }
    }
}