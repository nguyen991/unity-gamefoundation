using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class RewardTableItem
    {
        public float percent;
        public List<TransactionItem<Currency>> currencies = new List<TransactionItem<Currency>>();
        public List<TransactionItem<Item>> items = new List<TransactionItem<Item>>();
        public List<TransactionItem<CatalogItem>> rewards => 
            currencies.Select(c => new TransactionItem<CatalogItem>() { item = c.item, amount = c.amount })
            .Concat(items.Select(c => new TransactionItem<CatalogItem>() { item = c.item, amount = c.amount }))
            .ToList();
    }

    [System.Serializable]
    public class Reward : CatalogItem
    {
        public enum RewardType
        {
            Progressive,
            Randomized,
        }

        [System.Serializable]
        public class DurationTime
        {
            public enum DurationType
            {
                Minutes,
                Days,
            }

            public int duration;
            public DurationType type;
        }

        [Header("Data")]
        public RewardType type;
        public int limit;
        public DurationTime limitTime;
        public DurationTime cooldown;
        public DurationTime expire;
        [SerializeField] protected List<RewardTableItem> rewardTable;
        public List<RewardTableItem> RewardTable
        {
            get
            {
                rewardTable?.ForEach(it =>
                {
                    it.currencies.RemoveAll(c => c.item == null);
                    it.items.RemoveAll(i => i.item == null);
                });
                return rewardTable;
            }
        }
    }

    [System.Serializable]
    public class RewardCatalog : Catalog<Reward> { }
}