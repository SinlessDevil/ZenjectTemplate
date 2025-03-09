using UnityEngine;
using UI;
using UI.Game;
using UI.Menu;
using UI.Menu.Windows.Map;
using Window;

namespace Services.Factories.UIFactory
{
    public interface IUIFactory
    {
        public GameHud GameHud { get; }
        public MenuHud MenuHud { get; }

        public void CreateUiRoot();
        public RectTransform CrateWindow(WindowTypeId windowTypeId);
        public GameHud CreateGameHud();
        public MenuHud CreateMenuHud();
        public Widget CreateWidget(Vector3 position, Quaternion rotation);
        public ItemLevel CreateItemLevel(Transform parent);
        public StartLevelInfoDisplayer CreateStartLevelInfoDisplayer();
    }
}