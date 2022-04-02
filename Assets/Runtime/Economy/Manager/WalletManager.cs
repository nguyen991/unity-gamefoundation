using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    public class WalletManager
    {
        [System.Serializable]
        protected class CurrencyData
        {
            [System.NonSerialized] public Currency currency;
            public string key;
            public long balance;
        }

        private CurrencyCatalog catalog;

        private Dictionary<string, CurrencyData> currencies;

        public WalletManager(CurrencyCatalog catalog)
        {
            this.catalog = catalog;
            this.currencies = new Dictionary<string, CurrencyData>();

            //TODO: load currencies data from persistent storage

            // init new currencies
            catalog.items.ForEach(currency =>
            {
                if (!currencies.ContainsKey(currency.key))
                {
                    currencies.Add(currency.key, new CurrencyData()
                    {
                        currency = currency,
                        balance = currency.initBalance
                    });
                }
            });
        }

        public long Get(string key)
        {
            if (currencies.TryGetValue(key, out CurrencyData data))
            {
                return data.balance;
            }
            return 0;
        }

        public long Set(string key, long value)
        {
            if (currencies.TryGetValue(key, out CurrencyData data))
            {
                if (data.currency.maxBalance > 0)
                {
                    value = value > data.currency.maxBalance ? data.currency.maxBalance : value;
                }
                data.balance = value;
            }
            return value;
        }

        public long Add(string key, long value)
        {
            if (currencies.TryGetValue(key, out CurrencyData data))
            {
                return Set(key, data.balance + value);
            }
            return 0;
        }

        public Currency Find(string key)
        {
            return catalog.Find(key);
        }
    }
}
