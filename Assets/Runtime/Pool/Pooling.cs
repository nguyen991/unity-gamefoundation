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
        public static readonly string MethodOnTake = "OnTakeFromPool";
        public static readonly string MethodOnReturn = "OnReturnToPool";
        
        [SerializeField] private GenericDictionary<string, PoolNode> pools = new GenericDictionary<string, PoolNode>();        

        public async UniTask Reload()
        {
            await UniTask.WhenAll(pools.Select(p => p.Value.Reload(transform)));
        }

        public async UniTask<T> Take<T>(string id, Transform parent = null) where T : Component
        {
            if (pools.TryGetValue(id, out var pool))
            {
                var node = await pool.Take<T>();
                node.transform.SetParent(parent, false);
                node.gameObject.SetActive(true);
                node.SendMessage(MethodOnTake, SendMessageOptions.DontRequireReceiver);
                return node;
            }
            return null;
        }

        public async UniTask<Transform> Take(string id, Transform parent = null)
        {
            return await Take<Transform>(id, parent);
        }

        public void Return(string id, Transform node)
        {
            if (node == null)
            {
                Debug.LogWarning($"Pooler: return null object for {id}");
                return;
            }

            if (pools.TryGetValue(id, out var pool))
            {
                node.SendMessage(MethodOnReturn, SendMessageOptions.DontRequireReceiver);
                node.gameObject.SetActive(false);
                node.SetParent(transform);
                pool.Return(node.gameObject);
            }                
        }

        public void Return(string id, GameObject node)
        {
            Return(id, node?.transform);
        }

        public async UniTask AddPoolNode(string id, GameObject prefab, int size)
        {
            if (pools.ContainsKey(id))
            {
                Debug.LogWarning($"Pooler: {id} already exists");
            }
            else
            {
                var pool = new PoolNode() { id = id, prefab = prefab, size = size };
                pools.Add(id, pool);
                await pool.Reload(transform);
            }
        }
    }
}