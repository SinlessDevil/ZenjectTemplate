using UnityEngine;
using Window;

namespace Services.Window
{
    public interface IWindowService
    {
        RectTransform Open(WindowTypeId windowTypeId);
    }
}