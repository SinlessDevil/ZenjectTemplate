using System.Collections.Generic;
using Code.StaticData;
using Code.StaticData.Levels;
using Code.Window;

namespace Code.Services.StaticData
{
    public interface IStaticDataService
    {
        GameStaticData GameConfig { get; }
        BalanceStaticData Balance { get; }
        List<ChapterStaticData> Chapters { get; }
        void LoadData();
        WindowConfig ForWindow(WindowTypeId windowTypeId);
        LevelStaticData ForLevel(int chapterId, int levelId);
        ChapterStaticData ForChapter(int chapterId);
    }
}