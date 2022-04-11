using System.Collections;
using System.Collections.Generic;
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

        public void Init()
        {
            Wallet = new WalletManager(economyData.currencyCatalog);
            Inventory = new InventoryManager(economyData.itemCatalog);
            Transaction = new TransactionManager(economyData.transactionCatalog, Wallet, Inventory);
            Store = new StoreManager(economyData.storeCatalog);
            Intialized = true;
        }
    }
}
