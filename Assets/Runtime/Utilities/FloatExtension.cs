using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class FloatExtension
    {
        public static bool FuzzyEqual(this float number, float other, float epsilon = 0.0000001f)
        {
            return number == other || Mathf.Abs(number - other) <= epsilon;
        }
    }
}