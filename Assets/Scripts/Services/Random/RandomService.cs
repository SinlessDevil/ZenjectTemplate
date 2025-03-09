using System.Collections.Generic;
using Services.Levels;
using UnityEngine;

namespace Services.Random
{
    public class RandomService : IRandomService
    {
        private readonly ILevelService _levelService;

        public RandomService(ILevelService levelService)
        {
            _levelService = levelService;
        }

        public List<Color> GetColorsByLevelRandomConfig()
        {
            var levelRandomConfig = _levelService.GetCurrentLevelStaticData().LevelConfig.RandomConfig;
            var count = _levelService.GetCurrentLevelStaticData().LevelConfig.CountItem;

            if (levelRandomConfig == null || levelRandomConfig.ItemProbabilities.Count == 0)
            {
                return new List<Color>();
            }

            List<Color> selectedColors = new List<Color>();
            
            foreach (var itemProbability in levelRandomConfig.ItemProbabilities)
            {
                int exactCount = Mathf.RoundToInt(count * (itemProbability.Probability / 100f));
                for (int i = 0; i < exactCount; i++)
                {
                    selectedColors.Add(itemProbability.Item.ItemColor);
                }
            }
            
            for (int i = selectedColors.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (selectedColors[i], selectedColors[randomIndex]) = (selectedColors[randomIndex], selectedColors[i]);
            }

            return selectedColors;
        }

        public Color GetColorByCurrentItems(List<Item> items, Item currentItem)
        {
            if (items == null || items.Count == 0)
                return Color.clear;

            List<Color> availableColors = new List<Color>();
            
            foreach (var item in items)
            {
                if (!availableColors.Contains(item.Color))
                    availableColors.Add(item.Color);
            }

            if (availableColors.Count == 1) 
                return availableColors[0];
            
            if (currentItem != null && availableColors.Contains(currentItem.Color) && availableColors.Count > 1)
                availableColors.Remove(currentItem.Color);

            return availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
        }
        
        public string GenerateId() => System.Guid.NewGuid().ToString();
    }
}