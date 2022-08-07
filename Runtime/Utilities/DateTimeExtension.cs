using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class DateTimeExtension
    {
        public static DateTime ChangeTime(this DateTime t, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            return new DateTime(t.Year, t.Month, t.Day, hours, minutes, seconds, milliseconds, t.Kind);
        }

        public static DateTime Reset(this DateTime t)
        {
            return t.ChangeTime(0, 0, 0, 0);
        }

        public static int DiffDays(this DateTime t, DateTime t2)
        {
            return (t2.Reset() - t.Reset()).Days;
        }
    }
}