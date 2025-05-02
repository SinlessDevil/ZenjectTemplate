namespace Code.Services.AudioVibrationFX.Vibration
{
    public interface IVibrationService
    {
        void Play(VibrationType type);
        void Stop();
        bool IsEnabled { get; }
        void SetStateVibration(bool enabled);
    }
}