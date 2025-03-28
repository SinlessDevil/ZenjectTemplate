using Infrastructure.StateMachine;
using Infrastructure.StateMachine.Game;
using Infrastructure.StateMachine.Game.States;
using Services.Factories.Game;
using Services.Factories.UIFactory;
using Services.Finish;
using Services.Finish.Lose;
using Services.Finish.Win;
using Services.Input;
using Services.Levels;
using Services.LocalProgress;
using Services.PersistenceProgress;
using Services.Provides.Widgets;
using Services.Random;
using Services.SaveLoad;
using Services.SFX;
using Services.StaticData;
using Services.Storage;
using Services.Timer;
using Services.Window;
using UnityEngine;
using Zenject;
using Application = UnityEngine.Application;

namespace Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private CoroutineRunner _coroutineRunner;
        [SerializeField] private LoadingCurtain _curtain;
        [SerializeField] private SoundService _soundService;
        [SerializeField] private MusicService _musicService;
        [SerializeField] private TimeService _timeService;
        
        private RuntimePlatform Platform => Application.platform;

        public override void InstallBindings()
        {
            Debug.Log("Installer");

            BindMonoServices();
            BindServices();
            BindGameStateMachine();
            MakeInitializable();
        }
        
        public void Initialize() => BootstrapGame();

        private void BindMonoServices()
        {
            Container.Bind<ICoroutineRunner>().FromMethod(() => Container.InstantiatePrefabForComponent<ICoroutineRunner>(_coroutineRunner)).AsSingle();
            Container.Bind<ILoadingCurtain>().FromMethod(() => Container.InstantiatePrefabForComponent<ILoadingCurtain>(_curtain)).AsSingle();
            Container.Bind<ISoundService>().FromMethod(() => Container.InstantiatePrefabForComponent<ISoundService>(_soundService)).AsSingle();
            Container.Bind<IMusicService>().FromMethod(() => Container.InstantiatePrefabForComponent<IMusicService>(_musicService)).AsSingle();
            Container.Bind<ITimeService>().FromMethod(() => Container.InstantiatePrefabForComponent<ITimeService>(_timeService)).AsSingle();
            
            BindSceneLoader();
        }

        private void BindServices()
        {
            BindStaticDataService();
            
            Container.BindInterfacesTo<UIFactory>().AsSingle();
            Container.BindInterfacesTo<GameFactory>().AsSingle();
            Container.BindInterfacesTo<WindowService>().AsSingle();
            Container.BindInterfacesTo<InputService>().AsSingle();
            Container.BindInterfacesTo<PersistenceProgressService>().AsSingle();
            Container.BindInterfacesTo<RandomService>().AsSingle();
            Container.BindInterfacesTo<SaveLoadService>().AsSingle();
            Container.BindInterfacesTo<WidgetProvider>().AsSingle();
            Container.BindInterfacesTo<LevelService>().AsSingle();
            Container.BindInterfacesTo<FinishService>().AsSingle();
            Container.BindInterfacesTo<WinService>().AsSingle();
            Container.BindInterfacesTo<LoseService>().AsSingle();
            Container.BindInterfacesTo<LevelLocalProgressService>().AsSingle();
            Container.BindInterfacesTo<StorageService>().AsSingle();
        }

        private void BindGameStateMachine()
        {
            Container.Bind<GameStateFactory>().AsSingle();
            Container.BindInterfacesTo<GameStateMachine>().AsSingle();
            
            BindGameStates();
        }

        private void MakeInitializable() => Container.Bind<IInitializable>().FromInstance(this);

        private void BindSceneLoader()
        {
            ISceneLoader sceneLoader = new SceneLoader(Container.Resolve<ICoroutineRunner>());
            Container.Bind<ISceneLoader>().FromInstance(sceneLoader).AsSingle();
        }

        private void BindStaticDataService()
        {
            IStaticDataService staticDataService = new StaticDataService();
            staticDataService.LoadData();
            Container.Bind<IStaticDataService>().FromInstance(staticDataService).AsSingle();
        }
        
        private void BindGameStates()
        {
            Container.Bind<BootstrapState>().AsSingle();
            Container.Bind<LoadProgressState>().AsSingle();
            Container.Bind<BootstrapAnalyticState>().AsSingle();
            Container.Bind<PreLoadGameState>().AsSingle();
            Container.Bind<LoadMenuState>().AsSingle();
            Container.Bind<LoadLevelState>().AsSingle();
            Container.Bind<GameLoopState>().AsSingle();
        }

        private void BootstrapGame() => 
            Container.Resolve<IStateMachine<IGameState>>().Enter<BootstrapState>();
    }
}