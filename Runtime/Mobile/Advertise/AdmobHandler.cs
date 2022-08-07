#pragma warning disable CS0414

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using UniRx;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

#if GF_ADS
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif

namespace GameFoundation.Mobile
{
    class AdmobHandler : IAdsHandler
    {
        private AdController.AdConfig config;

#if GF_ADS
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        private RewardedInterstitialAd rewardedInterstitialAd;
#endif

        private UnityAction<bool> adResultCallback = null;
        private bool earnedReward = false;

        public void Init(AdController.AdConfig config, UnityAction onInitCompleted)
        {
            this.config = config;

#if UNITY_IOS
            if (config.requestIDFA)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif

#if GF_ADS
            // Configure TagForChildDirectedTreatment and test device IDs.
            List<string> deviceIds = new List<string>() { AdRequest.TestDeviceSimulator };
            deviceIds.AddRange(config.testDevices);
            RequestConfiguration requestConfiguration =
                new RequestConfiguration.Builder()
                .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
                .SetTestDeviceIds(deviceIds).build();

            // Initialize the Google Mobile Ads SDK.
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.SetRequestConfiguration(requestConfiguration);
            MobileAds.Initialize(status =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Debug.Log($"Admob initialized");

                    // log mediation
                    Dictionary<string, AdapterStatus> map = status.getAdapterStatusMap();
                    foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                    {
                        string className = keyValuePair.Key;
                        AdapterStatus status = keyValuePair.Value;
                        switch (status.InitializationState)
                        {
                            case AdapterState.NotReady:
                                Debug.Log("Adapter: " + className + " not ready.");
                                break;

                            case AdapterState.Ready:
                                Debug.Log("Adapter: " + className + " is initialized.");
                                break;
                        }
                    }

                    // init completed
                    onInitCompleted?.Invoke();
                });
            });
#else
            // init completed
            onInitCompleted?.Invoke();
#endif
        }

#if GF_ADS
        private AdRequest CreateAdRequest()
        {
            return new AdRequest.Builder()
                .Build();
        }
#endif

        #region Banner
        public void RequestBanner()
        {
#if GF_ADS
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = config.banner.android;
#elif UNITY_IPHONE
            string adUnitId = config.banner.ios;
#else
            string adUnitId = "unexpected_platform";
#endif
            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Create adaptive banner
            AdSize adaptiveSize =
                    AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);

            // Add Event Handlers
            bannerView.OnAdLoaded += (sender, args) => Debug.Log("[Ads] Banner ad loaded.");
            bannerView.OnAdFailedToLoad += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("[Ads] Banner ad failed to load: " + args.LoadAdError.GetMessage());
                BIAdFailed(adUnitId, AdController.AdType.Banner, args.LoadAdError.GetResponseInfo()?.GetMediationAdapterClassName(), args.LoadAdError.GetMessage());
            });
            bannerView.OnAdOpening += (sender, args) => Debug.Log("[Ads] Banner ad opened.");
            bannerView.OnAdClosed += (sender, args) => Debug.Log("[Ads] Banner ad closed.");
            bannerView.OnPaidEvent += (sender, value) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BIPaidEvent(adUnitId, AdController.AdType.Banner, bannerView.GetResponseInfo()?.GetMediationAdapterClassName(), value);
            });

            // Load a banner ad
            bannerView.LoadAd(CreateAdRequest());
