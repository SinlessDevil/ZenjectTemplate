using UnityEngine;

namespace Code.Services.AudioVibrationFX.Sound
{
    public interface ISoundService
    {
        void Cache2DSounds();
        void CreateSoundsPool();
        void PlaySound(Sound2DType type);
        void PlaySound(Sound3DType type, Vector3 position);
        void SetGlobalVolume(float volume);
        float GetGlobalVolume();
        void SetStateSound(bool enabled);
    }
}