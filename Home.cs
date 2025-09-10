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
using thecalcify.MarketWatch;
using Button = System.Windows.Forms.Button;
using Excel = Microsoft.Office.Interop.Excel;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace thecalcify
{
    public partial class thecalcify : Form
    {
        public string token, licenceDate, username, password;

        public bool _headersWritten = false, _isResizing = false;
        public int fontSize = 12, RemainingDays;
        private bool isRunning = true;
        private DateTime _lastReconnectAttempt = DateTime.MinValue;
        private readonly TimeSpan _reconnectThrottle = TimeSpan.FromSeconds(10); // prevent spam
        public HubConnection connection;
        private ConcurrentQueue<MarketDataDTO> _updateQueue = new ConcurrentQueue<MarketDataDTO>();
        private System.Windows.Forms.Timer _updateTimer;
        private readonly Dictionary<string, int> symbolRowMap = new Dictionary<string, int>();
        private DateTime lastUiUpdate = DateTime.MinValue;
        public List<string> identifiers;
        public List<string> selectedSymbols = new List<string>();
        public bool isLoadedSymbol = false;
        private System.Windows.Forms.Timer signalRTimer;
        public List<MarketDataDTO> pastRateTickDTO = new List<MarketDataDTO>();
        public MarketApiResponse resultdefault;
        //public System.Data.DataTable marketDataTable = new System.Data.DataTable();
        public Common commonClass;
        public List<string> symbolMaster = new List<string>();
        //public List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();
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

        List<string> instruments = new List<string>();
        //{
        //    "GOLDFUTURE_I",
        //    "GOLDFUTURE_II",
        //    "GOLDFUTURE_III",
        //    "SILVERFUTURE_I",
        //    "SILVERFUTURE_II",
        //    "SILVERFUTURE_III",
        //    "GOLDM_I",
        //    "GOLDM_II",
        //    "SILVERM_I",
        //    "SILVERM_II",
        //    "GOLDSPOT_I",
        //    "SILVERSPOT_I",
        //    "INRSPOT_I",
        //    "GOLDCOMEX_I",
        //    "GOLDCOMEX_II",
        //    "SILVERCOMEX_I",
        //    "SILVERCOMEX_II",
        //    "DGINR_I",
        //    "DGINR_II",
        //    "GOLDAM_I",
        //    "GOLDPM_I",
        //    "SILVERFIX_I",
        //    "FBIL_USD",
        //    "DGINRSPOT_I",
        //    "CDUTY",
        //    "DGINRSPOT_II"
        //};
        private readonly string excelFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "thecalcify", "thecalcify.xlsx");
        private Excel.Application excelApp;
        private Excel.Workbook workbook;
        private Excel.Worksheet worksheet;
        public bool isEdit = false;
        private readonly Dictionary<string, decimal> previousAsks = new Dictionary<string, decimal>();
        private bool _excelInitialized = false;
        private bool isFullScreen = false;
        private System.Drawing.Rectangle _dragBoxFromMouseDown = System.Drawing.Rectangle.Empty, prevBounds;
        private FormWindowState prevState;
        private FormBorderStyle prevStyle;
        public string saveFileName;
        private Thread licenceThread;
        public bool isGrid = true, reloadGrid = true;
        private CheckedListBox checkedListColumns;
        private Button btnSelectAllColumns;
        private Button btnConfirmAddColumns;
        private Button btnCancelAddColumns;
        public bool isdeleted = false;
        public readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "thecalcify");
        public static thecalcify CurrentInstance { get; private set; }
        public List<string> FileLists = new List<string>();
        public List<string> columnPreferences;
        public string lastOpenMarketWatch = string.Empty;
        public enum MarketWatchViewMode
        {
            Default,
            New
        }
        public MarketWatchViewMode marketWatchViewMode = MarketWatchViewMode.Default;
        private readonly object _tableLock = new object();
        private readonly object _reconnectLock = new object();
        public ConnectionViewMode connectionViewMode = ConnectionViewMode.Connect;
        public enum ConnectionViewMode
        {
            Connect,
            Disconnect
        }
        public List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();


        public thecalcify()
        {
            InitializeComponent();
        }

        private async void Home_Load(object sender, EventArgs e)
        {
            commonClass = new Common();

            // --- UI SETUP (non-data related) ---
            this.AutoScaleMode = AutoScaleMode.Dpi;

            this.KeyPreview = true;
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            // --- PARALLEL INITIALIZATION ---
            var initializationTasks = new List<Task>();

            // Get login info (if not already available)
            Login login = Login.CurrentInstance;
            token = login?.token ?? string.Empty;
            licenceDate = login?.licenceDate ?? string.Empty;
            username = login?.username ?? string.Empty;
            password = login?.userpassword ?? string.Empty;

            DateTime txtlicenceDate = DateTime.Parse(licenceDate);
            DateTime currentDate = DateTime.Now.Date;
            TimeSpan diff = txtlicenceDate - currentDate;
            RemainingDays = diff.Days;
            if (RemainingDays <= 7)
            {
                licenceThread = new Thread(new ThreadStart(CheckLicenceLoop));
                licenceThread.IsBackground = true; // Thread will close when app closes
                licenceThread.Start();
            }
            else
            {
                licenceExpire.Text = licenceExpire.Text + licenceDate;
            }
            initializationTasks.Add(Task.Run(() =>
            {
                // --- COMMON CLASS ---
                commonClass = new Common(this);
                commonClass.StartInternetMonitor();

                // --- MARKET WATCH + COLUMNS ---
                var (currentWatch, currentColumns) = CredentialManager.GetCurrentMarketWatchWithColumns();
                lastOpenMarketWatch = currentWatch ?? "Default";
                columnPreferences = (currentColumns?.Count == 0 || currentColumns == null) ?
                    (columnPreferencesDefault ?? new List<string>()) : currentColumns;
            }));

            //licenceExpire.Text = "Licence Expired On :- " + licenceDate;
            MenuLoad();


            // --- LOAD INITIAL DATA ASYNCHRONOUSLY ---
            await LoadInitialMarketDataAsync();

            // --- FORM PROPERTIES ---
            this.WindowState = FormWindowState.Maximized;
            defaultGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

            CurrentInstance = this;

            // --- INITIALIZE DATA STRUCTURES ---
            //marketDataTable = new System.Data.DataTable();
            //SetupDataTable();
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
        private void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            isRunning = false;

            try
            {
                KillProcess();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

            System.Windows.Forms.Application.Exit();
        }


        /// <summary>
        /// Method Used to check licence lable update 
        /// </summary>
        private void CheckLicenceLoop()
        {
            try
            {
                while (isRunning)
                {
                    DateTime txtlicenceDate = DateTime.Parse(licenceDate);
                    DateTime currentDate = DateTime.Now.Date;
                    TimeSpan diff = txtlicenceDate - currentDate;
                    int licenceRemainingDays = diff.Days;

                    if (!this.IsHandleCreated || this.IsDisposed)
                        break; // Exit if form is disposed or handle not created

                    try
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)(() =>
                            {
                                if (!this.IsDisposed)
                                    UpdateLicenceLabel(licenceRemainingDays);
                            }));
                        }
                        else
                        {
                            if (!this.IsDisposed)
                                UpdateLicenceLabel(licenceRemainingDays);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Form is disposed during invoke — safely exit
                        break;
                    }

                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void UpdateLicenceLabel(int licenceRemainingDays)
        {
            if (licenceRemainingDays < 0)
            {
                //Console.WriteLine("❌ Licence expired. Application will now exit.");
                isRunning = false;
                Login login = new Login();
                login.Show();

                //this.Close();
                this.Hide();
                this.Dispose();
                return;
            }
            else if (licenceRemainingDays <= 7)
            {
                licenceExpire.Text = $"⚠ Licence expires in {licenceRemainingDays} days!";
                licenceExpire.ForeColor = Color.Red;
                licenceExpire.Visible = !licenceExpire.Visible; // blink
            }
            else
            {
                licenceExpire.Text = $"Licence valid for {licenceRemainingDays} days.";
                licenceExpire.ForeColor = Color.Green;
            }
        }

        public void LiveRateGrid()
        {
            if (!isLoadedSymbol)
                marketWatchViewMode = MarketWatchViewMode.Default;

            // Hide the DataGridView
            defaultGrid.Visible = true;
            defaultGrid.BringToFront();
            defaultGrid.Focus();
            newMarketWatchMenuItem.Enabled = true;
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
                        if (selectedSymbols.Count != 0)
                            identifiers = new List<string>(selectedSymbols);

                        await connection.InvokeAsync("SubscribeSymbols", identifiers);
                        Console.WriteLine("Resubscribed after reconnect.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to resubscribe after reconnect.");
                        ApplicationLogger.LogException(ex);
                    }
                };

                var currentIdentifiers = new List<string>(identifiers); // snapshot copy

                //for (int attempt = 0; attempt < 3; attempt++)
                //{
                //    try
                //    {
                //        await connection.StartAsync();
                //        break; // success
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
                //        if (attempt < 2) await Task.Delay(3000);
                //    }
                //}
                await connection.StartAsync();


                try
                {
                    if (connection.State == HubConnectionState.Connected)
                    {
                        if (selectedSymbols.Count != 0)
                            identifiers = new List<string>(selectedSymbols);

                        if (currentIdentifiers.Count() != identifiers.Count())
                            identifiers = currentIdentifiers;

                        await connection.InvokeAsync("SubscribeSymbols", identifiers);
                        SetupUpdateTimer();
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine("SignalR task canceled: likely due to timeout or connection issue.");
                    ApplicationLogger.LogException(ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during SubscribeSymbols call.");
                    ApplicationLogger.LogException(ex);
                }
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
                if (defaultGrid.InvokeRequired)
                {
                    defaultGrid.BeginInvoke((MethodInvoker)(() =>
                    {
                        lock (_tableLock)
                        {
                            CleanupEmptyRows();
                            AddMissingRows();
                        }
                    }));
                }
                else
                {
                    lock (_tableLock)
                    {
                        CleanupEmptyRows();
                        AddMissingRows();
                    }
                }

                var json = DecompressGzip(Convert.FromBase64String(base64));
                var data = JsonConvert.DeserializeObject<MarketDataDTO>(json);
                if (data == null || !(identifiers?.Contains(data.i) ?? false)) return;

                _updateQueue.Enqueue(data);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void CleanupEmptyRows()
        {
            lock (_tableLock)
            {
                var rowsToRemove = new List<DataGridViewRow>();

                foreach (DataGridViewRow row in defaultGrid.Rows)
                {
                    // Skip new rows if AllowUserToAddRows is accidentally enabled
                    if (row.IsNewRow) continue;

                    var symbolCell = row.Cells["symbol"];
                    if (symbolCell == null) continue;

                    bool isEmpty = true;

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.OwningColumn.Name == "symbol") continue;

                        if (!IsNullOrEmptyOrPlaceholder(cell.Value))
                        {
                            isEmpty = false;
                            break;
                        }
                    }

                    if (isEmpty)
                        rowsToRemove.Add(row);
                }

                foreach (var row in rowsToRemove)
                {
                    defaultGrid.Rows.Remove(row);
                }
            }
        }


        private void AddMissingRows()
        {
            lock (_tableLock)
            {
                foreach (var symbol in identifiers)
                {
                    if (!IsSymbolPresentInGrid(symbol))
                    {
                        var dto = pastRateTickDTO?.FirstOrDefault(x => x.i == symbol);
                        if (dto != null)
                        {
                            AddRowFromDTO(dto);
                        }
                    }
                }
            }
        }

        private bool IsSymbolPresentInGrid(string symbol)
        {
            foreach (DataGridViewRow row in defaultGrid.Rows)
            {
                if (row.IsNewRow) continue;

                var cell = row.Cells["symbol"];
                if (cell?.Value?.ToString() == symbol)
                    return true;
            }
            return false;
        }


        private bool IsNullOrEmptyOrPlaceholder(object val)
        {
            return val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString()) || val.ToString() == "--";
        }

        private void AddRowFromDTO(MarketDataDTO dto)
        {
            object[] rowData = new object[]
            {
                dto.i,                                // symbol
                dto.n ?? "--",                        // Name
                dto.b ?? "--",                        // Bid
                dto.a ?? "--",                        // Ask
                dto.ltp ?? "--",                      // LTP
                dto.h ?? "--",                        // High
                dto.l ?? "--",                        // Low
                dto.o ?? "--",                        // Open
                dto.c ?? "--",                        // Close
                dto.d ?? "--",                        // Net Chng
                dto.atp ?? "--",                      // ATP
                dto.bq ?? "--",                       // Bid Size
                dto.tbq ?? "--",                      // Total Bid Size
                dto.sq ?? "--",                       // Ask Size
                dto.tsq ?? "--",                      // Total Ask Size
                dto.vt ?? "--",                       // Volume
                dto.oi ?? "--",                       // Open Interest
                dto.ltq ?? "--",                      // Last Size
                dto.v ?? "--",                        // V
                commonClass.TimeStampConvert(dto.t)   // Time
            };

            if (defaultGrid.Columns.Count == 0)
            {
                InitializeDataGridView(); // or any custom setup that defines columns
            }

            int newRowIdx = defaultGrid.Rows.Add(rowData);

            // Update symbolRowMap with new row index
            symbolRowMap[dto.i] = newRowIdx;
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

                //updates = updates.Where(x => !string.IsNullOrEmpty(x.t) && x.t != "N/A").OrderByDescending(x => DateTime.ParseExact(x.t, "hh:mm:ss tt", CultureInfo.InvariantCulture)).ToList();
                updates = updates.Where(x => long.TryParse(x.t, out _)).OrderByDescending(x => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(x.t)).LocalDateTime).ToList();

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
                lock (_tableLock)
                {
                    foreach (var newData in updates)
                    {

                        if (newData == null || string.IsNullOrEmpty(newData.i))
                            continue;

                        // Add missing row if not present
                        if (!symbolRowMap.TryGetValue(newData.i, out int rowIndex))
                        {
                            var dto = pastRateTickDTO?.FirstOrDefault(x => x.i == newData.i);
                            if (dto != null)
                                AddRowFromDTO(dto);

                            if (!symbolRowMap.TryGetValue(newData.i, out rowIndex))
                                continue; // Skip if still not added
                        }

                        var row = defaultGrid.Rows[rowIndex];

                        // Store previous values for color comparison
                        var previousValues = new Dictionary<string, string>();
                        foreach (string colName in numericColumns)
                        {
                            if (defaultGrid.Columns.Contains(colName))
                            {
                                previousValues[colName] = row.Cells[colName].Value?.ToString() ?? "";
                            }
                        }



                        // Update values
                        SetCellValue(row, "Bid", newData.b);
                        SetCellValue(row, "Ask", newData.a);
                        SetCellValue(row, "LTP", newData.ltp);
                        SetCellValue(row, "High", newData.h);
                        SetCellValue(row, "Low", newData.l);
                        SetCellValue(row, "Open", newData.o);
                        SetCellValue(row, "Close", newData.c);
                        SetCellValue(row, "Net Chng", newData.d);
                        SetCellValue(row, "V", newData.v);
                        SetCellValue(row, "ATP", newData.atp);
                        SetCellValue(row, "Bid Size", newData.bq);
                        SetCellValue(row, "Total Bid Size", newData.tbq);
                        SetCellValue(row, "Ask Size", newData.sq);
                        SetCellValue(row, "Total Ask Size", newData.tsq);
                        SetCellValue(row, "Volume", newData.vt);
                        SetCellValue(row, "Open Interest", newData.oi);
                        SetCellValue(row, "Last Size", newData.ltq);
                        SetCellValue(row, "Time", commonClass.TimeStampConvert(newData.t));

                        // Set name if still default
                        var nameCell = row.Cells["Name"];
                        if ((nameCell.Value?.ToString() ?? "N/A") == "N/A")
                        {
                            var name = pastRateTickDTO?.FirstOrDefault(x => x.i == newData.i)?.n ?? "--";
                            nameCell.Value = name;
                        }

                        if (nameCell.Value.ToString() == "slmini")
                        {

                        }

                        // Ask price arrow direction
                        bool hasAskChange = false;
                        int askDirection = 0;
                        string askStr = newData.a;

                        if (!string.IsNullOrEmpty(askStr) && double.TryParse(askStr, out double newAsk))
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

                        // Highlight changed numeric values
                        foreach (string colName in numericColumns)
                        {
                            if (!defaultGrid.Columns.Contains(colName)) continue;

                            var cell = row.Cells[colName];
                            var prev = previousValues.TryGetValue(colName, out string prevVal) ? prevVal : "";
                            var curr = cell.Value?.ToString() ?? "";

                            if (IsNumericChange(prev, curr, out int direction))
                            {
                                if (direction == 1)
                                    cell.Style.ForeColor = Color.Green;
                                else if (direction == -1)
                                    cell.Style.ForeColor = Color.Red;
                            }
                        }

                        // Update "Name" column with arrows
                        if (hasAskChange)
                        {
                            string baseName = (nameCell.Value?.ToString() ?? "").Replace(" ▲", "").Replace(" ▼", "").Trim();
                            if (askDirection == 1)
                            {
                                nameCell.Value = baseName + " ▲";
                                nameCell.Style.ForeColor = Color.Green;
                            }
                            else if (askDirection == -1)
                            {
                                nameCell.Value = baseName + " ▼";
                                nameCell.Style.ForeColor = Color.Red;
                            }
                        }
                    }

                    UpdateExcelDataEfficiently(defaultGrid);

                    // Throttle font refresh
                    if ((DateTime.Now - lastUiUpdate).TotalMilliseconds > 120)
                    {
                        defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
                        defaultGrid.RowHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 1.5f, FontStyle.Bold);
                        lastUiUpdate = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch update: {ex}");
            }
            finally
            {
                defaultGrid.ResumeLayout();
            }
        }

        private void SetCellValue(DataGridViewRow row, string columnName, object value)
        {
            if (defaultGrid.Columns.Contains(columnName))
                row.Cells[columnName].Value = value ?? "--";
        }


        private bool IsNumericChange(object oldVal, object newVal, out int direction)
        {
            direction = 0;

            if (oldVal == null || newVal == null) return false;

            string oldStr = oldVal.ToString();
            string newStr = newVal.ToString();

            if (double.TryParse(oldStr, out double oldNum) && double.TryParse(newStr, out double newNum))
            {
                if (newNum > oldNum)
                {
                    direction = 1;
                    return true;
                }
                else if (newNum < oldNum)
                {
                    direction = -1;
                    return true;
                }
            }

            return false;
        }


        //private void UpdateRowValue(DataRow row, string columnName, object value)
        //{
        //    if (!row.Table.Columns.Contains(columnName)) return;

        //    try
        //    {
        //        var currentValue = row[columnName];

        //        // Default: keep original value (e.g., "--", "N/A", "text") unless it's null
        //        var newValue = value ?? "";


        //        // If it's a numeric column, try to parse the value
        //        if (IsNumericColumn(columnName))
        //        {
        //            try
        //            {
        //                // Only parse if it's not "--" or empty
        //                if (value is string s &&
        //                s != "--" &&
        //                !string.IsNullOrWhiteSpace(s) &&
        //                decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
        //                {
        //                    newValue = decimalValue;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error parsing rate value at UpdateRowValue: " + ex.Message);
        //            }
        //        }

        //        // Update only if changed
        //        if (!object.Equals(currentValue, newValue))
        //        {
        //            row[columnName] = newValue;
        //        }
        //    }
        //    catch
        //    {
        //        // Fallback: set raw value
        //        row[columnName] = value ?? "";
        //    }
        //}

        //private bool IsNumericColumn(string columnName)
        //{
        //    return columnName == "Bid" ||
        //           columnName == "Ask" ||
        //           columnName == "LTP" ||
        //           columnName == "High" ||
        //           columnName == "Low" ||
        //           columnName == "Open" ||
        //           columnName == "Close" ||
        //           columnName == "Net Chng" ||
        //           columnName == "ATP" ||
        //           columnName == "Bid Size" ||
        //           columnName == "Total Bid Size" ||
        //           columnName == "Ask Size" ||
        //           columnName == "Total Ask Size" ||
        //           columnName == "Volume" ||
        //           columnName == "Open Interest" ||
        //           columnName == "Last Size";
        //}

        private void DisconnectESCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Switch to LoginForm and dispose current form
            Login loginForm = new Login();
            loginForm.Show();

            this.Hide();
            this.Dispose();
        }

        private void BuildSymbolRowMap()
        {
            symbolRowMap.Clear();
            for (int i = 0; i < defaultGrid.Rows.Count; i++)
            {
                string symbol = defaultGrid.Rows[i].Cells["symbol"].Value?.ToString();
                if (!string.IsNullOrEmpty(symbol))
                    symbolRowMap[symbol] = i;
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
                        this.Invoke((MethodInvoker)delegate
                        {
                            pastRateTickDTO = resultdefault.data;

                            if (identifiers == null)
                            {
                                // Extract all non-null, non-empty "i" values into identifiers list
                                identifiers = resultdefault.data
                                    .Where(x => !string.IsNullOrEmpty(x.i))
                                    .Select(x => x.i)
                                    .ToList();

                                SymbolName = resultdefault.data
                                     .Where(x => !string.IsNullOrEmpty(x.i) && !string.IsNullOrEmpty(x.n))
                                     .Select(x => (Symbol: x.i, SymbolName: x.n))

                             .ToList();
                                symbolMaster = identifiers;
                            }

                            // ✅ Filter resultdefault.data to keep only symbols in identifiers
                            resultdefault.data = resultdefault.data
                                .Where(x => identifiers.Contains(x.i))
                                .ToList();

                            ApplyBatchUpdates(resultdefault.data);
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

        //private void SetupDataTable()
        //{
        //    marketDataTable.Clear();
        //    marketDataTable.Columns.Clear();


        //    string[] columns = {
        //        "symbol", "Name", "Bid", "Ask", "LTP", "High", "Low", "Open", "Close", "Net Chng", "ATP",
        //        "Bid Size", "Total Bid Size", "Ask Size", "Total Ask Size", "Volume", "Open Interest", "Last Size", "V", "Time"
        //    };

        //    Type[] types = {
        //        typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
        //        typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
        //        typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)
        //    };

        //    for (int i = 0; i < columns.Length; i++) marketDataTable.Columns.Add(columns[i], types[i]);

        //    foreach (var symbol in instruments)
        //    {
        //        marketDataTable.Rows.Add(
        //            symbol,         // symbol
        //            "N/A",          // Name
        //            "N/A",          // Bid
        //            "N/A",          // Ask
        //            "N/A",          // LTP
        //            "N/A",          // High
        //            "N/A",          // Low
        //            "N/A",          // Open
        //            "N/A",          // Close
        //            "N/A",          // Net Chng
        //            "N/A",          // ATP
        //            "N/A",          // Bid Size
        //            "N/A",          // Total Bid Size
        //            "N/A",          // Ask Size
        //            "N/A",          // Total Ask Size
        //            "N/A",          // Volume
        //            "N/A",          // Open Interest
        //            "N/A",          // Last Size
        //            "N/A",          // V
        //            "N/A"           // Time
        //        );
        //    }

        //    foreach (DataColumn column in marketDataTable.Columns)
        //        if (!columnPreferencesDefault.Contains(column.ColumnName))
        //            column.ColumnMapping = MappingType.Hidden; // ✅ Call symbol map builder here

        //    BuildSymbolRowMap();
        //    if (resultdefault != null && resultdefault.data != null)
        //    {
        //        ApplyBatchUpdates(resultdefault.data);
        //    }
        //}

        private void InitializeGridColumns()
        {
            defaultGrid.Columns.Clear();

            string[] columns = {
        "symbol", "Name", "Bid", "Ask", "LTP", "High", "Low", "Open", "Close", "Net Chng", "ATP",
        "Bid Size", "Total Bid Size", "Ask Size", "Total Ask Size", "Volume", "Open Interest", "Last Size", "V", "Time"
    };

            foreach (string colName in columns)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Name = colName,
                    HeaderText = colName,
                    ReadOnly = true
                };
                defaultGrid.Columns.Add(col);
            }
        }

        private void PopulateGridRows()
        {
            defaultGrid.Rows.Clear();

            foreach (var symbol in identifiers)
            {
                defaultGrid.Rows.Add(new object[]
                {
            symbol, "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A",
            "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A"
                });
            }
        }

        private void ApplyColumnPreferences()
        {
            foreach (DataGridViewColumn col in defaultGrid.Columns)
            {
                col.Visible = columnPreferencesDefault.Contains(col.Name);
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.Automatic;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.Resizable = DataGridViewTriState.True;


                if (col.Name == "symbol" || col.Name == "V")
                {
                    col.Visible = false; // Always hide symbol column
                }


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
        }


        private void InitializeDataGridView()
        {
            defaultGrid.SuspendLayout();

            defaultGrid.DataSource = null;
            defaultGrid.Rows.Clear();
            defaultGrid.Columns.Clear();

            defaultGrid.AllowUserToAddRows = false;
            defaultGrid.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            defaultGrid.AutoGenerateColumns = false;

            InitializeGridColumns();
            PopulateGridRows();
            ApplyColumnPreferences();
            BuildSymbolRowMap();

            if (resultdefault?.data != null)
            {
                // ✅ Filter resultdefault.data to keep only symbols in identifiers
                resultdefault.data = resultdefault.data
                    .Where(x => identifiers.Contains(x.i))
                    .ToList();
                ApplyBatchUpdates(resultdefault.data);
            }

            defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, FontStyle.Regular);
            defaultGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, FontStyle.Bold);
            defaultGrid.ColumnHeadersHeight = 70;
            defaultGrid.AllowUserToResizeColumns = true;

            // Smooth scrolling
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                defaultGrid,
                new object[] { true }
            );

            defaultGrid.ResumeLayout();
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
                _ = AttemptReconnectAsync("Network availability restored.");
            else
                ApplicationLogger.Log("Network unavailable.");
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            _ = AttemptReconnectAsync("Network address changed.");
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                _ = AttemptReconnectAsync("System resumed from sleep/hibernate.");
        }

        private async Task AttemptReconnectAsync(string reason)
        {
            lock (_reconnectLock)
            {
                if (DateTime.Now - _lastReconnectAttempt < _reconnectThrottle)
                    return;

                _lastReconnectAttempt = DateTime.Now;
            }

            ApplicationLogger.Log($"Attempting reconnect due to: {reason}");

            try
            {
                if (connection == null)
                {
                    connection = BuildConnection();
                    connection.On<string>("excelRate", OnExcelRateReceived);
                }

                if (connection.State != HubConnectionState.Connected)
                {
                    await connection.StartAsync();
                    await connection.InvokeAsync("SubscribeSymbols", identifiers);
                    ApplicationLogger.Log("Reconnected and resubscribed.");
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"Reconnect failed: {ex.Message}");
            }
        }

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
            KillProcess();
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

                    // Get visible and exportable columns from the grid
                    var exportableColumns = defaultGrid.Columns
                        .Cast<DataGridViewColumn>()
                        .Where(c => c.Visible && c.Name != "symbol" && c.Name != "V")
                        .OrderBy(c => c.DisplayIndex)
                        .ToList();

                    if (!File.Exists(excelFilePath))
                    {
                        excelApp = new Excel.Application
                        {
                            Visible = false,
                            DisplayAlerts = false
                        };

                        workbook = excelApp.Workbooks.Add();
                        worksheet = (Excel.Worksheet)workbook.Sheets[1];
                        worksheet.Name = "Sheet1";

                        // Write headers
                        for (int i = 0; i < exportableColumns.Count; i++)
                        {
                            worksheet.Cells[1, i + 1] = exportableColumns[i].HeaderText;
                        }

                        workbook.SaveAs(excelFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook);
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
                    commonClass.CreateShortCut(excelFilePath);

                    excelApp = new Excel.Application
                    {
                        Visible = false,
                        DisplayAlerts = false,
                        UserControl = true
                    };

                    workbook = excelApp.Workbooks.Open(excelFilePath);
                    worksheet = workbook.Sheets[1] as Excel.Worksheet;

                    if (worksheet == null)
                        worksheet = (Excel.Worksheet)workbook.Sheets[1];

                    // Write headers (overwrite)
                    for (int i = 0; i < exportableColumns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1] = exportableColumns[i].HeaderText;
                    }

                    int rowCount = defaultGrid.Rows.Count;
                    int colCount = exportableColumns.Count;
                    object[,] dataArray = new object[rowCount, colCount];

                    for (int r = 0; r < rowCount; r++)
                    {
                        var gridRow = defaultGrid.Rows[r];
                        for (int c = 0; c < colCount; c++)
                        {
                            var col = exportableColumns[c];
                            dataArray[r, c] = gridRow.Cells[col.Name].Value ?? "--";
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
                    ApplicationLogger.Log($"Error accessing Excel instance ExportExcelOnClick: {ex.Message}\n{ex.StackTrace}");
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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var aboutForm = new About(username, password, licenceDate))
            {
                if (isFullScreen)
                {
                    aboutForm.StartPosition = FormStartPosition.CenterParent;
                    aboutForm.TopMost = true; // Ensures it stays above the full-screen window
                    aboutForm.ShowDialog(this); // Pass the main form as owner
                }
                else
                {
                    aboutForm.ShowDialog();
                }
            }
        }

        private void Txtsearch_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Ctrl + Backspace is pressed
            if (e.Control && e.KeyCode == Keys.Back)
            {
                txtsearch.Clear();  // Clear all text
                e.SuppressKeyPress = true; // Prevent default backspace behavior
            }
        }

        private void SetActiveMenuItem(ToolStripMenuItem activeItem)
        {
            foreach (ToolStripMenuItem item in viewToolStripMenuItem.DropDownItems)
            {
                item.Enabled = true;
                item.Checked = false;
            }

            activeItem.Enabled = false;
            activeItem.Checked = true;
        }

        public void MenuLoad()
        {
            try
            {

                // Final folder path
                string finalPath = Path.Combine(AppFolder, username);

                // Get all .slt files from the application folder
                List<string> fileNames = Directory.GetFiles(finalPath, "*.slt")
                                                 .Select(Path.GetFileNameWithoutExtension)
                                                 .ToList();

                FileLists = fileNames;

                // Clear existing menu items
                viewToolStripMenuItem.DropDownItems.Clear();
                // Add Default menu item with click handler
                ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
                defaultMenuItem.Click += async (sender, e) =>
                {
                    var clickedItem = (ToolStripMenuItem)sender;
                    await DefaultToolStripMenuItem_Click(sender, e);
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    SetActiveMenuItem(clickedItem);
                    saveMarketWatchHost.Visible = false;
                    lastOpenMarketWatch = "Default";
                    await LoadInitialMarketDataAsync();
                    isGrid = true;
                    reloadGrid = true;
                };
                if (fileNames.Count > 0)
                {
                    if (isdeleted == true)
                    {
                        defaultMenuItem.Enabled = false;
                    }
                }

                defaultMenuItem.Enabled = true;
                viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

                // Add each file as a menu item with a click handler
                foreach (string fileName in fileNames)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(fileName);
                    menuItem.Click += async (sender, e) =>
                    {
                        var clickedItem = (ToolStripMenuItem)sender;

                        saveFileName = clickedItem.Text;
                        addEditSymbolsToolStripMenuItem.Enabled = true;

                        LoadSymbol(Path.Combine(saveFileName + ".slt"));

                        SetActiveMenuItem(clickedItem);
                        titleLabel.Text = saveFileName.ToUpper();
                        isEdit = false;
                        saveMarketWatchHost.Visible = false;
                        lastOpenMarketWatch = saveFileName;
                        await LoadInitialMarketDataAsync();
                        isGrid = true;
                        reloadGrid = true;

                    };
                    viewToolStripMenuItem.DropDownItems.Add(menuItem);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Clear existing menu items
                viewToolStripMenuItem.DropDownItems.Clear();
                // Add Default menu item with click handler
                ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
                defaultMenuItem.Click += async (sender, e) =>
                {

                    var clickedItem = (ToolStripMenuItem)sender;
                    await DefaultToolStripMenuItem_Click(sender, e);
                    MenuLoad();
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    saveFileName = null;
                    SetActiveMenuItem(clickedItem);
                    saveMarketWatchHost.Visible = false;
                    titleLabel.Text = "DEFAULT";
                    lastOpenMarketWatch = "Default";
                    await LoadInitialMarketDataAsync();
                    isGrid = true;
                    reloadGrid = true;
                };
                defaultMenuItem.Enabled = false;
                viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                DefaultGrid_DataSourceChanged(this, EventArgs.Empty);
            }
        }

        public async void LoadSymbol(string Filename)
        {
            try
            {
                string finalPath = Path.Combine(AppFolder, username);
                selectedSymbols.Clear();
                Filename = Path.Combine(finalPath, Filename);
                string cipherText = File.ReadAllText(Filename);
                string json = CryptoHelper.Decrypt(cipherText, EditableMarketWatchGrid.passphrase);
                var symbols = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                selectedSymbols.AddRange(symbols);
                identifiers = selectedSymbols;
                isLoadedSymbol = true;
                titleLabel.Text = Path.GetFileNameWithoutExtension(Filename).ToUpper();
                //marketDataTable = new System.Data.DataTable(); // Ensure this is created first
                //SetupDataTable();                  // Set up columns
                InitializeDataGridView();          // Configure the grid
                await SignalREvent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Was Never Save Or Moved Please Try Again!", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ApplicationLogger.LogException(ex);
            }

            LiveRateGrid();

            MenuLoad();

        }

        public async Task DefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
            editableMarketWatchGrid?.Dispose();
            toolsToolStripMenuItem.Enabled = true;
            isLoadedSymbol = false;
            LiveRateGrid();
            txtsearch.Text = string.Empty;
            await LoadInitialMarketDataAsync();

            MenuLoad();
            titleLabel.Text = "DEFAULT";
            saveFileName = null;
            isEdit = false;
            identifiers = symbolMaster;
            //marketDataTable = new System.Data.DataTable(); // Ensure this is created first
            //SetupDataTable();                  // Set up columns
            InitializeDataGridView();          // Configure the grid
            await SignalREvent();
        }

        private void NewCTRLNToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                // 6. Clean up current resources before switching
                CleanupBeforeViewSwitch();


                // 1. Set new view mode
                marketWatchViewMode = MarketWatchViewMode.New;

                // 2. Reset state if not in edit mode
                if (!isEdit)
                {
                    selectedSymbols.Clear();
                    saveFileName = null;
                    isLoadedSymbol = false;
                }

                // 3. Create and configure new editable grid
                var editableGrid = new EditableMarketWatchGrid
                {
                    Name = "editableMarketWatchGridView",
                    Dock = DockStyle.Fill,
                    columnPreferences = columnPreferences,
                    columnPreferencesDefault = columnPreferencesDefault,
                    fontSize = fontSize,
                    pastRateTickDTO = pastRateTickDTO,
                    isEditMarketWatch = true,
                    SymbolName = SymbolName,
                };

                // 4. Handle edit mode specific setup
                if (isEdit && editableGrid.selectedSymbols != null && saveFileName != null)
                {
                    editableGrid.saveFileName = saveFileName;
                }

                // 5. Add to controls and bring to front
                this.Controls.Add(editableGrid);
                editableGrid.BringToFront();
                editableGrid.Focus();


                // 7. Update UI state
                UpdateUIStateForNewMarketWatch();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error switching to new market watch: {ex.Message}");
            }
        }

        private void ClearCollections()
        {
            lock (_updateQueue)
            {
                while (_updateQueue.TryDequeue(out _)) { }
            }

            lock (symbolRowMap)
            {
                symbolRowMap.Clear();
            }

            //lock (marketDataTable)
            //{
            //    marketDataTable.Clear();
            //    marketDataTable.Dispose();
            //    marketDataTable = new System.Data.DataTable(); // Reinitialize if needed
            //}

            previousAsks.Clear();
            //pastRateTickDTO.Clear();
        }

        private void UpdateUIStateForNewMarketWatch()
        {
            try
            {

                ClearCollections();

                // Update menu items
                toolsToolStripMenuItem.Enabled = true;
                newMarketWatchMenuItem.Enabled = false;

                // Update save button visibility
                saveMarketWatchHost.Visible = true;
                saveMarketWatchHost.Text = "Save MarketWatch";

                // Update status label

                // Update title based on edit mode
                titleLabel.Text = isEdit
                    ? $"Edit {saveFileName?.ToUpper() ?? "Unknown"} MarketWatch"
                    : "New MarketWatch";

                // Reset save file name
                saveFileName = null;

                // Enable all items in the Open menu
                foreach (ToolStripMenuItem item in viewToolStripMenuItem.DropDownItems)
                {
                    item.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void CleanupBeforeViewSwitch()
        {
            // 1. Dispose SignalR connection properly
            DisposeSignalRConnection();

            // 2. Stop and dispose timers
            signalRTimer?.Stop();
            signalRTimer?.Dispose();
            signalRTimer = null;

            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            _updateTimer = null;
            while (_updateQueue.TryDequeue(out _)) { }
            txtsearch.Text = string.Empty;
            // 3. Clean up DataGridView
            CleanupDataGridView();

            // 4. Dispose existing editable grid if exists
            var existingGrid = this.Controls.Find("editableMarketWatchGridView", true).FirstOrDefault();
            if (existingGrid != null)
            {
                this.Controls.Remove(existingGrid);
                existingGrid.Dispose();
            }

            // 5. Clean up Excel resources
            CleanupExcel();

        }

        private void CleanupExcel()
        {
            try
            {
                if (worksheet != null)
                {
                    Marshal.ReleaseComObject(worksheet);
                    worksheet = null;
                }
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                    workbook = null;
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                    excelApp = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void DisposeSignalRConnection()
        {
            if (connection != null)
            {
                try
                {
                    connection.StopAsync().Wait();
                    connection.DisposeAsync().AsTask().Wait();
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                }
                finally
                {
                    connection = null; // ✅ CRUCIAL
                }
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileLists == null || FileLists.Count == 0)
            {
                MessageBox.Show("No Market Watch available to delete.", "Information",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var selectionForm = new Form())
            {
                selectionForm.Text = "Select Market Watch to Delete";
                selectionForm.Width = 600;
                selectionForm.Height = 500;
                selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                selectionForm.StartPosition = FormStartPosition.CenterParent;
                selectionForm.BackColor = Color.White;
                selectionForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9);
                selectionForm.Icon = SystemIcons.WinLogo;

                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = Color.FromArgb(0, 120, 215)
                };

                var headerLabel = new Label
                {
                    Text = "Select Market Watch to Delete",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                    Padding = new Padding(15, 0, 0, 0)
                };
                headerPanel.Controls.Add(headerLabel);

                // Search box for filtering
                var searchBox = new TextBox
                {
                    Dock = DockStyle.Top,
                    Height = 30,
                    Margin = new Padding(10, 10, 10, 5),
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 9),
                    Text = "Search Here..."

                };

                // Modern list view with checkboxes
                var listView = new ListView
                {
                    Dock = DockStyle.Fill,
                    CheckBoxes = true,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = false,
                    MultiSelect = false,
                    BorderStyle = BorderStyle.None,
                    BackColor = SystemColors.Window
                };

                // Modern column headers
                listView.Columns.Add("Market Watch Name", 300);
                listView.Columns.Add("Path", 250);

                // Add files to list view
                foreach (string filePath in FileLists)
                {
                    if (filePath != saveFileName)
                    {
                        var item = new ListViewItem(Path.GetFileName(filePath));
                        item.SubItems.Add(filePath);
                        item.Tag = filePath; // Store full path in tag
                        listView.Items.Add(item);
                    }
                }

                if (listView.Items.Count == 0)
                {
                    MessageBox.Show("There is only one MarketWatch and that Open so can't Delete.", "Information",
                             MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Selection controls panel
                var controlsPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(240, 240, 240)
                };

                // Modern flat buttons
                var selectAllButton = new Button
                {
                    Text = "Select All",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(0, 120, 215),
                    Height = 30,
                    Width = 120,
                    Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                    Margin = new Padding(10, 10, 0, 10)
                };


                var deleteButton = new Button
                {
                    Text = "Delete Selected",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Height = 30,
                    Width = 120,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    Margin = new Padding(0, 10, 90, 10)
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(0, 120, 215),
                    Height = 30,
                    Width = 80,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    Margin = new Padding(0, 10, 10, 10)
                };

                // Button event handlers
                selectAllButton.Click += (s, args) =>
                {
                    foreach (ListViewItem item in listView.Items)
                    {
                        item.Checked = true;
                    }
                };


                cancelButton.Click += (s, args) => selectionForm.DialogResult = DialogResult.Cancel;

                deleteButton.Click += (s, args) =>
                {
                    var selectedFiles = listView.CheckedItems.Cast<ListViewItem>()
                                             .Select(item => item.Tag.ToString())
                                             .ToList();

                    if (selectedFiles.Count == 0)
                    {
                        MessageBox.Show("Please select at least one Market Watch to delete.",
                                        "No Selection",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                        return;
                    }

                    // Modern confirmation dialog
                    var confirmResult = MessageBox.Show($"Are you sure you want to delete {selectedFiles.Count} Market Watch(s)?",
                                                     "Confirm Deletion",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning,
                                                     MessageBoxDefaultButton.Button2);

                    if (confirmResult == DialogResult.Yes)
                    {
                        int successCount = 0;
                        var failedDeletions = new List<string>();

                        foreach (string filePath in selectedFiles)
                        {
                            if (saveFileName == filePath)
                            {
                                MessageBox.Show("Can't Delete Open MarketWatch", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            string fullpath = Path.Combine(AppFolder, username, $"{filePath}.slt");
                            try
                            {
                                File.Delete(fullpath);
                                successCount++;
                                isdeleted = true;
                            }
                            catch (Exception ex)
                            {
                                failedDeletions.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                                ApplicationLogger.LogException(ex);
                            }
                        }

                        // Modern result display
                        var resultMessage = new StringBuilder();
                        resultMessage.AppendLine($"Successfully deleted {successCount} Market Watch(s).");

                        if (failedDeletions.Count > 0)
                        {
                            resultMessage.AppendLine();
                            resultMessage.AppendLine("The following files couldn't be deleted:");
                            resultMessage.AppendLine(string.Join(Environment.NewLine, failedDeletions));
                        }

                        MessageBox.Show(resultMessage.ToString(),
                                      "Deletion Results",
                                      MessageBoxButtons.OK,
                                      failedDeletions.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                        if (successCount > 0)
                        {
                            selectionForm.DialogResult = DialogResult.OK;
                        }

                        MenuLoad();
                    }
                };

                // Search functionality
                searchBox.TextChanged += (s, args) =>
                {
                    listView.BeginUpdate();
                    listView.Items.Clear();

                    foreach (string filePath in FileLists.Where(f =>
                        Path.GetFileName(f).IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        var item = new ListViewItem(Path.GetFileName(filePath));
                        item.SubItems.Add(filePath);
                        item.Tag = filePath;
                        listView.Items.Add(item);
                    }

                    listView.EndUpdate();
                };

                // Add controls to panels
                controlsPanel.Controls.Add(selectAllButton);
                controlsPanel.Controls.Add(deleteButton);
                controlsPanel.Controls.Add(cancelButton);

                // Add controls to form
                selectionForm.Controls.Add(listView);
                selectionForm.Controls.Add(searchBox);
                selectionForm.Controls.Add(headerPanel);
                selectionForm.Controls.Add(controlsPanel);

                // Set form buttons
                selectionForm.AcceptButton = deleteButton;
                selectionForm.CancelButton = cancelButton;

                // Show dialog
                if (selectionForm.ShowDialog() == DialogResult.OK)
                {
                    saveFileName = null;
                }
            }
        }

        private void CleanupDataGridView()
        {
            defaultGrid.SuspendLayout();
            defaultGrid.Visible = false;

            // Unbind data
            defaultGrid.DataSource = null;

            // Clear the grid only after unbinding
            defaultGrid.Rows.Clear();
            defaultGrid.Columns.Clear();

            //// Dispose cell styles and other resources
            //dataGridView1.DefaultCellStyle = new DataGridViewCellStyle();
            //dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle();

            // Dispose cell styles and other resources
            defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
            defaultGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 1.5f, FontStyle.Bold);

            defaultGrid.ResumeLayout();
        }

        private void FullScreenF11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isFullScreen)
            {
                // Store previous state
                prevState = this.WindowState;
                prevStyle = this.FormBorderStyle;
                prevBounds = this.Bounds;

                // Set up full screen
                this.FormBorderStyle = FormBorderStyle.None;
                this.TopMost = true;
                System.Drawing.Rectangle full = Screen.GetBounds(this);
                this.Bounds = full;
                WinApi.SetFullScreen(this.Handle);

                isFullScreen = true;

                fullScreenF11ToolStripMenuItem.Text = "Exit Full Screen (Esc)";

            }
            else
            {
                // Restore previous layout
                this.WindowState = prevState;
                this.FormBorderStyle = prevStyle;
                this.Bounds = prevBounds;
                this.TopMost = false;

                isFullScreen = false;

                fullScreenF11ToolStripMenuItem.Text = "Full Screen (ESC)";
            }
        }

        private void DefaultGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                defaultGrid.ClearSelection();
            }
        }

        private void DefaultGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                defaultGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            }
        }

        private void DefaultGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.RowIndex % 2 == 0)
                    defaultGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                else
                    defaultGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            }
        }

        private void Thecalcify_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift && e.KeyCode == Keys.Escape)
            {
                var result = MessageBox.Show("Do you want to Exit Application?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    this.Close(); // Close the login form
                    System.Windows.Forms.Application.Exit(); // Terminate the application

                }
            }

            if (e.Control && e.KeyCode == Keys.N && marketWatchViewMode != MarketWatchViewMode.New)
            {
                NewCTRLNToolStripMenuItem1_Click(this, EventArgs.Empty);
                e.Handled = true;
            }

            //if (e.KeyCode == Keys.F11)
            //{
            //    FullScreenF11ToolStripMenuItem_Click(this, EventArgs.Empty);
            //    e.Handled = true;
            //}
            

            if (e.KeyCode == Keys.Escape)
            {
                FullScreenF11ToolStripMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }

            if (e.KeyCode == Keys.U && e.Control)
            {
                aboutToolStripMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void DefaultGrid_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_excelInitialized && !TryInitializeExcel())
                    return;

                // Clear entire sheet (Sheet1)
                worksheet.Cells.Clear();

                // Get visible and exportable columns from the grid
                var exportableColumns = defaultGrid.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(c => c.Visible && c.Name != "symbol" && c.Name != "V")
                    .OrderBy(c => c.DisplayIndex)
                    .ToList();

                // Write headers
                for (int i = 0; i < exportableColumns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = exportableColumns[i].HeaderText;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[DefaultGrid_DataSourceChanged] Stuck On : {ex.Message}");
            }
        }

        private void Thecalcify_Layout(object sender, LayoutEventArgs e)
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
                if (commonClass.IsFileLocked(excelFilePath))
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

                    if (worksheet == null)
                        return false;

                    // ✅ Clear data except header
                    Excel.Range usedRange = worksheet.UsedRange;
                    if (usedRange.Rows.Count > 1)
                    {
                        Excel.Range rowsToClear = worksheet.Range["A2", usedRange.Cells[usedRange.Rows.Count, usedRange.Columns.Count]];
                        rowsToClear.ClearContents(); // Clears data but keeps formatting and headers

                    }


                    _excelInitialized = worksheet != null;
                    return _excelInitialized;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[Excel Init Error] {ex.Message}");
                return false;
            }
        }

        private void AddEditColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (panelAddSymbols != null && panelAddSymbols.Visible)
                panelAddSymbols.Visible = false;

            // Create panel if it hasn't been initialized yet
            if (panelAddColumns == null)
            {
                // Initialize panel
                panelAddColumns = new Panel
                {
                    Size = new System.Drawing.Size(500, 500),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    Visible = false,
                    Padding = new Padding(20),
                };

                panelAddColumns.Paint += (s2, e2) =>
                {
                    ControlPaint.DrawBorder(e2.Graphics, panelAddColumns.ClientRectangle,
                        Color.LightGray, 2, ButtonBorderStyle.Solid,
                        Color.LightGray, 2, ButtonBorderStyle.Solid,
                        Color.LightGray, 2, ButtonBorderStyle.Solid,
                        Color.LightGray, 2, ButtonBorderStyle.Solid);
                };

                panelAddColumns.Location = new System.Drawing.Point(
                    (this.Width - panelAddColumns.Width) / 2,
                    (this.Height - panelAddColumns.Height) / 2
                );

                // Title label
                Label titleLabel = new Label
                {
                    Text = "📊 Add / Edit Columns",
                    Font = new System.Drawing.Font("Microsoft Sans Serif Semibold", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(0, 10, 0, 10)
                };

                // CheckedListBox
                checkedListColumns = new CheckedListBox
                {
                    Height = 320,
                    Dock = DockStyle.Top,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true,
                    BackColor = Color.White
                };

                // Add Spacer Panel for spacing
                Panel spacerPanel = new Panel
                {
                    Height = 10, // Adjust height as needed
                    Dock = DockStyle.Top
                };

                // Button container
                Panel buttonPanel = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(10),
                    BackColor = Color.White
                };

                // Buttons
                btnSelectAllColumns = new Button
                {
                    Text = "Select All",
                    Height = 40,
                    Width = 120,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSelectAllColumns.FlatAppearance.BorderSize = 0;

                btnConfirmAddColumns = new Button
                {
                    Text = "✔ Save",
                    Height = 40,
                    Width = 120,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnConfirmAddColumns.FlatAppearance.BorderSize = 0;

                btnCancelAddColumns = new Button
                {
                    Text = "✖ Cancel",
                    Height = 40,
                    Width = 120,
                    BackColor = Color.LightGray,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancelAddColumns.FlatAppearance.BorderSize = 0;

                // Layout
                btnSelectAllColumns.Location = new System.Drawing.Point(30, 22);
                btnConfirmAddColumns.Location = new System.Drawing.Point(170, 22);
                btnCancelAddColumns.Location = new System.Drawing.Point(310, 22);

                buttonPanel.Controls.Add(btnSelectAllColumns);
                buttonPanel.Controls.Add(btnConfirmAddColumns);
                buttonPanel.Controls.Add(btnCancelAddColumns);

                panelAddColumns.Controls.Add(checkedListColumns);
                panelAddColumns.Controls.Add(spacerPanel);
                panelAddColumns.Controls.Add(buttonPanel);
                panelAddColumns.Controls.Add(titleLabel);

                this.Controls.Add(panelAddColumns);

                this.Resize += (s3, e3) =>
                {
                    panelAddColumns.Location = new System.Drawing.Point(
                        (this.Width - panelAddColumns.Width) / 2,
                        (this.Height - panelAddColumns.Height) / 2
                    );
                };

                // Hook up events
                btnSelectAllColumns.Click += (s, e2) =>
                {
                    bool allChecked = true;
                    for (int i = 0; i < checkedListColumns.Items.Count; i++)
                    {
                        if (!checkedListColumns.GetItemChecked(i))
                        {
                            allChecked = false;
                            break;
                        }
                    }

                    bool check = !allChecked;
                    btnSelectAllColumns.Text = check ? "Unselect All" : "Select All";

                    for (int i = 0; i < checkedListColumns.Items.Count; i++)
                    {
                        checkedListColumns.SetItemChecked(i, check);
                    }
                };

                btnConfirmAddColumns.Click += (s, e2) =>
                {
                    var currentlyChecked = checkedListColumns.CheckedItems.Cast<string>().ToList();
                    var previouslySelected = columnPreferences.Count > 0 ? columnPreferences : columnPreferencesDefault;


                    if (!currentlyChecked.Any())
                    {
                        MessageBox.Show("Please select at least one column.");
                        return;
                    }

                    if (currentlyChecked.SequenceEqual(previouslySelected))
                    {
                        MessageBox.Show("No changes made.");
                        panelAddColumns.Visible = false;
                        return;
                    }

                    // Save the new column preferences
                    columnPreferences = currentlyChecked;

                    // Make sure Symbol column is always visible in the grid
                    if (!columnPreferences.Contains("symbol"))
                    {
                        columnPreferences.Add("symbol");
                        columnPreferences.Add("V");
                    }

                    InitializeDataGridView();

                    // Update DataGridView column visibility
                    foreach (DataGridViewColumn column in defaultGrid.Columns)
                    {
                        if (column.Name.ToLower() == "symbol" || column.Name.ToLower() == "v")
                        {
                            column.Visible = false;
                        }
                        else
                        {
                            column.Visible = columnPreferences.Contains(column.Name);
                        }
                    }

                    panelAddColumns.Visible = false;

                    DefaultGrid_DataSourceChanged(sender, EventArgs.Empty);
                    //MessageBox.Show("Columns updated successfully!");

                };

                btnCancelAddColumns.Click += (s, e2) =>
                {
                    panelAddColumns.Visible = false;
                };
            }

            // Refresh items before showing
            checkedListColumns.Items.Clear();


            // Get the columns to display (use allColumns if no preferences set)
            var columnsToShow = columnPreferences.Count > 0 ? columnPreferences : columnPreferencesDefault;

            // Add selected columns first (preserving order)
            foreach (string column in columnPreferencesDefault)
            {
                if (columnsToShow.Contains(column) && column != "symbol" && column != "V")
                {
                    checkedListColumns.Items.Add(column, true);

                }
            }

            // Then add unselected columns
            foreach (string column in columnPreferencesDefault)
            {
                if (!columnsToShow.Contains(column) && column != "symbol" && column != "V")
                {
                    checkedListColumns.Items.Add(column, false);
                }
            }


            // Update Select All button text
            btnSelectAllColumns.Text = checkedListColumns.CheckedItems.Count == checkedListColumns.Items.Count
                ? "Unselect All"
                : "Select All";



            // Make sure Symbol column is always visible in the grid
            if (!columnPreferences.Contains("symbol"))
            {
                columnPreferences.Add("symbol");
                columnPreferences.Add("V");
            }

            // Update DataGridView column visibility to ensure Symbol & V are always visible
            foreach (DataGridViewColumn column in defaultGrid.Columns)
            {
                if (column.Name == "symbol" || column.Name == "V")
                {
                    column.Visible = false;
                }
                //else
                //{
                //    column.Visible = columnPreferences.Contains(column.Name) ? true : false;
                //}
            }

            panelAddColumns.Visible = true;
            panelAddColumns.BringToFront();
        }

        private void AddEditSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (panelAddColumns != null && panelAddColumns.Visible)
                panelAddColumns.Visible = false;

            // Create panel if it hasn't been initialized yet
            if (panelAddSymbols == null)
            {
                // Initialize panel
                panelAddSymbols = new Panel
                {
                    Size = new System.Drawing.Size(500, 500),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    Visible = false,
                    Padding = new Padding(20),
                };

                panelAddSymbols.Paint += (s2, e2) =>
                {
                    ControlPaint.DrawBorder(e2.Graphics, panelAddSymbols.ClientRectangle,
                        Color.LightGray, 2, ButtonBorderStyle.Solid,
                        Color.LightGray, 2, ButtonBorderStyle.Solid,
                        Color.LightGray, 2, ButtonBorderStyle.Solid,
                        Color.LightGray, 2, ButtonBorderStyle.Solid);
                };

                panelAddSymbols.Location = new System.Drawing.Point(
                    (this.Width - panelAddSymbols.Width) / 2,
                    (this.Height - panelAddSymbols.Height) / 2
                );

                // Title label
                Label titleLabel = new Label
                {
                    Text = "🔄 Add / Edit Symbols",
                    Font = new System.Drawing.Font("Microsoft Sans Serif Semibold", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(0, 10, 0, 10)
                };

                // CheckedListBox
                checkedListSymbols = new CheckedListBox
                {
                    Height = 320,
                    Dock = DockStyle.Top,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true,
                    BackColor = Color.White
                };

                // Button container
                Panel buttonPanel = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(10),
                    BackColor = Color.White
                };

                // Buttons
                btnSelectAllSymbols = new Button
                {
                    Text = "Select All",
                    Height = 40,
                    Width = 120,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSelectAllSymbols.FlatAppearance.BorderSize = 0;

                btnConfirmAddSymbols = new Button
                {
                    Text = "✔ Save",
                    Height = 40,
                    Width = 120,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnConfirmAddSymbols.FlatAppearance.BorderSize = 0;

                btnCancelAddSymbols = new Button
                {
                    Text = "✖ Cancel",
                    Height = 40,
                    Width = 120,
                    BackColor = Color.LightGray,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancelAddSymbols.FlatAppearance.BorderSize = 0;

                // Layout
                btnSelectAllSymbols.Location = new System.Drawing.Point(30, 35);
                btnConfirmAddSymbols.Location = new System.Drawing.Point(170, 35);
                btnCancelAddSymbols.Location = new System.Drawing.Point(310, 35);

                titleLabel.Dock = DockStyle.Top;
                checkedListSymbols.Dock = DockStyle.Fill; // So it takes remaining space
                buttonPanel.Dock = DockStyle.Bottom;


                buttonPanel.Controls.Add(btnSelectAllSymbols);
                buttonPanel.Controls.Add(btnConfirmAddSymbols);
                buttonPanel.Controls.Add(btnCancelAddSymbols);

                panelAddSymbols.Controls.Add(buttonPanel);  // bottom first
                panelAddSymbols.Controls.Add(checkedListSymbols); // middle
                panelAddSymbols.Controls.Add(titleLabel);   // top last


                this.Controls.Add(panelAddSymbols);

                this.Resize += (s3, e3) =>
                {
                    panelAddSymbols.Location = new System.Drawing.Point(
                        (this.Width - panelAddSymbols.Width) / 2,
                        (this.Height - panelAddSymbols.Height) / 2
                    );
                };

                // Hook up events

                btnSelectAllSymbols.Click += (s, e2) =>
                {
                    bool allChecked = true;
                    for (int i = 0; i < checkedListSymbols.Items.Count; i++)
                    {
                        if (!checkedListSymbols.GetItemChecked(i))
                        {
                            allChecked = false;
                            break;
                        }
                    }

                    bool check = !allChecked;
                    btnSelectAllSymbols.Text = check ? "Unselect All" : "Select All";

                    for (int i = 0; i < checkedListSymbols.Items.Count; i++)
                    {
                        checkedListSymbols.SetItemChecked(i, check);
                    }
                };

                btnConfirmAddSymbols.Click += async (s, e2) =>
                {
                    // Get the checked display names (SymbolName)
                    var currentlyCheckedNames = checkedListSymbols.CheckedItems.Cast<string>().ToList();


                    // If nothing is selected
                    if (!currentlyCheckedNames.Any())
                    {
                        MessageBox.Show("Please select at least one symbol to confirm.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    // Map checked names back to their symbols
                    var currentlyCheckedSymbols = SymbolName
                        .Where(x => currentlyCheckedNames.Contains(x.SymbolName))
                        .Select(x => x.Symbol)
                        .ToList();

                    // Compare with previous selection
                    var previouslySelected = selectedSymbols;

                    var addedSymbols = currentlyCheckedSymbols.Except(previouslySelected).ToList();
                    var removedSymbols = previouslySelected.Except(currentlyCheckedSymbols).ToList();

                    if (!addedSymbols.Any() && !removedSymbols.Any())
                    {
                        MessageBox.Show("No changes made.");
                        return;
                    }


                    // Save changes
                    EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance ?? new EditableMarketWatchGrid();
                    editableMarketWatchGrid.isGrid = false;
                    editableMarketWatchGrid.saveFileName = saveFileName;
                    editableMarketWatchGrid.username = username;
                    selectedSymbols = currentlyCheckedSymbols;
                    editableMarketWatchGrid.SaveSymbols(selectedSymbols);
                    identifiers = selectedSymbols;
                    BeginInvoke((MethodInvoker)(() =>
                    {
                        InitializeDataGridView();
                    }));
                    await LoadInitialMarketDataAsync();
                    await SignalREvent();

                    panelAddSymbols.Visible = false;
                };

                btnCancelAddSymbols.Click += (s, e2) =>
                {
                    panelAddSymbols.Visible = false;
                };
            }

            // Refresh items before showing
            checkedListSymbols.Items.Clear();

            // Add selected symbols first
            foreach (var item in SymbolName)
            {
                if (identifiers.Contains(item.Symbol))
                {
                    checkedListSymbols.Items.Add(item.SymbolName, true); // Display symbol name
                }
            }

            // Then unselected symbols
            foreach (var item in SymbolName)
            {
                if (!identifiers.Contains(item.Symbol))
                {
                    checkedListSymbols.Items.Add(item.SymbolName, false);
                }
            }


            panelAddSymbols.Visible = true;
            panelAddSymbols.BringToFront();

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
