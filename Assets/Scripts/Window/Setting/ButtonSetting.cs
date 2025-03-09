using Services.SFX;
using Services.Timer;
using Services.Window;
using UnityEngine;
using UnityEngine.UI;
using Window;
using Zenject;

namespace UI.PauseDisplayer
{
    public class ButtonSetting : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        private SettingWindow _settingWindow;

        private ITimeService _timeService;
        private ISoundService _soundService;
        private IWindowService _windowService;
        
        [Inject]
        public void Constructor(
            ITimeService timeService,
            ISoundService soundService,
            IWindowService windowService)
        {
            _timeService = timeService;
            _soundService = soundService;
            _windowService = windowService;
        }
        
        private void OnEnable() => _button.onClick.AddListener(OpenPauseWindow);
        private void OnDisable() => _button.onClick.RemoveListener(OpenPauseWindow);

        private void OpenPauseWindow()
        {
            _soundService.ButtonClick();
            _timeService.Pause();

            var prefab = _windowService.Open(WindowTypeId.Setting);
            _settingWindow = prefab.GetComponent<SettingWindow>();
            
            _settingWindow.UpdateWindow();
            _settingWindow.ResetButtonScale();
        }
    }
}