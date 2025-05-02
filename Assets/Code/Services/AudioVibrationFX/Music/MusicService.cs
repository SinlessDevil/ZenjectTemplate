using System.Collections.Generic;
using Code.Services.AudioVibrationFX.StaticData;
using Code.Services.PersistenceProgress;
using Code.Services.SaveLoad;
using Code.StaticData.AudioVibration;
using UnityEngine;

namespace Code.Services.AudioVibrationFX.Music
{
    public class MusicService : IMusicService
    {
        private readonly IAudioVibrationStaticDataService _audioVibrationStaticDataService;
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly ISaveLoadFacade _saveLoadFacade;
        private readonly Dictionary<MusicType, SoundData> _cachedMusic = new();
        
        private AudioSource _musicSource;

        public MusicService(
            IAudioVibrationStaticDataService staticDataService, 
            IPersistenceProgressService persistenceProgressService, 
            ISaveLoadFacade saveLoadFacade)
        {
            _audioVibrationStaticDataService = staticDataService;
            _persistenceProgressService = persistenceProgressService;
            _saveLoadFacade = saveLoadFacade;
        }

        public void CreateMusicRoot()
        {
            var go = new GameObject("[MusicPlayer]");
            Object.DontDestroyOnLoad(go);
            _musicSource = go.AddComponent<AudioSource>();
        }

        public void CacheMusic()
        {
            foreach (var music in _audioVibrationStaticDataService.SoundsData.MusicData)
            {
                if (!_cachedMusic.ContainsKey(music.MusicType))
                    _cachedMusic.Add(music.MusicType, music);
            }
        }

        public void SetStateMusic(bool enabled)
        {
            _persistenceProgressService.PlayerData.PlayerSettings.Music = enabled;
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
        }
        
        public void Play(MusicType type, bool loop = true)
        {
            if(_persistenceProgressService.PlayerData.PlayerSettings.Music == false)
                return;
            
            if (!_cachedMusic.TryGetValue(type, out var data))
            {
                Debug.LogWarning($"[MusicService] No music found for type: {type}");
                return;
            }

            if (_musicSource.isPlaying)
                _musicSource.Stop();

            _musicSource.clip = data.Clip;
            _musicSource.volume = data.Volume;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void Pause() => _musicSource.Pause();

        public void Resume()
        {
            if (_musicSource.clip != null)
                _musicSource.UnPause();
        }

        public void Stop() => _musicSource.Stop();
        
        public void SetVolume(float volume)
        {
            _musicSource.volume = Mathf.Clamp01(volume);
        }

        public float GetVolume()
        {
            return _musicSource.volume;
        }
    }
}
