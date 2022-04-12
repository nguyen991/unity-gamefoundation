using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx.Model;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class Controller : MonoBehaviour
    {
        public abstract void ChangeState(object ID);

        public T GetModel<T>() where T : UniRxModel
        {
            return GetComponent<StateModel<T>>().Model;
        }
    }

    /// <summary>
    /// StateController with "St" is the type of the StateScript. And "Ids" is the type of ID to compare.
    /// </summary>
    public class StateController<St, IDs> : Controller where St : StateScript<IDs>
    {
        [SerializeField] protected St[] states;

        [System.NonSerialized] protected St current = null;

        public override void ChangeState(object ID)
        {
            var state = states.FirstOrDefault(x => x.ID.Equals(ID));
            if (state)
            {
                ChangeState(state);
            }
            else
            {
                Debug.LogError("State not found: " + ID);
            }
        }

        public void ChangeState(St state)
        {
            if (state == current)
            {
                return;
            }
            ChangeStateAsync(state).Forget();
        }

        protected async UniTaskVoid ChangeStateAsync(St state)
        {
            // cancellation token
            var token = this.GetCancellationTokenOnDestroy();

            // check emit event state exit
            if (current != null)
            {
                // normal method
                current.OnStateExit(this);

                // async method
                await UniTask
                    .Create(async () => await current.OnStateExitAsync(this))
                    .AttachExternalCancellation(token);
            }

            // wait for end of frame
            await UniTask.WaitForEndOfFrame(this, token);
            current = state;

            // normal method
            state.OnStateEnter(this);

            // async method
            await UniTask
                .Create(async () => await state.OnStateEnterAsync(this))
                .AttachExternalCancellation(token);

            // emit new state
            OnStateChanged(current);
        }

        protected void Update()
        {
            if (current)
            {
                current.OnStateUpdate(this);
            }
        }

        protected virtual void OnStateChanged(St state)
        {
        }
    }
}