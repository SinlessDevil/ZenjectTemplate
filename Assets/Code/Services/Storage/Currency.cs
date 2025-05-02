using System;

namespace Code.Services.Storage
{
    [Serializable]
    public class Currency
    {
        public CurrencyType CurrencyType;
        public int Value;

        public Currency(CurrencyType currencyType)
        {
            CurrencyType = currencyType;
        }
    }
}