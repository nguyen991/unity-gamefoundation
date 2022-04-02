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

    private async UniTaskVoid Start()
    {
        // preload
        await UniTask.WaitUntil(() => SpriteLoader.Instance.IsLoaded);
        await UniTask.WaitUntil(() => EconomyManager.Instance.Intialized);

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
        // // var itemId = EconomyManager.Instance.Inventory.Create("item_persistance");
        // Debug.Log("Create persistance item: " + itemId);
        // Debug.Log("Amount persistance item: " + EconomyManager.Instance.Inventory.TotalAmount("item_persistance"));
        // Debug.Log("Remove persistance item: " + EconomyManager.Instance.Inventory.Remove(itemId));
        // EconomyManager.Instance.Inventory.RemoveRange("item_persistance", 1);
        // Debug.Log("Remove Range:");
        // Debug.Log("Amount persistance item: " + EconomyManager.Instance.Inventory.TotalAmount("item_persistance"));

        // itemId = EconomyManager.Instance.Inventory.Create("item_persistance");
        // Debug.Log("Create more persistance item: " + itemId);

        // test transaction
        Debug.Log("------------ Test Transaction ------------");
        var result = EconomyManager.Instance.Transaction.BeginTransaction("coin_gem");
        if (result != null)
        {
            Debug.Log("Coins: " + EconomyManager.Instance.Wallet.Get("coin"));
            Debug.Log("Gems: " + EconomyManager.Instance.Wallet.Get("gem"));
            result.currencies.ForEach(rw =>
            {
                Debug.Log("Reward: " + rw.item.display + "-" + rw.amount);
            });
        }
    }
}
