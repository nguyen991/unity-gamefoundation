using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class StateScript<T> : ScriptableObject
    {
        public abstract T ID { get; }

        public virtual void OnStateEnter(Controller controller) { }

        public virtual async UniTask OnStateEnterAsync(Controller controller) { await UniTask.CompletedTask; }

        public virtual void OnStateExit(Controller controller) { }

        public virtual async UniTask OnStateExitAsync(Controller controller) { await UniTask.CompletedTask; }

        public virtual void OnStateUpdate(Controller controller) { }
    }
}
