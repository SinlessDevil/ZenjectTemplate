using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Game.States;
using Code.Services.AudioVibrationFX.Sound;
using Code.Services.Levels;
using Code.Services.StaticData;
using Zenject;

namespace Code.Window.Finish.Win
{
    public class WinWindow : FinishWindow
    {
        private IStateMachine<IGameState> _gameStateMachine;
        private ILevelService _levelService;
        private IStaticDataService _staticDataService;
        private ISoundService _soundService;
        
        [Inject]
        public void Constructor(
            ILevelService levelService,
            IStateMachine<IGameState> gameStateMachine,
            IStaticDataService staticDataService,
            ISoundService soundService)
        {
            _levelService = levelService;
            _gameStateMachine = gameStateMachine;
            _staticDataService = staticDataService;
            _soundService = soundService;
        }

        public void OnDestroy()
        {
            UnsubscribeEvents();
        }
        
        public void Initialize()
        {
            SubscribeEvents();
        }
        
        protected override void OnLoadLevelButtonClick()
        {
            _soundService.PlaySound(Sound2DType.Click);
            
            _gameStateMachine.Enter<LoadLevelState, string>(_staticDataService.GameConfig.GameScene);
        }

        protected override void OnExitToMenuButtonClick()
        {
            _soundService.PlaySound(Sound2DType.Click);
            
            _gameStateMachine.Enter<LoadMenuState, string>(_staticDataService.GameConfig.MenuScene);
        }
    }   
}