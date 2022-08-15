using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation.Economy
{
    public class WalletManager
    {
        [System.Serializable]
        protected class CurrencyData
        {
            [System.NonSerialized] public Currency currency;
            public BigInteger balance;
        }

        private CurrencyCatalog catalog;

        [JsonProperty] private Dictionary<string, CurrencyData> currencies;

        [JsonIgnore] public UnityEvent<string, BigInteger> OnChanged = new UnityEvent<string, BigInteger>();

        public WalletManager(CurrencyCatalog catalog)
        {
            this.catalog = catalog;
            this.currencies = new Dictionary<string, CurrencyData>();

            // init new currencies
            catalog.Items.ForEach(currency =>
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

        public BigInteger Get(string key)
        {
            if (currencies.TryGetValue(key, out CurrencyData data))
            {
                return data.balance;
            }
            return 0;
        }

        public BigInteger Set(string key, BigInteger value)
        {
            if (currencies.TryGetValue(key, out CurrencyData data))
            {
                if (data.currency.maxBalance > 0)
                {
                    value = value > data.currency.maxBalance ? data.currency.maxBalance : value;
                }
                data.balance = value;
                OnChanged.Invoke(key, value);
            }
            return value;
        }

        public BigInteger Add(string key, BigInteger value)
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

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // popuplate currencies data
            foreach (var currency in currencies)
            {
                currency.Value.currency = Find(currency.Key);
            }

            // remove currencies not in catalog
            var keys = currencies.Keys.Where(key => currencies[key].currency == null).ToList();
            keys.ForEach(key => currencies.Remove(key));
        }
    }
}
