using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class RewardTableItem
    {
        public float percent;
        public List<TransactionItem<Currency>> currencies = new List<TransactionItem<Currency>>();
        public List<TransactionItem<Item>> items = new List<TransactionItem<Item>>();
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