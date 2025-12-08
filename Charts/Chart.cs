using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Charts.DTOs;   // Tick
using thecalcify.Charts.Helper; // TimeFrame → ToTimeSpan()

namespace thecalcify.Charts
{
    public partial class Chart : Form
    {
        private readonly string _symbol;
        private readonly SkiaChartView _chartView;
        private readonly Panel _topPanel;

        private CandleBuilder _builder;
        private TimeFrame _currentTF = TimeFrame.Min1;

        private readonly Timer _uiTimer;

        // Keep a list in case you want to inspect ticks; not used for rebuild now
        private readonly List<Tick> _ticksHistory = new List<Tick>();
        private readonly object _sync = new object();
        private const int MaxTicksHistory = 50000; // adjust as you like

        public Chart(string symbol)
        {
            InitializeComponent();

            _symbol = symbol;
            Text = $"Chart - {symbol}";

            // builder
            _builder = new CandleBuilder(_currentTF.ToTimeSpan());

            // Top panel for timeframe buttons
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(5),
                BackColor = Color.FromArgb(81, 213, 220)
            };

            // Chart view
            _chartView = new SkiaChartView
            {
                Dock = DockStyle.Fill,
                ChartBackground = new SKColor(255,255,255)
            };


            Controls.Add(_chartView);
            Controls.Add(_topPanel);

            CreateTimeFrameButtons();

            // UI timer
            _uiTimer = new Timer
            {
                Interval = 50 // ~20 FPS
            };
            _uiTimer.Tick += UiTimer_Tick;
        }

        #region Timeframe buttons

        private void CreateTimeFrameButtons()
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            _topPanel.Controls.Add(flow);

            var items = new (string Text, TimeFrame TF)[]
            {
                ("1m",  TimeFrame.Min1),
                ("5m",  TimeFrame.Min5),
                ("15m", TimeFrame.Min15),
                ("30m", TimeFrame.Min30),
                ("1H",  TimeFrame.Hour1),
                ("1D",  TimeFrame.Day1)
            };

            foreach (var item in items)
            {
                var btn = new Button
                {
                    Text = item.Text,
                    Tag = item.TF,
                    Width = 50,
                    Height = 24,
                    Margin = new Padding(3),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(45, 45, 45),
                    ForeColor = Color.White
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += TimeFrameButton_Click;
                flow.Controls.Add(btn);
            }

            // Initial highlight
            HighlightTimeframeButtons();
        }

        private void TimeFrameButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            if (btn.Tag is TimeFrame)
            {
                ChangeTimeFrame((TimeFrame)btn.Tag);
            }
        }

        /// <summary>
        /// Re-color timeframe buttons based on _currentTF.
        /// </summary>
        private void HighlightTimeframeButtons()
        {
            if (_topPanel.Controls.Count == 0)
                return;

            var flow = _topPanel.Controls[0] as FlowLayoutPanel;
            if (flow == null)
                return;

            foreach (Control c in flow.Controls)
            {
                var b = c as Button;
                if (b == null || !(b.Tag is TimeFrame))
                    continue;

                var tf = (TimeFrame)b.Tag;
                bool isActive = tf == _currentTF;

                b.BackColor = isActive
                    ? Color.FromArgb(80, 80, 80)
                    : Color.FromArgb(45, 45, 45);
            }
        }

        private async void ChangeTimeFrame(TimeFrame tf)
        {
            _currentTF = tf;

            // reload historical candles for new timeframe
            await LoadAndApplyHistorical();

            // update button highlight
            HighlightTimeframeButtons();
        }

        #endregion

        #region Form lifecycle

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 1️⃣ Load historical candles for initial timeframe
            await LoadAndApplyHistorical();

            // 2️⃣ Start listening to live ticks
            GlobalTickDispatcher.TickReceived += OnTick;

            // 3️⃣ Start chart UI updates
            _uiTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GlobalTickDispatcher.TickReceived -= OnTick;
            _uiTimer.Stop();
            base.OnFormClosing(e);
        }

        #endregion

        #region Ticks + UI timer

        // Called from your Global dispatcher
        private void OnTick(Tick t)
        {
            if (!string.Equals(t.Symbol, _symbol, StringComparison.OrdinalIgnoreCase))
                return;

            lock (_sync)
            {
                _ticksHistory.Add(t);
                if (_ticksHistory.Count > MaxTicksHistory)
                    _ticksHistory.RemoveRange(0, _ticksHistory.Count - MaxTicksHistory);

                // update builder with real-time tick
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
                double price = 182793;

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
