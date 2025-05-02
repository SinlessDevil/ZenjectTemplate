using System.Collections;
using Code.Services.AudioVibrationFX.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Code.UI
{
    public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _image;
        [Header("Setup")]
        [SerializeField] private float ScaleAmount = 0.8f;
        [SerializeField] private float ScaleDuration = 0.2f;
    
        private Vector3 _originalScale;
        private ISoundService _soundService;

        [Inject]
        public void Constructor(ISoundService soundService)
        {
            _soundService = soundService;
        }
    
        private void Start() => _originalScale = _image.transform.localScale;

        public void OnPointerDown(PointerEventData eventData)
        {
            StopAllCoroutines();
            _image.transform.localScale = _originalScale;
            
            if (!_image.gameObject.activeInHierarchy)
                return;
            
            StartCoroutine(ScaleButton(_originalScale * ScaleAmount, ScaleDuration));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopAllCoroutines();
            
            if (!_image.gameObject.activeInHierarchy)
                return;

            StartCoroutine(ScaleButton(_originalScale, ScaleDuration));
        }

        private IEnumerator ScaleButton(Vector3 targetScale, float duration)
        {
            float time = 0;
            Vector3 startScale = _image.transform.localScale;

            while (time < duration)
            {
                float t = time / duration;
                _image.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            _image.transform.localScale = targetScale;
        }
    }   
}