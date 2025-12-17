using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using thecalcify.Charts.DTOs;

namespace thecalcify.Charts
{
    public class SkiaChartView : UserControl
    {
        private readonly SKControl _skControl;

        private readonly List<Candle> _candles = new List<Candle>();

        // price viewport
        private double _minPrice = 0;
        private double _maxPrice = 1;

        private bool _isSelectingRegression = false;
        private DateTime _regressionStartTime;

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

        private ChartType _chartType = ChartType.Candle;

        private ShapeTool _shapeTool = ShapeTool.None;
        private readonly List<Shape> _shapes = new List<Shape>();
        private Shape _currentShape = null;
        private bool _isDrawingShape = false;

        private DrawingMode _drawingMode = DrawingMode.None;
        private readonly List<TrendLine> _trendLines = new List<TrendLine>();
        private TrendLine _currentTrendLine = null;
        private bool _isDrawingTrendLine = false;
        private CursorTool _cursorTool = CursorTool.Cross;

        public SKColor ChartBackground { get; set; } = new SKColor(18, 18, 18);
        public event Action DrawingCompleted;

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

        public void SetCursorTool(CursorTool tool)
        {
            _cursorTool = tool;
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            switch (_cursorTool)
            {
                case CursorTool.Arrow:
                    _skControl.Cursor = Cursors.Default;
                    break;

                case CursorTool.Cross:
                    _skControl.Cursor = CreateCrossCursor();
                    break;

                case CursorTool.Dot:
                    _skControl.Cursor = CreateDotCursor();
                    break;

            }
        }

        private Cursor CreateDotCursor()
        {
            int size = 32;
            int dotSize = 8;

            using (Bitmap bmp = new Bitmap(size, size))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw dot (white with black border)
                using (Brush whiteBrush = new SolidBrush(Color.White))
                using (Pen blackPen = new Pen(Color.Black, 2))
                {
                    int center = size / 2;
                    int radius = dotSize / 2;

                    // White filled circle
                    g.FillEllipse(whiteBrush,
                        center - radius,
                        center - radius,
                        dotSize,
                        dotSize);

                    // Black border
                    g.DrawEllipse(blackPen,
                        center - radius,
                        center - radius,
                        dotSize,
                        dotSize);
                }

                // Create cursor
                IntPtr hIcon = bmp.GetHicon();
                Icon icon = Icon.FromHandle(hIcon);
                return new Cursor(icon.Handle);
            }
        }

        public void SetShapeTool(ShapeTool tool)
        {
            _shapeTool = tool;
            _currentShape = null;
            _isDrawingShape = false;
        }

        public void ClearShapes()
        {
            _shapes.Clear();
            _currentShape = null;
            _isDrawingShape = false;
            _skControl.Invalidate();
        }

