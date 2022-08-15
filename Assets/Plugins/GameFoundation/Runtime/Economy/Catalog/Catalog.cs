using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFoundation.Economy
{
    public class CatalogItem : ScriptableObject
    {
        [Header("Basic")]
        public string key;
        public string display;
        public List<string> tags;
        public GenericDictionary<string, Property> properties;

        public bool IsHaveTag(string tag)
        {
            return tags.Contains(tag);
        }

        public bool IsHaveProperty(string prop)
        {
            return properties.ContainsKey(prop);
        }
    }

    public class Catalog<T> where T : CatalogItem
    {
        [SerializeField] protected List<T> items = new List<T>();

        public bool IsHaveKey(string key)
        {
            return Items.Find(item => item.key == key) != null;
        }

        public T Find(string key)
        {
            return Items.Find(item => item.key == key);
        }

        public List<T> Items => items;
    }
}
