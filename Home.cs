using ClosedXML.Excel;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;
using Excel = Microsoft.Office.Interop.Excel;

namespace thecalcify
{
    public partial class thecalcify : Form
    {
        public string token, licenceDate, username, password;
        public bool _headersWritten = false;
        public int fontSize = 12;
        private DateTime _lastReconnectAttempt = DateTime.MinValue;
        private readonly TimeSpan _reconnectThrottle = TimeSpan.FromSeconds(10); // prevent spam
        public HubConnection connection;
        private ConcurrentQueue<MarketDataDTO> _updateQueue = new ConcurrentQueue<MarketDataDTO>();
        private System.Windows.Forms.Timer _updateTimer;
        private readonly Dictionary<string, DataRow> symbolRowMap = new Dictionary<string, DataRow>();
        private DateTime lastUiUpdate = DateTime.MinValue;
        //public bool isLoadedSymbol = false;
        //public List<string> selectedSymbols = new List<string>();
        //public List<string> symbolMaster = new List<string>();
        //public List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();
        //public List<string> identifiers;
        private System.Windows.Forms.Timer signalRTimer;
        public List<MarketDataDTO> pastRateTickDTO = new List<MarketDataDTO>();
        public MarketApiResponse resultdefault;
        public System.Data.DataTable marketDataTable = new System.Data.DataTable();
        public Common commonClass;
        private Dictionary<string, double> previousAskMap = new Dictionary<string, double>();
        public string[] numericColumns = new[] {
            "Bid", "Ask", "LTP", "High", "Low", "Open", "Close", "Net Chng",
            "ATP", "Bid Size", "Total Bid Size", "Ask Size",
            "Total Ask Size", "Volume", "Open Interest", "Last Size"
        };
        public List<string> columnPreferencesDefault = new List<string>()
        {
            "symbol",
            "Name",
            "Bid",
            "Ask",
            "High",
            "Low",
            "Open",
            "Close",
            "LTP",
            "Net Chng",
            "V",
            "Time",
            "ATP",
            "Bid Size",
            "Total Bid Size",
            "Ask Size",
            "Total Ask Size",
            "Volume",
            "Open Interest",
            "Last Size"
        };

        List<string> instruments = new List<string>
        {
            "GOLDFUTURE_I",
            "GOLDFUTURE_II",
            "GOLDFUTURE_III",
            "SILVERFUTURE_I",
            "SILVERFUTURE_II",
            "SILVERFUTURE_III",
            "GOLDM_I",
            "GOLDM_II",
            "SILVERM_I",
            "SILVERM_II",
            "GOLDSPOT_I",
            "SILVERSPOT_I",
            "INRSPOT_I",
            "GOLDCOMEX_I",
            "GOLDCOMEX_II",
            "SILVERCOMEX_I",
            "SILVERCOMEX_II",
            "DGINR_I",
            "DGINR_II",
            "GOLDAM_I",
            "GOLDPM_I",
            "SILVERFIX_I",
            "FBIL_USD",
            "DGINRSPOT_I",
            "CDUTY",
            "DGINRSPOT_II"
        };
        private readonly string excelFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "thecalcify", "thecalcify.xlsx");
        private Excel.Application excelApp;
        private Excel.Workbook workbook;
        private Excel.Worksheet worksheet;
        private bool _excelInitialized = false;



        public thecalcify()
        {
            InitializeComponent();
        }

        private async void Home_Load(object sender, EventArgs e)
        {
            commonClass = new Common(this);

            // --- UI SETUP (non-data related) ---
            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.KeyPreview = true;
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);


            // Get login info (if not already available)
            Login login = Login.CurrentInstance;
            token = login?.token ?? string.Empty;
            licenceDate = login?.licenceDate ?? string.Empty;
            username = login?.username ?? string.Empty;
            password = login?.userpassword ?? string.Empty;

            licenceExpire.Text = "Licence Expired On :- " + licenceDate;


            // --- FORM PROPERTIES ---
            this.WindowState = FormWindowState.Maximized;
            defaultGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

