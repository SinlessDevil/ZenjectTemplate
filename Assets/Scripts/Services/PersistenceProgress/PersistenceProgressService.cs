using Services.PersistenceProgress.Analytic;
using Services.PersistenceProgress.Player;

namespace Services.PersistenceProgress
{
    public class PersistenceProgressService : IPersistenceProgressService
    {
        public PlayerData PlayerData { get; set; }
        public AnalyticsData AnalyticsData { get; set; }
    }
}