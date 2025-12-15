using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace thecalcify.Charts
{
    public static class CandlePainter
    {
        // ============================================================
        //  PRICE → PIXEL MAPPING
        // ============================================================
        private static float PriceToY(double price, double minPrice, double maxPrice, SKRect area)
        {
            if (maxPrice <= minPrice)
                return area.Bottom;

            double t = (price - minPrice) / (maxPrice - minPrice);
            t = Math.Max(0, Math.Min(1, t));

            return area.Bottom - (float)(t * area.Height);
        }

        private static float TimeToX(DateTime time, DateTime min, DateTime max, SKRect area)
        {
            double total = (max - min).TotalSeconds;
            if (total <= 0)
                total = 1;

            double sec = (time - min).TotalSeconds;
            if (sec < 0 || sec > total)
                return float.NaN; // outside visible

            double t = sec / total;
            t = Math.Max(0, Math.Min(1, t));

            return area.Left + (float)(t * area.Width);
        }

        // ============================================================
        //  CANDLE DRAWING
        // ============================================================
        public static void DrawCandles(
            SKCanvas canvas,
            SKRect rect,
            IReadOnlyList<Candle> candles,
            DateTime minTime,
            DateTime maxTime,
            double minPrice,
            double maxPrice,
            SKPaint upPaint,
            SKPaint downPaint,
            SKPaint wickPaint,
            float minWidth = 3f,
            float maxWidth = 18f)
        {
            if (candles == null || candles.Count == 0)
                return;

            double totalSec = (maxTime - minTime).TotalSeconds;
            if (totalSec <= 0)
                totalSec = 1;

            float width = rect.Width;
            float candleW = width / Math.Max(candles.Count, 1);
            candleW = Math.Max(minWidth, Math.Min(maxWidth, candleW));
            float half = candleW / 2f;

            foreach (var c in candles)
            {
                float x = TimeToX(c.OpenTime, minTime, maxTime, rect);
                if (float.IsNaN(x)) continue;

                float yHigh = PriceToY(c.High, minPrice, maxPrice, rect);
                float yLow = PriceToY(c.Low, minPrice, maxPrice, rect);
                float yOpen = PriceToY(c.Open, minPrice, maxPrice, rect);
                float yClose = PriceToY(c.Close, minPrice, maxPrice, rect);

                bool isUp = c.Close >= c.Open;
                var bodyPaint = isUp ? upPaint : downPaint;

                // Wick
                canvas.DrawLine(x, yHigh, x, yLow, wickPaint);

                // Body
                float top = Math.Min(yOpen, yClose);
                float bottom = Math.Max(yOpen, yClose);
                if (bottom - top < 1f) bottom = top + 1f;

                var body = new SKRect(x - half, top, x + half, bottom);
                canvas.DrawRect(body, bodyPaint);
            }
        }

        public static void DrawBars(
    SKCanvas canvas, SKRect rect, IReadOnlyList<Candle> candles,
    DateTime minTime, DateTime maxTime, double minPrice, double maxPrice,
    SKPaint upPaint, SKPaint downPaint, float minWidth = 2f, float maxWidth = 15f)
        {
            if (candles == null || candles.Count == 0) return;

            double totalSec = (maxTime - minTime).TotalSeconds;
            if (totalSec <= 0) totalSec = 1;

            float barW = rect.Width / Math.Max(candles.Count, 1);
            barW = Math.Max(minWidth, Math.Min(maxWidth, barW));
            float tickLength = barW / 3f;

            foreach (var c in candles)
            {
                float x = TimeToX(c.OpenTime, minTime, maxTime, rect);
                if (float.IsNaN(x)) continue;

                float yHigh = PriceToY(c.High, minPrice, maxPrice, rect);
                float yLow = PriceToY(c.Low, minPrice, maxPrice, rect);
                float yOpen = PriceToY(c.Open, minPrice, maxPrice, rect);
                float yClose = PriceToY(c.Close, minPrice, maxPrice, rect);

                bool isUp = c.Close >= c.Open;
                var paint = isUp ? upPaint : downPaint;

                // Vertical line (High to Low)
                canvas.DrawLine(x, yHigh, x, yLow, paint);

                // Open tick (left)
                canvas.DrawLine(x - tickLength, yOpen, x, yOpen, paint);

                // Close tick (right)
                canvas.DrawLine(x, yClose, x + tickLength, yClose, paint);
            }
        }

        public static void DrawColumns(
    SKCanvas canvas, SKRect rect, IReadOnlyList<Candle> candles,
    DateTime minTime, DateTime maxTime, double minPrice, double maxPrice,
    SKPaint upPaint, SKPaint downPaint, float minWidth = 5f, float maxWidth = 20f)
        {
            if (candles == null || candles.Count == 0) return;

            float colW = rect.Width / Math.Max(candles.Count, 1);
            colW = Math.Max(minWidth, Math.Min(maxWidth, colW));
            float half = colW / 2f;
            float baseline = rect.Bottom;

            foreach (var c in candles)
            {
                float x = TimeToX(c.OpenTime, minTime, maxTime, rect);
                if (float.IsNaN(x)) continue;

                float yClose = PriceToY(c.Close, minPrice, maxPrice, rect);
                bool isUp = c.Close >= c.Open;
                var paint = isUp ? upPaint : downPaint;

                float top = Math.Min(yClose, baseline);
                float bottom = Math.Max(yClose, baseline);

                var column = new SKRect(x - half, top, x + half, bottom);
                canvas.DrawRect(column, paint);
            }
        }


        // ============================================================
        //  GRID + AXES
        // ============================================================
        public static void DrawGridAndAxes(
            SKCanvas canvas,
            SKRect rect,
            double minPrice,
            double maxPrice,
            DateTime minTime,
            DateTime maxTime,
            SKPaint gridPaint,
            SKPaint textPaint,
            SKPaint borderPaint,
            int hLines = 6)
        {
            // ---- HORIZONTAL PRICE LINES ----
            for (int i = 0; i <= hLines; i++)
            {
                float y = rect.Top + (i / (float)hLines) * rect.Height;
                canvas.DrawLine(rect.Left, y, rect.Right, y, gridPaint);

                double price = maxPrice - (i / (float)hLines) * (maxPrice - minPrice);
                canvas.DrawText(price.ToString("0.00"), rect.Right + 5, y + 4, textPaint);
            }

            // ---- VERTICAL TIME LINES ----
            TimeSpan total = maxTime - minTime;
            if (total.TotalSeconds <= 0)
                total = TimeSpan.FromSeconds(1);

            TimeSpan interval =
                total.TotalMinutes <= 5 ? TimeSpan.FromMinutes(1) :
                total.TotalMinutes <= 20 ? TimeSpan.FromMinutes(5) :
                total.TotalHours <= 3 ? TimeSpan.FromMinutes(15) :
                total.TotalHours <= 6 ? TimeSpan.FromMinutes(30) :
                total.TotalDays <= 1 ? TimeSpan.FromHours(1) :
                TimeSpan.FromDays(1);

            DateTime t = RoundUp(minTime, interval);

            while (t <= maxTime)
            {
                float x = TimeToX(t, minTime, maxTime, rect);
                if (!float.IsNaN(x))
                {
                    canvas.DrawLine(x, rect.Top, x, rect.Bottom, gridPaint);
                    canvas.DrawText(FormatTime(t, minTime, maxTime), x - 20, rect.Bottom + 14, textPaint);
                }

                t = t.Add(interval);
            }

            canvas.DrawRect(rect, borderPaint);
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan interval)
        {
            long delta = interval.Ticks - (dt.Ticks % interval.Ticks);
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        private static string FormatTime(DateTime t, DateTime min, DateTime max)
        {
            TimeSpan span = max - min;

            if (span.TotalMinutes < 3) return t.ToString("HH:mm:ss");
            if (span.TotalHours < 12) return t.ToString("HH:mm");
            if (span.TotalDays < 7) return t.ToString("dd MMM HH:mm");
            return t.ToString("dd MMM");
        }

        // ============================================================
        //  LAST PRICE LINE
        // ============================================================
        public static void DrawLastPriceLine(
            SKCanvas canvas,
            SKRect rect,
            double lastPrice,
            double minPrice,
            double maxPrice,
            SKPaint linePaint,
            SKPaint bgPaint,
            SKPaint textPaint)
        {
            float y = PriceToY(lastPrice, minPrice, maxPrice, rect);

            canvas.DrawLine(rect.Left, y, rect.Right, y, linePaint);

            string label = lastPrice.ToString("0.00");

            var bounds = new SKRect();
            textPaint.MeasureText(label, ref bounds);

            float pad = 5;
            float w = bounds.Width + pad * 2;
            float h = bounds.Height + pad * 2;

            var bg = new SKRect(rect.Right + 4, y - h / 2, rect.Right + 4 + w, y + h / 2);
            canvas.DrawRect(bg, bgPaint);
            canvas.DrawText(label, bg.Left + pad, bg.Bottom - pad, textPaint);
        }

        public static void DrawHighLow(
            SKCanvas canvas,
            SKRect rect,
            IReadOnlyList<Candle> candles,
            DateTime minTime,
            DateTime maxTime,
            double minPrice,
            double maxPrice,
            SKPaint linePaint,
            float minWidth = 1.5f,
            float maxWidth = 4f)
        {
            if (candles == null || candles.Count == 0)
                return;

            foreach (var c in candles)
            {
                float x = TimeToX(c.OpenTime, minTime, maxTime, rect);
                if (float.IsNaN(x))
                    continue;

                float yHigh = PriceToY(c.High, minPrice, maxPrice, rect);
                float yLow = PriceToY(c.Low, minPrice, maxPrice, rect);

                // Direct use
                canvas.DrawLine(x, yHigh, x, yLow, linePaint);
            }
        }
        public static void DrawLine(
    SKCanvas canvas,
    SKRect rect,
    IReadOnlyList<Candle> candles,
    DateTime minTime,
    DateTime maxTime,
    double minPrice,
    double maxPrice,
    SKPaint linePaint,
    float lineWidth = 2f)
        {
            if (candles == null || candles.Count == 0)
                return;

            // Create path for smooth line
            using (var path = new SKPath())
            {
                bool firstPoint = true;

                foreach (var c in candles)
                {
                    float x = TimeToX(c.OpenTime, minTime, maxTime, rect);
                    if (float.IsNaN(x))
                        continue;

                    // Use Close price for line chart
                    float y = PriceToY(c.Close, minPrice, maxPrice, rect);

                    if (firstPoint)
                    {
                        path.MoveTo(x, y);
                        firstPoint = false;
                    }
                    else
                    {
                        path.LineTo(x, y);
                    }
                }

                // Draw the line
                using (var paint = new SKPaint())
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = linePaint.Color;
                    paint.StrokeWidth = lineWidth;
                    paint.IsAntialias = true;
                    paint.StrokeCap = SKStrokeCap.Round;
                    paint.StrokeJoin = SKStrokeJoin.Round;

                    canvas.DrawPath(path, paint);
                }
            }
        }


        // ============================================================
        //  CROSSHAIR + TOOLTIP
        // ============================================================
        public static void DrawCrosshair(
            SKCanvas canvas,
            SKRect rect,
            System.Drawing.Point? point,
            double minPrice,
            double maxPrice,
            SKPaint linePaint,
            SKPaint bgPaint,
            SKPaint textPaint)
        {
            if (!point.HasValue) return;
            var p = point.Value;

            if (!rect.Contains(p.X, p.Y)) return;

            canvas.DrawLine(rect.Left, p.Y, rect.Right, p.Y, linePaint);
            canvas.DrawLine(p.X, rect.Top, p.X, rect.Bottom, linePaint);

            float rel = (rect.Bottom - p.Y) / rect.Height;
            rel = Math.Max(0, Math.Min(1, rel));

            double price = minPrice + (maxPrice - minPrice) * rel;
            string text = price.ToString("0.00");

            var bounds = new SKRect();
            textPaint.MeasureText(text, ref bounds);

            float pad = 5f;
            float w = bounds.Width + pad * 2;
            float h = bounds.Height + pad * 2;

            var bg = new SKRect(rect.Right + 4, p.Y - h / 2, rect.Right + 4 + w, p.Y + h / 2);
            canvas.DrawRect(bg, bgPaint);
            canvas.DrawText(text, bg.Left + pad, bg.Bottom - pad, textPaint);
        }

        public static void DrawCrosshairInfo(
            SKCanvas canvas,
            SKRect rect,
            System.Drawing.Point? point,
            IReadOnlyList<Candle> candles,
            DateTime minTime,
            DateTime maxTime,
            SKPaint bgPaint,
            SKPaint textPaint)
        {
            if (!point.HasValue || candles == null || candles.Count == 0) return;

            var p = point.Value;
            if (!rect.Contains(p.X, p.Y)) return;

            double total = (maxTime - minTime).TotalSeconds;
            if (total <= 0) total = 1;

            float relX = (p.X - rect.Left) / rect.Width;
            relX = Math.Max(0, Math.Min(1, relX));

            DateTime cursorTime = minTime.AddSeconds(total * relX);

            Candle nearest = candles
                .OrderBy(c => Math.Abs((c.OpenTime - cursorTime).TotalSeconds))
                .FirstOrDefault();

            if (nearest == null) return;

            string t1 = nearest.OpenTime.ToString("dd MMM yyyy HH:mm");
            string t2 = $"O: {nearest.Open:0.00}  H: {nearest.High:0.00}";
            string t3 = $"L: {nearest.Low:0.00}  C: {nearest.Close:0.00}";
            string t4 = $"V: {nearest.Volume:0.##}";

            float pad = 6;
            float lineH = textPaint.TextSize + 4;

            float boxW = Math.Max(
                Math.Max(textPaint.MeasureText(t1), textPaint.MeasureText(t2)),
                Math.Max(textPaint.MeasureText(t3), textPaint.MeasureText(t4))
            ) + pad * 2;

            float boxH = lineH * 4 + pad * 2;

            var bg = new SKRect(rect.Left + 10, rect.Top + 10,
                                rect.Left + 10 + boxW, rect.Top + 10 + boxH);

            canvas.DrawRect(bg, bgPaint);

            float x = bg.Left + pad;
            float y = bg.Top + pad + textPaint.TextSize;

            canvas.DrawText(t1, x, y, textPaint); y += lineH;
            canvas.DrawText(t2, x, y, textPaint); y += lineH;
            canvas.DrawText(t3, x, y, textPaint); y += lineH;
            canvas.DrawText(t4, x, y, textPaint);
        }
    }
}
