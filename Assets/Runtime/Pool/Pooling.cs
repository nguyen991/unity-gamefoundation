using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace GameFoundation.Pool
{
    public class Pooling : Utilities.SingletonBehaviour<Pooling>
    {
        [ValidateInput("ValidatePoolNodes", "Node ids are conflict")][SerializeField] private List<PoolNode> pools;

        public static readonly string MethodOnTake = "OnTakeFromPool";

        public static readonly string MethodOnReturn = "OnReturnToPool";

        private bool ValidatePoolNodes()
        {
            return pools.Select(p => p.id).Distinct().Count() == pools.Count;
        }

        public async UniTask Preload()
        {
            await UniTask.WhenAll(pools.Select(p => p.Reload(transform)));
        }

        public async UniTask<T> Take<T>(string id, Transform parent = null) where T : Component
        {
            var pool = pools.Find(p => p.id == id);
            if (pool == null)
            {
                return null;
            }
            var node = await pool.Take<T>();
            node.transform.parent = parent;
            node.gameObject.SetActive(true);
            node.SendMessage(MethodOnTake, SendMessageOptions.DontRequireReceiver);
            return node;
        }

        public async UniTask<Transform> Take(string id, Transform parent = null)
        {
            return await Take<Transform>(id, parent);
        }

        public void Return(string id, Transform node)
        {
            if (node == null)
            {
                Debug.LogWarning($"Pooler: return null object to {id.ToString()}");
                return;
            }

            var pool = pools.Find(p => p.id == id);
            if (pool != null)
            {
                node.SendMessage(MethodOnReturn, SendMessageOptions.DontRequireReceiver);
                node.gameObject.SetActive(false);
                node.parent = transform;
                pool.Return(node.gameObject);
            }
        }

        public void Return(string id, GameObject node)
        {
            Return(id, node?.transform);
        }
    }
}