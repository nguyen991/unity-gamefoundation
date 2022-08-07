using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace GameFoundation.Mobile
{
    public interface IAdsHandler
    {
        void Init(AdController.AdConfig config, UnityAction onInitCompleted);
        void RequestBanner();
        void DestroyBanner();
        void RequestInterstitial();
        void ShowInterstitial(UnityAction<bool> callback = null);
        bool IsInterstitalAvailable();
        void DestroyInterstitial();
        void RequestReward();
        void ShowReward(UnityAction<bool> callback = null);
        bool IsRewardAvailable();
        bool IsBanner();
    }
}
