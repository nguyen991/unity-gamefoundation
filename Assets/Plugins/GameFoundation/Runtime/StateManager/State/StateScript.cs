using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class StateScript<M, T> : ScriptableObject where M : StateModel<T> where T: System.Enum
    {
        public abstract T ID { get; }

        public virtual void OnStateEnter(Controller<M, T> controller) { }

        public virtual async UniTask OnStateEnterAsync(Controller<M, T> controller) { await UniTask.CompletedTask; }

        public virtual void OnStateExit(Controller<M, T> controller) { }

        public virtual async UniTask OnStateExitAsync(Controller<M, T> controller) { await UniTask.CompletedTask; }

        public virtual void OnStateUpdate(Controller<M, T> controller) { }
    }
}
