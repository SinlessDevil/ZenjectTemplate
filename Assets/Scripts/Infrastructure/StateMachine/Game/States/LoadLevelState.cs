﻿using System;
using Services.BallControllers;
using Services.Factories.Game;
using Services.Factories.UIFactory;
using Services.Input;
using Services.Input.Device;
using Services.Levels;
using Services.Provides.Balls;
using Services.Provides.Widgets;
using Services.StaticData;
using UnityEngine;
using Cysharp.Threading.Tasks;
using StaticData;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoadLevelState : IPayloadedState<string>, IGameState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IUIFactory _uiFactory;
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IInputService _inputService;
        private readonly ILevelService _levelService;
        private readonly IBallProvider _ballProvider;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IBallChainController _ballChainController;
        private readonly IStaticDataService _staticDataService;

        public LoadLevelState(
            IStateMachine<IGameState> gameStateMachine, 
            ISceneLoader sceneLoader,
            ILoadingCurtain loadingCurtain, 
            IUIFactory uiFactory, 
            IGameFactory gameFactory,
            IInputService inputService,
            ILevelService levelService,
            IBallProvider ballProvider,
            IWidgetProvider widgetProvider,
            IBallChainController ballChainController,
            IStaticDataService staticDataService)
        {
            _gameFactory = gameFactory;
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _uiFactory = uiFactory;
            _inputService = inputService;
            _levelService = levelService;
            _ballProvider = ballProvider;
            _widgetProvider = widgetProvider;
            _ballChainController = ballChainController;
            _staticDataService = staticDataService;
        }

        public void Enter(string payload)
        {
            _loadingCurtain.Show();
            _sceneLoader.Load(payload, OnLevelLoad);
        }

        public void Exit()
        {
            _loadingCurtain.Hide();
        }

        protected virtual void OnLevelLoad()
        {
            InitGameWorld();

            _gameStateMachine.Enter<GameLoopState>();
        }

        private void InitGameWorld()
        {
            _uiFactory.CreateUiRoot();
            
            InitHud();
            
            InitLevelHolder();

            InitProviders();
            
            InitPlayer();

            InitBallChainController();
            
            PlayStartLevelAsync().Forget();
        }

        private void InitLevelHolder()
        {
            var levelStaticData = _levelService.GetCurrentLevelStaticData();
            var levelHolder = _gameFactory.CreateLevelHolder(levelStaticData);
            _levelService.SetLevelHolder(levelHolder);
            
            levelHolder.RoadMeshCreator.TriggerUpdate();
            levelHolder.RoadMeshCreator.MeshHolder.transform.SetParent(levelHolder.transform);
            levelHolder.RoadMeshCreator.MeshHolder.transform.position = new Vector3(0f, -0.11f, 0f);
            levelHolder.RoadMeshCreator.MeshHolder.transform.rotation = Quaternion.Euler(-0.6f, 0f, 0f);
        }

        private void InitProviders()
        {
            _ballProvider.CreatePoolBall();
            _widgetProvider.CreatePoolWidgets();
        }
        
        private void InitPlayer()
        {
            var levelHolder = _levelService.GetLevelHolder();
            var player = _gameFactory.CreatePlayer(levelHolder.SpawnPositionPlayer.position, Quaternion.identity);
            player.Initialize(levelHolder.PathCreator);
        }

        private void InitHud()
        {
            var gameHud = _uiFactory.CreateGameHud();
            gameHud.Initialize();
        }

        private void InitBallChainController()
        {
            var levelHolder = _levelService.GetLevelHolder();

            var ballChainDTO = new BallChainDTO();
            var ballChainStaticData = _staticDataService.BallChainConfig;
            
            ballChainDTO.DurationSpawnBall = ballChainStaticData.DurationSpawnBall;
            ballChainDTO.MoveSpeed = ballChainStaticData.MoveSpeed;
            ballChainDTO.SpacingBalls = ballChainStaticData.SpacingBalls;
            ballChainDTO.DurationMovingOffset = ballChainStaticData.DurationMovingOffset;
            ballChainDTO.CollisionThreshold = ballChainStaticData.CollisionThreshold;
            ballChainDTO.MatchingCount = ballChainStaticData.MatchingCount;
            ballChainDTO.InitialSpeedMultiplier = ballChainStaticData.InitialSpeedMultiplier;
            ballChainDTO.BoostDuration = ballChainStaticData.BoostDuration;
            ballChainDTO.MinParticleSpeed = ballChainStaticData.MinParticleSpeed;
            ballChainDTO.MaxParticleSpeed = ballChainStaticData.MaxParticleSpeed;
            ballChainDTO.BaseColorWidget = ballChainStaticData.BaseColorWidget;
            ballChainDTO.SetToSpawnWidget = ballChainStaticData.SetToSpawnWidget;
            ballChainDTO.TimeToSpawnWidget = ballChainStaticData.TimeToSpawnWidget;
            ballChainDTO.PercentToDetectionLose = ballChainStaticData.PercentToDetectionLose;
            ballChainDTO.BoostSpeedBallForLose = ballChainStaticData.BoostSpeedBallForLose;
            
            _ballChainController.Initialize(levelHolder.PathCreator, ballChainDTO);
        }
        
        private async UniTask PlayStartLevelAsync()
        {
            await UniTask.WaitUntil(() => !_loadingCurtain.IsActive);
            await UniTask.Delay(150);
            
            var startLevelInfoDisplayer = _uiFactory.CreateStartLevelInfoDisplayer();
            var nameLevel = _levelService.GetCurrentLevelStaticData().LevelName;
            var numberLevel = _levelService.GetCurrentChapter() + "-" + _levelService.GetCurrentLevel();
            var level = nameLevel + " " + numberLevel;
            startLevelInfoDisplayer.Initialize(level);
            startLevelInfoDisplayer.Play();
            await UniTask.WaitUntil(() => !startLevelInfoDisplayer.IsActive);
            
            await _ballChainController.MoveParticleAlongPathAsync(_levelService.GetLevelHolder().ParticleSystemHolder);
            
            _ballChainController.StartBallSpawning();
            
            _gameFactory.Player.PlayerShooting.Activate();

            switch (_staticDataService.GameConfig.TypeInput)
            {
                case TypeInput.PC:
                    _inputService.SetInputDevice(new MouseInputDevice());
                    break;
                case TypeInput.Mobile:
                    _inputService.SetInputDevice(new TouchInputDevice());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}