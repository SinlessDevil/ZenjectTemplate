using Code.Services.Levels;

namespace Code.Services.Random
{
    public class RandomService : IRandomService
    {
        private readonly ILevelService _levelService;

        public RandomService(ILevelService levelService)
        {
            _levelService = levelService;
        }
        
        public string GenerateId() => System.Guid.NewGuid().ToString();
    }
}