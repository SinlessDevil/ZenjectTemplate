using System;
using Code.Services.PersistenceProgress;
using Code.Services.SaveLoad;
using UnityEngine;

namespace Code.Services.Storage
{
    public class StorageService : IStorageService
    {
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly ISaveLoadFacade _saveLoadFacade;

        public StorageService(
            IPersistenceProgressService persistenceProgressService, 
            ISaveLoadFacade saveLoadFacade)
        {
            _persistenceProgressService = persistenceProgressService;
            _saveLoadFacade = saveLoadFacade;
        }
        
        public event Action<Currency> ChangedCurrencyEvent;
        
        public void AddCurrency(CurrencyType currencyType, int value)
        {
            Currency currency = GetCurrency(currencyType);
            currency.Value += value;
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
            ChangedCurrencyEvent?.Invoke(currency);
        }

        public void AddCurrency(Currency currency)
        {
            Currency targetCurrency = GetCurrency(currency.CurrencyType);
            targetCurrency.Value += currency.Value;
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
            ChangedCurrencyEvent?.Invoke(targetCurrency);
        }

        public void SubstractCurrency(CurrencyType currencyType, int value)
        {
            Currency currency = GetCurrency(currencyType);
            currency.Value = Mathf.Max(currency.Value - value, 0);
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
            ChangedCurrencyEvent?.Invoke(currency);
        }

        public void SubstractCurrency(Currency currency)
        {
            Currency targetCurrency = GetCurrency(currency.CurrencyType);
            targetCurrency.Value = Mathf.Max(targetCurrency.Value - currency.Value, 0);
            _saveLoadFacade.SaveProgress(SaveMethod.PlayerPrefs);
            ChangedCurrencyEvent?.Invoke(currency);
        }
        
        public Currency GetCurrency(CurrencyType currencyType)
        {
            return _persistenceProgressService.PlayerData.PlayerResources.GetOrCreateCurrencySave(currencyType);
        }
    }
}