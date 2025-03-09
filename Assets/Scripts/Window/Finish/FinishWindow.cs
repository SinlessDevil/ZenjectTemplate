using UnityEngine;
using UnityEngine.UI;

namespace Window.Finish
{
    public abstract class FinishWindow : MonoBehaviour
    {
        [SerializeField] protected Text _textTime;
        [SerializeField] protected Text _textScore;
        [SerializeField] protected Button _buttonLoadLevel;
        [SerializeField] protected Button _buttonExitToMenu;

        public abstract void SetTime(string time);

        public abstract void SetScore(string score);

        protected virtual void SubscribeEvents()
        {
            _buttonLoadLevel.onClick.AddListener(OnLoadLevelButtonClick);
            _buttonExitToMenu.onClick.AddListener(OnExitToMenuButtonClick);
        }
        
        protected virtual void UnsubscribeEvents()
        {
            _buttonLoadLevel.onClick.RemoveListener(OnLoadLevelButtonClick);
            _buttonExitToMenu.onClick.RemoveListener(OnExitToMenuButtonClick);
        }
        
        protected abstract void OnLoadLevelButtonClick();
        protected abstract void OnExitToMenuButtonClick();
    }   
}