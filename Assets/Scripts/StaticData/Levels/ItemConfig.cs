using UnityEngine;

namespace StaticData.Levels
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = "StaticData/ItemConfig", order = 1)]
    public class ItemConfig : ScriptableObject
    {
        public Color ItemColor;
    }
}