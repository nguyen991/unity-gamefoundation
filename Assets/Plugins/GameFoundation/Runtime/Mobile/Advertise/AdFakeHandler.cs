using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameFoundation.Mobile
{
    class AdFakeHandler : IAdsHandler
    {
        public bool fakeSuccess = true;

        public AdFakeHandler(bool fakeSuccess)
        {
            this.fakeSuccess = fakeSuccess;
        }

        public void DestroyBanner()
        {
        }

        public void DestroyInterstitial()
        {
        }

        public void Init(AdController.AdConfig config, UnityAction onInitCompleted)
        {
            Debug.Log($"AdFake initialized");
            onInitCompleted?.Invoke();
        }

        public bool IsInterstitalAvailable()
        {
            return true;
        }

        public bool IsRewardAvailable()
        {
            return true;
        }

        public void RequestBanner()
        {
        }

        public bool IsBanner()
        {
            return true;
        }

        public void RequestInterstitial()
        {
        }

        public void RequestReward()
        {
        }

        public void ShowInterstitial(UnityAction<bool> callback = null)
        {
            callback?.Invoke(fakeSuccess);
        }

        public void ShowReward(UnityAction<bool> callback = null)
        {
            callback?.Invoke(fakeSuccess);
        }
    }
}
