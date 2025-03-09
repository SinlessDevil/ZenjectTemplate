using Services.Levels;
using Services.LocalProgress;
using Services.Timer;
using Services.Window;
using Window;
using Window.Finish.Lose;

namespace Services.Finish.Lose
{
    public class LoseService : ILoseService
    {
        private IWindowService _windowService;
        private ILevelService _levelService;
        private ITimeService _timeService;
        private readonly ILevelLocalProgressService _levelLocalProgressService;

        public LoseService(
            IWindowService windowService, 
            ILevelService levelService,
            ITimeService timeService,
            ILevelLocalProgressService levelLocalProgressService)
        {
            _windowService = windowService;
            _levelService = levelService;
            _timeService = timeService;
            _levelLocalProgressService = levelLocalProgressService;
        }
        
        public void Lose()
        {
            var window = _windowService.Open(WindowTypeId.Lose);
            var loseWindow = window.GetComponent<LoseWindow>();
            loseWindow.Initialize();
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

        private string GetRecordText()
        {
            var currentRecordTime = GetCurrentRecordTime();
            var currentTime = _timeService.GetElapsedTime();
            
            if(currentRecordTime == 0 || currentTime > currentRecordTime)
            {
                return "New Record! Time: " + _timeService.GetFormattedElapsedTime();
            }

            return "Record: " + _timeService.GetFormattedElapsedTime();
        }
        
        private string GetScoreText()
        {
            return "Score: " + _levelLocalProgressService.Score;
        }

    }
}