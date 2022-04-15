using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GameFoundation.Pool;
using UnityEngine.UI;
using GameFoundation.Addressable;
using GameFoundation.Economy;

public class TestAtlasLoader : MonoBehaviour
{
    public Image image;

    public async void Run()
    {
        // get sprite
        var sprite = await SpriteLoader.Instance.GetSprite("Tiles/block_14.png");
        image.sprite = sprite;
        image.SetNativeSize();

        // test wallet
        Debug.Log("------------ Test Wallet ------------");
        Debug.Log("Coin Initialize: " + EconomyManager.Instance.Wallet.Get("coin"));
        Debug.Log("Coin Add: " + EconomyManager.Instance.Wallet.Add("coin", 100));
        Debug.Log("Coin Sub: " + EconomyManager.Instance.Wallet.Add("coin", -100));
        Debug.Log("Coin Set: " + EconomyManager.Instance.Wallet.Set("coin", 1000));
        Debug.Log("Coin Limit: " + EconomyManager.Instance.Wallet.Set("coin", 6000));

        var currency = EconomyManager.Instance.Wallet.Find("coin");
        Debug.Log("Coin Find: " + currency.key);
        Debug.Log("Coin Tag True: " + currency.IsHaveTag("soft"));
        Debug.Log("Coin Tag False: " + currency.IsHaveTag("false"));
        Debug.Log("Coin Icon: " + currency.properties["icon"].GetSprite().name);

        // test inventory
        Debug.Log("------------ Test Inventory ------------");
        var items = EconomyManager.Instance.Inventory.Create("immutable", 1);
        Debug.Log("Create immutable instances: " + items?.Count);

        items = EconomyManager.Instance.Inventory.Create("stackable", 30);
        Debug.Log($"Create stackable instances: {items?.Count}, amount: {EconomyManager.Instance.Inventory.TotalAmount("stackable")}");
        items = EconomyManager.Instance.Inventory.Create("stackable", 130);
        Debug.Log($"Create stackable instances: {items?.Count}, amount: {EconomyManager.Instance.Inventory.TotalAmount("stackable")}");

        items = EconomyManager.Instance.Inventory.Create("unstackable", 10);
        Debug.Log($"Create unstackable instances: {items?.Count}, amount: {EconomyManager.Instance.Inventory.TotalAmount("unstackable")}");
        items.ForEach(item =>
        {
            item.data.Add("bag", "main");
        });
        items[0].data.Remove("bag");

        var queryCount = EconomyManager.Instance.Inventory.Query(tags: new string[] { "tag_1" }).Count();
        Debug.Log($"Query instances with tag: {queryCount}");

        queryCount = EconomyManager.Instance.Inventory.Query(data: new string[] { "bag" }).Count();
        Debug.Log($"Query instances with data: {queryCount}");

        // test transaction
        Debug.Log("------------ Test Transaction ------------");
        var result = await EconomyManager.Instance.Transaction.BeginTransaction("virual_transaction");
        if (result != null)
        {
            Debug.Log("Coins: " + EconomyManager.Instance.Wallet.Get("coin"));
            Debug.Log("Gems: " + EconomyManager.Instance.Wallet.Get("gem"));
            result.currencies.ForEach(rw =>
            {
                Debug.Log("Reward: " + rw.item.display + "-" + rw.amount);
            });
        }
        else
        {
            Debug.LogError("Transaction virual_transaction error");
        }

        result = await EconomyManager.Instance.Transaction.BeginTransaction("ads_transation");
        if (result != null)
        {
            Debug.Log("Coins: " + EconomyManager.Instance.Wallet.Get("coin"));
            Debug.Log("Gems: " + EconomyManager.Instance.Wallet.Get("gem"));
            result.currencies.ForEach(rw =>
            {
                Debug.Log("Reward: " + rw.item.display + "-" + rw.amount);
            });
        }
        else
        {
            Debug.LogError("Transaction ads_transation error");
        }

        // test ads
        // Debug.Log("------------ Test ShowInterstitial Ad ------------");
        // GameFoundation.Mobile.AdController.Instance.ShowInterstitial();

        // Debug.Log("------------ Test ShoReward Ad ------------");
        // GameFoundation.Mobile.AdController.Instance.ShowReward((success) =>
        // {
        //     Debug.Log("ShowReward: " + success);
        // });
    }

    public async void IAPTest()
    {
        Debug.Log("Price " + EconomyManager.Instance.Transaction.Get("iap_transaction").cost.product.metadata.localizedPriceString);

        var result = await EconomyManager.Instance.Transaction.BeginTransaction("iap_transaction");
        if (result != null)
        {
            Debug.Log("Coins: " + EconomyManager.Instance.Wallet.Get("coin"));
            Debug.Log("Gems: " + EconomyManager.Instance.Wallet.Get("gem"));
            result.currencies.ForEach(rw =>
            {
                Debug.Log("Reward: " + rw.item.display + "-" + rw.amount);
            });
        }
        else
        {
            Debug.LogWarning("Transaction iap_transaction error");
        }
    }
}
