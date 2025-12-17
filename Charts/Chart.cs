using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Charts.DTOs;   
using thecalcify.Charts.Helper; 

namespace thecalcify.Charts
{
    public partial class Chart : Form
    {
        private readonly string _symbol;
        private readonly SkiaChartView _chartView;
        //private readonly Panel _topPanel;
        private readonly FlowLayoutPanel _topPanel;
        private ComboBox _timeframeDropdown;
        private ComboBox _drawingToolDropdown;
        private ComboBox _cursorToolDropdown;
        private CandleBuilder _builder;
        private TimeFrame _currentTF = TimeFrame.Min1;

        private readonly Timer _uiTimer;

        // Keep a list in case you want to inspect ticks; not used for rebuild now
        private readonly List<Tick> _ticksHistory = new List<Tick>();
        private readonly object _sync = new object();
        private const int MaxTicksHistory = 50000; // adjust as you like
        private ComboBox _shapesDropdown;

        private ComboBox _chartTypeDropdown;
        private ChartType _currentChartType = ChartType.Candle;

        public Chart(string symbol)
        {
            InitializeComponent();
            _symbol = symbol;
            Text = $"Chart - {symbol}";

            _builder = new CandleBuilder(_currentTF.ToTimeSpan());

            // ✅ Changed to FlowLayoutPanel
            _topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,  // slightly taller
                Padding = new Padding(5),
                BackColor = Color.FromArgb(81, 213, 220),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            _chartView = new SkiaChartView
            {
                Dock = DockStyle.Fill,
                ChartBackground = new SKColor(255, 255, 255)
            };

            Controls.Add(_chartView);
            Controls.Add(_topPanel);

            // ✅ Replace button method with dropdown
            CreateTimeFrameDropdown();
            CreateDrawingToolDropdown();
            CreateCursorToolDropdown();
            CreateShapesDropdown();
            CreateChartTypeDropdown();


            _uiTimer = new Timer { Interval = 50 };
            _uiTimer.Tick += UiTimer_Tick;
        }

        #region Timeframe Dropdown

        private void CreateTimeFrameDropdown()
        {
            // Label
            //var label = new Label
            //{
            //    Text = "Timeframe:",
            //    AutoSize = true,
            //    Margin = new Padding(5, 7, 3, 3),
            //    ForeColor = Color.FromArgb(30, 30, 30),
            //    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            //};
            //_topPanel.Controls.Add(label);

            // Dropdown
            _timeframeDropdown = new ComboBox
            {
                Width = 80,
                Height = 24,
                Margin = new Padding(0, 3, 15, 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };

            // Items
            _timeframeDropdown.Items.Add("1 Minute");
            _timeframeDropdown.Items.Add("5 Minutes");
            _timeframeDropdown.Items.Add("15 Minutes");
            _timeframeDropdown.Items.Add("30 Minutes");
            _timeframeDropdown.Items.Add("1 Hour");
            _timeframeDropdown.Items.Add("1 Day");

            _timeframeDropdown.SelectedIndex = 0;
            _timeframeDropdown.SelectedIndexChanged += TimeframeDropdown_Changed;

            _topPanel.Controls.Add(_timeframeDropdown);
        }

        private async void TimeframeDropdown_Changed(object sender, EventArgs e)
        {
            if (_timeframeDropdown.SelectedIndex == -1)
                return;

            TimeFrame selectedTF;

            // Traditional switch (C# 7.3 compatible)
            switch (_timeframeDropdown.SelectedIndex)
            {
                case 0:
                    selectedTF = TimeFrame.Min1;
                    break;
                case 1:
                    selectedTF = TimeFrame.Min5;
                    break;
                case 2:
                    selectedTF = TimeFrame.Min15;
                    break;
                case 3:
                    selectedTF = TimeFrame.Min30;
                    break;
                case 4:
                    selectedTF = TimeFrame.Hour1;
                    break;
                case 5:
                    selectedTF = TimeFrame.Day1;
                    break;
                default:
                    selectedTF = TimeFrame.Min1;
                    break;
            }

            if (selectedTF != _currentTF)
            {
                await ChangeTimeFrame(selectedTF);
            }
        }


        private async Task ChangeTimeFrame(TimeFrame tf)
        {
            _currentTF = tf;
            //await LoadAndApplyHistorical();
        }

        #endregion


        #region Timeframe buttons


        #endregion

        #region Form lifecycle

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 1️⃣ Load historical candles for initial timeframe
            //await LoadAndApplyHistorical();

            // 2️⃣ Start listening to live ticks
            GlobalTickDispatcher.TickReceived += OnTick;

            // 3️⃣ Start chart UI updates
            _uiTimer.Start();

            _chartView.DrawingCompleted += OnDrawingCompleted;

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _chartView.DrawingCompleted -= OnDrawingCompleted;
            GlobalTickDispatcher.TickReceived -= OnTick;
            _uiTimer.Stop();
            base.OnFormClosing(e);
        }

