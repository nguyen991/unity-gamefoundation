using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFoundation.Addressable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace GameFoundation
{
    public class GameFoundationInitializer : MonoBehaviour
    {
        [Tooltip("If null, if will try to load from resources folder at start")]
        public GameFoundationSetting setting;

        [Header("Data Layer")]
        public Data.DataLayer.DataLayerType dataLayerType = Data.DataLayer.DataLayerType.Persistence;

        [Header("Economy")]
        public Economy.EconomyData economyData;

        [Header("Advertise")]
        public Mobile.AdController.AdConfig adConfig;
        public bool useAdFakeOnEditor = true;

        [Space(10)]
        public List<AssetLabelReference> spriteAtlasLabels;

        [Space(10)]
        [Header("Callback")]
        public UnityEvent<bool> onInitialized;

        public bool Initialized { get; private set; } = false;

        async UniTask Start()
        {
            if (Initialized)
            {
                return;
            }
            await UniTask.NextFrame();

            // load setting
            if (setting == null)
            {
                setting = await Resources.LoadAsync<GameFoundationSetting>("GameFoundationSetting") as GameFoundationSetting;
            }

            // wait for init data layer completed
            var dataLayer = Data.DataLayer.Instance;
            dataLayer.Init(dataLayerType);

            // init model Repository
            Model.Repository.Instance.DataLayer = dataLayer.Layer;

            var tasks = new List<UniTask>();

            // initialize sprite loader
            var spriteLoader = SpriteLoader.Instance;
            spriteLoader.spriteAtlasLabels = spriteAtlasLabels;
            tasks.Add(spriteLoader.Init());

            // initialize admob
            if (setting.enableAds)
            {
                var adController = Mobile.AdController.Instance;
                adController.config = adConfig;
                adController.useAdFakeOnEditor = useAdFakeOnEditor;
                adController.Init();
            }

            // init firebase
            if (setting.enableFirebase)
            {
                Mobile.FirebaseInstance.Init();
            }

            // initialize economy
            if (economyData != null)
            {
                Economy.EconomyManager.Instance.Init(dataLayer.Layer, economyData);
            }

            // wait for all task
            await UniTask.WhenAll(tasks);

            Initialized = true;
            onInitialized.Invoke(true);
        }
    }
}
