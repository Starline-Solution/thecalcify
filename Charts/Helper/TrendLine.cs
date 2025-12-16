using SkiaSharp;
using System;
using thecalcify.Charts.DTOs;

namespace thecalcify.Charts
{
    public class TrendLine
    {
        public DateTime StartTime { get; set; }
        public double StartPrice { get; set; }
        public DateTime EndTime { get; set; }
        public double EndPrice { get; set; }
        public SKColor Color { get; set; }
        public float StrokeWidth { get; set; }
        public bool IsDashed { get; set; }
        public double? RSquared { get; set; }
        public bool ExtendLine { get; set; }
        public bool ShowArrows { get; set; }
        public ArrowStyle ArrowStyle { get; set; }

        public TrendLine()
        {
            Color = new SKColor(255, 0, 0);
            StrokeWidth = 2f;
            IsDashed = false;
            ExtendLine = false;
            ShowArrows = false;          
            ArrowStyle = ArrowStyle.End; 
        }
    }

}