        private void OnDrawingCompleted()
        {
            // Reset drawing dropdown
            if (_drawingToolDropdown != null)
                _drawingToolDropdown.SelectedIndex = 0; // None

            // Reset shapes dropdown
            if (_shapesDropdown != null)
                _shapesDropdown.SelectedIndex = 0; // None
        }

        #endregion

        #region Ticks + UI timer

        private void CreateChartTypeDropdown()
        {
            // Label
            //var label = new Label
            //{
            //    Text = "Chart Type:",
            //    AutoSize = true,
            //    Margin = new Padding(5, 7, 3, 3),
            //    ForeColor = Color.FromArgb(30, 30, 30),
            //    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            //};
            //_topPanel.Controls.Add(label);

            // Dropdown
            _chartTypeDropdown = new ComboBox
            {
                Width = 100,
                Height = 24,
                Margin = new Padding(0, 3, 3, 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };

            _chartTypeDropdown.Items.Add("▮▯ Candles");
            _chartTypeDropdown.Items.Add("┼  OHLC Bar");
            _chartTypeDropdown.Items.Add("▌  Column");
            _chartTypeDropdown.Items.Add("│  High–Low");
            _chartTypeDropdown.Items.Add("╱  Line");


            _chartTypeDropdown.SelectedIndex = 0;
            _chartTypeDropdown.SelectedIndexChanged += ChartTypeDropdown_Changed;

            _topPanel.Controls.Add(_chartTypeDropdown);
        }

        private void CreateDrawingToolDropdown()
        {
            // Separator
            var separator = new Label
            {
                Text = " | ",
                AutoSize = true,
                Margin = new Padding(10, 7, 10, 3),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            _topPanel.Controls.Add(separator);

            // Label
            var label = new Label
            {
                Text = "Drawing:",
                AutoSize = true,
                Margin = new Padding(5, 7, 3, 3),
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _topPanel.Controls.Add(label);

            // Dropdown
            _drawingToolDropdown = new ComboBox
            {
                Width = 150,
                Height = 24,
                Margin = new Padding(0, 3, 15, 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };

            // Items
            _drawingToolDropdown.Items.Add("None");
            _drawingToolDropdown.Items.Add("📐 Trend Line");
            _drawingToolDropdown.Items.Add("📈 Regression Trend");
            _drawingToolDropdown.Items.Add("➖ Horizontal Line");

            _drawingToolDropdown.SelectedIndex = 0; // Default: None
            _drawingToolDropdown.SelectedIndexChanged += DrawingToolDropdown_Changed;

            _topPanel.Controls.Add(_drawingToolDropdown);

            // Clear button
            var clearBtn = new Button
            {
                Text = "🗑️ Clear",
                Width = 80,
                Height = 28,
                Margin = new Padding(2, 2, 3, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            clearBtn.FlatAppearance.BorderSize = 0;
            clearBtn.Click += (s, e) => {
                _chartView.ClearTrendLines();
                _chartView.ClearShapes();
            };
            _topPanel.Controls.Add(clearBtn);
        }

        private void CreateShapesDropdown()
        {
            var separator = new Label
            {
                Text = " | ",
                AutoSize = true,
                Margin = new Padding(10, 7, 10, 3),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            _topPanel.Controls.Add(separator);

            var label = new Label
            {
                Text = "Shapes:",
                AutoSize = true,
                Margin = new Padding(5, 7, 3, 3),
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _topPanel.Controls.Add(label);

            _shapesDropdown = new ComboBox
            {
                Width = 120,
                Height = 24,
                Margin = new Padding(0, 3, 15, 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };

            _shapesDropdown.Items.Add("None");
            _shapesDropdown.Items.Add("⭕ Circle");
            _shapesDropdown.Items.Add("▭ Rectangle");
            _shapesDropdown.Items.Add("⬭ Ellipse");
            _shapesDropdown.Items.Add("✏️ Path");

            _shapesDropdown.SelectedIndex = 0; // Default: None
            _shapesDropdown.SelectedIndexChanged += ShapesDropdown_Changed;

            _topPanel.Controls.Add(_shapesDropdown);
        }

        private void ShapesDropdown_Changed(object sender, EventArgs e)
        {
            ShapeTool tool;

            switch (_shapesDropdown.SelectedIndex)
            {
                case 0:
                    tool = ShapeTool.None;
                    break;
                case 1:
                    tool = ShapeTool.Circle;
                    break;
                case 2:
                    tool = ShapeTool.Rectangle;
                    break;
                case 3:
                    tool = ShapeTool.Ellipse;
                    break;
                case 4:
                    tool = ShapeTool.Path;
                    break;
                default:
                    tool = ShapeTool.None;
                    break;
            }

            _chartView.SetShapeTool(tool);
        }

        private void CreateCursorToolDropdown()
        {
            // Label
            var label = new Label
            {
                Text = "Cursor:",
                AutoSize = true,
                Margin = new Padding(5, 7, 3, 3),
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _topPanel.Controls.Add(label);

            // Dropdown
            _cursorToolDropdown = new ComboBox
            {
                Width = 100,
                Height = 24,
                Margin = new Padding(0, 3, 15, 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };

            // Items with icons
            _cursorToolDropdown.Items.Add("✛ Cross");
            _cursorToolDropdown.Items.Add("➤ Arrow"); 
            _cursorToolDropdown.Items.Add("● Dot");

            _cursorToolDropdown.SelectedIndex = 0; // Default: Cross
            _cursorToolDropdown.SelectedIndexChanged += CursorToolDropdown_Changed;
            _chartView.SetCursorTool(CursorTool.Cross);

            _topPanel.Controls.Add(_cursorToolDropdown);
        }

        private void CursorToolDropdown_Changed(object sender, EventArgs e)
        {
            CursorTool tool;

            switch (_cursorToolDropdown.SelectedIndex)
            {
                case 0:
                    tool = CursorTool.Cross;
                    break;
                case 1:
                    tool = CursorTool.Arrow;
                    break;
                case 2:
                    tool = CursorTool.Dot;
                    break;
                default:
                    tool = CursorTool.Cross;
                    break;
            }

            _chartView.SetCursorTool(tool);
        }

        private void DrawingToolDropdown_Changed(object sender, EventArgs e)
        {
            DrawingMode mode;

            switch (_drawingToolDropdown.SelectedIndex)
            {
                case 0:
                    mode = DrawingMode.None;
                    break;
                case 1:
                    mode = DrawingMode.TrendLine;
                    break;
                case 2:
                    mode = DrawingMode.RegressionTrend;
                    break;
                case 3:
                    mode = DrawingMode.HorizontalLine;
                    break;
                default:
                    mode = DrawingMode.None;
                    break;
            }

            _chartView.SetDrawingMode(mode);
        }

        private void ChartTypeDropdown_Changed(object sender, EventArgs e)
        {
            if (_chartTypeDropdown.SelectedIndex == -1)
                return;

            switch (_chartTypeDropdown.SelectedIndex)
            {
                case 0: _currentChartType = ChartType.Candle; break;
                case 1: _currentChartType = ChartType.Bar; break;
                case 2: _currentChartType = ChartType.Column; break;
                case 3: _currentChartType = ChartType.HighLow; break;
                case 4: _currentChartType = ChartType.Line; break;
            }

            _chartView.SetChartType(_currentChartType);
        }

        private void OnTick(Tick t)
        {
            if (!string.Equals(t.Symbol, _symbol, StringComparison.OrdinalIgnoreCase))
                return;

            lock (_sync)
            {
                _ticksHistory.Add(t);
                if (_ticksHistory.Count > MaxTicksHistory)
                    _ticksHistory.RemoveRange(0, _ticksHistory.Count - MaxTicksHistory);

                _builder.AddTick(t.Time, t.Price, t.Volume);
            }
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            IReadOnlyList<Candle> candles;

            lock (_sync)
            {
                candles = _builder.GetAll(includeCurrent: true);
            }

            if (candles != null && candles.Count > 0)
                _chartView.UpdateCandles(candles, autoFit: false);
        }

        #endregion

        #region Historical load

        /// <summary>
        /// MOCK: replace this with your real REST API call
        /// which returns OHLC candles for _symbol and _currentTF.
        /// </summary>
        private async Task<List<Candle>> LoadHistoricalFromApi()
        {
            return await Task.Run(() =>
            {
                // generate dummy 300 historical candles
                var list = new List<Candle>();
                DateTime now = DateTime.UtcNow.AddMinutes(-3);

                var rnd = new Random();
                double price = 5090;

                TimeSpan span = _currentTF.ToTimeSpan();

                for (int i = 0; i < 300; i++)
                {
                    double open = price;
                    double high = open + rnd.NextDouble() * 5;
                    double low = open - rnd.NextDouble() * 5;
                    double close = low + rnd.NextDouble() * (high - low);

                    list.Add(new Candle
                    {
                        OpenTime = now,
                        Interval = span,
                        Open = open,
                        High = high,
                        Low = low,
                        Close = close,
                        Volume = rnd.Next(1, 30)
                    });

                    now = now.Add(span);
                    price = close;
                }

                return list;
            });
        }

        private async Task LoadAndApplyHistorical()
        {
            var historical = await LoadHistoricalFromApi();
            if (historical == null || historical.Count == 0)
                return;

            lock (_sync)
            {
                // reset builder with current timeframe
                _builder = new CandleBuilder(_currentTF.ToTimeSpan());

                // base candles come from historical data
                _builder.LoadHistorical(historical);

                // we only keep real-time ticks in _ticksHistory
                _ticksHistory.Clear();
            }

            // Push initial candles to chart
            _chartView.SetCandles(_builder.GetAll(true), autoFit: true);
        }

        #endregion
    }
}
