using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class Vector3Extension
    {
        public static Vector3 X(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        public static Vector3 Y(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 Z(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3 XY(this Vector3 v, float x, float y)
        {
            return new Vector3(x, y, v.z);
        }

        public static Vector3 XZ(this Vector3 v, float x, float z)
        {
            return new Vector3(x, v.y, z);
        }

        public static Vector3 YZ(this Vector3 v, float y, float z)
        {
            return new Vector3(v.x, y, z);
        }
    }
}