#endif
        }

        public void DestroyBanner()
        {
#if GF_ADS
            if (bannerView != null)
            {
                bannerView.Destroy();
            }
#endif
        }

        public bool IsBanner()
        {
#if GF_ADS
            return bannerView != null;
#else
            return false;
#endif
        }
        #endregion

        #region Interstitial
        public void RequestInterstitial()
        {
#if GF_ADS
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = config.interstitial.android;
#elif UNITY_IPHONE
            string adUnitId = config.interstitial.ios;
#else
            string adUnitId = "unexpected_platform";
#endif

            // Clean up interstitial before using it
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
            }

            interstitialAd = new InterstitialAd(adUnitId);

            // Add Event Handlers
            interstitialAd.OnAdLoaded += (sender, args) => Debug.Log("[Ads] Interstitial ad loaded.");
            interstitialAd.OnAdFailedToLoad += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("[Ads] Interstitial ad failed to load: " + args.LoadAdError.GetMessage());
                BIAdFailed(adUnitId, AdController.AdType.Interstitial, args.LoadAdError.GetResponseInfo()?.GetMediationAdapterClassName(), args.LoadAdError.GetMessage());
                adResultCallback?.Invoke(false);
            });
            interstitialAd.OnAdOpening += (sender, args) => Debug.Log("[Ads] Interstitial ad opened.");
            interstitialAd.OnAdClosed += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                RequestInterstitial();
                adResultCallback?.Invoke(true);
            });
            interstitialAd.OnPaidEvent += (sender, value) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BIPaidEvent(adUnitId, AdController.AdType.Interstitial, interstitialAd.GetResponseInfo()?.GetMediationAdapterClassName(), value);
            });
            interstitialAd.OnAdDidRecordImpression += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BIRecordImpression(adUnitId, AdController.AdType.Interstitial, interstitialAd.GetResponseInfo()?.GetMediationAdapterClassName());
            });

            // Load an interstitial ad
            interstitialAd.LoadAd(CreateAdRequest());
#endif
        }

        public bool IsInterstitalAvailable()
        {
#if GF_ADS
            return interstitialAd != null && interstitialAd.IsLoaded(); ;
#else
            return false;
#endif
        }

        public void ShowInterstitial(UnityAction<bool> callback = null)
        {
#if GF_ADS
            if (IsInterstitalAvailable())
            {
                adResultCallback = callback;
                interstitialAd.Show();
            }
            else
            {
                callback?.Invoke(false);
            }
#else
            callback?.Invoke(false);
#endif
        }

        public void DestroyInterstitial()
        {
#if GF_ADS
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
            }
#endif
        }
        #endregion

        #region Reward
        public void RequestReward()
        {
#if GF_ADS
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = config.reward.android;
#elif UNITY_IPHONE
            string adUnitId = config.reward.ios;
#else
            string adUnitId = "unexpected_platform";
#endif

            // create new rewarded ad instance
            rewardedAd = new RewardedAd(adUnitId);

            // Add Event Handlers
            rewardedAd.OnAdLoaded += (sender, args) => Debug.Log("[Ads] Rewarded ad loaded.");
            rewardedAd.OnAdFailedToLoad += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("[Ads] Rewarded ad failed to load: " + args.LoadAdError.GetMessage());
                BIAdFailed(adUnitId, AdController.AdType.Reward, args.LoadAdError.GetResponseInfo()?.GetMediationAdapterClassName(), args.LoadAdError.GetMessage());
            });
            rewardedAd.OnAdOpening += (sender, args) => Debug.Log("[Ads] Rewarded ad opened.");
            rewardedAd.OnPaidEvent += (sender, value) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BIPaidEvent(adUnitId, AdController.AdType.Reward, rewardedAd.GetResponseInfo()?.GetMediationAdapterClassName(), value);
            });
            rewardedAd.OnAdDidRecordImpression += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BIRecordImpression(adUnitId, AdController.AdType.Reward, rewardedAd.GetResponseInfo()?.GetMediationAdapterClassName());
            });
            rewardedAd.OnAdFailedToShow += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                adResultCallback?.Invoke(false);
            });
            rewardedAd.OnAdClosed += (sender, args) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                RequestReward();
#if UNITY_EDITOR
                earnedReward = true;
#endif
                adResultCallback?.Invoke(earnedReward);
            });
            rewardedAd.OnUserEarnedReward += (sender, args) =>
            {
                earnedReward = true;
            };

            // Create empty ad request
            rewardedAd.LoadAd(CreateAdRequest());
