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

        private UniTaskCompletionSource<bool> iapTask;

#if GF_IAP
        private IAPListener iapListener;
#endif

        public TransactionManager(GameObject parent, TransactionCatalog catalog, WalletManager wallet, InventoryManager inventory)
        {
            this.catalog = catalog;
            this.wallet = wallet;
            this.inventory = inventory;

#if GF_IAP
            // initialize IAP Listener
            iapListener = new IAPListener();
            iapListener.onIntialized = OnPurchaseIntialized;
            iapListener.onPurchaseComplete = OnPurchaseComplete;
            iapListener.onPurchaseFailed = OnPurchaseFailed;
            iapListener.Init();
#endif
        }

        public Transaction Get(string key)
        {
            return catalog.Find(key);
        }

        public bool IsOwnedIAPProduct(string productId)
        {
#if GF_IAP
            var product = iapListener.GetProduct(productId);
            return product != null ? product.hasReceipt : false;
#else
            return false;
#endif
        }

        public void RestoreIAP()
        {
            iapListener.Restore();
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

        private async UniTask<bool> IapProductTransaction(Transaction transaction, TransactionData result)
        {
            iapTask = new UniTaskCompletionSource<bool>();
#if GF_IAP
            iapListener.Purchase(transaction.cost.productId);
#else
            iapTask.TrySetResult(true);
#endif
            var success = await iapTask.Task;
            if (success)
            {
                AddReward(transaction, result);
                return true;
            }
            return false;
        }

#if GF_IAP
        private void OnPurchaseIntialized(bool result)
        {
            Debug.Log($"Purchase initialized: {result}");

            // populate iap product
            catalog.items.ForEach(item =>
            {
                if (item.transactionType == Transaction.TransactionType.IAP && !string.IsNullOrEmpty(item.cost.productId))
                {
                    item.cost.product = iapListener.GetProduct(item.cost.productId);
                }
            });
        }

        private void OnPurchaseComplete(Product product)
        {
            Debug.Log($"Purchase success: {product.definition.id}");
            iapTask?.TrySetResult(true);
        }

        private void OnPurchaseFailed(Product product, PurchaseFailureReason error)
        {
            Debug.Log($"Purchase failed: {product.definition.id} - {error.ToString()}");
            iapTask?.TrySetResult(false);
        }
#endif

        private async UniTask<bool> AdsTransaction(Transaction transaction, TransactionData result)
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
    }
}
