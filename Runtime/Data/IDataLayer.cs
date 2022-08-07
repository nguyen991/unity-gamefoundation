using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Data
{
    public interface IDataLayer
    {
        public bool Save(string name, object target);

        public bool Load(string name, object target);

        public bool Delete(string name);

        public bool Exists(string name);
    }
}
