using System;
using Code.Services.PersistenceProgress;
using Code.Services.SaveLoad;
using UnityEngine;

namespace Code.Services.Storage
{
    public class StorageService : IStorageService
    {
        private readonly IPersistenceProgressService _persistenceProgressService;
        private readonly ISaveLoadService _saveLoadService;

        public StorageService(
            IPersistenceProgressService persistenceProgressService, 
            ISaveLoadService saveLoadService)
        {
            _persistenceProgressService = persistenceProgressService;
            _saveLoadService = saveLoadService;
        }
        
        public event Action<Currency> ChangedCurrencyEvent;
        
        public void AddCurrency(CurrencyType currencyType, int value)
        {
            Currency currency = GetCurrency(currencyType);
            currency.Value += value;
            _saveLoadService.SaveProgress();
            ChangedCurrencyEvent?.Invoke(currency);
        }

        public void AddCurrency(Currency currency)
        {
            Currency targetCurrency = GetCurrency(currency.CurrencyType);
            targetCurrency.Value += currency.Value;
            _saveLoadService.SaveProgress();
            ChangedCurrencyEvent?.Invoke(targetCurrency);
        }

        public void SubstractCurrency(CurrencyType currencyType, int value)
        {
            Currency currency = GetCurrency(currencyType);
            currency.Value = Mathf.Min(currency.Value - value, 0);
            _saveLoadService.SaveProgress();
            ChangedCurrencyEvent?.Invoke(currency);
        }

        public void SubstractCurrency(Currency currency)
        {
            Currency targetCurrency = GetCurrency(currency.CurrencyType);
            targetCurrency.Value = Mathf.Min(currency.Value - currency.Value, 0);
            _saveLoadService.SaveProgress();
            ChangedCurrencyEvent?.Invoke(currency);
        }
        
        public Currency GetCurrency(CurrencyType currencyType)
        {
            return _persistenceProgressService.PlayerData.PlayerResources.GetOrCreateCurrencySave(currencyType);
        }
    }
}