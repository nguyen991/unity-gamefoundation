using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

#if GF_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace GameFoundation.Economy
{
    public class IAPListener : IStoreListener
    {
        public bool consumePurchase = true;
        public UnityAction<bool> onIntialized;
        public UnityAction<Product> onPurchaseComplete;
        public UnityAction<Product, PurchaseFailureReason> onPurchaseFailed;

        protected IStoreController controller;
        protected IExtensionProvider extensions;

        protected ConfigurationBuilder builder;
        protected ProductCatalog catalog;

        public void Init()
        {
            catalog = ProductCatalog.LoadDefaultCatalog();

            StandardPurchasingModule module = StandardPurchasingModule.Instance();
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

            builder = ConfigurationBuilder.Instance(module);
            IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, catalog);

            UnityPurchasing.Initialize(this, builder);
        }

        public T GetStoreConfiguration<T>() where T : IStoreConfiguration
        {
            return builder.Configure<T>();
        }

        public T GetStoreExtensions<T>() where T : IStoreExtension
        {
            return extensions.GetExtension<T>();
        }

        public IStoreController StoreController
        {
            get { return controller; }
        }

        public bool HasProductInCatalog(string productID)
        {
            foreach (var product in catalog.allProducts)
            {
                if (product.id == productID)
                {
                    return true;
                }
            }
            return false;
        }

        public Product GetProduct(string productID)
        {
            if (controller != null && controller.products != null && !string.IsNullOrEmpty(productID))
            {
                return controller.products.WithID(productID);
            }
            return null;
        }

        public void Purchase(string productID)
        {
            if (controller == null)
            {
                Debug.LogError("Purchase failed because Purchasing was not initialized correctly");
                return;
            }
            controller.InitiatePurchase(productID);
        }

        public void Restore(UniTaskCompletionSource<(bool, string)> task)
        {            
            GetStoreExtensions<IAppleExtensions>().RestoreTransactions((result, msg) => task.TrySetResult((result, msg)));
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.controller = controller;
            this.extensions = extensions;
            onIntialized.Invoke(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string msg)
        {
            Debug.LogError(string.Format("Purchasing failed to initialize. Reason: {0}", msg));
            onIntialized.Invoke(false);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError(string.Format("Purchasing failed to initialize. Reason: {0}", error.ToString()));
            onIntialized.Invoke(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            onPurchaseComplete.Invoke(e.purchasedProduct);
            return (consumePurchase) ? PurchaseProcessingResult.Complete : PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            onPurchaseFailed.Invoke(product, reason);
        }
    }
}
#endif