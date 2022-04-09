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
        [ReadOnly] public bool enableAnalytics = false;
        [ReadOnly] public bool enableIap = false;
    }
}
