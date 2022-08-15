using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFoundation.Data
{
    public class DataLayer : Utilities.SingletonBehaviour<DataLayer>
    {
        public enum DataLayerType
        {
            None,
            Persistence,
            PlayerPref,
        }

        public IDataLayer Layer { get; private set; } = null;

        public bool Initialized { get; private set; } = false;

        public void Init(DataLayerType type)
        {
            if (Initialized)
            {
                return;
            }
            Initialized = true;

#if !UNITY_EDITOR && UNITY_WEBGL
            type = DataLayerType.PlayerPref;
            Debug.Log("WebGL platform, use PlayerPref data layer");
#endif

            switch (type)
            {
                case DataLayerType.Persistence:
                    Layer = new PersistenceDataLayer();
                    break;
                case DataLayerType.PlayerPref:
                    Layer = new PlayerPrefDataLayer();
                    break;
                default:
                    Layer = new MemoryDataLayer();
                    break;
            }
        }
    }
}
