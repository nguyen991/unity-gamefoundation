using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFoundation.Addressable;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation
{
    public class GameFoudationInitializer : MonoBehaviour
    {
        [Tooltip("If null, if will try to load from resources folder at start")]
        public GameFoundationSetting setting;

        [Space(10)]
        public UnityEvent<bool> onInitialized;

        public bool Initialized { get; private set; } = false;

        async UniTask Start()
        {
            if (Initialized)
            {
                return;
            }

            await UniTask.NextFrame();

            var tasks = new List<UniTask>();

            // load setting
            if (setting == null)
            {
                setting = await Resources.LoadAsync<GameFoundationSetting>("GameFoundationSetting") as GameFoundationSetting;
            }

            // initialize sprite loader
            if (SpriteLoader.Instance != null)
            {
                tasks.Add(SpriteLoader.Instance.Init());
            }

            // initialize admob
            if (setting.enableAds && Mobile.AdController.Instance != null)
            {
                Mobile.AdController.Instance.Init();
            }

            // initialize economy
            if (Economy.EconomyManager.Instance != null)
            {
                Economy.EconomyManager.Instance.Init();
            }

            // wait for all task
            await UniTask.WhenAll(tasks);

            Initialized = true;
            onInitialized.Invoke(true);
        }
    }
}
