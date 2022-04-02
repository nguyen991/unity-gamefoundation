using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFoundation.State
{
    public class StateScript<T> : ScriptableObject
    {
        public T ID;

        public virtual void OnStateEnter(Controller controller) { }

        public virtual async UniTask OnStateEnterAsync(Controller controller)
        {
            await UniTask.Yield();
        }

        public virtual void OnStateExit(Controller controller) { }

        public virtual async UniTask OnStateExitAsync(Controller controller)
        {
            await UniTask.Yield();
        }

        public virtual void OnStateUpdate(Controller controller) { }
    }
}