            // --- INITIALIZE DATA STRUCTURES ---
            marketDataTable = new System.Data.DataTable();
            SetupDataTable();
            BeginInvoke((MethodInvoker)(() =>
            {
                InitializeDataGridView();
            })); 
            await LoadInitialMarketDataAsync();
            SignalRTimer();
            await SignalREvent();

            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            KillProcess();


        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ApplicationLogger.LogException(e.Exception);
            MessageBox.Show("A fatal error occurred:\n" + e.Exception.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show("A fatal non-UI error occurred:\n" + ex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #region SignalR Methods
        public void SignalRTimer()
        {
            signalRTimer = new System.Windows.Forms.Timer { Interval = 10_000 };
            signalRTimer.Tick += async (s, e) => await TryReconnectAsync();
            signalRTimer.Start();
        }

        private async Task TryReconnectAsync()
        {
            if (connection?.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await SignalREvent();
                }
                catch (Exception ex) when (
                    ex is OperationCanceledException ||
                    ex is ObjectDisposedException ||
                    ex is TargetInvocationException ||
                    ex is InvalidOperationException)
                {
                    Console.WriteLine("SignalR reconnection attempt failed, retrying...");
                    ApplicationLogger.LogException(ex);
                    await SignalREvent();
                }
            }
        }

        private HubConnection BuildConnection()
        {
            return new HubConnectionBuilder()
                .WithUrl("http://api.thecalcify.com/excel?user=calcify&auth=Starline@1008&type=mobile", options =>
                {
                    options.Headers.Add("Origin", "http://api.thecalcify.com/");
                })
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task SignalREvent()
        {
            try
            {
                connection = BuildConnection();

                connection.On<string>("excelRate", OnExcelRateReceived);

                connection.Closed += async (error) =>
                {
                    Console.WriteLine("Connection closed");
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                };

                connection.Reconnected += async (connectionId) =>
                {
                    Console.WriteLine("Reconnected to SignalR hub");
                    try
                    {
                        await connection.InvokeAsync("SubscribeSymbols", instruments);
                        Console.WriteLine("Resubscribed after reconnect.");
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.LogException(ex);
                    }
                };

                await connection.StartAsync();

                if (connection.State == HubConnectionState.Connected)
                {
                    await connection.InvokeAsync("SubscribeSymbols", instruments);
                    SetupUpdateTimer();
                }
            }
            catch (TaskCanceledException ex)
            {
                ApplicationLogger.LogException(ex);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void SetupUpdateTimer()
        {
            _updateTimer = new System.Windows.Forms.Timer
            {
                Interval = 120
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void OnExcelRateReceived(string base64)
        {
            if (connection == null) return;

            try
            {
                lock (marketDataTable)
                {
                    CleanupEmptyRows();
                    AddMissingRows();
                }

                var json = DecompressGzip(Convert.FromBase64String(base64));
                var data = JsonConvert.DeserializeObject<MarketDataDTO>(json);
                if (data == null || !(instruments?.Contains(data.i) ?? false)) return;

                _updateQueue.Enqueue(data);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void CleanupEmptyRows()
        {
            var rowsToRemove = marketDataTable.AsEnumerable()
                .Where(row => row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
                .Where(row => row.Table.Columns.Contains("symbol"))
                .Where(row => row.Table.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName != "symbol")
                    .All(c => IsNullOrEmptyOrPlaceholder(row[c])))
                .ToList();

            foreach (var row in rowsToRemove)
            {
                marketDataTable.Rows.Remove(row);
            }
        }

        private void AddMissingRows()
        {
            foreach (var symbol in instruments)
            {
                if (!marketDataTable.AsEnumerable().Any(row => row.Field<string>("symbol") == symbol))
                {
                    var dto = pastRateTickDTO?.FirstOrDefault(x => x.i == symbol);
                    if (dto != null) AddRowFromDTO(dto);
                }
            }
        }

        private bool IsNullOrEmptyOrPlaceholder(object val)
        {
            return val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString()) || val.ToString() == "--";
        }

        private void AddRowFromDTO(MarketDataDTO dto)
        {
            var row = marketDataTable.NewRow();

            row["symbol"] = dto.i;
            row["Name"] = dto.n ?? "--";
            row["Bid"] = dto.b ?? "--";
            row["Ask"] = dto.a ?? "--";
            row["LTP"] = dto.ltp ?? "--";
            row["High"] = dto.h ?? "--";
            row["Low"] = dto.l ?? "--";
            row["Open"] = dto.o ?? "--";
            row["Close"] = dto.c ?? "--";
            row["V"] = dto.v ?? "--";
            row["Net Chng"] = dto.d ?? "--";
            row["Time"] = commonClass.timeStampConvert(dto.t);
            row["ATP"] = dto.atp ?? "--";
            row["Bid Size"] = dto.bq ?? "--";
            row["Total Bid Size"] = dto.tbq ?? "--";
            row["Ask Size"] = dto.sq ?? "--";
            row["Total Ask Size"] = dto.tsq ?? "--";
            row["Volume"] = dto.vt ?? "--";
            row["Open Interest"] = dto.oi ?? "--";
            row["Last Size"] = dto.ltq ?? "--";

            marketDataTable.Rows.Add(row);
            symbolRowMap[dto.i] = row;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_updateQueue.IsEmpty) return;

            var updates = new List<MarketDataDTO>();
            while (_updateQueue.TryDequeue(out var data))
            {
                updates.Add(data);
            }

            if (updates.Count == 0) return;


            // If queue has too many records, keep only the newest 1000
            if (updates.Count > 1000)
            {
                // Sort by Time (assuming MarketDataDTO has a Time property)
                updates = updates
                    .OrderByDescending(x => x.t)  // Newest first
                    .Take(1000)                     // Keep only 1000 newest
                    .OrderBy(x => x.t)           // Restore original order if needed
                    .ToList();
            }


            try
            {

                updates = updates.Where(x => !string.IsNullOrEmpty(x.t) && x.t != "N/A").OrderByDescending(x => DateTime.ParseExact(x.t, "hh:mm:ss tt", CultureInfo.InvariantCulture)).ToList();

            }
            catch (Exception)
            {
                //ApplicationLogger.LogException(ex);
            }

            if (updates != null)
            {
                if (defaultGrid.InvokeRequired)
                {
                    defaultGrid.BeginInvoke((MethodInvoker)(() => ApplyBatchUpdates(updates)));
                }
                else
                    ApplyBatchUpdates(updates);
            }
        }

        private void ApplyBatchUpdates(List<MarketDataDTO> updates)
        {
            try
            {
                defaultGrid.SuspendLayout();

                int count = instruments.Count;

                foreach (var newData in updates)
                {
                    if (newData == null || string.IsNullOrEmpty(newData.i)) continue;

                    // Find or create row
                    if (!symbolRowMap.TryGetValue(newData.i, out var row))
                    {
                        row = marketDataTable.NewRow();
                        row["symbol"] = newData.i;
                        row["Name"] = "N/A"; // Initialize Name if needed
                        marketDataTable.Rows.Add(row);
                        symbolRowMap[newData.i] = row;
                    }

                    // Keep previous values before update
                    object[] previousRow = row.ItemArray.Clone() as object[];

                    // Update data
                    if (row["Name"].ToString() == "N/A")
                    {
                        // Find the symbol in pastRateTickDTO and get the name
                        var symbolName = pastRateTickDTO?
                            .FirstOrDefault(x => x.i == newData.i)?.n ?? "N/A";

                        UpdateRowValue(row, "Name", symbolName);
                    }
                    UpdateRowValue(row, "Bid", newData.b);
                    UpdateRowValue(row, "Ask", newData.a);
                    UpdateRowValue(row, "LTP", newData.ltp);
                    UpdateRowValue(row, "High", newData.h);
                    UpdateRowValue(row, "Low", newData.l);
                    UpdateRowValue(row, "Open", newData.o);
                    UpdateRowValue(row, "Close", newData.c);
                    UpdateRowValue(row, "Net Chng", newData.d);
                    UpdateRowValue(row, "V", newData.v);
                    UpdateRowValue(row, "ATP", newData.atp);
                    UpdateRowValue(row, "Bid Size", newData.bq);
                    UpdateRowValue(row, "Total Bid Size", newData.tbq);
                    UpdateRowValue(row, "Ask Size", newData.sq);
                    UpdateRowValue(row, "Total Ask Size", newData.tsq);
                    UpdateRowValue(row, "Volume", newData.vt);
                    UpdateRowValue(row, "Open Interest", newData.oi);
                    UpdateRowValue(row, "Last Size", newData.ltq);
                    UpdateRowValue(row, "Time", commonClass.timeStampConvert(newData.t));

                    // Track Ask price change
                    bool hasAskChange = false;
                    int askDirection = 0; // 1 for up, -1 for down
                    string askValue = newData.a?.ToString();

                    try
                    {
                        if (!string.IsNullOrEmpty(askValue)
                                        && double.TryParse(askValue, out double newAsk))
                        {
                            if (previousAskMap.TryGetValue(newData.i, out double previousAsk))
                            {
                                if (newAsk > previousAsk)
                                {
                                    askDirection = 1;
                                    hasAskChange = true;
                                }
                                else if (newAsk < previousAsk)
                                {
                                    askDirection = -1;
                                    hasAskChange = true;
                                }
                            }

                            previousAskMap[newData.i] = newAsk;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing rate value at ApplyBatch: " + ex.Message);
                    }

                    // Update DataGridView row
                    var dgvRow = defaultGrid.Rows
                        .Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.Cells["symbol"].Value?.ToString() == newData.i);

                    if (dgvRow != null)
                    {
                        // Color numeric columns based on value change
                        foreach (var colName in numericColumns)
                        {
                            if (!marketDataTable.Columns.Contains(colName)) continue;
                            if (!defaultGrid.Columns.Contains(colName)) continue;

                            var cell = dgvRow.Cells[colName];
                            var colIndex = marketDataTable.Columns[colName].Ordinal;
                            var oldVal = previousRow[colIndex];
                            var newVal = row[colName];

                            if (IsNumericChange(oldVal, newVal, out var changeDirection))
                            {
                                if (changeDirection == 1)
                                    cell.Style.ForeColor = Color.Green;
                                else if (changeDirection == -1)
                                    cell.Style.ForeColor = Color.Red;
                            }
                        }

                        var nameCell = dgvRow.Cells["Name"];

                        // Update "Name" column with arrow and color based on Ask direction
                        // Get current name value and remove any existing arrows
                        string rawName = row["Name"]?.ToString() ?? string.Empty;
                        string baseName = rawName.Replace(" ▲", "").Replace(" ▼", "").Trim();
                        Color color = nameCell.Style.ForeColor;

                        if (hasAskChange)
                        {
                            if (askDirection == 1)
                            {
                                nameCell.Value = $"{baseName} ▲";
                                nameCell.Style.ForeColor = Color.Green;
                            }
                            else if (askDirection == -1)
                            {
                                nameCell.Value = $"{baseName} ▼";
                                nameCell.Style.ForeColor = Color.Red;
                            }
                        }
                    }
                }

                //RequestExcelUpdate(defaultGrid);

                // Just before Notify
                //if (MarketDataEventHub.Instance.SubscriberCount == 0)
                //{
                //    ApplicationLogger.Log("No subscribers to MarketDataEventHub.");
                //}

                UpdateExcelDataEfficiently(defaultGrid);

                // Throttle UI updates
                if ((DateTime.Now - lastUiUpdate).TotalMilliseconds > 120)
                {
                    defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
                    defaultGrid.RowHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 1.5f, FontStyle.Bold);
                    //defaultGrid.Invalidate();
                    lastUiUpdate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch update: {ex}");
                //ApplicationLogger.LogException(ex);
            }
            finally
            {
                defaultGrid.ResumeLayout();
            }
        }

        private void UpdateRowValue(DataRow row, string columnName, object value)
        {
            if (!row.Table.Columns.Contains(columnName)) return;

            try
            {
                var currentValue = row[columnName];

                // Default: keep original value (e.g., "--", "N/A", "text") unless it's null
                var newValue = value ?? "";


                // If it's a numeric column, try to parse the value
                if (IsNumericColumn(columnName))
                {
                    try
                    {
                        // Only parse if it's not "--" or empty
                        if (value is string s &&
                        s != "--" &&
                        !string.IsNullOrWhiteSpace(s) &&
                        decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                        {
                            newValue = decimalValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing rate value at UpdateRowValue: " + ex.Message);
                    }
                }

                // Update only if changed
                if (!object.Equals(currentValue, newValue))
                {
                    row[columnName] = newValue;
                }
            }
            catch
            {
                // Fallback: set raw value
                row[columnName] = value ?? "";
            }
        }

        private bool IsNumericColumn(string columnName)
        {
            return columnName == "Bid" ||
                   columnName == "Ask" ||
                   columnName == "LTP" ||
                   columnName == "High" ||
                   columnName == "Low" ||
                   columnName == "Open" ||
                   columnName == "Close" ||
                   columnName == "Net Chng" ||
                   columnName == "ATP" ||
                   columnName == "Bid Size" ||
                   columnName == "Total Bid Size" ||
                   columnName == "Ask Size" ||
                   columnName == "Total Ask Size" ||
                   columnName == "Volume" ||
                   columnName == "Open Interest" ||
                   columnName == "Last Size";
        }

        private void disconnectESCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Switch to LoginForm and dispose current form
            Login loginForm = new Login();
            loginForm.Show();

            this.Hide();
            this.Dispose();
        }

        private bool IsNumericChange(object oldValue, object newValue, out int changeDirection)
        {
            changeDirection = 0;
            const decimal tolerance = 0.0000001m;

            try
            {
                if (oldValue == DBNull.Value) oldValue = null;
                if (newValue == DBNull.Value) newValue = null;

                if (oldValue == null || newValue == null || oldValue.ToString() == "--" || oldValue.ToString() == "N/A" || newValue.ToString() == "--" || newValue.ToString() == "N/A")
                    return false;


                decimal oldDec = oldValue == null ? 0 : Convert.ToDecimal(oldValue);
                decimal newDec = newValue == null ? 0 : Convert.ToDecimal(newValue);

                if (Math.Abs(newDec - oldDec) > tolerance)
                {
                    changeDirection = newDec > oldDec ? 1 : -1;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void BuildSymbolRowMap()
        {
            symbolRowMap.Clear();
            foreach (DataRow row in marketDataTable.Rows)
            {
                var symbol = row["symbol"]?.ToString();
                if (!string.IsNullOrEmpty(symbol))
                {
                    symbolRowMap[symbol] = row;
                }
            }
        }

        private string DecompressGzip(byte[] compressed)
        {
            using (var input = new MemoryStream(compressed))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return Encoding.UTF8.GetString(output.ToArray());
            }
        }

        #endregion

        #region SignalR Helper Method
        public async Task LoadInitialMarketDataAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "http://api.thecalcify.com/getInstrument");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Use async call instead of .Result
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        return;
                    }

                    string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    resultdefault = JsonConvert.DeserializeObject<MarketApiResponse>(jsonString);

                    if (resultdefault?.data != null)
                    {
                        // Filter out instruments not in the valid list
                        resultdefault.data = resultdefault.data
                            .Where(x => !string.IsNullOrEmpty(x.i) && instruments.Contains(x.i))
                            .ToList();

                        // Update on UI thread
                        this.Invoke((MethodInvoker)delegate
                        {
                            pastRateTickDTO = resultdefault.data;

                            if (resultdefault.data != null && marketDataTable.Columns.Contains("symbol") )
                            {
                                ApplyBatchUpdates(resultdefault.data);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading initial market data: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
        }

        private void SetupDataTable()
        {
            marketDataTable.Clear();
            marketDataTable.Columns.Clear();


            string[] columns = {
                "symbol", "Name", "Bid", "Ask", "LTP", "High", "Low", "Open", "Close", "Net Chng", "ATP",
                "Bid Size", "Total Bid Size", "Ask Size", "Total Ask Size", "Volume", "Open Interest", "Last Size", "V", "Time"
            };

            Type[] types = {
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)
            };

            for (int i = 0; i < columns.Length; i++) marketDataTable.Columns.Add(columns[i], types[i]);

            foreach (var symbol in instruments)
            {
                marketDataTable.Rows.Add(
                    symbol,         // symbol
                    "N/A",          // Name
                    "N/A",          // Bid
                    "N/A",          // Ask
                    "N/A",          // LTP
                    "N/A",          // High
                    "N/A",          // Low
                    "N/A",          // Open
                    "N/A",          // Close
                    "N/A",          // Net Chng
                    "N/A",          // ATP
                    "N/A",          // Bid Size
                    "N/A",          // Total Bid Size
                    "N/A",          // Ask Size
                    "N/A",          // Total Ask Size
                    "N/A",          // Volume
                    "N/A",          // Open Interest
                    "N/A",          // Last Size
                    "N/A",          // V
                    "N/A"           // Time
                );
            }

            foreach (DataColumn column in marketDataTable.Columns)
                if (!columnPreferencesDefault.Contains(column.ColumnName))
                    column.ColumnMapping = MappingType.Hidden; // ✅ Call symbol map builder here

            BuildSymbolRowMap();
            if (resultdefault != null && resultdefault.data != null)
            {
                ApplyBatchUpdates(resultdefault.data);
            }
        }

        private void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void InitializeDataGridView()
        {
            defaultGrid.SuspendLayout();
            defaultGrid.DataSource = null;
            defaultGrid.Columns.Clear();

            defaultGrid.AutoGenerateColumns = true;
            defaultGrid.AllowUserToAddRows = false;
            defaultGrid.DataSource = marketDataTable;
            defaultGrid.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal | System.Windows.Forms.ScrollBars.Vertical;

            // Manually recreate columns if mismatch
            if (defaultGrid.ColumnCount != marketDataTable.Columns.Count)
            {
                defaultGrid.Columns.Clear();
                foreach (DataColumn col in marketDataTable.Columns)
                {
                    var gridCol = new DataGridViewTextBoxColumn
                    {
                        Name = col.ColumnName,
                        HeaderText = col.ColumnName,
                        DataPropertyName = col.ColumnName,
                        ReadOnly = true
                    };
                    defaultGrid.Columns.Add(gridCol);
                }

                defaultGrid.DataSource = marketDataTable;
            }

            // Immediately hide non-preferred columns before resume layout
            foreach (DataGridViewColumn col in defaultGrid.Columns)
            {
                if (!columnPreferencesDefault.Contains(col.Name))
                {
                    col.Visible = false; // 🔑 This prevents flicker!
                    continue;
                }

                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.Automatic;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.Resizable = DataGridViewTriState.True;

                switch (col.Name)
                {
                    case "Name": col.Width = 210; break;
                    case "Time": col.Width = 250; break;
                    case "Bid":
                    case "Ask":
                    case "LTP":
                    case "High":
                    case "Low":
                    case "Open":
                    case "ATP":
                    case "Close":
                        col.Width = 150;
                        break;
                    case "Volume":
                    case "Total Ask Size":
                    case "Total Bid Size":
                        col.Width = 120;
                        break;
                    case "Last Size":
                    case "Net Chng":
                    case "Bid Size":
                    case "Ask Size":
                    case "Open Interest":
                    default:
                        col.Width = 100;
                        break;
                }
            }

            defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, FontStyle.Regular);
            defaultGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, FontStyle.Bold);
            defaultGrid.ColumnHeadersHeight = 70;
            defaultGrid.AllowUserToResizeColumns = true;

            defaultGrid.ResumeLayout();

            Console.WriteLine("Client Size: " + defaultGrid.ClientSize);
            Console.WriteLine("Display Rectangle: " + defaultGrid.DisplayRectangle);
            Console.WriteLine("Row count: " + defaultGrid.Rows.Count);
            Console.WriteLine("Columns: " + defaultGrid.Columns.Count);

            // Enable smooth scrolling
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                defaultGrid,
                new object[] { true }
            );

            defaultGrid.CellMouseDown += (s, e) =>
            {
                Console.WriteLine($"CellMouseDown at Row {e.RowIndex}, Col {e.ColumnIndex}");
            };

            defaultGrid.MouseClick += (s, e) =>
            {
                Console.WriteLine($"MouseClick: Button={e.Button}");
            };

        }

        private void DefaultGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Force-hide specific columns
            if (defaultGrid.Columns.Contains("symbol"))
                defaultGrid.Columns["symbol"].Visible = false;
            if (defaultGrid.Columns.Contains("V"))
                defaultGrid.Columns["V"].Visible = false;
        }

        private void DefaultGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value?.ToString() == "N/A")
                e.CellStyle.ForeColor = Color.Gray;
        }

        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
                AttemptReconnect("Network availability restored.");
            else
                ApplicationLogger.Log("Network unavailable.");
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            AttemptReconnect("Network address changed.");
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                AttemptReconnect("System resumed from sleep/hibernate.");
        }

        private async void AttemptReconnect(string reason)
        {
            if (DateTime.Now - _lastReconnectAttempt < _reconnectThrottle)
                return;

            _lastReconnectAttempt = DateTime.Now;
            ApplicationLogger.Log($"Attempting reconnect due to: {reason}");

            if (connection == null || connection.State != HubConnectionState.Connected)
            {
                try
                {
                    await connection.StartAsync();
                    await connection.InvokeAsync("SubscribeSymbols", instruments);
                    ApplicationLogger.Log("Reconnected and resubscribed.");
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"Reconnect failed: {ex.Message}");
                }
            }
        }

        private bool _isResizing = false;

        private void ResizeDataGridToFitWindow()
        {
            if (_isResizing) return;

            try
            {
                _isResizing = true;

                int availableHeight = this.ClientSize.Height;
                int availableWidth = this.ClientSize.Width;

                if (menuStrip1 != null)
                    availableHeight -= menuStrip1.Height;

                if (statusStrip1 != null)
                    availableHeight -= statusStrip1.Height;

                defaultGrid.Location = new System.Drawing.Point(0, menuStrip1?.Height ?? 0);
                defaultGrid.Size = new Size(availableWidth, availableHeight);
            }
            finally
            {
                _isResizing = false;
            }
        }



        #endregion

        #region Excel Export

        private void ExportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportExcelOnClick();
        }

