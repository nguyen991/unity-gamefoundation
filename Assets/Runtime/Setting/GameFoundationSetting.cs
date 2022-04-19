using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace GameFoundation
{
    [CreateAssetMenu(fileName = "GameFoundationSetting", menuName = "Game Foundation/Setting")]
    public class GameFoundationSetting : ScriptableObject
    {
        [ReadOnly] public bool enableAds = false;
        [ReadOnly] public bool enableFirebase = false;
        [ReadOnly] public bool enableIap = false;
        [ReadOnly] public bool enableDotween = false;

        public Data.DataLayer.DataLayerType dataLayerType = Data.DataLayer.DataLayerType.Persistence;
    }
}
