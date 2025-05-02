namespace Code.Services.AudioVibrationFX.Music
{
    public interface IMusicService
    {
        void CreateMusicRoot();
        void CacheMusic();
        void Play(MusicType type, bool loop = true);
        void Pause();
        void Resume();
        void Stop();
        void SetVolume(float volume);
        float GetVolume();
        void SetStateMusic(bool enabled);
    }
}