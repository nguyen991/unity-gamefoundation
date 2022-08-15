using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class Controller<M, T> : MonoBehaviour where M : StateModel<T> where T: System.Enum
    {
        public abstract void ChangeState(T stateId);

        public M Model { get; set; }

        protected CompositeDisposable disposables = new CompositeDisposable();

        public CompositeDisposable Disposable => disposables;

        private void OnDestroy()
        {
            Disposable.Dispose();
        }
    }

    /// <summary>
    /// StateController
    /// S: StateScript Type
    /// M: StateModel Type
    /// T: StateID Type
    /// </summary>
    public class StateController<S, M, T> : Controller<M, T> where S : StateScript<M, T> where M : StateModel<T> where T: System.Enum 
    {
        [SerializeField] protected S[] states;
        [System.NonSerialized] protected S current = null;

        public T currentState => current != null ? current.ID : default;

        private CancellationTokenSource cancelToken;

        private void Start()
        {
            foreach (var state in states)
            {
                Repository.Instance.ResolveDI(state);
            }
        }

        public override void ChangeState(T stateId)
        {
            var state = states.FirstOrDefault(x => x.ID.Equals(stateId));
            if (state)
            {
                ChangeState(state);
            }
            else
            {
                Debug.LogWarning("State not found: " + stateId);
            }
        }

        private void OnDisable()
        {
            cancelToken.Cancel();
        }

        private void OnEnable()
        {
            cancelToken = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
        }

        public void ChangeState(S state)
        {
            if (state == current)
            {
                return;
            }
            ChangeStateAsync(state).Forget();
        }

        protected async UniTaskVoid ChangeStateAsync(S state)
        {
            // check emit event state exit
            if (current != null)
            {
                // clear observables
                Disposable.Clear();

                // normal method
                current.OnStateExit(this);

                // async method
                await UniTask
                    .Create(async () => await current.OnStateExitAsync(this))
                    .AttachExternalCancellation(cancelToken.Token);
            }

            // wait for end of frame
            await UniTask.WaitForEndOfFrame(this, cancelToken.Token);
            current = state;

            // normal method
            state.OnStateEnter(this);

            // async method
            await UniTask
                .Create(async () => await state.OnStateEnterAsync(this))
                .AttachExternalCancellation(cancelToken.Token);

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

        protected virtual void OnStateChanged(S state)
        {
            Model.State.Value = state.ID;
        }
    }
}