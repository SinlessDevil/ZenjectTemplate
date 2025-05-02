using System;
using System.Collections.Generic;
using System.Linq;
using Code.Services.Storage;

namespace Code.Services.PersistenceProgress.Player
{
    [Serializable]
    public class PlayerResources
    {
        public List<Currency> Currencies = new(2);
        
        public Currency GetOrCreateCurrencySave(CurrencyType currencyType)
        {
            var currency = Currencies.FirstOrDefault(x => x.CurrencyType == currencyType);
    
            if (currency != null)
                return currency;
    
            currency = new Currency(currencyType);
            Currencies.Add(currency);
            return currency;
        }
    }
}