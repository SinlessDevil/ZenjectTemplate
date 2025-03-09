using System;

namespace Services.PersistenceProgress.Player
{
    [Serializable]
    public class PlayerSettings
    {
        public bool Sound = true;
        public bool Music = true;
        public bool Vibration = true;
    }
}