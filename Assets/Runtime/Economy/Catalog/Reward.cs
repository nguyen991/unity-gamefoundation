using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class RewardTableItem
    {
        public float percent;
        public List<Currency> currencies;
        public List<Item> items;
    }

    [System.Serializable]
    public class RewardTable : List<RewardTableItem> { }

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
        public RewardTable rewardTable;
    }

    [System.Serializable]
    public class RewardCatalog : Catalog<Reward> { }
}