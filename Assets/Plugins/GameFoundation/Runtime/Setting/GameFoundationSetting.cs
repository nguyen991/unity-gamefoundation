using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameFoundation
{
    [CreateAssetMenu(fileName = "GameFoundationSetting", menuName = "Game Foundation/Setting")]
    public class GameFoundationSetting : ScriptableObject
    {
        [ReadOnly] public bool enableAds = false;
        [ReadOnly] public bool enableFirebase = false;
        [ReadOnly] public bool enableIap = false;
        [ReadOnly] public bool enableDotween = false;

        public int startSceneIndex = 0;

        public Vector2Int designResolution = new Vector2Int(1024, 1366);
        public int fps = 60;
        public bool multiTouch = false;
        public List<NotificationSchedule> notifications;
        public bool autoScheduleNotification = false;
        public Mobile.AdController.AdConfig adConfig;
        public bool useAdFakeOnEditor = true;
        public bool adFakeAvailable = true;

        public Economy.EconomyData economyData;
        public Data.DataLayer.DataLayerType dataLayerType = Data.DataLayer.DataLayerType.Persistence;
        public bool saveOnLostFocus = true;

        public List<AssetLabelReference> spriteAtlasLabels;

        [System.Serializable]
        public class NotificationSchedule
        {
            public string Title;
            public string Body;
            public int Hour;
            public int Minute;
        }
    }
}
