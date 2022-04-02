using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class Store : CatalogItem
    {
        [Header("Data")]
        public List<Transaction> transactions = new List<Transaction>();
    }

    [System.Serializable]
    public class StoreCatalog : Catalog<Store> { }
}
