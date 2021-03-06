using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GF_IAP
using UnityEngine.Purchasing;
#endif

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class TransactionItem<T> where T : CatalogItem
    {
        public T item;
        public long amount;
    }

    [System.Serializable]
    public class TransactionData
    {
        public List<TransactionItem<Currency>> currencies = new List<TransactionItem<Currency>>();
        public List<TransactionItem<Item>> items = new List<TransactionItem<Item>>();
        public string adsId;
        public string productId;

#if GF_IAP
        [System.NonSerialized] public Product product;
#endif
    }

    [System.Serializable]
    public class Transaction : CatalogItem
    {
        public enum TransactionType
        {
            Virtual,
            IAP,
            Ads
        }

        [Header("Data")]
        public TransactionType transactionType;
        [SerializeField] protected TransactionData cost;
        [SerializeField] protected TransactionData reward;

        public TransactionData Cost
        {
            get
            {
                cost.currencies.RemoveAll(c => c.item == null);
                cost.items.RemoveAll(i => i.item == null);
                return cost;
            }
        }

        public TransactionData Reward
        {
            get
            {
                reward.currencies.RemoveAll(c => c.item == null);
                reward.items.RemoveAll(i => i.item == null);
                return reward;
            }
        }
    }

    [System.Serializable]
    public class TransactionCatalog : Catalog<Transaction> { }
}
