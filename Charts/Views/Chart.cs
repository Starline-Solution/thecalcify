using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;
using thecalcify.Charts.Models;
using thecalcify.Charts.Services;
using thecalcify.Helper;

namespace thecalcify.Charts.Views
{
    /// <summary>
    /// Represents the main charting window containing the WebView2 control.
    /// Handles the initialization of the browser environment and real-time data bridging.
    /// </summary>
    public partial class Chart : Form
    {
        private readonly WebView2 _webView;
        private readonly string _currentSymbol;
        private readonly string _displaySymbol;
        private bool _isWebViewReady = false;
        private const string VirtualHostName = "local.chart";

        /// <summary>
        /// Initializes a new instance of the Chart form.
        /// </summary>
        /// <param name="currentSymbol">The internal symbol key used for data subscription.</param>
        /// <param name="displaySymbol">The symbol name displayed on the chart UI.</param>
        public Chart(string currentSymbol, string displaySymbol)
        {
            _currentSymbol = currentSymbol;
            _displaySymbol = displaySymbol;

            InitializeComponent();
            InitializeChartUI();

            _webView = new WebView2 { Dock = DockStyle.Fill };
            this.Controls.Add(_webView);

            InitializeWebViewAsync();

            GlobalTickDispatcher.TickReceived += OnTickReceived;
        }

        /// <summary>
        /// Configures the form's visual properties.
        /// </summary>
        private void InitializeChartUI()
        {
            this.Text = $"Chart - {_displaySymbol}";
            this.Size = new System.Drawing.Size(1400, 900);
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = System.Drawing.Color.White;
        }

        /// <summary>
        /// Cleans up resources and unsubscribes from events when the form closes.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            GlobalTickDispatcher.TickReceived -= OnTickReceived;
            _webView?.Dispose();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Asynchronously initializes the WebView2 environment and sets up virtual host mapping.
        /// </summary>
        private async void InitializeWebViewAsync()
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await _webView.EnsureCoreWebView2Async(env);

                _webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

                await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    @"console.error = function(m) { 
                        window.chrome.webview.postMessage('JS_ERR::' + m); 
                    };"
                );

                _webView.CoreWebView2.WebMessageReceived += (s, e) =>
                {
                    var msg = e.TryGetWebMessageAsString();
                    if (msg.StartsWith("JS_ERR::"))
                        ApplicationLogger.Log("[CHART] " + msg);
                };

                string assetsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Charts", "WebAssets");

                if (!Directory.Exists(assetsFolderPath))
                {
                    ApplicationLogger.Log($"Critical Error: The chart assets folder was not found at:\n{assetsFolderPath} - InitializeWebViewAsync()");
                    return;
                }

                _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    VirtualHostName,
                    assetsFolderPath,
                    CoreWebView2HostResourceAccessKind.Allow
                );

                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                _isWebViewReady = true;

                LoadChartContent();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Constructs the dynamic URL and navigates the WebView to the chart index.
        /// </summary>
        private void LoadChartContent()
        {
            try
            {
                var builder = new UriBuilder("http", VirtualHostName)
                {
                    Path = "index.html",
                    Query = $"symbol={Uri.EscapeDataString(_displaySymbol)}"
                };

                _webView.CoreWebView2.Navigate(builder.ToString());
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Filters ticks for the current symbol and pushes them to the JavaScript chart.
        /// </summary>
        /// <param name="tick">The tick data received.</param>
        private void OnTickReceived(Tick tick)
        {
            if (!_isWebViewReady || _webView?.CoreWebView2 == null) return;

            if (!string.Equals(tick.Symbol, _currentSymbol, StringComparison.OrdinalIgnoreCase))
                return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnTickReceived(tick)));
                return;
            }

            try
            {
                long unixTimestamp = new DateTimeOffset(tick.Time).ToUnixTimeMilliseconds();

                var chartData = new
                {
                    time = unixTimestamp,
                    ask = tick.Price,
                    volume = tick.Volume
                };

                string jsonPayload = JsonConvert.SerializeObject(chartData);

                _webView.CoreWebView2.ExecuteScriptAsync($"if(window.updateTick) window.updateTick('{jsonPayload}')");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }
    }
}