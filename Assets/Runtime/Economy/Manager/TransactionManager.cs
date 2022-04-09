using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFoundation.Mobile;
using Cysharp.Threading.Tasks;

#if GF_IAP
using UnityEngine.Purchasing;
#endif

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

        public async UniTask<TransactionData> BeginTransaction(string key)
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
                        success = await AdsTransaction(transaction, result);
                        break;
                    case Transaction.TransactionType.IAP:
                        success = await IapProductTransaction(transaction, result);
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
                inventory.Remove(cost.item.key, cost.amount);
            }

            // add reward
            AddReward(transaction, result);

            return true;
        }

        public async UniTask<bool> IapProductTransaction(Transaction transaction, TransactionData result)
        {
            await UniTask.NextFrame();
            return false;
        }

        public async UniTask<bool> AdsTransaction(Transaction transaction, TransactionData result)
        {
            var task = new UniTaskCompletionSource<bool>();
            AdController.Instance.ShowReward((success) => task.TrySetResult(success));
            var success = await task.Task;

            if (success)
            {
                AddReward(transaction, result);
                return true;
            }
            return false;
        }

        private void AddReward(Transaction transaction, TransactionData result)
        {
            // add reward currency
            foreach (var reward in transaction.reward.currencies)
            {
                wallet.Add(reward.item.key, reward.amount);
                result.currencies.Add(new TransactionItem<Currency>() { item = reward.item, amount = reward.amount });
            }

            // add reward inventory
            foreach (var reward in transaction.reward.items)
            {
                inventory.Create(reward.item.key, reward.amount);
                result.items.Add(new TransactionItem<Item>() { item = reward.item, amount = reward.amount });
            }
        }

#if GF_IAP
        private class IAPManager : IStoreListener
        {
            private IStoreController controller;
            private IExtensionProvider extensions;

            public void Initialize()
            {
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                UnityPurchasing.Initialize(this, builder);
            }

            public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
            {
                this.controller = controller;
                this.extensions = extensions;
                Debug.Log("[IAPManager] Initialized");
            }

            public void OnInitializeFailed(InitializationFailureReason error)
            {
                Debug.Log("[IAPManager] Initialize failed: " + error.ToString());
            }

            public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
            {
                Debug.Log("[IAPManager] Purchase failed: " + product.definition.id + " - " + failureReason.ToString());
            }

            public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
            {
                Debug.Log("[IAPManager] Process purchase: " + purchaseEvent.purchasedProduct.definition.id);
                return PurchaseProcessingResult.Complete;
            }
        }
#endif
    }
}
