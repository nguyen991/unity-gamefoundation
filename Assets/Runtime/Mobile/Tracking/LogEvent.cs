using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Mobile
{
    public static class LogEvent
    {
        public sealed class Parameter
        {
            public string name = "";
            public string stringValue = null;
            public long? longValue = null;
            public double? doubleValue = null;

            public Parameter(string parameterName, string parameterValue)
            {
                this.name = parameterName;
                this.stringValue = parameterValue;
            }

            public Parameter(string parameterName, long parameterValue)
            {
                this.name = parameterName;
                this.longValue = parameterValue;
            }

            public Parameter(string parameterName, double parameterValue)
            {
                this.name = parameterName;
                this.doubleValue = parameterValue;
            }

#if GF_ANALYTICS && (UNITY_ANDROID || UNITY_IOS)
            public Firebase.Analytics.Parameter ToFirebaseParam()
            {
                if (stringValue != null)
                {
                    return new Firebase.Analytics.Parameter(name, stringValue);
                }
                if (longValue != null)
                {
                    return new Firebase.Analytics.Parameter(name, (long)longValue);
                }
                if (doubleValue != null)
                {
                    return new Firebase.Analytics.Parameter(name, (double)doubleValue);
                }
                return null;
            }
#endif

            public override string ToString()
            {
                if (stringValue != null)
                {
                    return stringValue;
                }
                if (longValue != null)
                {
                    return longValue.ToString();
                }
                if (doubleValue != null)
                {
                    return doubleValue.ToString();
                }
                return "";
            }
        }

        public static void Log(string eventName, params Parameter[] parameters)
        {
#if GF_ANALYTICS && (UNITY_ANDROID || UNITY_IOS)
            var fb_params = new List<Firebase.Analytics.Parameter>();
            var ap_params = new Dictionary<string, string>();

            foreach (var value in parameters)
            {
                fb_params.Add(value.ToFirebaseParam());
                ap_params.Add(value.name, value.ToString());
            }

            // AppsFlyer
            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, ap_params);

            // Firebase
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, fb_params.ToArray());
#endif
        }
    }
}