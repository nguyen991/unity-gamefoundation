using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFoundation.Addressable;
using GameFoundation.State;
using GameFoundation.Utilities;
using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using NaughtyAttributes;

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace GameFoundation
{
    public class GameFoundationInitializer : SingletonBehaviour<GameFoundationInitializer>
    {
        [Tooltip("If null, if will try to load from resources folder at start")]
        public GameFoundationSetting setting;

        [Header("Unity Services")]
        public bool enableUnityServices = false;
        [ShowIf("enableUnityServices")]
        public string environment = "production";

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

            DontDestroyOnLoad(gameObject);
            MobileOptimize().Forget();
            InitNotification();

            await UniTask.NextFrame();

            // init game service
            if (enableUnityServices)
            {
                var options = new InitializationOptions().SetEnvironmentName(environment);
                await UnityServices.InitializeAsync(options);
            }

            // load setting
            if (setting == null)
            {
                setting = await Resources.LoadAsync<GameFoundationSetting>("GameFoundationSetting") as GameFoundationSetting;
            }

            // wait for init data layer completed
            var dataLayer = Data.DataLayer.Instance;
            dataLayer.Init(setting.dataLayerType);

            // init model Repository
            State.Repository.Instance.DataLayer = dataLayer.Layer;

            var tasks = new List<UniTask>();

            // initialize sprite loader
            var spriteLoader = SpriteLoader.Instance;
            spriteLoader.spriteAtlasLabels = setting.spriteAtlasLabels;
            tasks.Add(spriteLoader.Init());

            // initialize admob
            if (setting.enableAds)
            {
                var adController = Mobile.AdController.Instance;
                adController.config = setting.adConfig;
                adController.useAdFakeOnEditor = setting.useAdFakeOnEditor;
                adController.adFakeAvailble = setting.adFakeAvailable;
                adController.Init();
            }

            // init firebase
            if (setting.enableFirebase)
            {
                Mobile.FirebaseInstance.Init();
            }

            // initialize economy
            if (setting.economyData != null)
            {
                Economy.EconomyManager.Instance.Init(dataLayer.Layer, setting.economyData);
                Economy.EconomyManager.Instance.Load();
            }

            // wait for all task
            await UniTask.WhenAll(tasks);

            Initialized = true;
            onInitialized.Invoke(true);
        }

        private async UniTaskVoid MobileOptimize()
        {
            Input.multiTouchEnabled = setting.multiTouch;
            Application.targetFrameRate = setting.fps;
#if UNITY_ANDROID || UNITY_IOS
            var rate = setting.designResolution.y / (float)Screen.currentResolution.height;
            var res = new Vector2(Screen.currentResolution.width * rate, Screen.currentResolution.height * rate);
            var fullScreen = Application.platform == RuntimePlatform.WebGLPlayer ? false : true;
            Debug.Log($"Resolution {Screen.currentResolution.width},{Screen.currentResolution.height}");

            // change resolution
            Screen.SetResolution((int)res.x, (int)res.y, fullScreen);
            await UniTask.NextFrame();
            Debug.Log($"Change to resolution {Screen.currentResolution.width},{Screen.currentResolution.height}");
#else
            await UniTask.CompletedTask;
#endif

        }

        private void InitNotification()
        {
#if UNITY_ANDROID || UNITY_IOS
            var channel = new Mobile.Notification.GameNotificationChannel("default", "Default", "Default Channel");
            Mobile.Notification.GameNotificationsManager.Instance.Initialize(channel);
            if (setting.autoScheduleNotification)
            {
                ScheduleNotifications().Forget();
            }
#endif
        }

        public async UniTask ScheduleNotifications(bool cancelAll = true)
        {
            Debug.Log("ScheduleNotifications");
#if UNITY_IOS
            await iOSNotificationRequest();
#else
            await UniTask.CompletedTask;
#endif

#if UNITY_ANDROID || UNITY_IOS
            if (cancelAll)
            {
                Mobile.Notification.GameNotificationsManager.Instance.CancelAllNotifications();
            }

            var manager = Mobile.Notification.GameNotificationsManager.Instance;
            foreach (var notify in setting.notifications)
            {
                var deliver = DateTime.Now.ToLocalTime().ChangeTime(notify.Hour, notify.Minute);
                if (deliver < DateTime.Now.ToLocalTime())
                {
                    deliver = deliver.AddDays(1);
                }
                Debug.Log($"Schedule notification at {deliver}");

                var info = manager.CreateNotification();
                info.Title = notify.Title;
                info.Body = notify.Body;
                info.DeliveryTime = deliver;
                info.ShouldAutoCancel = true;

                var scheduler = manager.ScheduleNotification(info);
                scheduler.Reschedule = true;
            }
#endif
        }

#if UNITY_IOS
        private IEnumerator iOSNotificationRequest()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                };

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                Debug.Log(res);
            }
        }
#endif

        private void OnApplicationFocus(bool focus)
        {
            if (Initialized && !focus && setting.saveOnLostFocus)
            {
                Economy.EconomyManager.Instance.Save();
                Repository.Instance.SaveAll();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (Initialized && pause && setting.saveOnLostFocus)
            {
                Economy.EconomyManager.Instance.Save();
                Repository.Instance.SaveAll();
            }
        }
    }
}
