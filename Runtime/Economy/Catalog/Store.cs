using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class Store : CatalogItem
    {
        [Header("Data")]
        [SerializeField]
        protected List<Transaction> transactions = new List<Transaction>();

        public List<Transaction> Transactions
        {
            get
            {
                transactions.RemoveAll(t => t == null);
                return transactions;
            }
        }
    }

    [System.Serializable]
    public class StoreCatalog : Catalog<Store> { }
}
