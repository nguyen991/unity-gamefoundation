using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class Currency : CatalogItem
    {
        [Header("Data")]
        public long initBalance = 0;
        public long maxBalance = 0;
    }

    [System.Serializable]
    public class CurrencyCatalog : Catalog<Currency> { }
}
