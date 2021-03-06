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
            FirebaseInstance.Log(eventName, parameters);
        }
    }
}