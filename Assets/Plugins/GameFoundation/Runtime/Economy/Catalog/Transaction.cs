using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public List<TransactionItem<CatalogItem>> rewards => 
            currencies.Select(c => new TransactionItem<CatalogItem>() { item = c.item, amount = c.amount })
            .Concat(items.Select(c => new TransactionItem<CatalogItem>() { item = c.item, amount = c.amount }))
            .ToList();

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
