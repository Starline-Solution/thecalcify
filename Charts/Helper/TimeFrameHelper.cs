using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using thecalcify.Charts.DTOs;

namespace thecalcify.Charts.Helper
{
    public static class TimeFrameHelper
    {
        public static TimeSpan ToTimeSpan(this TimeFrame tf)
        {
            switch (tf)
            {
                case TimeFrame.Min1: return TimeSpan.FromMinutes(1);
                case TimeFrame.Min5: return TimeSpan.FromMinutes(5);
                case TimeFrame.Min15: return TimeSpan.FromMinutes(15);
                case TimeFrame.Min30: return TimeSpan.FromMinutes(30);
                case TimeFrame.Hour1: return TimeSpan.FromHours(1);
                case TimeFrame.Day1: return TimeSpan.FromDays(1);
                default: return TimeSpan.FromMinutes(1);
            }
        }

        public static string GetLabel(this TimeFrame tf)
        {
            switch (tf)
            {
                case TimeFrame.Min1: return "1m";
                case TimeFrame.Min5: return "5m";
                case TimeFrame.Min15: return "15m";
                case TimeFrame.Min30: return "30m";
                case TimeFrame.Hour1: return "1h";
                case TimeFrame.Day1: return "1d";
                default: return "?";
            }
        }
    }
}
