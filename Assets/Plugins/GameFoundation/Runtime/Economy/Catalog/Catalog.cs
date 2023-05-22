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
        public List<string> tags = new List<string>();
        public GenericDictionary<string, Property> properties = new GenericDictionary<string, Property>();

        public bool IsHaveTag(string tag)
        {
            return tags != null && tags.Contains(tag);
        }

        public bool IsHaveProperty(string key)
        {
            return properties != null && properties.ContainsKey(key);
        }

        public bool TryGetProperty(string key, out Property prop)
        {
            return properties.TryGetValue(key, out prop);
        }

        public Property GetProperty(string key)
        {
            return properties.ContainsKey(key) ? properties[key] : null;
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
