using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.Home
{
    public partial class Home_Usercontrol : UserControl
    {
        private HubConnection _connection;
        private BindingList<MarketDataDto> _marketDataList;
        private readonly Dictionary<string, MarketDataDto> _latestUpdates = new Dictionary<string, MarketDataDto>();
        private readonly object _updateLock = new object();
        private System.Windows.Forms.Timer _updateTimer;

        public Home_Usercontrol()
        {
            InitializeComponent();

            if (!DesignMode && !LicenseManager.UsageMode.Equals(LicenseUsageMode.Designtime))
            {
                InitializeGrid();       // Step 1: Grid ready
                PrepareSignalR();       // Step 2: Prepare connection, don't start yet

                // After UI ready, start SignalR
                Task.Run(async () =>
                {
                    await StartSignalRConnection(); // Step 3: Connect and subscribe
                });
            }

            Application.ThreadException += (s, e) =>
            {
                Console.WriteLine("❌ UI Thread Exception: " + e.Exception.Message);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("❌ Unhandled Exception: " + ((Exception)e.ExceptionObject).Message);
            };
        }

        private void InitializeGrid()
        {
            _marketDataList = new BindingList<MarketDataDto>();
            dataGridView1.DataSource = _marketDataList;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 11);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.FixedSingle;

            System.Reflection.PropertyInfo doubleBufferedPropertyInfo = typeof(DataGridView)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            doubleBufferedPropertyInfo.SetValue(dataGridView1, true, null);

            dataGridView1.DataBindingComplete += (s, e) =>
            {
                if (dataGridView1.Columns.Count == 0) return;

                void SetColumn(string oldName, string newHeader, int width)
                {
                    if (dataGridView1.Columns.Contains(oldName))
                    {
                        var col = dataGridView1.Columns[oldName];
                        col.HeaderText = newHeader;
                        col.Width = width;
                    }
                }

                SetColumn("n", "Name", 140);
                SetColumn("b", "Bid", 80);
                SetColumn("a", "Ask", 80);
                SetColumn("ltp", "Last Traded Price", 80);
                SetColumn("h", "High", 80);
                SetColumn("l", "Low", 80);
                SetColumn("o", "Open", 80);
                SetColumn("c", "Close", 80);
                SetColumn("d", "Change", 80);
                SetColumn("v", "Volume", 80);
                SetColumn("t", "Time", 180);
                SetColumn("atp", "ATP", 100);
                SetColumn("bq", "Bid Quantity", 80);
                SetColumn("tbq", "Total Bid Quantity", 100);
                SetColumn("sq", "Sell Quantity", 80);
                SetColumn("tsq", "Total Sell Quantity", 100);
                SetColumn("vt", "Value Traded", 80);
                SetColumn("oi", "Open Interest", 80);
                SetColumn("ltq", "Last Traded Quantity", 80);

                if (dataGridView1.Columns.Contains("i"))
                    dataGridView1.Columns["i"].Visible = false;

                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    if (col.Name != "n" && col.Name != "t") // You can adjust alignment if needed
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            };


            var symbolMaster = GetSymbolMaster();
            foreach (var symbol in symbolMaster)
            {
                _marketDataList.Add(new MarketDataDto
                {
                    i = symbol,
                    n = symbol,
                    b = "N/A",
                    a = "N/A",
                    ltp = "N/A",
                    h = "N/A",
                    l = "N/A",
                    o = "N/A",
                    c = "N/A",
                    d = "N/A",
                    v = "N/A",
                    t = "N/A",
                    atp = "N/A",
                    bq = "N/A",
                    tbq = "N/A",
                    sq = "N/A",
                    tsq = "N/A",
                    vt = "N/A",
                    oi = "N/A",
                    ltq = "N/A"
                });
            }

            // Timer to batch updates every 150ms
            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Interval = 150;
            _updateTimer.Tick += (s, e) =>
            {
                List<MarketDataDto> updates = null;
                lock (_updateLock)
                {
                    if (_latestUpdates.Count == 0) return;
                    updates = new List<MarketDataDto>(_latestUpdates.Values);
                    _latestUpdates.Clear();
                }

                foreach (var data in updates)
                    UpdateGrid(data);
            };
            _updateTimer.Start();
        }

        private void PrepareSignalR()
        {
            string username = Environment.UserName;

            _connection = new HubConnectionBuilder()
                .WithUrl("http://api.thecalcify.com/excel?user=" + username + "&auth=Starline@1008&type=desktop")
                .WithAutomaticReconnect()
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                })
                .Build();

            // Optional: extend timeout (in case server is slow or idle)
            _connection.ServerTimeout = TimeSpan.FromMinutes(2); // default is 30s
            _connection.KeepAliveInterval = TimeSpan.FromSeconds(15); // default is 15s


            _connection.On<string>("excelRate", base64 =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(base64)) return;

                        var data = ProcessExcelRate(base64);

                        if (data != null)
                            UpdateGrid(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Error processing record: " + ex.Message);
                    }
                });
            });

            _connection.Closed += async (error) =>
            {
                Console.WriteLine("SignalR connection closed. Error: " + (error?.Message ?? "None"));
                await ReconnectWithBackoff();
            };
        }

        private async Task StartSignalRConnection()
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("✅ SignalR Connected");

                var symbolMaster = GetSymbolMaster();
                await _connection.InvokeAsync("SubscribeSymbols", symbolMaster);
                Console.WriteLine("📡 Subscribed to symbols successfully! at " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Initial connect failed: " + ex.Message);
                await ReconnectWithBackoff();
            }
        }

        private List<string> GetSymbolMaster()
        {
            return new List<string>
            {
                "GOLDFUTURE_I","GOLDFUTURE_II","GOLDFUTURE_III",
                "SILVERFUTURE_I","SILVERFUTURE_II","SILVERFUTURE_III",
                "GOLDM_I","GOLDM_II","GOLDM_III",
                "SILVERM_I","SILVERM_II","SILVERM_III",
                "ALUMINIUM_I","ALUMINIUM_II","ALUMINIUM_III",
                "COPPER_I","COPPER_II","COPPER_III",
                "CRUDEOIL_I","CRUDEOIL_II","CRUDEOIL_III",
                "LEAD_I","LEAD_II","LEAD_III",
                "NATURALGAS_I","NATURALGAS_II","NATURALGAS_III",
                "ZINC_I","ZINC_II","ZINC_III",
                "GOLDSPOT_I","SILVERSPOT_I","INRSPOT_I",
                "GOLDCOMEX_I","GOLDCOMEX_II","GOLDCOMEX_III",
                "SILVERCOMEX_I","SILVERCOMEX_II","SILVERCOMEX_III",
                "COPPERCOMEX_I","COPPERCOMEX_II","COPPERCOMEX_III",
                "DGINR_I","DGINR_II",
                "EURUSD_I","GBPUSD_I","USDSWISSFRANCE_I","INDIANRUPEES_I","USDYEN_I",
                "GOLDAM_I","GOLDPM_I","SILVERFIX_I",
                "DOWJONES_I","DOWJONES_II",
                "NASDAQ_I","NASDAQ_II",
                "S&P500_I","S&P500_II",
                "NIKKEI_I","NIKKEI_II",
                "FBIL_USD","FBIL_GBP","FBIL_EUR","FBIL_JPY",
                "DGINRSPOT_I",
                "XPTUSD_I","XPDUSD_I",
                "PLATINUMAM_I","PLATINUMPM_I",
                "PALLADIUMAM_I","PALLADIUMPM_I",
                "DGINRSPOT_II","CDUTY",
                "SILVERMIC_I","SILVERMIC_II","SILVERMIC_III"
            };
        }

        private async Task ReconnectWithBackoff()
        {
            int retry = 0;
            while (_connection.State != HubConnectionState.Connected)
            {
                retry++;
                int delay = Math.Min(5000, 1000 * retry);
                Console.WriteLine($"⏳ Reconnecting in {delay} ms...");
                await Task.Delay(delay);

                try
                {
                    await _connection.StartAsync();
                    Console.WriteLine("✅ Reconnected to SignalR Hub");
                    var symbolMaster = GetSymbolMaster();
                    await _connection.InvokeAsync("SubscribeSymbols", symbolMaster);
                    Console.WriteLine("📡 Subscribed to symbols successfully!");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Reconnect attempt failed: " + ex.Message);
                }
            }
        }

        private MarketDataDto ProcessExcelRate(string base64)
        {
            try
            {
                if (string.IsNullOrEmpty(base64))
                    return null;

                byte[] bytes = Convert.FromBase64String(base64);
                string json = DecompressGzip(bytes);
                MarketDataDto data = JsonConvert.DeserializeObject<MarketDataDto>(json);
                if (data == null || string.IsNullOrEmpty(data.i)) return null;

                // Optional: store in latest updates for tracking
                lock (_updateLock)
                {
                    _latestUpdates[data.i] = data;
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in ProcessExcelRate: " + ex.Message);
                return null;
            }
        }

        private void UpdateGrid(MarketDataDto data)
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.BeginInvoke(new Action(() => UpdateGrid(data)));
                return;
            }

            var existing = _marketDataList.FirstOrDefault(x => x.i == data.i);
            if (existing != null)
            {
                existing.n = data.n;
                existing.b = data.b;
                existing.a = data.a;
                existing.ltp = data.ltp;
                existing.h = data.h;
                existing.l = data.l;
                existing.o = data.o;
                existing.c = data.c;
                existing.d = data.d;
                existing.v = data.v;
                existing.t = TimeStampConvert(data.t);
                existing.atp = data.atp;
                existing.bq = data.bq;
                existing.tbq = data.tbq;
                existing.sq = data.sq;
                existing.tsq = data.tsq;
                existing.vt = data.vt;
                existing.oi = data.oi;
                existing.ltq = data.ltq;

                int index = _marketDataList.IndexOf(existing);
                _marketDataList.ResetItem(index);
            }
            else
            {
                _marketDataList.Add(data);
            }
        }

        private string DecompressGzip(byte[] data)
        {
            using (MemoryStream compressedStream = new MemoryStream(data))
            {
                using (GZipStream decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(decompressionStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static string TimeStampConvert(string timestamp)
        {
            long ts;
            if (long.TryParse(timestamp, out ts))
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(ts);
                return dateTimeOffset.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss:fff");
            }
            return timestamp;
        }

    }
}
