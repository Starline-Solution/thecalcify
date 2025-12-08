using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecalcify.Charts
{
    public class Tick
    {
        public string Symbol { get; set; }
        public DateTime Time { get; set; }
        public double Price { get; set; }
        public double? Volume { get; set; }
    }

    public class Candle
    {
        public DateTime OpenTime { get; set; }
        public TimeSpan Interval { get; set; }

        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public double Volume { get; set; } // optional
    }


}
