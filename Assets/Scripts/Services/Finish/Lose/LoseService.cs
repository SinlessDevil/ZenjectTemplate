using Services.Window;
using Window;
using Window.Finish.Lose;

namespace Services.Finish.Lose
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