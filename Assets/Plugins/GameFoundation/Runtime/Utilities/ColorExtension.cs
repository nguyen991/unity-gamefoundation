using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class ColorExtension
    {
        public static Color A(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color A(this Color color, int alpha)
        {
            return new Color(color.r, color.g, color.b, alpha / 255f);
        }
    }
}