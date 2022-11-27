using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Services.Analytics;

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

            public override string ToString()
            {
                return Value.ToString();
            }

            public object Value
            {
                get
                {
                    if (stringValue != null)
                    {
                        return stringValue;
                    }
                    if (longValue != null)
                    {
                        return longValue;
                    }
                    if (doubleValue != null)
                    {
                        return doubleValue;
                    }
                    return "";
                }
            }
        }

        public static void Log(string eventName, params Parameter[] parameters)
        {
            Debug.Log($"----[Tracking]: {eventName}\n{string.Join("\n", parameters.Select(p => $"{p.name}={p.ToString()}"))}");
            FirebaseInstance.Log(eventName, parameters);
            AnalyticsService.Instance.CustomData(
                eventName,
                parameters.Aggregate(new Dictionary<string, object>(), (dict, param) => 
                {
                    dict.Add(param.name, param.Value);
                    return dict;
                })
            );
        }
    }
}