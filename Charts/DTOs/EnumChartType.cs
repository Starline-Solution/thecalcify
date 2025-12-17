using SkiaSharp;
using System;
using System.Collections.Generic;

namespace thecalcify.Charts.DTOs
{
    public enum ChartType
    {
        Candle,
        Bar,
        Column,
        HighLow,
        Line
    }

    public enum DrawingMode
    {
        None,
        TrendLine,
        HorizontalLine,
        RegressionTrend
    }

    public enum ArrowStyle
    {
        None,
        End,
        Start,
        Both
    }

    public enum CursorTool
    {
        Cross,     
        Arrow,
        Dot
        
    }

    public enum ShapeTool
    {
        None,
        Circle,
        Rectangle,
        Ellipse,     
        Path         
    }

    public class Shape
    {
        public ShapeTool ShapeType { get; set; }
        public DateTime StartTime { get; set; }
        public double StartPrice { get; set; }
        public DateTime EndTime { get; set; }
        public double EndPrice { get; set; }

        public List<(DateTime Time, double Price)> PathPoints { get; set; }

        public SKColor StrokeColor { get; set; }
        public SKColor FillColor { get; set; }
        public float StrokeWidth { get; set; }
        public bool IsFilled { get; set; }

        public Shape()
        {
            StrokeColor = new SKColor(0, 120, 215);
            FillColor = new SKColor(0, 120, 215, 80);  
            StrokeWidth = 2f;
            IsFilled = false;
            PathPoints = new List<(DateTime, double)>();
        }
    }
}
