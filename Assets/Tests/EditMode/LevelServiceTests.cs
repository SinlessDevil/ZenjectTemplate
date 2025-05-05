using System.Collections.Generic;
using Code.Services.Levels;
using Code.Services.PersistenceProgress;
using Code.Services.PersistenceProgress.Player;
using Code.Services.StaticData;
using Code.Services.Timer;
using Code.StaticData.Levels;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class LevelServiceTests
    {
        private IPersistenceProgressService _progress;
        private IStaticDataService _staticData;
        private ITimeService _timer;

        private LevelService _levelService;
        private PlayerLevelData _playerLevelData;

        [SetUp]
        public void SetUp()
        {
            _progress = Substitute.For<IPersistenceProgressService>();
            _staticData = Substitute.For<IStaticDataService>();
            _timer = Substitute.For<ITimeService>();

            _playerLevelData = new PlayerLevelData
            {
                CurrentProgress = new LevelContainer { ChapterId = 1, LevelId = 1 },
                LastProgress = new LevelContainer { ChapterId = 1, LevelId = 1 },
                LevelsComleted = new List<LevelContainer>()
            };

            var playerData = new PlayerData { PlayerLevelData = _playerLevelData };
            _progress.PlayerData.Returns(playerData);

            var chapter = ScriptableObject.CreateInstance<ChapterStaticData>();
            chapter.Levels = new List<LevelStaticData>
            {
                ScriptableObject.CreateInstance<LevelStaticData>(),
                ScriptableObject.CreateInstance<LevelStaticData>(),
                ScriptableObject.CreateInstance<LevelStaticData>(),
            };
            _staticData.ForChapter(Arg.Any<int>()).Returns(chapter);

            _timer.GetElapsedTime().Returns(123f);

            _levelService = new LevelService(_progress, _staticData, _timer);
        }

        [Test]
        public void LevelsComplete_Should_Add_New_LevelContainer()
        {
            _levelService.LevelsComplete();

            var completed = _playerLevelData.LevelsComleted;
            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual(1, completed[0].ChapterId);
            Assert.AreEqual(1, completed[0].LevelId);
            Assert.AreEqual(123f, completed[0].Time);
        }

        [Test]
        public void LevelsComplete_Should_Advance_Level()
        {
            _levelService.LevelsComplete();
            Assert.AreEqual(2, _playerLevelData.LastProgress.LevelId);
            Assert.AreEqual(2, _playerLevelData.CurrentProgress.LevelId);
        }

        [Test]
        public void LevelsComplete_Should_Advance_Chapter_If_Level_Overflow()
        {
            _playerLevelData.LastProgress.LevelId = 3;
            _playerLevelData.CurrentProgress.LevelId = 3;
            _levelService.LevelsComplete();
            Assert.AreEqual(2, _playerLevelData.LastProgress.ChapterId);
            Assert.AreEqual(1, _playerLevelData.LastProgress.LevelId);
        }

        [Test]
        public void LevelsComplete_Should_Not_Overflow_If_Only_One_Chapter()
        {
            for (int i = 0; i < 9; i++)
            {
                _levelService.LevelsComplete();
            }

            Assert.AreEqual(4, _playerLevelData.LastProgress.ChapterId);
            Assert.LessOrEqual(_playerLevelData.LastProgress.LevelId, 3);
        }
    }
}
