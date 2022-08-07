using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class StateModel<T> : DIMonoBehaviour where T : Model
    {
        public T Model { get; protected set; }

        public abstract void CreateModel();

        protected virtual void Awake()
        {
            CreateModel();
        }
    }
}
