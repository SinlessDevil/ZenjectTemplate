using System.Threading;
using Code.Services.AudioVibrationFX.StaticData;
using Code.Services.PersistenceProgress;
using Code.Services.SaveLoad;
using Code.StaticData.AudioVibration;
using Cysharp.Threading.Tasks;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace Code.Services.AudioVibrationFX.Vibration
{
    public class VibrationService : IVibrationService
    {
        private readonly IAudioVibrationStaticDataService _staticDataService;
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly ISaveLoadFacade _saveLoadFacade;

        private CancellationTokenSource _curveCts;

        public bool IsEnabled { get; private set; } = true;

        public VibrationService(
            IAudioVibrationStaticDataService staticDataService, 
            IPersistenceProgressService persistenceProgressService, 
            ISaveLoadFacade saveLoadFacade)
        {
            _staticDataService = staticDataService;
            _persistenceProgressService = persistenceProgressService;
            _saveLoadFacade = saveLoadFacade;
        }

        public void SetStateVibration(bool enabled)
        {
            _persistenceProgressService.PlayerData.PlayerSettings.Vibration = enabled;
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
        }
        
        public void Play(VibrationType type)
        {
            if(_persistenceProgressService.PlayerData.PlayerSettings.Vibration == false)
                return;
            
            var data = _staticDataService.VibrationsData.Vibrations.Find(v => v.VibrationType == type);
            if (data == null || !IsEnabled)
            {
                Debug.LogWarning($"[VibrationService] Vibration not found or disabled: {type}");
                return;
            }

            switch (data.Mode)
            {
                case VibrationMode.Preset:
                    MMVibrationManager.Haptic(data.HapticType);
                    break;

                case VibrationMode.Constant:
                    MMVibrationManager.ContinuousHaptic(
                        Mathf.Clamp01(data.ConstantIntensity),
                        0.5f,
                        data.ConstantDuration);
                    break;

                case VibrationMode.Emphasis:
                    MMVibrationManager.TransientHaptic(
                        Mathf.Clamp01(data.EmphasisIntensity),
                        Mathf.Clamp01(data.EmphasisSharpness));
                    break;

                case VibrationMode.CustomCurve:
                    _curveCts?.Cancel();
                    _curveCts = new CancellationTokenSource();
                    PlayCurveAsync(data.Curve, data.CurveDuration, _curveCts.Token).Forget();
                    break;
            }
        }

        public void Stop()
        {
            MMVibrationManager.StopAllHaptics();
            _curveCts?.Cancel();
            _curveCts = null;
        }

        private async UniTaskVoid PlayCurveAsync(AnimationCurve curve, float duration, CancellationToken token)
        {
            float time = 0f;
            float step = 0.05f;

            while (time < duration)
            {
                token.ThrowIfCancellationRequested();

                float t = time / duration;
                float intensity = Mathf.Clamp01(curve.Evaluate(t));
                MMVibrationManager.ContinuousHaptic(intensity, 1f, step);

                time += step;
                await UniTask.Delay((int)(step * 1000), cancellationToken: token);
            }

            MMVibrationManager.StopAllHaptics();
        }

    }
}
