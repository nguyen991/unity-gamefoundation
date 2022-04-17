using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GameFoundation.Economy
{
    public class EconomyManager : Utilities.SingletonBehaviour<EconomyManager>
    {
        public EconomyData economyData;

        public bool Intialized { get; private set; } = false;

        public WalletManager Wallet { get; private set; } = null;
        public InventoryManager Inventory { get; private set; } = null;
        public TransactionManager Transaction { get; private set; } = null;
        public StoreManager Store { get; private set; } = null;

        private Data.IDataLayer dataLayer;

        public void Init(Data.IDataLayer dataLayer, EconomyData data = null)
        {
            if (Intialized)
            {
                return;
            }

            economyData = data ?? economyData;
            if (economyData == null)
            {
                Debug.LogError("Economy data is null");
                return;
            }

            this.dataLayer = dataLayer;
            Wallet = new WalletManager(economyData.currencyCatalog);
            Inventory = new InventoryManager(economyData.itemCatalog);
            Transaction = new TransactionManager(gameObject, economyData.transactionCatalog, Wallet, Inventory);
            Store = new StoreManager(economyData.storeCatalog);

            Intialized = true;
        }

        public void Save()
        {
            var data = new Dictionary<string, string>()
            {
                { "wallet", JsonConvert.SerializeObject(Wallet) },
                { "inventory", JsonConvert.SerializeObject(Inventory) },
            };
            dataLayer.Save("economy", data);
        }

        public void Load()
        {
            var data = new Dictionary<string, string>();
            if (!dataLayer.Load("economy", data))
            {
                return;
            }
            Debug.Log(data["wallet"]);
            Debug.Log(data["inventory"]);
            if (data.ContainsKey("wallet"))
            {
                JsonConvert.PopulateObject(data["wallet"], Wallet);
            }
            if (data.ContainsKey("inventory"))
            {
                JsonConvert.PopulateObject(data["inventory"], Inventory);
            }
        }
    }
}