#endif
        }

        public bool IsRewardAvailable()
        {
#if GF_ADS
            return rewardedAd != null && rewardedAd.IsLoaded();
#else
            return false;
#endif
        }

        public void ShowReward(UnityAction<bool> callback = null)
        {
#if GF_ADS
            if (IsRewardAvailable())
            {
                adResultCallback = callback;
                earnedReward = false;
                rewardedAd.Show();
            }
            else
            {
                callback?.Invoke(false);
            }
#else
            callback?.Invoke(false);
#endif
        }
        #endregion

        #region BI_EVENTS
#if GF_ADS
        private void BIPaidEvent(string adUnitId, string adFormat, string mediation, AdValueEventArgs adValue)
        {
            LogEvent.Log("bi_ad_value",
                new LogEvent.Parameter("ad_platform", "admob"),
                new LogEvent.Parameter("ad_source", string.IsNullOrEmpty(mediation) ? "UNKNOWN" : mediation),
                new LogEvent.Parameter("ad_platform_unit_id", adUnitId),
                new LogEvent.Parameter("ad_source_unit_id", "UNKNOWN"),
                new LogEvent.Parameter("ad_format", adFormat),
                new LogEvent.Parameter("ad_number", AdNumberToday(adFormat, false)),
                new LogEvent.Parameter("estimated_value", adValue.AdValue.Value),
                new LogEvent.Parameter("est_value_currency", adValue.AdValue.CurrencyCode),
                new LogEvent.Parameter("precision_type", (long)adValue.AdValue.Precision),
                new LogEvent.Parameter("est_value_usd", adValue.AdValue.Value)
            );
            Debug.Log($"[Ads] bi_ad_value: {adFormat} {adValue.AdValue.Value}{adValue.AdValue.CurrencyCode}");
        }

        private void BIRecordImpression(string adUnitId, string adFormat, string mediation)
        {
            LogEvent.Log("bi_ad_impression",
                new LogEvent.Parameter("ad_platform", "admob"),
                new LogEvent.Parameter("ad_source", string.IsNullOrEmpty(mediation) ? "UNKNOWN" : mediation),
                new LogEvent.Parameter("ad_platform_unit_id", adUnitId),
                new LogEvent.Parameter("ad_source_unit_id", "UNKNOWN"),
                new LogEvent.Parameter("ad_format", adFormat),
                new LogEvent.Parameter("ad_number", AdNumberToday(adFormat, true))
            );
            Debug.Log($"[Ads] bi_ad_impression: {adFormat}");
        }

        private void BIAdFailed(string adUnitId, string adFormat, string mediation, string message)
        {
            LogEvent.Log("bi_ad_request_failed",
               new LogEvent.Parameter("ad_platform", "admob"),
               new LogEvent.Parameter("ad_source", string.IsNullOrEmpty(mediation) ? "UNKNOWN" : mediation),
               new LogEvent.Parameter("ad_platform_unit_id", adUnitId),
               new LogEvent.Parameter("ad_source_unit_id", "UNKNOWN"),
               new LogEvent.Parameter("ad_format", adFormat),
               new LogEvent.Parameter("ad_number", AdNumberToday(adFormat, false)),
               new LogEvent.Parameter("error_message", message)
           );
        }

        private int AdNumberToday(string adFormat, bool increase)
        {
            var adNumber = PlayerPrefs.GetInt(adFormat, 0);
            if (!increase)
            {
                return adNumber;
            }

            adNumber += 1;

            var lastDateStr = PlayerPrefs.GetString($"{adFormat}_date");
            var now = DateTime.Now;
            if (now.ToString("d") != lastDateStr)
            {
                adNumber = 1;
            }

            PlayerPrefs.SetInt(adFormat, adNumber);
            PlayerPrefs.SetString($"{adFormat}_date", now.ToString("d"));

            return adNumber;
        }
#endif
        #endregion BI_EVENTS
    }
}
