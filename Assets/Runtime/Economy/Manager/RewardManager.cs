using System;
using System.Collections;
using System.Collections.Generic;
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
            public long lastClaimed = 0;
        }

        private RewardCatalog catalog;
        private InventoryManager inventory;
        private WalletManager wallet;

        [JsonProperty] private Dictionary<string, RewardRecord> rewardsRecorded;

        public RewardManager(RewardCatalog catalog)
        {
            this.catalog = catalog;
        }

        public Reward Find(string key)
        {
            return catalog.Find(key);
        }

        public bool Claim(string key)
        {
            if (IsClaimable(key))
            {
                // update last claimed
                if (!rewardsRecorded.TryGetValue(key, out RewardRecord record))
                {
                    record = new RewardRecord();
                    rewardsRecorded.Add(key, record);
                };
                record.claimed += 1;
                record.lastClaimed = DateTime.Now.Ticks;

                // grant rewards
                Reward reward = catalog.Find(key);
                if (reward != null)
                {
                    GrantReward(reward, record.claimed);
                }
                return true;
            }
            return false;
        }

        private void GrantReward(Reward reward, int claimed)
        {
            List<RewardTableItem> items = new List<RewardTableItem>();
            if (reward.type == Reward.RewardType.Progressive)
            {
                items.Add(reward.RewardTable[claimed]);
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
        }

        public bool IsClaimable(string key)
        {
            return Remain(key) > 0 && UntilNextClaim(key).Ticks <= 0;
        }

        /// <summary>
        /// Returns the number of rewards that can be claimed.
        /// </summary>
        /// <param name="key">The reward key.</param>
        /// <returns>The number of rewards that can be claimed</returns>
        public int Remain(string key)
        {
            ResetExpiredReward(key);
            var reward = Find(key);
            if (reward != null && rewardsRecorded.TryGetValue(key, out RewardRecord record))
            {
                return reward.limit > 0 ? reward.limit - record.claimed : int.MaxValue;
            }
            return 0;
        }

        public TimeSpan UntilNextClaim(string key)
        {
            ResetExpiredReward(key);
            if (rewardsRecorded.TryGetValue(key, out RewardRecord record))
            {
                var reward = Find(key);
                var remain = Remain(key);
                var now = DateTime.Now.Ticks;
                long nextClaim = 0;
                var expire = remain > 0 ? reward.cooldown : reward.expire;
                var lastClaimDate = new DateTime(record.lastClaimed);
                if (expire.type == Reward.DurationTime.DurationType.Days)
                {
                    lastClaimDate = lastClaimDate.Reset();
                }
                lastClaimDate.AddTicks(expire.duration * (expire.type == Reward.DurationTime.DurationType.Minutes ? TimeSpan.TicksPerMinute : TimeSpan.TicksPerDay));
                nextClaim = lastClaimDate.Ticks;
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
                reward.expire.duration > 0 &&
                IsTimeUp(reward.expire, record.lastClaimed)
            )
            {
                rewardsRecorded.Remove(key);
            }
        }

        private bool IsTimeUp(Reward.DurationTime duration, long lastClaimed)
        {
            if (duration.type == Reward.DurationTime.DurationType.Days)
            {
                var diff = new DateTime(lastClaimed).DiffDays(DateTime.Now);
                if (diff >= duration.duration)
                {
                    return true;
                }
            }
            else
            {
                var expire = duration.duration * TimeSpan.TicksPerMinute;
                if (lastClaimed + expire <= DateTime.Now.Ticks)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
