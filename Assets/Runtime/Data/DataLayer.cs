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

        public IDataLayer Layer { get; private set; }

        public bool Initialized { get; private set; } = false;

        public void Init(DataLayerType type)
        {
            if (Initialized)
            {
                return;
            }
            Initialized = true;

            switch (type)
            {
                case DataLayerType.Persistence:
                    Layer = new PersistenceDataLayer();
                    break;
                case DataLayerType.PlayerPref:
                    break;
                default:
                    break;
            }
        }
    }
}
