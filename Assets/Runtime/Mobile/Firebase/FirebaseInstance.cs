using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Mobile
{
    public static class FirebaseInstance
    {
        public static void Init()
        {
#if GF_FIREBASE
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    var app = Firebase.FirebaseApp.DefaultInstance;
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                }
            });
#endif
        }

        public static void Log(string eventName, params Mobile.LogEvent.Parameter[] parameters)
        {
#if GF_FIREBASE
            var fb_params = new List<Firebase.Analytics.Parameter>();
            foreach (var value in parameters)
            {
                fb_params.Add(ToFirebaseParam(value));
            }
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, fb_params.ToArray());
#endif
        }

#if GF_FIREBASE
        private static Firebase.Analytics.Parameter ToFirebaseParam(Mobile.LogEvent.Parameter param)
        {
            if (param.stringValue != null)
            {
                return new Firebase.Analytics.Parameter(param.name, param.stringValue);
            }
            if (param.longValue != null)
            {
                return new Firebase.Analytics.Parameter(param.name, (long)param.longValue);
            }
            if (param.doubleValue != null)
            {
                return new Firebase.Analytics.Parameter(param.name, (double)param.doubleValue);
            }
            return null;
        }
#endif
    }
}
