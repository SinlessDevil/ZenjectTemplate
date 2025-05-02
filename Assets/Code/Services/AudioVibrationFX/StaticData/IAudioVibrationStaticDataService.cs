using Code.StaticData.AudioVibration;

namespace Code.Services.AudioVibrationFX.StaticData
{
    public interface IAudioVibrationStaticDataService
    {
        SoundsData SoundsData { get; }
        VibrationsData VibrationsData { get; }
        void LoadData();
    }
}