using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    public class TransactionManager
    {
        private TransactionCatalog catalog;
        private WalletManager wallet;
        private InventoryManager inventory;

        public TransactionManager(TransactionCatalog catalog, WalletManager wallet, InventoryManager inventory)
        {
            this.catalog = catalog;
            this.wallet = wallet;
            this.inventory = inventory;
        }

        public TransactionData BeginTransaction(string key)
        {
            var transaction = catalog.Find(key);
            if (transaction != null)
            {
                TransactionData result = new TransactionData();
                bool success = true;
                switch (transaction.transactionType)
                {
                    case Transaction.TransactionType.Virtual:
                        success = VirtualTransaction(transaction, result);
                        break;
                    case Transaction.TransactionType.Ads:
                        AdsTransaction();
                        break;
                    case Transaction.TransactionType.IAP:
                        IapProductTransaction();
                        break;
                }
                return success ? result : null;
            }
            return null;
        }

        private bool VirtualTransaction(Transaction transaction, TransactionData result)
        {
            // check cost currency
            foreach (var cost in transaction.cost.currencies)
            {
                if (wallet.Get(cost.item.key) < cost.amount)
                {
                    return false;
                }
            }

            // check cost inventory
            foreach (var cost in transaction.cost.items)
            {
                if (inventory.TotalAmount(cost.item.key) < cost.amount)
                {
                    return false;
                }
            }

            // consume cost currency
            foreach (var cost in transaction.cost.currencies)
            {
                wallet.Add(cost.item.key, -cost.amount);
            }

            // consume cost inventory
            foreach (var cost in transaction.cost.items)
            {
                inventory.RemoveRange(cost.item.key, (int)cost.amount);
            }

            // add reward currency
            foreach (var reward in transaction.reward.currencies)
            {
                wallet.Add(reward.item.key, reward.amount);
                result.currencies.Add(new TransactionItem<Currency>() { item = reward.item, amount = reward.amount });
            }

            // add reward inventory
            foreach (var reward in transaction.reward.items)
            {
                //TODO: update here
                // inventory.CreateRange(reward.item.key, (int)reward.amount);
                result.items.Add(new TransactionItem<Item>() { item = reward.item, amount = reward.amount });
            }

            return true;
        }

        public void IapProductTransaction()
        {
        }

        public void AdsTransaction()
        {
        }
    }
}
