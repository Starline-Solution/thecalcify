using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
