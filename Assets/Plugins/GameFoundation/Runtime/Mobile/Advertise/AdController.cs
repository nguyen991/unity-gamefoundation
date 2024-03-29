using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation.Mobile
{
    public class AdController : Utilities.SingletonBehaviour<AdController>
    {
        [System.Serializable]
        public struct AdId
        {
            public string android;
            public string ios;
        }

        [System.Serializable]
        public struct AdConfig
        {
            public bool requestIDFA;
            public bool autoLoadBanner;
            public List<string> testDevices;
            public AdId banner;
            public AdId interstitial;
            public AdId reward;
        }

        public static class AdType
        {
            public const string Banner = "banner";
            public const string Interstitial = "interstitial";
            public const string Reward = "rewarded";
        }

        public AdConfig config;

        public bool useAdFakeOnEditor = true;

        [ShowIf("useAdFakeOnEditor")]
        public bool adFakeAvailble = true;

        public bool Initialized { get; private set; } = false;

        private IAdsHandler handler = null;

        public void Init()
        {
            if (Initialized)
            {
                return;
            }

#if UNITY_EDITOR || UNITY_WEBGL
            if (useAdFakeOnEditor)
            {
                handler = new AdFakeHandler(adFakeAvailble);
            }
            else
            {
                handler = new AdmobHandler();
            }
#else
            handler = new AdmobHandler();
#endif
            handler.Init(config, () =>
            {
                // request ads
                if (config.autoLoadBanner)
                {
                    handler.RequestBanner();
                }
                RequestInterstitial();
                RequestReward();
                Initialized = true;
            });
        }

        public void RequestBanner()
        {
            handler.RequestBanner();
        }

        public void DestroyBanner()
        {
            handler.DestroyBanner();
        }

        public bool IsBanner()
        {
            return handler.IsBanner();
        }

        public void RequestInterstitial()
        {
            handler.RequestInterstitial();
        }

        public void ShowInterstitial(UnityAction<bool> callback = null)
        {
            handler.ShowInterstitial(callback);
        }

        public async UniTask<bool> ShowInterstitialAsync()
        {
            var task = new UniTaskCompletionSource<bool>();
            ShowInterstitial((success) => task.TrySetResult(success));
            var result = await task.Task;
            // await UniTask.SwitchToMainThread();
            return result;
        }

        public bool IsInterstitalAvailable()
        {
            return handler.IsInterstitalAvailable();
        }

        public void DestroyInterstitial()
        {
            handler.DestroyInterstitial();
        }

        public void RequestReward()
        {
            handler.RequestReward();
        }

        public void ShowReward(UnityAction<bool> callback = null)
        {
            handler.ShowReward(callback);
        }

        public async UniTask<bool> ShowRewardAsync()
        {
            var task = new UniTaskCompletionSource<bool>();
            ShowReward((success) => task.TrySetResult(success));
            var result = await task.Task;
            // await UniTask.SwitchToMainThread();
            return result;
        }

        public bool IsRewardAvailable()
        {
            return handler.IsRewardAvailable();
        }
    }
}
