using Services.PersistenceProgress.Player;

namespace Services.SaveLoad
{
    public interface ISaveLoadService
    {
        PlayerData LoadProgress();
        void SaveProgress();
        void ResetProgress();
    }
}