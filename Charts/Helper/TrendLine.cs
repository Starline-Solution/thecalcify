using System;
using SkiaSharp;

namespace thecalcify.Charts
{
    public class TrendLine
    {
        // Price coordinates (NOT pixels!)
        public DateTime StartTime { get; set; }
        public double StartPrice { get; set; }
        public DateTime EndTime { get; set; }
        public double EndPrice { get; set; }
        public double? RSquared { get; set; }

        // Visual properties
        public SKColor Color { get; set; }
        public float StrokeWidth { get; set; }
        public bool IsDashed { get; set; }

        public TrendLine()
        {
            Color = new SKColor(255, 0, 0); // Red default
            StrokeWidth = 2f;
            IsDashed = false;
        }
    }
}
