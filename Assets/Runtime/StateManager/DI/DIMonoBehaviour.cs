using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFoundation.State
{
    public class DIMonoBehaviour : MonoBehaviour
    {
        protected void Start()
        {
            Repository.Instance.ResolveDI(this);
            OnStart();
            OnStartAsync().Forget();
        }

        protected virtual void OnStart() { }

        protected virtual async UniTaskVoid OnStartAsync() { await UniTask.CompletedTask; }
    }
}