using System;

namespace Services.PersistenceProgress.Player
{
    [Serializable]
    public class PlayerTutorialData
    {
        public bool HasFirstPushItem = false;
        public bool HasFirstReloadItem = false;
        public bool HasFirstCompleteLevel = false;
    }
}