using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    public class StoreManager
    {
        private StoreCatalog catalog;

        public StoreManager(StoreCatalog catalog)
        {
            this.catalog = catalog;
        }

        public Store Find(string key)
        {
            return catalog.Find(key);
        }
    }
}
