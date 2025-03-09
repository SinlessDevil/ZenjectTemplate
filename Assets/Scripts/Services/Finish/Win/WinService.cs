using Services.Levels;
using Services.PersistenceProgress;
using Services.SaveLoad;
using Services.Timer;
using Services.Window;
using Window;
using Window.Finish.Win;

namespace Services.Finish.Win
{
    public class WinService : IWinService
    {
        private readonly IWindowService _windowService;
        private readonly ILevelService _levelService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly ITimeService _timeService;

        public WinService(
            IWindowService windowService, 
            ILevelService levelService,
            ISaveLoadService saveLoadService,
            IPersistenceProgressService persistenceProgressService,
            ITimeService timeService)
        {
            _windowService = windowService;
            _levelService = levelService;
            _saveLoadService = saveLoadService;
            _persistenceProgressService = persistenceProgressService;
            _timeService = timeService;
        }
        
        public void Win()
        {
            CompleteLevel();
            
            CompleteTutor();

            SetRecordText();
            
            SaveProgress();
            
            var window = _windowService.Open(WindowTypeId.Win);
            var winWindow = window.GetComponent<WinWindow>();
            winWindow.Initialize();
        }

        public void BonusWin()
        {
            CompleteLevel();
            
            CompleteTutor();

            SetRecordText();
            
            SaveProgress();
            
            var window = _windowService.Open(WindowTypeId.Bonus);
            var bonusWindow = window.GetComponent<BonusWindow>();
            bonusWindow.Initialize();
        }
        
        private void CompleteLevel()
        {
            _levelService.LevelsComplete();
        }

        private void CompleteTutor()
        {
            _persistenceProgressService.PlayerData.PlayerTutorialData.HasFirstCompleteLevel = true;
        }

        private void SetRecordText()
        {
            var currentRecordTime = GetCurrentRecordTime();
            var currentTime = _timeService.GetElapsedTime();
            var currentLevelContainer = _levelService.GetCurrentLevelContainer();
            
            if(currentRecordTime == 0)
            { 
                return;   
            }
            
            if (currentTime > currentRecordTime)
            {
                var existingLevel = _persistenceProgressService.PlayerData.PlayerLevelData.LevelsComleted.Find(level => level == currentLevelContainer);
                existingLevel.Time = currentTime;
            }
        }

        private float GetCurrentRecordTime()
        {
            var currentLevelContainer = _levelService.GetCurrentLevelContainer();
            if(currentLevelContainer == null)
            {
                return 0;
            }
            
            return currentLevelContainer.Time;
        }
        
        private void SaveProgress()
        {
            _saveLoadService.SaveProgress();
        }
    }
}
