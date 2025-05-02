using System;

namespace Code.Services.Storage
{
    public interface IStorageService
    {
        event Action<Currency> ChangedCurrencyEvent;
        
        void AddCurrency(CurrencyType currencyType, int value);
        void AddCurrency(Currency currency);
        
        void SubstractCurrency(CurrencyType currencyType, int value);
        void SubstractCurrency(Currency currency);
        
        Currency GetCurrency(CurrencyType currencyType);
    }
}