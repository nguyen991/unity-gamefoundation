using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class CollectionExtension
    {
        public static T Sample<T>(this ICollection<T> collection)
        {
            var index = Random.Range(0, collection.Count);
            return collection.ElementAt(index);
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static IEnumerable<T> SampleSize<T>(this IEnumerable<T> list, int size)
        {
            if (size > list.Count())
            {
                Debug.LogError("SampleSize: size is bigger than list size");
                return list;
            }
            return list.ToList().Shuffle().Take(size);
        }
    }
}