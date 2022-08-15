using UnityEngine;

namespace GameFoundation.Pool
{
    public class PoolingRef : MonoBehaviour
    {
        public string poolId;

        public void ReturnPool()
        {
            Pooling.Instance.Return(poolId, transform);
        }
    }
}