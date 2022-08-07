using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace GameFoundation.State
{
    public abstract class Controller : MonoBehaviour
    {
        protected Dictionary<string, Model> models = new Dictionary<string, Model>();

        public abstract void ChangeState(object ID);

        public T GetModel<T>() where T : Model
        {
            if (models.TryGetValue(typeof(T).Name, out Model model))
            {
                return model as T;
            }
            model = GetComponent<StateModel<T>>()?.Model;
            if (model == null)
            {
                // try get model from repository
                model = Repository.Instance.Get<T>();
            }
            if (model != null)
            {
                models.Add(typeof(T).Name, model);
            }
            return model != null ? model as T : null;
        }

        public void ClearModels()
        {
            models.Clear();
        }

        protected CompositeDisposable disposables = new CompositeDisposable();

        public CompositeDisposable Disposable => disposables;

        private void OnDestroy()
        {
            models.Clear();
            Disposable.Dispose();
        }
    }

    /// <summary>
    /// StateController with "St" is the type of the StateScript. And "Ids" is the type of ID to compare.
    /// </summary>
    public class StateController<St, IDs> : Controller where St : StateScript<IDs>
    {
        [SerializeField] protected St[] states;
        [System.NonSerialized] protected St current = null;

        public IDs currentState => current != null ? current.ID : default;

        private CancellationTokenSource cancelToken;

        private void Start()
        {
            foreach (var state in states)
            {
                Repository.Instance.ResolveDI(state);
            }
        }

        public override void ChangeState(object ID)
        {
            var state = states.FirstOrDefault(x => x.ID.Equals(ID));
            if (state)
            {
                ChangeState(state);
            }
            else
            {
                Debug.LogWarning("State not found: " + ID);
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

        protected virtual void OnStateChanged(St state)
        {
        }
    }
}