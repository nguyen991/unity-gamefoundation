using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace GameFoundation.Economy
{
    [System.Serializable]
    public class Property
    {
        public enum ValueType
        {
            Int,
            Float,
            String,
            Bool,
            Prefab,
            Sprite,
        }

        [SerializeField]
        private ValueType type = ValueType.String;

        [SerializeField]
        [ShowIf("type", ValueType.Int)]
        [AllowNesting]
        private int intValue;

        [SerializeField]
        [ShowIf("type", ValueType.Float)]
        [AllowNesting]
        private float floatValue;

        [SerializeField]
        [ShowIf("type", ValueType.String)]
        [AllowNesting]
        private string stringValue;

        [SerializeField]
        [ShowIf("type", ValueType.Bool)]
        [AllowNesting]
        private bool boolValue;

        [SerializeField]
        [ShowIf("type", ValueType.Prefab)]
        [AllowNesting]
        private GameObject prefabValue;

        [SerializeField]
        [ShowIf("type", ValueType.Sprite)]
        [AllowNesting]
        private Sprite spriteValue;

        public ValueType Type
        {
            get { return type; }
        }

        public string GetString()
        {
            return stringValue;
        }

        public int GetInt()
        {
            return intValue;
        }

        public float GetFloat()
        {
            return floatValue;
        }

        public bool GetBool()
        {
            return boolValue;
        }

        public GameObject GetPrefab()
        {
            return prefabValue;
        }

        public Sprite GetSprite()
        {
            return spriteValue;
        }
    }
}