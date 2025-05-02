using System;
using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Game.States;
using Code.Services.AudioVibrationFX.Music;
using Code.Services.AudioVibrationFX.Sound;
using Code.Services.AudioVibrationFX.Vibration;
using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Code.Services.StaticData;
using Code.Services.Timer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Window.Setting
{
    public class SettingWindow : MonoBehaviour
    {
        private const float AnimationDuration = 0.5f;
        
        [SerializeField] private ToggleContainer _toggleMusic;
        [SerializeField] private ToggleContainer _toggleSound;
        [SerializeField] private ToggleContainer _toggleVibrations;
        [Space(10)] 
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _quitToMenuButton;
        [SerializeField] private Button _restartLevelButton;
        [Space(10)]
        [SerializeField] private Color _enabledColor;
        [SerializeField] private Color _disabledColor;
        
        private PlayerSettings _playerSettings;
        
        private ITimeService _timeService;
        private IStaticDataService _staticDataService;
        private IStateMachine<IGameState> _gameStateMachine;
        
        private IMusicService _musicService;
        private ISoundService _soundService;
        private IVibrationService _vibrationService;

        [Inject]
        public void Constructor(
            IPersistenceProgressService progressService, 
            ITimeService timeService,
            IStaticDataService staticDataService,
            IStateMachine<IGameState> gameStateMachine,
            ISoundService soundService, 
            IMusicService musicService,
            IVibrationService vibrationService)
        {
            _timeService = timeService;
            _staticDataService = staticDataService;
            _gameStateMachine = gameStateMachine;
            _soundService = soundService;
            _musicService = musicService;
            _vibrationService = vibrationService;
            
            _playerSettings = progressService.PlayerData.PlayerSettings;
        }

        private void OnEnable()
        {
            _toggleMusic.Button.onClick.AddListener(() => UpdateSetting(TypeSFX.Music, _playerSettings.Music));
            _toggleSound.Button.onClick.AddListener(() => UpdateSetting(TypeSFX.Sound, _playerSettings.Sound));
            _toggleVibrations.Button.onClick.AddListener(() => UpdateSetting(TypeSFX.Vibration, _playerSettings.Vibration));
            
            _continueButton.onClick.AddListener(OnHideWindow);
            _quitToMenuButton.onClick.AddListener(OnQuitToMenu);
            _restartLevelButton.onClick.AddListener(OnRestartLevel);
        }
        private void OnDisable()
        {
            _toggleMusic.Button.onClick.RemoveListener(() => UpdateSetting(TypeSFX.Music, _playerSettings.Music));
            _toggleSound.Button.onClick.RemoveListener(() => UpdateSetting(TypeSFX.Sound, _playerSettings.Sound));
            _toggleVibrations.Button.onClick.RemoveListener(() => UpdateSetting(TypeSFX.Vibration, _playerSettings.Vibration));
            
            _continueButton.onClick.RemoveListener(OnHideWindow);
            _quitToMenuButton.onClick.RemoveListener(OnQuitToMenu);
            _restartLevelButton.onClick.RemoveListener(OnRestartLevel);
        }

        private void OnHideWindow()
        {
            _soundService.PlaySound(Sound2DType.Click);
            _timeService.SimpleMode();
            
            Destroy(this.gameObject);
        }
        
        private void OnQuitToMenu()
        {
            _soundService.PlaySound(Sound2DType.Click);
            _timeService.SimpleMode();
            
            _gameStateMachine.Enter<LoadMenuState, string>(_staticDataService.GameConfig.MenuScene);
        }
        
        private void OnRestartLevel()
        {
            _soundService.PlaySound(Sound2DType.Click);
            _timeService.SimpleMode();
            
            _gameStateMachine.Enter<LoadLevelState, string>(_staticDataService.GameConfig.GameScene);
        }

        private void UpdateSetting(TypeSFX typeSfx, bool setting)
        {
            setting = !setting;
            
            switch(typeSfx)
            {
                case TypeSFX.Sound:
                    _soundService.SetStateSound(setting);
                    break;
                case TypeSFX.Music:
                    _musicService.SetStateMusic(setting);
                    break;
                case TypeSFX.Vibration:
                    _vibrationService.SetStateVibration(setting);
                    break;
            }
            
            _soundService.PlaySound(Sound2DType.Click);
            
            UpdateWindow();
        }

        public void UpdateWindow()
        {
            UpdateColor();
            UpdateRectTransform();
        }
        private void UpdateColor()
        {
            _toggleMusic.Image.DOColor(SelectColor(_playerSettings.Music), AnimationDuration)
                .SetUpdate(true);
            _toggleSound.Image.DOColor(SelectColor(_playerSettings.Sound), AnimationDuration)
                .SetUpdate(true);
            _toggleVibrations.Image.DOColor(SelectColor(_playerSettings.Vibration), AnimationDuration)
                .SetUpdate(true);
        }
        private void UpdateRectTransform()
        {
            _toggleMusic.Image.rectTransform.DOAnchorPosX(SelectPositionX(_playerSettings.Music), AnimationDuration)
                .SetUpdate(true);
            _toggleSound.Image.rectTransform.DOAnchorPosX(SelectPositionX(_playerSettings.Sound), AnimationDuration)
                .SetUpdate(true);
            _toggleVibrations.Image.rectTransform.DOAnchorPosX(SelectPositionX(_playerSettings.Vibration), AnimationDuration)
                .SetUpdate(true);
        }
        
        private Color SelectColor(bool value) => value ? _enabledColor : _disabledColor;
        private float SelectPositionX(bool value) => value ? 50f : -50f;

        public void ResetButtonScale()
        {
            foreach (var button in _buttons)
            {
                button.transform.localScale = Vector3.one;
            }
        }
    }

    [Serializable]
    public class ToggleContainer
    {
        public Button Button;
        public Image Image;
    }

    [Serializable]
    public enum TypeSFX
    {
        Sound,
        Music,
        Vibration
    }
}