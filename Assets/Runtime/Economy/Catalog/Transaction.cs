using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public TransactionData cost;
        public TransactionData reward;
    }

    [System.Serializable]
    public class TransactionCatalog : Catalog<Transaction> { }
}
