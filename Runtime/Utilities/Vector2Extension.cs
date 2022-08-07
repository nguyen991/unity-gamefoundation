using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class Vector2Extension
    {
        public static Vector2 X(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }

        public static Vector2 Y(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }

        public static Vector2 XY(this Vector2 v, float x, float y)
        {
            return new Vector2(x, y);
        }
    }
}
