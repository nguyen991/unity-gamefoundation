using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class IntExtension
    {
        public static IEnumerable<int> Range(this int number, int start = 0)
        {
            var increase = start < number ? 1 : -1;
            for (int i = start; i < number; i += increase)
            {
                yield return i;
            }
        }

        public static IEnumerable<long> Range(this long number, int start = 0)
        {
            var increase = start < number ? 1 : -1;
            for (int i = start; i < number; i += increase)
            {
                yield return i;
            }
        }
    }
}