        private Cursor CreateCrossCursor()
        {
            int size = 32;
            int lineLength = 12;
            int lineWidth = 2;

            using (Bitmap bmp = new Bitmap(size, size))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                int center = size / 2;

                // Black outline
                using (Pen pen = new Pen(Color.Black, lineWidth + 1))
                {
                    g.DrawLine(pen, center, center - lineLength, center, center + lineLength);
                    g.DrawLine(pen, center - lineLength, center, center + lineLength, center);
                }

                // White crosshair
                using (Pen pen = new Pen(Color.White, lineWidth))
                {
                    g.DrawLine(pen, center, center - lineLength, center, center + lineLength);
                    g.DrawLine(pen, center - lineLength, center, center + lineLength, center);
                }

                IntPtr hIcon = bmp.GetHicon();
                Icon icon = Icon.FromHandle(hIcon);
                return new Cursor(icon.Handle);
            }
        }


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
        public void ClearTrendLines()
        {
            _trendLines.Clear();
            _currentTrendLine = null;
            _isDrawingTrendLine = false;
            _skControl.Invalidate();
        }


        public void SetDrawingMode(DrawingMode mode)
        {
            _drawingMode = mode;
            _currentTrendLine = null;
            _isDrawingTrendLine = false;
            _isSelectingRegression = false;

            if (mode != DrawingMode.None)
            {
                _skControl.Cursor = Cursors.Cross;
            }
            else
            {
                UpdateCursor(); 
            }
        }


        public DrawingMode GetDrawingMode()
        {
            return _drawingMode;
        }

        private DateTime PixelToTime(float pixelX)
        {
            if (_viewMax <= _viewMin)
                return _viewMin;

            float relX = (pixelX - 10) / (Width - 70); // Adjust for margins
            relX = Math.Max(0, Math.Min(1, relX));

            double totalSec = (_viewMax - _viewMin).TotalSeconds;
            return _viewMin.AddSeconds(totalSec * relX);
        }

        private double PixelToPrice(float pixelY)
        {
            if (_maxPrice <= _minPrice)
                return _minPrice;

            float relY = (Height - 32 - pixelY) / (Height - 52); // Adjust for margins
            relY = Math.Max(0, Math.Min(1, relY));

            return _minPrice + (_maxPrice - _minPrice) * relY;
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

        public void SetChartType(ChartType type)
        {
            _chartType = type;
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

            switch (_chartType)
            {
                case ChartType.Candle:
                    CandlePainter.DrawCandles(canvas, priceRect, _candles, _viewMin, _viewMax,
                                               _minPrice, _maxPrice, _upBodyPaint, _downBodyPaint, _wickPaint);
                    break;
                case ChartType.Bar:
                    CandlePainter.DrawBars(canvas, priceRect, _candles, _viewMin, _viewMax,
                                            _minPrice, _maxPrice, _upBodyPaint, _downBodyPaint);
                    break;
                case ChartType.Column:
                    CandlePainter.DrawColumns(canvas, priceRect, _candles, _viewMin, _viewMax,
                                               _minPrice, _maxPrice, _upBodyPaint, _downBodyPaint);
                    break;

                case ChartType.HighLow:
                    CandlePainter.DrawHighLow(canvas, priceRect, _candles, _viewMin, _viewMax, _minPrice, _maxPrice, _wickPaint);
                    break;

                case ChartType.Line:    // 👈 NEW
                    CandlePainter.DrawLine(canvas, priceRect, _candles, _viewMin, _viewMax, _minPrice, _maxPrice, _upBodyPaint, 2.5f);
                    break;
            }


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

            foreach (var trendLine in _trendLines)
            {
                DrawTrendLine(canvas, priceRect, trendLine);
            }

            // Draw current trend line being drawn (dashed preview)
            if (_isDrawingTrendLine && _currentTrendLine != null)
            {
                DrawTrendLine(canvas, priceRect, _currentTrendLine, isDashed: true);
            }

            foreach (var shape in _shapes)
            {
                DrawShape(canvas, priceRect, shape);
            }

            // Draw current shape preview
            if (_isDrawingShape && _currentShape != null)
            {
                DrawShape(canvas, priceRect, _currentShape, isPreview: true);
            }

            // Draw regression selection highlight
            if (_drawingMode == DrawingMode.RegressionTrend && _isSelectingRegression)
            {
                if (_crosshair.HasValue)
                {
                    float x1 = TimeToPixelX(_regressionStartTime, priceRect);
                    float x2 = _crosshair.Value.X;

                    using (var paint = new SKPaint())
                    {
                        paint.Style = SKPaintStyle.Stroke;
                        paint.Color = new SKColor(0, 150, 255, 180);
                        paint.StrokeWidth = 2f;
                        paint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0);

                        // Vertical lines
                        canvas.DrawLine(x1, priceRect.Top, x1, priceRect.Bottom, paint);
                        canvas.DrawLine(x2, priceRect.Top, x2, priceRect.Bottom, paint);

                        // Shaded area
                        paint.Style = SKPaintStyle.Fill;
                        paint.Color = new SKColor(0, 150, 255, 30);
                        var rect = new SKRect(
                            Math.Min(x1, x2),
                            priceRect.Top,
                            Math.Max(x1, x2),
                            priceRect.Bottom
                        );
                        canvas.DrawRect(rect, paint);
                    }
                }
            }

            // Draw custom cursor on canvas
            if (_cursorTool == CursorTool.Dot && _crosshair.HasValue)
            {
                var p = _crosshair.Value;

                using (var paint = new SKPaint())
                {
                    // Outer white circle
                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = SKColors.White;
                    paint.IsAntialias = true;
                    canvas.DrawCircle(p.X, p.Y, 6, paint);

                    // Black border
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Black;
                    paint.StrokeWidth = 2f;
                    canvas.DrawCircle(p.X, p.Y, 6, paint);

                    // Inner red dot
                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = new SKColor(255, 0, 0);
                    canvas.DrawCircle(p.X, p.Y, 2, paint);
                }
            }

        }

        private void DrawShape(SKCanvas canvas, SKRect rect, Shape shape, bool isPreview = false)
        {
            float x1 = TimeToPixelX(shape.StartTime, rect);
            float y1 = PriceToPixelY(shape.StartPrice, rect);
            float x2 = TimeToPixelX(shape.EndTime, rect);
            float y2 = PriceToPixelY(shape.EndPrice, rect);

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = shape.StrokeColor;
                paint.StrokeWidth = shape.StrokeWidth;

                if (isPreview)
                {
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0);
                }

                switch (shape.ShapeType)
                {
                    case ShapeTool.Circle:
                        DrawCircleShape(canvas, x1, y1, x2, y2, paint, shape);
                        break;
                    case ShapeTool.Rectangle:
                        DrawRectangleShape(canvas, x1, y1, x2, y2, paint, shape);
                        break;
                    case ShapeTool.Ellipse:
                        DrawEllipseShape(canvas, x1, y1, x2, y2, paint, shape);
                        break;
                    case ShapeTool.Path:
                        DrawPathShape(canvas, rect, shape, paint);
                        break;
                }
            }
        }

        private void DrawCircleShape(SKCanvas canvas, float x1, float y1, float x2, float y2,
    SKPaint paint, Shape shape)
        {
            float centerX = (x1 + x2) / 2;
            float centerY = (y1 + y2) / 2;
            float radius = Math.Min(Math.Abs(x2 - x1), Math.Abs(y2 - y1)) / 2;

            if (shape.IsFilled)
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = shape.FillColor;
                canvas.DrawCircle(centerX, centerY, radius, paint);
            }

            paint.Style = SKPaintStyle.Stroke;
            paint.Color = shape.StrokeColor;
            canvas.DrawCircle(centerX, centerY, radius, paint);
        }

        private void DrawRectangleShape(SKCanvas canvas, float x1, float y1, float x2, float y2,
            SKPaint paint, Shape shape)
        {
            var rect = new SKRect(
                Math.Min(x1, x2), Math.Min(y1, y2),
                Math.Max(x1, x2), Math.Max(y1, y2)
            );

            if (shape.IsFilled)
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = shape.FillColor;
                canvas.DrawRect(rect, paint);
            }

            paint.Style = SKPaintStyle.Stroke;
            paint.Color = shape.StrokeColor;
            canvas.DrawRect(rect, paint);
        }

        private void DrawEllipseShape(SKCanvas canvas, float x1, float y1, float x2, float y2,
            SKPaint paint, Shape shape)
        {
            var rect = new SKRect(
                Math.Min(x1, x2), Math.Min(y1, y2),
                Math.Max(x1, x2), Math.Max(y1, y2)
            );

            if (shape.IsFilled)
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = shape.FillColor;
                canvas.DrawOval(rect, paint);
            }

            paint.Style = SKPaintStyle.Stroke;
            paint.Color = shape.StrokeColor;
            canvas.DrawOval(rect, paint);
        }

        private void DrawPathShape(SKCanvas canvas, SKRect rect, Shape shape, SKPaint paint)
        {
            if (shape.PathPoints.Count < 2)
                return;

            using (var path = new SKPath())
            {
                var firstPoint = shape.PathPoints[0];
                path.MoveTo(
                    TimeToPixelX(firstPoint.Time, rect),
                    PriceToPixelY(firstPoint.Price, rect)
                );

                for (int i = 1; i < shape.PathPoints.Count; i++)
                {
                    var point = shape.PathPoints[i];
                    path.LineTo(
                        TimeToPixelX(point.Time, rect),
                        PriceToPixelY(point.Price, rect)
                    );
                }

                paint.Style = SKPaintStyle.Stroke;
                canvas.DrawPath(path, paint);
            }
        }


        private void DrawArrow(
    SKCanvas canvas,
    float x1, float y1,    // Line start
    float x2, float y2,    // Line end (arrow location)
    SKPaint paint,
    float arrowSize = 12f)
        {
            // Calculate angle of the line
            double angle = Math.Atan2(y2 - y1, x2 - x1);

            // Calculate arrow points (triangle)
            float angle1 = (float)(angle + Math.PI * 0.85);  // 150 degrees
            float angle2 = (float)(angle - Math.PI * 0.85);  // -150 degrees

            float x_left = x2 + arrowSize * (float)Math.Cos(angle1);
            float y_left = y2 + arrowSize * (float)Math.Sin(angle1);

            float x_right = x2 + arrowSize * (float)Math.Cos(angle2);
            float y_right = y2 + arrowSize * (float)Math.Sin(angle2);

            // Draw filled triangle arrow
            using (var path = new SKPath())
            {
                path.MoveTo(x2, y2);          // Arrow tip
                path.LineTo(x_left, y_left);  // Left wing
                path.LineTo(x_right, y_right); // Right wing
                path.Close();

                var arrowPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = paint.Color,
                    IsAntialias = true
                };

                canvas.DrawPath(path, arrowPaint);
            }
        }

        private void DrawTrendLine(SKCanvas canvas, SKRect rect, TrendLine line, bool isDashed = false)
        {
            float x1, y1, x2, y2;

            if (line.ExtendLine)
            {
                // Extended line logic (existing code)
                double totalSeconds = (line.EndTime - line.StartTime).TotalSeconds;
                if (totalSeconds <= 0)
                    totalSeconds = 1;

                double pricePerSecond = (line.EndPrice - line.StartPrice) / totalSeconds;

                DateTime extendedStartTime = _viewMin;
                DateTime extendedEndTime = _viewMax;

                double deltaStart = (extendedStartTime - line.StartTime).TotalSeconds;
                double deltaEnd = (extendedEndTime - line.StartTime).TotalSeconds;

                double extendedStartPrice = line.StartPrice + (pricePerSecond * deltaStart);
                double extendedEndPrice = line.StartPrice + (pricePerSecond * deltaEnd);

                x1 = TimeToPixelX(extendedStartTime, rect);
                y1 = PriceToPixelY(extendedStartPrice, rect);
                x2 = TimeToPixelX(extendedEndTime, rect);
                y2 = PriceToPixelY(extendedEndPrice, rect);
            }
            else
            {
                // Normal line logic (existing code)
                x1 = TimeToPixelX(line.StartTime, rect);
                y1 = PriceToPixelY(line.StartPrice, rect);
                x2 = TimeToPixelX(line.EndTime, rect);
                y2 = PriceToPixelY(line.EndPrice, rect);
            }

            // Draw the line
            using (var paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = line.Color;
                paint.StrokeWidth = line.StrokeWidth;
                paint.IsAntialias = true;

                if (isDashed || line.IsDashed)
                {
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0);
                }

                canvas.DrawLine(x1, y1, x2, y2, paint);

                // ========== NEW: DRAW ARROWS ==========
                if (line.ShowArrows)
                {
                    if (line.ArrowStyle == ArrowStyle.End || line.ArrowStyle == ArrowStyle.Both)
                    {
                        DrawArrow(canvas, x1, y1, x2, y2, paint);
                    }

                    if (line.ArrowStyle == ArrowStyle.Start || line.ArrowStyle == ArrowStyle.Both)
                    {
                        DrawArrow(canvas, x2, y2, x1, y1, paint);
                    }
                }

                // Draw endpoint circles ONLY if not extended and no arrows
                if (!line.ExtendLine && !line.ShowArrows)
                {
                    paint.Style = SKPaintStyle.Fill;
                    canvas.DrawCircle(x1, y1, 4, paint);
                    canvas.DrawCircle(x2, y2, 4, paint);
                }
            }

            // R² label (existing code remains same)
            if (line.RSquared.HasValue)
            {
                float midX = (x1 + x2) / 2;
                float midY = (y1 + y2) / 2;

                string label = string.Format("R² = {0:0.000}", line.RSquared.Value);

                using (var textPaint = new SKPaint())
                {
                    textPaint.Color = SKColors.Black;
                    textPaint.TextSize = 11f;
                    textPaint.IsAntialias = true;

                    var bounds = new SKRect();
                    textPaint.MeasureText(label, ref bounds);

                    using (var bgPaint = new SKPaint())
                    {
                        bgPaint.Style = SKPaintStyle.Fill;
                        bgPaint.Color = new SKColor(255, 255, 255, 220);
                        var bg = new SKRect(
                            midX - 2,
                            midY - bounds.Height - 2,
                            midX + bounds.Width + 2,
                            midY + 2
                        );
                        canvas.DrawRect(bg, bgPaint);
                    }

                    canvas.DrawText(label, midX, midY, textPaint);
                }
            }
        }

        private float TimeToPixelX(DateTime time, SKRect rect)
        {
            double totalSec = (_viewMax - _viewMin).TotalSeconds;
            if (totalSec <= 0) totalSec = 1;

            double sec = (time - _viewMin).TotalSeconds;
            float t = (float)(sec / totalSec);
            t = Math.Max(0, Math.Min(1, t));

            return rect.Left + t * rect.Width;
        }

        private float PriceToPixelY(double price, SKRect rect)
        {
            if (_maxPrice <= _minPrice)
                return rect.Bottom;

            double t = (price - _minPrice) / (_maxPrice - _minPrice);
            t = Math.Max(0, Math.Min(1, t));

            return rect.Bottom - (float)(t * rect.Height);
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
            // TREND LINE MODE
            if (_drawingMode == DrawingMode.TrendLine && e.Button == MouseButtons.Left)
            {
                if (!_isDrawingTrendLine)
                {
                    // First click - start trend line
                    _currentTrendLine = new TrendLine
                    {
                        StartTime = PixelToTime(e.X),
                        StartPrice = PixelToPrice(e.Y),
                        EndTime = PixelToTime(e.X),
                        EndPrice = PixelToPrice(e.Y)
                    };
                    _isDrawingTrendLine = true;
                }
                else
                {
                    // Second click - finish trend line
                    _currentTrendLine.EndTime = PixelToTime(e.X);
                    _currentTrendLine.EndPrice = PixelToPrice(e.Y);

                    _trendLines.Add(_currentTrendLine);
                    _currentTrendLine = null;
                    _isDrawingTrendLine = false;

                    // 🔥 RESET MODE
                    _drawingMode = DrawingMode.None;
                    UpdateCursor();

                    // 🔔 NOTIFY
                    DrawingCompleted?.Invoke();

                    _skControl.Invalidate();
                    return;
                }

                _skControl.Invalidate();
                return;
            }

            if (_drawingMode == DrawingMode.RegressionTrend && e.Button == MouseButtons.Left)
            {
                if (!_isSelectingRegression)
                {
                    // First click - start selection
                    _regressionStartTime = PixelToTime(e.X);
                    _isSelectingRegression = true;
                    _skControl.Invalidate();
                }
                else
                {
                    // Second click - calculate regression
                    DateTime endTime = PixelToTime(e.X);

                    // Get candles in range
                    var selectedCandles = _candles
                        .Where(c => c.OpenTime >= _regressionStartTime && c.OpenTime <= endTime)
                        .ToList();

                    if (selectedCandles.Count >= 2)
                    {
                        var regression = CalculateLinearRegression(selectedCandles);
                        if (regression != null)
                        {
                            _trendLines.Add(regression);
                        }
                    }

                    _isSelectingRegression = false;

                    // 🔥 RESET MODE
                    _drawingMode = DrawingMode.None;
                    UpdateCursor();

                    // 🔔 NOTIFY
                    DrawingCompleted?.Invoke();

                    _skControl.Invalidate();
                    return;
                }
                return;
            }

            // NORMAL MODE (existing code)
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

            // ========== SHAPE DRAWING MODE ==========
            if (_shapeTool != ShapeTool.None && e.Button == MouseButtons.Left)
            {
                if (_shapeTool == ShapeTool.Path)
                {
                    // Path: Freehand drawing
                    if (!_isDrawingShape)
                    {
                        _currentShape = new Shape
                        {
                            ShapeType = ShapeTool.Path,
                            StartTime = PixelToTime(e.X),
                            StartPrice = PixelToPrice(e.Y)
                        };
                        _currentShape.PathPoints.Add((
                            PixelToTime(e.X),
                            PixelToPrice(e.Y)
                        ));
                        _isDrawingShape = true;
                    }
                    else
                    {
                        // Finish path
                        _shapes.Add(_currentShape);
                        _currentShape = null;
                        _isDrawingShape = false;

                        // 🔥 RESET SHAPE TOOL
                        _shapeTool = ShapeTool.None;
                        UpdateCursor();

                        // 🔔 NOTIFY
                        DrawingCompleted?.Invoke();

                        _skControl.Invalidate();
                        return;

                    }
                }
                else
                {
                    // Other shapes: Two-click drawing
                    if (!_isDrawingShape)
                    {
                        _currentShape = new Shape
                        {
                            ShapeType = _shapeTool,
                            StartTime = PixelToTime(e.X),
                            StartPrice = PixelToPrice(e.Y),
                            EndTime = PixelToTime(e.X),
                            EndPrice = PixelToPrice(e.Y)
                        };
                        _isDrawingShape = true;
                    }
                    else
                    {
                        // Finish shape
                        _currentShape.EndTime = PixelToTime(e.X);
                        _currentShape.EndPrice = PixelToPrice(e.Y);

                        _shapes.Add(_currentShape);
                        _currentShape = null;
                        _isDrawingShape = false;

                        // 🔥 RESET SHAPE TOOL
                        _shapeTool = ShapeTool.None;
                        UpdateCursor();

                        // 🔔 NOTIFY
                        DrawingCompleted?.Invoke();

                        _skControl.Invalidate();
                        return;

                    }
                }

                _skControl.Invalidate();
                return;
            }

        }

        private TrendLine CalculateLinearRegression(List<Candle> candles)
        {
            if (candles.Count < 2)
                return null;

            int n = candles.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = i;
                double y = candles[i].Close;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            // Linear regression: y = mx + b
            double m = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - m * sumX) / n;

            // Calculate R² (coefficient of determination)
            double meanY = sumY / n;
            double ssTotal = 0, ssResidual = 0;

            for (int i = 0; i < n; i++)
            {
                double yActual = candles[i].Close;
                double yPredicted = m * i + b;

                ssTotal += Math.Pow(yActual - meanY, 2);
                ssResidual += Math.Pow(yActual - yPredicted, 2);
            }

            double rSquared = ssTotal > 0 ? 1 - (ssResidual / ssTotal) : 0;

            // Calculate endpoints
            double startPrice = b;
            double endPrice = m * (n - 1) + b;

            return new TrendLine
            {
                StartTime = candles[0].OpenTime,
                StartPrice = startPrice,
                EndTime = candles[n - 1].OpenTime,
                EndPrice = endPrice,
                Color = new SKColor(0, 150, 255),  // Blue
                StrokeWidth = 2.5f,
                IsDashed = false,
                RSquared = rSquared
            };
        }


        private void SkControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_shapeTool != ShapeTool.None && _isDrawingShape)
            {
                if (_shapeTool == ShapeTool.Path && _currentShape != null)
                {
                    _currentShape.PathPoints.Add((
                        PixelToTime(e.X),
                        PixelToPrice(e.Y)
                    ));
                }
                else if (_currentShape != null)
                {
                    _currentShape.EndTime = PixelToTime(e.X);
                    _currentShape.EndPrice = PixelToPrice(e.Y);
                }

                _skControl.Invalidate();
                return;
            }

            if (_drawingMode == DrawingMode.TrendLine && _isDrawingTrendLine)
            {
                if (_currentTrendLine != null)
                {
                    _currentTrendLine.EndTime = PixelToTime(e.X);
                    _currentTrendLine.EndPrice = PixelToPrice(e.Y);
                    _skControl.Invalidate();
                }
                return;
            }

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
