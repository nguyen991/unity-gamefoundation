using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class StateModel<T> : MonoBehaviour where T : Model.GFModel
    {
        public T Model { get; protected set; }

        public abstract T CreateModel();
    }
}