        public void ExportExcelOnClick()
        {
            Thread excelThread = new Thread(() =>
            {

                try
                {
                    string folderPath = Path.GetDirectoryName(excelFilePath);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    if (!File.Exists(excelFilePath))
                    {
                        // Create new Excel application
                        excelApp = new Excel.Application
                        {
                            Visible = false,
                            DisplayAlerts = false
                        };

                        workbook = excelApp.Workbooks.Add();
                        worksheet = (Excel.Worksheet)workbook.Sheets[1];
                        worksheet.Name = "Sheet1";

                        // Write headers (excluding "symbol" and "V")
                        int headerIndex = 1;
                        foreach (DataColumn column in marketDataTable.Columns)
                        {
                            if (column.ColumnName == "symbol" || column.ColumnName == "V") continue;
                            worksheet.Cells[1, headerIndex++] = column.ColumnName;
                        }

                        // Save new file
                        workbook.SaveAs(
                            excelFilePath,
                            Excel.XlFileFormat.xlOpenXMLWorkbook);

                        workbook.Close(false);
                        excelApp.Quit();

                        ReleaseExcelObjects(worksheet, workbook, excelApp);

                        worksheet = null;
                        workbook = null;
                        excelApp = null;

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    EnsureFullFolderAccess(folderPath);

                    // Open existing file
                    excelApp = new Excel.Application
                    {
                        Visible = false,
                        DisplayAlerts = false,
                        UserControl = true,
                        //IgnoreRemoteRequests = true
                    };

                    workbook = excelApp.Workbooks.Open(excelFilePath);
                    worksheet = workbook.Sheets[1] as Excel.Worksheet;

                    if (worksheet == null)
                        worksheet = (Excel.Worksheet)workbook.Sheets[1];

                    var visibleColumns = marketDataTable.Columns.Cast<DataColumn>()
                        .Where(c => c.ColumnName != "symbol" && c.ColumnName != "V")
                        .ToList();

                    // Write headers again (overwrite)
                    for (int col = 0; col < visibleColumns.Count; col++)
                    {
                        worksheet.Cells[1, col + 1] = visibleColumns[col].ColumnName;
                    }

                    int rowCount = marketDataTable.Rows.Count;
                    int colCount = visibleColumns.Count;
                    object[,] dataArray = new object[rowCount, colCount];

                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            dataArray[r, c] = marketDataTable.Rows[r][visibleColumns[c].ColumnName];
                        }
                    }

                    Excel.Range startCell = worksheet.Cells[2, 1];
                    Excel.Range endCell = worksheet.Cells[rowCount + 1, colCount];
                    Excel.Range writeRange = worksheet.Range[startCell, endCell];
                    writeRange.Value2 = dataArray;

                    workbook.Save();

                    // Optional: Show Excel
                    excelApp.Visible = true;
                    excelApp.WindowState = Excel.XlWindowState.xlMaximized;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"Error accessing Excel instance ExportExcelOnClick: {ex.Message} And {ex.StackTrace}");
                }
            });

            excelThread.SetApartmentState(ApartmentState.STA);
            excelThread.Start();
        }

        private void ReleaseExcelObjects(params object[] comObjects)
        {
            foreach (var obj in comObjects)
            {
                if (obj != null)
                {
                    try
                    {
                        while (Marshal.ReleaseComObject(obj) > 0) { }
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.Log($"[COM Release Error] {ex.Message}");
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void EnsureFullFolderAccess(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            var dirSecurity = dirInfo.GetAccessControl();

            var accessRule = new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow
            );

            if (dirSecurity.ModifyAccessRule(AccessControlModification.Add, accessRule, out bool modified) && modified)
            {
                dirInfo.SetAccessControl(dirSecurity);
            }
        }

        public void UpdateExcelDataEfficiently(DataGridView grid)
        {
            if (!_excelInitialized && !TryInitializeExcel())
                return;

            try
            {
                var visibleCols = grid.Columns.Cast<DataGridViewColumn>()
                    .Where(c => c.Visible)
                    .OrderBy(c => c.DisplayIndex)
                    .ToList();

                int rows = grid.Rows.Count;
                int cols = visibleCols.Count;
                if (rows == 0 || cols == 0)
                    return;

                object[,] data = new object[rows, cols];
                int timeColIdx = visibleCols.FindIndex(c => c.Name == "Time");

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var val = grid.Rows[r].Cells[visibleCols[c].Name].Value;
                        data[r, c] = (c == timeColIdx && DateTime.TryParse(val?.ToString(), out var dt)) ? dt : val;
                    }
                }

                var range = RetryComCall(() => worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[rows + 1, cols]]);
                RetryComCall(() => range.Value2 = data);
                Marshal.ReleaseComObject(range);

                if (timeColIdx >= 0)
                {
                    var timeRange = RetryComCall(() => worksheet.Range[
                        worksheet.Cells[2, timeColIdx + 1], worksheet.Cells[rows + 1, timeColIdx + 1]]);
                    RetryComCall(() => timeRange.NumberFormat = "dd/MM/yyyy HH:mm:ss");
                    Marshal.ReleaseComObject(timeRange);
                }
            }
            catch (COMException ex)
            {
                ApplicationLogger.Log($"[Excel Update COM Error] {ex.Message}");
                _excelInitialized = false;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[Excel Update Error] {ex.Message}");
                _excelInitialized = false;
            }
        }

        private Excel.Application GetRunningExcelInstance()
        {
            try
            {
                dynamic tempWorkbook = Marshal.BindToMoniker(excelFilePath);
                Excel.Application tempExcelApp = tempWorkbook.Application;

                ((Excel.AppEvents_Event)tempExcelApp).NewWorkbook += ExcelApp_NewWorkbook;

                try
                {
                    if (tempExcelApp.Ready)
                        tempExcelApp.IgnoreRemoteRequests = true;
                }
                catch (COMException ex)
                {
                    Console.WriteLine($"IgnoreRemoteRequests failed: {ex.Message}");
                }

                return tempExcelApp;
            }
            catch { }

            foreach (var process in Process.GetProcessesByName("EXCEL"))
            {
                try
                {
                    var obj = Marshal.GetActiveObject("Excel.Application");
                    if (obj is Excel.Application app)
                    {
                        foreach (Excel.Workbook wb in app.Workbooks)
                        {
                            if (wb.Name.Equals("thecalcify.xlsx", StringComparison.OrdinalIgnoreCase))
                            {
                                ((Excel.AppEvents_Event)app).NewWorkbook += ExcelApp_NewWorkbook;

                                try
                                {
                                    if (app.Ready)
                                        app.IgnoreRemoteRequests = true;
                                }
                                catch (COMException ex)
                                {
                                    Console.WriteLine($"IgnoreRemoteRequests failed: {ex.Message}");
                                }

                                return app;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing Excel instance: {ex.Message}");
                    ApplicationLogger.Log($"Error accessing Excel instance: {ex.Message} And {ex.StackTrace}");
                }
            }

            Console.WriteLine("No instance found with the workbook 'thecalcify.xlsx'");
            return null;
        }

        private void ExcelApp_NewWorkbook(Excel.Workbook wb)
        {
            wb.Close(false);
            excelApp.StatusBar = "New workbook creation is disabled";
            Console.WriteLine("New workbook creation is disabled.");
        }

        private void DefaultGrid_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_excelInitialized && !TryInitializeExcel())
                    return;

                Range usedRange = worksheet.UsedRange;
                int totalRows = usedRange.Rows.Count;
                int totalCols = usedRange.Columns.Count;

                if (totalRows > 1 && totalCols > 0)
                {
                    Range clearRange = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[totalRows, totalCols]];
                    clearRange.ClearContents();
                    Marshal.ReleaseComObject(clearRange);
                }

                Marshal.ReleaseComObject(usedRange);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[DefaultGrid_DataSourceChanged] Stuck On : {ex.Message}");
            }
        }

        private void thecalcify_Layout(object sender, LayoutEventArgs e)
        {
            ResizeDataGridToFitWindow();
        }

        private T RetryComCall<T>(Func<T> comFunc, int retries = 5, int delayMs = 200, bool skipOnFailure = false)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    return comFunc();
                }
                catch (COMException ex) when (
                    (uint)ex.HResult == 0x800AC472 || // Excel busy
                    (uint)ex.HResult == 0x80010001 || // Call rejected
                    (uint)ex.HResult == 0x800706BE || // RPC failed
                    (uint)ex.HResult == 0x800706BA || // RPC unavailable
                    (uint)ex.HResult == 0x800A01A8 || // RPC unavailable
                    (uint)ex.HResult == 0x800401E3)    // MK_E_UNAVAILABLE
                {
                    if (attempt == retries)
                    {
                        if (skipOnFailure)
                        {
                            ApplicationLogger.LogException(ex);
                            return default;
                        }

                        throw;
                    }

                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(delayMs);
                }
            }

            return default;
        }

        public bool TryInitializeExcel()
        {
            try
            {
                excelApp = GetRunningExcelInstance();
                if (excelApp == null)
                    return false;

                workbook = RetryComCall(() => excelApp.Workbooks
                    .Cast<Excel.Workbook>()
                    .FirstOrDefault(w => w.Name.Equals("thecalcify.xlsx", StringComparison.OrdinalIgnoreCase)));

                if (workbook == null)
                    return false;

                worksheet = RetryComCall(() => (Excel.Worksheet)workbook.Sheets["Sheet1"]);

                _excelInitialized = worksheet != null;
                return _excelInitialized;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[Excel Init Error] {ex.Message}");
                return false;
            }
        }

        public void KillProcess() 
        {
            // Kill any EXCEL processes without a main window (ghost/background instances)
            foreach (var process in Process.GetProcessesByName("EXCEL"))
            {
                try
                {
                    if (string.IsNullOrEmpty(process.MainWindowTitle))
                    {
                        process.Kill();
                        process.WaitForExit(); // ensure it's gone
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error killing Excel process: " + ex.Message);
                    ApplicationLogger.LogException(ex);
                }
            }
        }

        #endregion
    }
}
