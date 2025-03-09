using Services.Factories.UIFactory;
using UnityEngine;
using Window;
using Zenject;

namespace Services.Window
{
    public class WindowService : IWindowService
    {
        private IUIFactory _uiFactory;

        [Inject]
        public void Constructor(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public RectTransform Open(WindowTypeId windowTypeId)
        {
            return _uiFactory.CrateWindow(windowTypeId);
        }
    }
}