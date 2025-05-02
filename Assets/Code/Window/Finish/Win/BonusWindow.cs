using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Game.States;
using Code.Services.SFX;
using Code.Services.StaticData;
using Zenject;

namespace Code.Window.Finish.Win
{
    public class BonusWindow : FinishWindow
    {
        private IStateMachine<IGameState> _gameStateMachine;
        private IStaticDataService _staticDataService;
        private ISoundService _soundService;
        
        [Inject]
        public void Constructor(
            IStateMachine<IGameState> gameStateMachine,
            IStaticDataService staticDataService,
            ISoundService soundService)
        {
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
            _soundService.ButtonClick();
            
            _gameStateMachine.Enter<LoadLevelState, string>(_staticDataService.GameConfig.GameScene);
        }

        protected override void OnExitToMenuButtonClick()
        {
            _soundService.ButtonClick();
            
            _gameStateMachine.Enter<LoadMenuState, string>(_staticDataService.GameConfig.MenuScene);
        }
    }   
}