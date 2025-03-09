using System.Collections.Generic;
using StaticData;
using StaticData.Levels;
using Window;

namespace Services.StaticData
{
    public interface IStaticDataService
    {
        GameStaticData GameConfig { get; }
        BalanceStaticData Balance { get; }
        List<ChapterStaticData> Chapters { get; }
        BallChainStaticData BallChainConfig { get; }
        void LoadData();
        WindowConfig ForWindow(WindowTypeId windowTypeId);
        LevelStaticData ForLevel(int chapterId, int levelId);
        ChapterStaticData ForChapter(int chapterId);
    }
}