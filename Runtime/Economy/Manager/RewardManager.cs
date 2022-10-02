using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using GameFoundation.Utilities;

namespace GameFoundation.Economy
{
    public class RewardManager
    {
        [System.Serializable]
        protected class RewardRecord
        {
            public int claimed = 0;
            public int step = 0;
            public long lastClaimed = 0;
        }

        private RewardCatalog catalog;
        private InventoryManager inventory;
        private WalletManager wallet;

        [JsonProperty] private Dictionary<string, RewardRecord> rewardsRecorded = new Dictionary<string, RewardRecord>();

        public RewardManager(RewardCatalog catalog, InventoryManager inventory, WalletManager wallet)
        {
            this.catalog = catalog;
            this.inventory = inventory;
            this.wallet = wallet;
        }

        public Reward Find(string key)
        {
            return catalog.Find(key);
        }

        public List<RewardTableItem> Claim(string key, bool forceClaim = false)
        {
            if (IsClaimable(key) || forceClaim)
            {
                Reward reward = Find(key);
                if (reward != null)
                {
                    // update last claimed
                    if (!rewardsRecorded.TryGetValue(key, out RewardRecord record))
                    {
                        record = new RewardRecord();
                        rewardsRecorded.Add(key, record);
                    };

                    // check if the reward table available
                    if (reward.type == Reward.RewardType.Progressive && record.step >= reward.RewardTable.Count)
                    {
                        Debug.LogError("RewardManager: Reward " + key + " is not claimable because it has no more reward table available.");
                        return null;
                    }

                    // increase claimed count
                    if (Remain(key) > 0)
                    {
                        record.claimed += 1;
                        record.step += 1;
                    }
                    record.lastClaimed = DateTime.Now.Ticks;

                    // grant rewards
                    return GrantReward(reward, record.step - 1);
                }
            }
            return null;
        }

        public List<int> GetRewardIndexInTable(string key, IList<RewardTableItem> rewards)
        {
            var reward = Find(key);
            return rewards.Select(it => reward.RewardTable.IndexOf(it)).ToList();
        }

        private List<RewardTableItem> GrantReward(Reward reward, int index)
        {
            List<RewardTableItem> items = new List<RewardTableItem>();
            if (reward.type == Reward.RewardType.Progressive)
            {
                items.Add(reward.RewardTable[index]);
            }
            else if (reward.type == Reward.RewardType.Randomized)
            {
                var r = UnityEngine.Random.Range(0f, 1f);
                for (int i = 0; i < reward.RewardTable.Count; i++)
                {
                    if (r <= reward.RewardTable[i].percent)
                    {
                        items.Add(reward.RewardTable[i]);
                        break;
                    }
                    r -= reward.RewardTable[i].percent;
                }
            }

            // increase currency and inventory
            items.ForEach(it =>
            {
                it.currencies.ForEach(c => wallet.Add(c.item.key, c.amount));
                it.items.ForEach(i => inventory.Create(i.item.key, i.amount));
            });

            return items;
        }

        public bool IsClaimable(string key)
        {
            var remain = Remain(key);
            return (remain == -1 || remain > 0) && UntilNextClaim(key, remain).Ticks <= 0;
        }

        /// <summary>
        /// Returns the number of rewards that can be claim.
        /// </summary>
        /// <param name="key">The reward key.</param>
        /// <returns>The number of rewards that can be claim (-1 mean infinite).</returns>
        public int Remain(string key)
        {
            ResetExpiredReward(key);
            var reward = Find(key);
            if (reward != null)
            {
                // check progressive reward available
                if (reward.type == Reward.RewardType.Progressive && Progressive(key) >= reward.RewardTable.Count)
                {
                    return 0;
                }

                // check limit reward available
                if (reward.limitTime.duration == 0)
                {
                    // infinite remain
                    return -1;
                }

                // reset reward record if it's expired
                if (rewardsRecorded.TryGetValue(key, out RewardRecord record) && IsExpireDuration(reward.limitTime, record.lastClaimed))
                {
                    record.claimed = 0;
                }
                return record != null ? reward.limit - record.claimed : reward.limit;
            }
            return 0;
        }

        public int Progressive(string key)
        {
            var reward = Find(key);
            return reward != null &&
                reward.type == Reward.RewardType.Progressive &&
                rewardsRecorded.ContainsKey(key) ? rewardsRecorded[key].step : 0;
        }

        public TimeSpan UntilNextClaim(string key)
        {
            return UntilNextClaim(key, Remain(key));
        }

        private TimeSpan UntilNextClaim(string key, int remain)
        {
            if (rewardsRecorded.TryGetValue(key, out RewardRecord record))
            {
                var reward = Find(key);
                var cooldown = remain == -1 || remain > 0 ? reward.cooldown : reward.limitTime;
                var lastClaimDate = new DateTime(record.lastClaimed);
                if (cooldown.type == Reward.DurationTime.DurationType.Days)
                {
                    lastClaimDate = lastClaimDate.Reset();
                }
                lastClaimDate = lastClaimDate.AddTicks(cooldown.duration * (cooldown.type == Reward.DurationTime.DurationType.Minutes ? TimeSpan.TicksPerMinute : TimeSpan.TicksPerDay));

                var now = DateTime.Now.Ticks;
                var nextClaim = lastClaimDate.Ticks;
                return now < nextClaim ? TimeSpan.FromTicks(nextClaim - now) : TimeSpan.Zero;
            }
            return TimeSpan.Zero;
        }

        public bool Reset(string key)
        {
            return rewardsRecorded.Remove(key);
        }

        private void ResetExpiredReward(string key)
        {
            var reward = Find(key);
            if (rewardsRecorded.TryGetValue(key, out RewardRecord record) &&
                reward.expire.duration > 0 && IsExpireDuration(reward.expire, record.lastClaimed)
            )
            {
                Reset(key);
            }
        }

        private bool IsExpireDuration(Reward.DurationTime t, long lastClaimed)
        {
            if (t.type == Reward.DurationTime.DurationType.Days)
            {
                var diff = new DateTime(lastClaimed).DiffDays(DateTime.Now);
                if (diff >= t.duration)
                {
                    return true;
                }
            }
            else
            {
                var expire = t.duration * TimeSpan.TicksPerMinute;
                if (lastClaimed + expire <= DateTime.Now.Ticks)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
