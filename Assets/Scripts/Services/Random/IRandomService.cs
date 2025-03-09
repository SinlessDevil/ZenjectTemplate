using System.Collections.Generic;
using UnityEngine;

namespace Services.Random
{
    public interface IRandomService
    {
        string GenerateId();
        List<Color> GetColorsByLevelRandomConfig();
        Color GetColorByCurrentItems(List<Item> items, Item currentItem);
    }
}