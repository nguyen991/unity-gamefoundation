using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Data
{
    public class MemoryDataLayer : IDataLayer
    {
        public bool Delete(string name)
        {
            return true;
        }

        public bool Exists(string name)
        {
            return false;
        }

        public bool Load(string name, object target)
        {
            return true;
        }

        public bool Save(string name, object target)
        {
            return true;
        }
    }
}
