using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace thecalcify.Charts
{
    public class SkiaChartView : UserControl
    {
        private readonly SKControl _skControl;

        private readonly List<Candle> _candles = new List<Candle>();

        // price viewport
        private double _minPrice = 0;
        private double _maxPrice = 1;

        // time ranges
        private DateTime _dataMin;
        private DateTime _dataMax;
        private DateTime _viewMin;
        private DateTime _viewMax;

        private bool _autoFit = true;

        // interaction
        private bool _isPanning = false;
        private Point _lastMouse;
        private Point? _crosshair;

        // paints
        private readonly SKPaint _upBodyPaint;
        private readonly SKPaint _downBodyPaint;
        private readonly SKPaint _wickPaint;
        private readonly SKPaint _gridPaint;
        private readonly SKPaint _axisLinePaint;
        private readonly SKPaint _axisTextPaint;
        private readonly SKPaint _crosshairLinePaint;
        private readonly SKPaint _crosshairLabelBgPaint;
        private readonly SKPaint _crosshairLabelTextPaint;
        private readonly SKPaint _volumePaint;
        private readonly SKPaint _lastPriceLinePaint;
        private readonly SKPaint _lastPriceLabelBgPaint;
        private readonly SKPaint _lastPriceLabelTextPaint;
        public SKColor ChartBackground { get; set; } = new SKColor(18, 18, 18);

        public SkiaChartView()
        {
            DoubleBuffered = true;

            _skControl = new SKControl
            {
                Dock = DockStyle.Fill
            };
            _skControl.PaintSurface += SkControl_PaintSurface;

            _skControl.MouseDown += SkControl_MouseDown;
            _skControl.MouseMove += SkControl_MouseMove;
            _skControl.MouseUp += SkControl_MouseUp;
            _skControl.MouseWheel += SkControl_MouseWheel;
            _skControl.MouseLeave += SkControl_MouseLeave;

            Controls.Add(_skControl);

            // ===============================
            // BACKGROUND WHITE
            // ===============================
            ChartBackground = SKColors.White;   // you already added this property

            // ===============================
            // CANDLE COLORS
            // ===============================
            _upBodyPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(0, 128, 0), // Green
                IsAntialias = true
            };

            _downBodyPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(204, 0, 0), // Red
                IsAntialias = true
            };

            _wickPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                Color = SKColors.Black,
                IsAntialias = true
            };

            // ===============================
            // GRID & AXES
            // ===============================
            _gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(220, 220, 220), // Light gray grid
                StrokeWidth = 1f,
                IsAntialias = false
            };

            _axisLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1.2f,
                IsAntialias = true
            };

            _axisTextPaint = new SKPaint
            {
                Color = SKColors.Black, // Black text
                TextSize = 12f,
                IsAntialias = true
            };

            // ===============================
            // CROSSHAIR
            // ===============================
            _crosshairLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(0, 0, 0, 180), // Black with transparency
                StrokeWidth = 1f,
                IsAntialias = true
            };

            _crosshairLabelBgPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(255, 255, 255, 240), // White tooltip
                IsAntialias = true
            };

            _crosshairLabelTextPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12f,
                IsAntialias = true
            };

            // ===============================
            // VOLUME BAR
            // ===============================
            _volumePaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(180, 180, 180), // gray volume bars
                IsAntialias = false
            };

            // ===============================
            // LAST PRICE LINE
            // ===============================
            _lastPriceLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(30, 144, 255), // DodgerBlue
                StrokeWidth = 1.5f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0)
            };

            _lastPriceLabelBgPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = new SKColor(255, 255, 255, 255), // white
                IsAntialias = true
            };

            _lastPriceLabelTextPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 12f,
                IsAntialias = true
            };
        }

        #region Public API

        public void SetCandles(IReadOnlyList<Candle> candles, bool autoFit = true)
        {
            _candles.Clear();
            if (candles != null && candles.Count > 0)
                _candles.AddRange(candles);

            _autoFit = autoFit;
            if (_candles.Count > 0)
                ComputeRanges();

            _skControl.Invalidate();
        }

        public void UpdateCandles(IReadOnlyList<Candle> candles, bool autoFit = false)
        {
            // Replace candle list
            _candles.Clear();
            if (candles != null && candles.Count > 0)
                _candles.AddRange(candles);

            // ONLY apply autoFit when requested
            if (autoFit)
            {
                _autoFit = true;
                ComputeRanges();          // full re-auto-scale
            }

            // IMPORTANT: never recompute price or time range during live ticks
            // Otherwise 1-day candle zooms into tick-size movement

            _skControl.Invalidate();
        }


        public void AutoFit()
        {
            _autoFit = true;
            if (_candles.Count > 0)
                ComputeRanges();
            _skControl.Invalidate();
        }

        public void ResetView()
        {
            _autoFit = true;
            if (_candles.Count > 0)
                ComputeRanges();
            _skControl.Invalidate();
        }

        #endregion

        #region Paint

        private void SkControl_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(ChartBackground);

            var info = e.Info;
            var fullRect = new SKRect(0, 0, info.Width, info.Height);

            if (_candles.Count == 0)
            {
                DrawEmptyMessage(canvas, fullRect);
                return;
            }

            const float priceAxisWidth = 60f;
            const float timeAxisHeight = 22f;

            // MAIN PRICE AREA (do not include axes)
            var priceRect = new SKRect(
                fullRect.Left + 10,
                fullRect.Top + 10,
                fullRect.Right - priceAxisWidth - 10,
                fullRect.Bottom - timeAxisHeight - 10
            );

            // Update ranges
            if (_autoFit)
                ComputeRanges();
            else
                ClampViewToData();

            // ==================================================
            // 1️⃣ DRAW GRID + AXES (NO CLIPPING)
            // ==================================================
            CandlePainter.DrawGridAndAxes(
                canvas,
                priceRect,
                _minPrice,
                _maxPrice,
                _viewMin,
                _viewMax,
                _gridPaint,
                _axisTextPaint,
                _axisLinePaint
            );

            // ==================================================
            // 2️⃣ DRAW CANDLES (CLIPPED INSIDE priceRect)
            // ==================================================
            canvas.Save();
            canvas.ClipRect(priceRect, SKClipOperation.Intersect, true);

            CandlePainter.DrawCandles(
                canvas,
                priceRect,
                _candles,
                _viewMin,
                _viewMax,
                _minPrice,
                _maxPrice,
                _upBodyPaint,
                _downBodyPaint,
                _wickPaint
            );

            var last = _candles.Last();

            CandlePainter.DrawLastPriceLine(
                canvas,
                priceRect,
                last.Close,
                _minPrice,
                _maxPrice,
                _lastPriceLinePaint,
                _lastPriceLabelBgPaint,
                _lastPriceLabelTextPaint
            );

            CandlePainter.DrawCrosshair(
                canvas,
                priceRect,
                _crosshair,
                _minPrice,
                _maxPrice,
                _crosshairLinePaint,
                _crosshairLabelBgPaint,
                _crosshairLabelTextPaint
            );

            CandlePainter.DrawCrosshairInfo(
                canvas,
                priceRect,
                _crosshair,
                _candles,
                _viewMin,
                _viewMax,
                _crosshairLabelBgPaint,
                _crosshairLabelTextPaint
            );

            canvas.Restore();
        }

        private void DrawEmptyMessage(SKCanvas canvas, SKRect rect)
        {
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Gray;
                paint.TextSize = 16;
                paint.IsAntialias = true;
                paint.TextAlign = SKTextAlign.Center;

                canvas.DrawText("No data", rect.MidX, rect.MidY, paint);
            }
        }

        #endregion

        #region Ranges

        private void ComputePriceRange()
        {
            if (_candles.Count == 0)
                return;

            double rawMin = _candles.Min(c => c.Low);
            double rawMax = _candles.Max(c => c.High);

            // range
            double range = rawMax - rawMin;
            if (range <= 0)
                range = rawMax * 0.001; // tiny safe range

            // === NEW: Dynamic padding (2% - 5% depending on volatility) ===
            double pad = range * 0.04;   // 4% padding
            if (pad < 1) pad = 1;        // minimum 1 rupee

            _minPrice = rawMin - pad;
            _maxPrice = rawMax + pad;

            // Prevent candle from touching the boundary even if sudden spike
            if (_maxPrice <= _minPrice)
                _maxPrice = _minPrice + 1;
        }

        private void ComputeRanges()
        {
            if (_candles.Count == 0)
                return;

            ComputePriceRange();

            _dataMin = _candles.First().OpenTime;
            _dataMax = _candles.Last().OpenTime + _candles.Last().Interval;

            var interval = _candles.First().Interval;
            if (interval.TotalSeconds <= 0)
                interval = TimeSpan.FromMinutes(1);

            _viewMin = _dataMin - interval;
            _viewMax = _dataMax + interval;

            // at least 5 candles visible
            var minSpan = TimeSpan.FromTicks(interval.Ticks * 5);
            if ((_viewMax - _viewMin) < minSpan)
                _viewMax = _viewMin + minSpan;
        }

        private void ClampViewToData()
        {
            if (_candles.Count == 0)
                return;

            if (_viewMin < _dataMin)
                _viewMin = _dataMin;
            if (_viewMax > _dataMax)
                _viewMax = _dataMax;

            if (_viewMax <= _viewMin)
                _viewMax = _viewMin.AddSeconds(1);
        }

        #endregion

        #region Mouse / Interaction

        private void SkControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = true;
                _lastMouse = e.Location;
                _autoFit = false;
            }

            if (e.Button == MouseButtons.Right)
            {
                _crosshair = e.Location;
                _skControl.Invalidate();
            }
        }

        private void SkControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning && _candles.Count > 0)
            {
                int dx = e.Location.X - _lastMouse.X;
                _lastMouse = e.Location;

                double viewSec = (_viewMax - _viewMin).TotalSeconds;
                if (viewSec <= 0) viewSec = 1;

                double secPerPixel = viewSec / Math.Max(Width, 1);

                double shift = -dx * secPerPixel;

                _viewMin = _viewMin.AddSeconds(shift);
                _viewMax = _viewMax.AddSeconds(shift);

                _autoFit = false;
                ClampViewToData();
                _skControl.Invalidate();
            }

            _crosshair = e.Location;
            _skControl.Invalidate();
        }

        private void SkControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _isPanning = false;
        }

        private void SkControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_candles.Count == 0)
                return;

            // simple zoom around center of view for now
            double factor = (e.Delta > 0) ? 0.8 : 1.25;

            DateTime mid = _viewMin + TimeSpan.FromTicks((_viewMax - _viewMin).Ticks / 2);
            TimeSpan span = TimeSpan.FromTicks((long)((_viewMax - _viewMin).Ticks * factor));

            _viewMin = mid - TimeSpan.FromTicks(span.Ticks / 2);
            _viewMax = mid + TimeSpan.FromTicks(span.Ticks / 2);

            _autoFit = false;
            ClampViewToData();
            _skControl.Invalidate();
        }

        private void SkControl_MouseLeave(object sender, EventArgs e)
        {
            _crosshair = null;
            _skControl.Invalidate();
        }

        #endregion

        #region Designer

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
            }
        }

        #endregion
    }
}
