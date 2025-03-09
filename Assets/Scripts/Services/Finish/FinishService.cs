using Services.Finish.Lose;
using Services.Finish.Win;
using Services.Levels;
using StaticData;
using UnityEngine;

namespace Services.Finish
{
    public class FinishService : IFinishService
    {
        private readonly IWinService _winService;
        private readonly ILoseService _loseService;
        private readonly ILevelService _levelService;

        public FinishService(
            IWinService winService, 
            ILoseService loseService,
            ILevelService levelService)
        {
            _winService = winService;
            _loseService = loseService;
            _levelService = levelService;
        }

        public void Win()
        {
            switch (_levelService.GetCurrentLevelStaticData().LevelTypeId)
            {
                case LevelTypeId.Regular:
                    _winService.Win();
                    break;
                case LevelTypeId.Special:
                case LevelTypeId.Bonus:
                    _winService.BonusWin();
                    break;
            }
        }

        public void Lose()
        {
            _loseService.Lose();
        }
    }
}
