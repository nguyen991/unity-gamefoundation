using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFoundation.Utilities;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace GameFoundation.Economy
{
    public class InventoryManager
    {
        [System.Serializable]
        public class ItemInstance
        {
            public string key;
            public string id;
            public long amount;
            public Dictionary<string, object> data = new Dictionary<string, object>();
        }

        [System.Serializable]
        protected class ItemData
        {
            [System.NonSerialized] public Item item;
            public List<ItemInstance> instances;
        }

        private ItemCatalog catalog;

        [JsonProperty] private Dictionary<string, ItemData> items;

        [JsonIgnore] public UnityEvent<Item, long> onItemAdded = new UnityEvent<Item, long>();
        [JsonIgnore] public UnityEvent<Item, long> onItemRemoved = new UnityEvent<Item, long>();
        [JsonIgnore] public UnityEvent<Item, long> onItemChanged = new UnityEvent<Item, long>();

        public InventoryManager(ItemCatalog catalog)
        {
            this.catalog = catalog;
            this.items = new Dictionary<string, ItemData>();

            // init new items
            catalog.Items.ForEach(item =>
            {
                if (!items.ContainsKey(item.key))
                {
                    // add item data
                    items.Add(item.key, new ItemData()
                    {
                        item = item,
                        instances = new List<ItemInstance>()
                    });

                    // add init balance
                    if (item.initBalance > 0)
                    {
                        Create(item.key, item.initBalance);
                    }
                }
            });
        }

        public List<ItemInstance> Create(string key, long amount = 1)
        {
            var item = Find(key);
            if (item != null)
            {
                // check limit amount
                var total = TotalAmount(key);
                if ((item.persistent && (amount > 1 || total > 0)) ||
                    (item.maxBalance > 0 && total >= item.maxBalance))
                {
                    Debug.LogError($"Create item [{key}] amount [{amount}] failed, meet limit");
                    return null;
                }

                // limit amount
                if (item.maxBalance > 0 && total + amount > item.maxBalance)
                {
                    amount = item.maxBalance - total;
                }

                // genreate new item instance
                var newInstance = new List<ItemInstance>();
                if (item.stackable)
                {
                    // fill current item instance
                    if (item.itemPerStack > 0)
                    {
                        items[key].instances.ForEach(instance =>
                        {
                            if (instance.amount < item.itemPerStack)
                            {
                                var available = item.itemPerStack - instance.amount;
                                var addAmount = available > amount ? amount : available;
                                instance.amount += addAmount;
                                amount -= addAmount;
                            }
                        });
                    }

                    // geneate new stacks
                    if (amount > 0)
                    {
                        var stacks = amount / item.itemPerStack;
                        var remain = amount % item.itemPerStack;

                        // create stacks
                        if (stacks > 0)
                        {
                            newInstance.AddRange(stacks.Range().Select(_ => new ItemInstance() { key = key, id = GenrateUUID(), amount = item.itemPerStack }));
                        }

                        // create remain stack
                        if (remain > 0)
                        {
                            newInstance.Add(new ItemInstance() { key = key, id = GenrateUUID(), amount = remain });
                        }
                    }
                }
                else
                {
                    newInstance = amount.Range().Select(_ => new ItemInstance() { key = key, id = GenrateUUID(), amount = 1 }).ToList();
                }

                // create a new item
                items[key].instances.AddRange(newInstance);

                // delay one frame and invoke
                UniTask.DelayFrame(1).ContinueWith(() => 
                {
                    onItemAdded.Invoke(item, amount);
                    onItemChanged.Invoke(item, TotalAmount(key));
                });

                return newInstance;
            }
            return null;
        }

        public bool Remove(string key, long amount = 1)
        {
            if (items.TryGetValue(key, out ItemData data))
            {
                if (data.item.persistent || TotalAmount(key) < amount)
                {
                    return false;
                }

                // sub items
                var count = amount;
                data.instances.ForEach(instance =>
                {
                    if (count > 0)
                    {
                        if (instance.amount >= count)
                        {
                            instance.amount -= count;
                            count = 0;
                        }
                        else
                        {
                            count -= instance.amount;
                            instance.amount = 0;
                        }
                    }
                });

                // remove empty items
                data.instances.RemoveAll(instance => instance.amount == 0);
                onItemRemoved.Invoke(data.item, amount);
                onItemChanged.Invoke(data.item, TotalAmount(key));
                return true;
            }
            return false;
        }

        public bool RemoveById(string id, long amount = 1)
        {
            // find instance by id
            var instance = items.Values.SelectMany(item => item.instances).FirstOrDefault(instance => instance.id == id);
            if (instance != null && instance.amount >= amount)
            {
                instance.amount -= amount;

                // remove instance if empty
                if (instance.amount == 0)
                {
                    items[instance.key].instances.Remove(instance);
                }

                var item = Find(instance.key);
                onItemRemoved.Invoke(item, instance.amount);
                onItemChanged.Invoke(item, TotalAmount(item.key));
                return true;
            }
            return false;
        }

        public long TotalAmount(string key)
        {
            return items.Where(item => item.Key == key).Sum(item => item.Value.instances.Sum(i => i.amount)); ;
        }

        public Item Find(string key)
        {
            return catalog.Find(key);
        }

        public IEnumerable<ItemInstance> Query(
            ICollection<string> keys = null,
            ICollection<string> tags = null,
            ICollection<string> properties = null,
            ICollection<string> data = null
        )
        {
            return items
                .Where(item =>
                    (keys == null || keys.Contains(item.Key)) &&
                    (tags == null || tags.Any(tag => item.Value.item.IsHaveTag(tag))) &&
                    (properties == null || properties.Any(prop => item.Value.item.IsHaveProperty(prop))))
                .SelectMany(item => item.Value.instances)
                .Where(item => (data == null || data.All(prop => item.data.ContainsKey(prop))));
        }

        private string GenrateUUID()
        {
            return System.Guid.NewGuid().ToString();
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // popuplate item data
            foreach (var item in items)
            {
                item.Value.item = Find(item.Key);
            }

            // remove items not in catalog
            var keys = items.Keys.Where(key => catalog.Find(key) == null).ToList();
            keys.ForEach(key => items.Remove(key));
        }
    }
}
