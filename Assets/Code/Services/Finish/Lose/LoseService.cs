using Code.Services.Window;
using Code.Window;
using Code.Window.Finish.Lose;

namespace Code.Services.Finish.Lose
{
    public class LoseService : ILoseService
    {
        private IWindowService _windowService;

        public LoseService(
            IWindowService windowService)
        {
            _windowService = windowService;
        }
        
        public void Lose()
        {
            var window = _windowService.Open(WindowTypeId.Lose);
            var loseWindow = window.GetComponent<LoseWindow>();
            loseWindow.Initialize();
        }
        
    }
}