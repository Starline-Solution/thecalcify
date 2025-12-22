using System;

namespace thecalcify.Charts.Models
{
    public class Tick
    {
        public string Symbol { get; set; }
        public DateTime Time { get; set; }
        public double Price { get; set; }
        public double? Volume { get; set; }
    }
}
