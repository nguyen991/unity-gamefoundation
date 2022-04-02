using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameFoundation.Pool
{
    [System.Serializable]
    class PoolNode
    {
        public string id;

        public GameObject prefab = null;

        public AssetReference asset = null;

        [Min(1)]
        public int size;

        private List<GameObject> pooled = null;

        public async UniTask Reload(Transform root)
        {
            // init list pooled
            if (pooled == null)
            {
                pooled = new List<GameObject>();
            }

            // calculate how many we need to add
            var length = size - pooled.Count;
            if (length > 0)
            {
                var arr = await UniTask.WhenAll(new int[size].Select(i => Create()));
                foreach (var go in arr)
                {
                    go.SetActive(false);
                    go.transform.parent = root;
                }
                pooled.AddRange(arr);
            }
        }

        public async UniTask<T> Take<T>() where T : Component
        {
            if (pooled.Count == 0)
            {
                // create new instance if needed
                var go = await Create();
                return go.GetComponent<T>();
            }
            else
            {
                // get pooled instance
                var instance = pooled[0].GetComponent<T>();
                pooled.RemoveAt(0);
                return instance;
            }
        }

        public async UniTask<GameObject> Create()
        {
            if (prefab != null)
            {
                return GameObject.Instantiate(prefab);
            }
            if (asset != null)
            {
                return await asset.InstantiateAsync().ToUniTask();
            }
            return null;
        }

        public void Return(GameObject t)
        {
            pooled.Add(t);
        }
    }
}