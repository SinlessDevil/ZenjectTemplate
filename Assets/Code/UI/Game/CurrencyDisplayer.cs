using Code.Services.Storage;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Game
{
    public class CurrencyDisplayer : MonoBehaviour
    {
        [SerializeField] private CurrencyType _currencyType;
        [SerializeField] private Text _textCurrency;

        private IStorageService _storageService;
    
        [Inject]
        private void Construct(IStorageService storageService)
        {
            _storageService = storageService;
        }
    
        private void Start()
        {
            SubscribeEvent();
            SetUpCurrency();
        }

        private void OnDestroy()
        {
            UnsubscribeEvent();
        }

        private void SubscribeEvent()
        {
            _storageService.ChangedCurrencyEvent += OnAnimationUpdateCurrency;
        }

        private void UnsubscribeEvent()
        {
            _storageService.ChangedCurrencyEvent -= OnAnimationUpdateCurrency;
        }

        private void OnAnimationUpdateCurrency(Currency currency)
        {
            _textCurrency.DOText(currency.Value.ToString(), 0.15f)
                .SetEase(Ease.Linear);
        }
    
        private void SetUpCurrency()
        {
            _textCurrency.text = _storageService.GetCurrency(_currencyType).Value.ToString();
        }
    }
   
}