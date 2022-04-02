using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class Item : CatalogItem
    {
        [Header("Data")]
        public long initBalance = 0;
        public long maxBalance = 0;
        public bool persistent = false;
        public bool stackable = false;
        public long itemPerStack = 0;
    }

    [System.Serializable]
    public class ItemCatalog : Catalog<Item> { }
}
