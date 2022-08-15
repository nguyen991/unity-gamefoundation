using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace GameFoundation.State
{
    public class StateModel<T> : Model where T : System.Enum
    {
        public ReactiveProperty<T> State = new ReactiveProperty<T>();
    }
}
