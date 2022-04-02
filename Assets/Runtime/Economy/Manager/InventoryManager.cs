using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Economy
{
    public class InventoryManager
    {
        protected class ItemInstance
        {
            public string id;
            public long amount;
            public GenericDictionary<string, Property> properties;
        }

        [System.Serializable]
        protected class ItemData
        {
            [System.NonSerialized] public Item item;
            public string key;
            public List<ItemInstance> instances;
        }

        private ItemCatalog catalog;

        private Dictionary<string, ItemData> items;

        public InventoryManager(ItemCatalog catalog)
        {
            this.catalog = catalog;
            this.items = new Dictionary<string, ItemData>();

            //TODO: load currencies data from persistent storage

            // init new items
            catalog.items.ForEach(item =>
            {
                if (!items.ContainsKey(item.key))
                {
                    items.Add(item.key, new ItemData()
                    {
                        item = item,
                        key = item.key,
                        instances = new List<ItemInstance>()
                    });

                    // add init balance
                    if (item.initBalance > 0)
                    {

                    }
                }
            });
        }

        public void Create(string key, long amount = 1)
        {
            var item = Find(key);
            if (item != null)
            {
                // check limit amount
                var total = TotalAmount(key);
                if ((item.persistent && total > 0) ||
                    (item.maxBalance > 0 && total >= item.maxBalance))
                {
                    return;
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
                        var itemPerStack = amount / item.itemPerStack;
                        var remain = amount % item.itemPerStack;
                        if (itemPerStack > 0)
                        {
                            newInstance = new int[amount].Select(i => new ItemInstance() { id = GenrateUUID(), amount = (long)itemPerStack }).ToList();
                        }

                        // generate remain stack
                        if (remain > 0)
                        {
                            newInstance.Add(new ItemInstance() { id = GenrateUUID(), amount = (long)itemPerStack });
                        }
                    }
                }
                else
                {
                    newInstance = new int[amount].Select(i => new ItemInstance() { id = GenrateUUID(), amount = (long)1 }).ToList();
                }

                // create a new item
                items[key].instances.AddRange(newInstance);
            }
        }

        public bool Remove(string id)
        {
            if (items.TryGetValue(id, out ItemData data))
            {
                if (data.item.persistent)
                {
                    return false;
                }

                // remove item by id
                items.Remove(id);

                return true;
            }
            return false;
        }

        public void RemoveRange(string key, int amount)
        {
            var ids = items.Where(x => x.Value.item.key == key).Select(x => x.Key).Take(amount).ToList();
            foreach (var id in ids)
            {
                Remove(id);
            }
        }

        public void RemoveAll(string key)
        {
            var ids = items.Where(x => x.Value.item.key == key).Select(x => x.Key).ToArray();
            foreach (var id in ids)
            {
                Remove(id);
            }
        }

        public long TotalAmount(string key)
        {
            return items.Where(item => item.Key == key).Sum(item => item.Value.instances.Sum(i => i.amount)); ;
        }

        public Item Find(string key)
        {
            return catalog.Find(key);
        }

        private string GenrateUUID()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}
