using UI;
using UnityEngine;

namespace Services.Provides.Widgets
{
    public interface IWidgetProvider 
    {
        public void CreatePoolWidgets();
        public void CleanupPool();
        public Widget GetWidget(Vector3 position, Quaternion rotation);
        public void ReturnWidget(Widget widget);
    }
}