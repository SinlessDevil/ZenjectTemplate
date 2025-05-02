using Code.Services.PersistenceProgress.Player;

namespace Code.Services.SaveLoad
{
    public interface ISaveLoadFacade
    {
        void SaveProgress(SaveMethod method);
        void Save(SaveMethod method, PlayerData data);
        PlayerData Load(SaveMethod method);
    }
}