using System.Collections;
using System.Collections.Generic;
using UniRx.Model;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class StateModel<T> : MonoBehaviour where T : BaseModel
    {
        public T Model { get; protected set; }

        public abstract T CreateModel();
    }
}
