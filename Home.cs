using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection; 
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Alert;
using thecalcify.Charts.Models;
using thecalcify.Charts.Services;
using thecalcify.Charts.Views;
using thecalcify.Excel_Helper;
using thecalcify.Helper;
using thecalcify.MarketWatch;
using thecalcify.Modern_UI;
using thecalcify.News;
using thecalcify.RTDWorker;
using thecalcify.Shared;
using thecalcify.Update_Service;
using static thecalcify.Helper.APIUrl;
using CellData = thecalcify.Helper.CellData;

namespace thecalcify
{
    public partial class thecalcify : Form, ILiveMarketGrid
    {
        #region Declaration and Initialization

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        // ======================
        // 📌 Config / Constants
        // ======================
        public readonly string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "thecalcify");

        public static string token, licenceDate, username, password;

        // ======================
        // 📌 Flags / States
        // ======================

        public bool isLoadedSymbol = false;
        public bool isEdit = false;
        public bool isGrid = true, reloadGrid = true;
        public bool isdeleted = false;
        private bool isRunning = true;
        private bool isFullScreen = false;


        // ======================
        // 📌 Runtime State / Data
        // ======================
        public int fontSize = 12, RemainingDays;
        private ConcurrentDictionary<string, MarketDataDto> _latestUpdates = new ConcurrentDictionary<string, MarketDataDto>();

        private Rectangle prevBounds;
        private FormWindowState prevState;
        private FormBorderStyle prevStyle;
        public string saveFileName;
        public string lastOpenMarketWatch = string.Empty;

        // ======================
        // 📌 Core Data Collections
        // ======================
        public List<string> identifiers { get; set; }

        public List<string> selectedSymbols = new List<string>();
        public List<MarketDataDto> pastRateTickDTO = new List<MarketDataDto>();
        public List<string> symbolMaster = new List<string>();
        public List<string> columnPreferences;
        public List<string> columnPreferencesDefault = new List<string>()
        {
            "symbol","Name","Bid","Ask","High","Low","Open","Close","LTP","Net Chng",
            "V","Time","ATP","Bid Size","Total Bid Size","Ask Size","Total Ask Size",
            "Volume","Open Interest","Last Size"
        };
        private static readonly string APIPath = APIUrl.ApplicationURL;

        public List<string> FileLists = new List<string>();
        public List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();

        // ======================
        // 📌 Dictionaries / Maps
        // ======================
        private readonly Dictionary<string, int> symbolRowMap = new Dictionary<string, int>();

        // ======================
        // 📌 Arrays
        // ======================
        public string[] numericColumns = new[]
        {
            "Bid","Ask","LTP","High","Low","Open","Close","Net Chng","ATP",
            "Bid Size","Total Bid Size","Ask Size","Total Ask Size","Volume",
            "Open Interest","Last Size"
        };

        // ======================
        // 📌 Services / External Connections
        // ======================
        //public HubConnection connection { get; set; }
        //private bool isReconnectTimerRunning = false;
        //private bool _isReconnectInProgress = false;
        //private bool _eventHandlersAttached = false;
        public Common commonClass;

        // ======================
        // 📌 Excel Interop
        // ======================
        private readonly string excelFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "thecalcify.xlsx");
        //private static readonly string marketInitDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "initdata.dat");

        // ======================
        // 📌 UI Elements
        // ======================
        private CheckedListBox checkedListColumns;
        private Button btnSelectAllColumns;
        private Button btnConfirmAddColumns;
        private Button btnCancelAddColumns;
        //private ModernUIManager uiManager;


        // ======================
        // 📌 Static & Singleton
        // ======================
        public static thecalcify CurrentInstance { get; private set; }

        // ======================
        // 📌 Enums
        // ======================
        private enum MarketWatchViewMode
        {
            Default,
            New
        }
        private MarketWatchViewMode marketWatchViewMode = MarketWatchViewMode.Default;


        // ======================
        // 📌 API Responses
        // ======================
        private MarketApiResponse resultdefault;
        public string keywords = string.Empty, topics = string.Empty;
        public bool isDND = false;
        private int userId;
        private readonly Dictionary<string, long> _rowLastUpdate = new Dictionary<string, long>();
        private readonly Dictionary<string, double> _prevAskMap = new Dictionary<string, double>();
        private SharedMemoryQueue _queue;
        private CancellationTokenSource _cts;
        private Task _consumerTask;
        private System.Windows.Forms.Timer _uiTimer;
        private readonly string RtwConfigPath = APIUrl.RtwConfigPath;
        private readonly ConcurrentDictionary<string, MarketDataDto> _latestTicks = new ConcurrentDictionary<string, MarketDataDto>(StringComparer.OrdinalIgnoreCase);
        private bool _isGridBuilding = false;
        private static bool _excelWarmedUp = false;
        private int _rightClickedRowIndex = -1;
        private MarketwatchServerAPI _marketwatchServerAPI;
        private MarketWatchItem marketWatchItem;
        //public List<(string Symbol, string SymbolName)> SubscribeSymbol = new List<(string Symbol, string SymbolName)>();
        // Add this at class level (Home.cs)
        private Dictionary<string, string> _symbolNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool reloadRequire = true;

        #endregion Declaration and Initialization

        #region Form Method
        public thecalcify()
        {
            InitializeComponent();
            //this.args = args;
            this.ShowInTaskbar = true;
            this.ShowIcon = true;

        }

        private async void Home_Load(object sender, EventArgs e)
        {
            try
            {
                // --- LOGIN INFO ---
                var login = Login.CurrentInstance;
                token = login?.token ?? string.Empty;
                licenceDate = login?.licenceDate ?? string.Empty;
                username = login?.username ?? string.Empty;
                password = login?.userpassword ?? string.Empty;

                // --- UI SETUP (non-data related) ---
                this.AutoScaleMode = AutoScaleMode.Dpi;

                this.KeyPreview = true;
                this.DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);


                // --- PARALLEL INITIALIZATION ---
                var initializationTasks = new List<Task>();

                menuStrip1.Renderer = new ModernMenuRenderer();
                Tools.Renderer = new ModernMenuRenderer();

                fontSizeComboBox.SelectedIndex = -1;

                SetupModernSearchBox();

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

                pnlSearch.Paint += PnlSearch_Paint;

                //// Warm up Excel COM server (faster first export)
                //var app = new Microsoft.Office.Interop.Excel.Application();
                //app.Quit();

                startRTWService();
                ExcelNotifier.StartExcelMonitor();

                PipeSignalReceiver.Start(message =>
                {
                    // OPTIONAL: logging / UI status only
                    ApplicationLogger.Log($"Pipe message received: {message}");
                });

                //uiManager = new ModernUIManager(this);
                //uiManager?.ApplyModernUI();

                // --- MENU SETUP ---
                if (LoginInfo.IsRate && LoginInfo.IsNews && LoginInfo.RateExpiredDate.Date >= DateTime.Today.Date && LoginInfo.NewsExpiredDate >= DateTime.Today.Date)
                {
                    _marketwatchServerAPI = new MarketwatchServerAPI(token);

                    await MenuLoadAsync();

                    // --- LOAD INITIAL DATA ASYNCHRONOUSLY ---
                    HandleLastOpenedMarketWatch();


                    // Initialize Grid on UI thread
                    SafeInvoke(InitializeDataGridView);


                    // AFTER: await LoadInitialMarketDataAsync(); HandleLastOpenedMarketWatch();
                    // AFTER: SafeInvoke(InitializeDataGridView);

                    _queue = new SharedMemoryQueue("thecalcifyQueue");

                    ApplicationLogger.Log("[RTW] Shared memory queue ready.");

                    // optional but recommended
                    _queue.Reset();

                    // drain old messages if any
                    byte[] old;
                    while (_queue.Read(0, out old)) { }

                    // start consumer
                    _cts = new CancellationTokenSource();
                    _consumerTask = Task.Run(() => ConsumeTicks(_cts.Token));

                    // start UI timer
                    _uiTimer = new System.Windows.Forms.Timer();
                    _uiTimer.Interval = 20;
                    _uiTimer.Tick += UiTimer_Tick;
                    _uiTimer.Start();

                    //pageSwitched = true;

                    // Start SignalR
                    //SignalRTimer();
                    //await EnsureSignalRConnectedAndSubscribedAsync();
                    licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");

                    RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                    if (RemainingDays <= 7)
                    {
                        await CheckLicenceLoop();
                    }
                    else
                    {
                        licenceExpire.Text = $"License Expired On :- {licenceDate}";
                    }
                    //SendRestartSignal();
                    RestartRTWService();

                }
                else if (LoginInfo.IsRate && LoginInfo.RateExpiredDate >= DateTime.Today.Date)
                {
                    _marketwatchServerAPI = new MarketwatchServerAPI(token);

                    licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");

                    await MenuLoadAsync();
                    newsToolStripMenuItem.Visible = false;


                    // --- LOAD INITIAL DATA ASYNCHRONOUSLY ---
                    //SendRestartSignal();

                    HandleLastOpenedMarketWatch();


                    // Initialize Grid on UI thread
                    SafeInvoke(InitializeDataGridView);


                    // AFTER: await LoadInitialMarketDataAsync(); HandleLastOpenedMarketWatch();
                    // AFTER: SafeInvoke(InitializeDataGridView);

                    _queue = new SharedMemoryQueue("thecalcifyQueue");

                    ApplicationLogger.Log("[RTW] Shared memory queue ready.");

                    // optional but recommended
                    _queue.Reset();

                    // drain old messages if any
                    byte[] old;
                    while (_queue.Read(0, out old)) { }

                    // start consumer
                    _cts = new CancellationTokenSource();
                    _consumerTask = Task.Run(() => ConsumeTicks(_cts.Token));

                    // start UI timer
                    _uiTimer = new System.Windows.Forms.Timer();
                    _uiTimer.Interval = 20;
                    _uiTimer.Tick += UiTimer_Tick;
                    _uiTimer.Start();




                    //pageSwitched = true;

                    // Start SignalR
                    //SignalRTimer();
                    //await EnsureSignalRConnectedAndSubscribedAsync();

                    RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                    if (RemainingDays <= 7)
                    {
                        await CheckLicenceLoop();
                    }
                    else
                    {
                        licenceExpire.Text = $"License Expired On :- {licenceDate}";
                    }

                    RestartRTWService();

                }
                else if (LoginInfo.IsNews && LoginInfo.NewsExpiredDate >= DateTime.Today.Date)
                {
                    //pageSwitched = true;

                    licenceDate = LoginInfo.NewsExpiredDate.ToString("dd:MM:yyyy");

                    this.NewsListToolStripMenuItem_Click(this, EventArgs.Empty);
                    newCTRLNToolStripMenuItem.Visible = false;
                    alertToolStripMenuItem.Visible = false;
                    toolsToolStripMenuItem.Enabled = true;

                    RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                    if (RemainingDays <= 7)
                    {
                        await CheckLicenceLoop();
                    }
                    else
                    {
                        licenceExpire.Text = $"License Expired On :- {LoginInfo.NewsExpiredDate:dd:MM:yyyy}";
                    }

                    //if (args != null)
                    //{
                    //    licenceDate = LoginInfo.NewsExpiredDate.ToString().Replace("0:00:00", "");

                    //    this.NewsListToolStripMenuItem_Click_1(this, EventArgs.Empty);
                    //    newCTRLNToolStripMenuItem.Visible = false;
                    //    toolsToolStripMenuItem.Enabled = true;


                    //    RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                    //    if (RemainingDays <= 7)
                    //    {
                    //        await CheckLicenceLoop();
                    //    }
                    //    else
                    //    {
                    //        licenceExpire.Text = $"Licence Expire At:- {licenceDate.Replace("0:00:00", "").Replace("12:00:00 AM", "").Replace("00:00:00", "").Replace("00:00", "").Replace("00:00 AM", "").TrimEnd('0').Trim()}";
                    //    }

                    //}

                }


                // --- FORM PROPERTIES ---
                defaultGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

                CurrentInstance = this;

                // --- GLOBAL EVENTS ---
                //NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
                //NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
                //SystemEvents.PowerModeChanged += OnPowerModeChanged;
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                KillProcess();

            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void PnlSearch_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, pnlSearch.ClientRectangle, Color.FromArgb(220, 220, 220), ButtonBorderStyle.Solid);
        }

        private void PnlSearch_Click(object sender, EventArgs e)
        {
            txtsearch.Focus();
        }

        private void SetupModernSearchBox()
        {
            Color inputBg = Color.FromArgb(245, 248, 250);
            Color inputBg1 = Color.FromArgb(245, 248, 250);
            Color borderColor = Color.FromArgb(220, 220, 220);

            pnlSearch.BackColor = inputBg;

            pnlSearch.Size = new Size(270, 28);
            pnlSearch.Location = new Point(pnlSearch.Location.X - 70, pnlSearch.Location.Y + 5);

            pnlSearch.Padding = new Padding(0);
            pnlSearch.Cursor = Cursors.IBeam;

            this.MinimumSize = new Size(1100, 700);
            pnlSearch.Paint -= PnlSearch_Paint;
            pnlSearch.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, pnlSearch.ClientRectangle, borderColor, ButtonBorderStyle.Solid);
            };

            searchTextLabel.Parent = pnlSearch;
            searchTextLabel.ForeColor = Color.Gray;
            searchTextLabel.BackColor = inputBg;
            searchTextLabel.Location = new Point(8, (pnlSearch.Height - searchTextLabel.Height) / 2);

            txtsearch.Parent = pnlSearch;
            txtsearch.BorderStyle = BorderStyle.None;
            txtsearch.BackColor = inputBg;
            txtsearch.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Position TextBox to the right of the Label
            int textX = searchTextLabel.Right + 5;
            txtsearch.Location = new Point(textX, (pnlSearch.Height - txtsearch.Height) / 2 + 1);

            // ✅ Auto-width: Fills the remaining space of the smaller panel
            txtsearch.Width = pnlSearch.Width - textX - 10;
            txtsearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            // 4. Focus Events
            pnlSearch.Click += (s, e) => txtsearch.Focus();
            searchTextLabel.Click += (s, e) => txtsearch.Focus();
        }

        public static void WarmUpExcelLazy()
        {
            if (_excelWarmedUp) return;

            Task.Run(() =>
            {
                try
                {
                    var xl = new Microsoft.Office.Interop.Excel.Application();
                    xl.Quit();
                    Marshal.ReleaseComObject(xl);
                }
                catch { }
                finally
                {
                    _excelWarmedUp = true;
                }
            });
        }

        private Task CheckLicenceLoop()
        {
            RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
            if (RemainingDays <= 7)
            {
                // Start a timer instead of thread
                var licenceTimer = new System.Windows.Forms.Timer
                {
                    Interval = 2000, // 1 second
                    Enabled = true
                };
                licenceTimer.Tick += async (s, e2) =>
                {
                    if (!isRunning || IsDisposed || !IsHandleCreated)
                    {
                        licenceTimer.Stop();
                        licenceTimer.Dispose();
                        return;
                    }

                    int licenceRemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                    await UpdateLicenceLabel(licenceRemainingDays);
                };
            }
            else
            {
                licenceExpire.Text = $"License Expired On :- {licenceDate}";
            }

            return Task.CompletedTask;
        }

        private async void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            isRunning = false;

            try
            {
                //_cancellationTokenSource?.Cancel();
                await LogoutAsync();
                KillProcess();
                // Correct way to call the static method
                CredentialManager.SaveMarketWatchWithColumns(lastOpenMarketWatch, (columnPreferences.Count == 0 || columnPreferences == null) ? columnPreferencesDefault : columnPreferences);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

            Application.Exit();
        }

        private async Task UpdateLicenceLabel(int licenceRemainingDays)
        {
            try
            {
                if (licenceRemainingDays < 0)
                {
                    try
                    {
                        await StopBackgroundTasks();
                        UnsubscribeAllEvents();

                        new Login().Show();

                        Close(); // Dispose + close safely
                        KillProcess();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("[UpdateLicenceLabel] Error during disconnect: " + ex.Message);
                        ApplicationLogger.Log("[UpdateLicenceLabel] Error during disconnect: " + ex.Message);
                        ApplicationLogger.LogException(ex);
                    }
                    finally
                    {
                        await StopBackgroundTasks();
                    }
                }
                else if (licenceRemainingDays == 0)
                {
                    licenceExpire.Text = "⚠ Licence expires today!";
                    licenceExpire.ForeColor = Color.Red;
                    licenceExpire.Visible = !licenceExpire.Visible; // blink
                }
                else if (licenceRemainingDays <= 7 && licenceRemainingDays != 0)
                {
                    licenceExpire.Text = $"⚠ Licence expires in {licenceRemainingDays} days!";
                    licenceExpire.ForeColor = Color.Red;
                    licenceExpire.Visible = !licenceExpire.Visible; // blink
                }
                else if (licenceRemainingDays > 7)
                {
                    licenceExpire.Visible = true;
                    licenceExpire.Text = $"Licence Expire At:- {licenceDate.Replace("0:00:00", "").Replace("12:00:00 AM", "").Replace("00:00:00", "").Replace("00:00", "").Replace("00:00 AM", "").TrimEnd('0').Trim()}";
                    licenceExpire.ForeColor = Color.Black;
                }
                else
                {
                    licenceExpire.Text = $"Licence valid for {licenceRemainingDays} days.";
                    licenceExpire.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public void thecalcifyGrid()
        {
            if (!isLoadedSymbol)
                marketWatchViewMode = MarketWatchViewMode.Default;

            // Hide the DataGridView
            defaultGrid.Visible = true;
            defaultGrid.BringToFront();
            defaultGrid.Focus();
            newCTRLNToolStripMenuItem1.Enabled = true;
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

        private void Txtsearch_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Ctrl + Backspace is pressed
            if (e.Control && e.KeyCode == Keys.Back)
            {
                txtsearch.Clear();  // Clear all text 
                e.SuppressKeyPress = true; // Prevent default backspace behavior 
            }
        }

        private void DefaultGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.Button == MouseButtons.Right)
            {
                // ✔ Prevent focus loss
                defaultGrid.Focus();

                // ✔ Select the row on right-click
                defaultGrid.ClearSelection();
                defaultGrid.Rows[e.RowIndex].Selected = true;

                // ✔ Set the current cell (important!)
                var col = defaultGrid.Columns["Name"];
                if (col != null && col.Visible)
                {
                    defaultGrid.CurrentCell = defaultGrid.Rows[e.RowIndex].Cells[col.Index];
                }

                // ⭐ Store row index for context menu
                _rightClickedRowIndex = e.RowIndex;

                // ✔ Show your context menu NEXT
                Tools.Show(Cursor.Position);
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
                    //this.Close(); // Close the login form
                    //Application.Exit(); // Terminate the application
                    DisconnectESCToolStripMenuItem_Click(sender, e);
                }
            }

            if (e.Control && e.KeyCode == Keys.N && marketWatchViewMode != MarketWatchViewMode.New && LoginInfo.IsRate)
            {
                NewCTRLNToolStripMenuItem1_Click(this, EventArgs.Empty);
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Escape && !(e.Shift && e.KeyCode == Keys.Escape))
            {
                FullScreenF11ToolStripMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }

            if (e.KeyCode == Keys.U && e.Control)
            {
                AboutToolStripMenuItem_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void TitleLabel_TextChanged(object sender, EventArgs e)
        {
            txtsearch.Clear();
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

        private async void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                //connection = null;
                //_eventHandlersAttached = false;

                //SignalRTimer();
                //await EnsureSignalRConnectedAndSubscribedAsync();
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        refreshMarketWatchHost.Visible = true;
                        savelabel.Text = string.Empty;
                        savelabel.Visible = false;
                    }));
                }
                else
                {
                    refreshMarketWatchHost.Visible = true;
                    savelabel.Text = string.Empty;
                    savelabel.Visible = false;
                }
            }
            else
            {
                ApplicationLogger.Log("Network unavailable.");
                //connection = null;
                //_eventHandlersAttached = false;

                //if (signalRTimer != null)
                //{
                //    signalRTimer.Stop();
                //}


                //_eventHandlersAttached = false;

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        refreshMarketWatchHost.Visible = false;
                        savelabel.Text = "Client is offline, switch network.";
                        savelabel.Visible = true;
                    }));
                }
                else
                {
                    refreshMarketWatchHost.Visible = false;
                    savelabel.Text = "Client is offline, switch network.";
                    savelabel.Visible = true;
                }
            }

        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            ApplicationLogger.Log("Network address changed.");
        }

        private async void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                ApplicationLogger.Log("Power Resume");
            //await EnsureSignalRConnectedAndSubscribedAsync();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.WindowState = FormWindowState.Maximized;
            WarmUpExcelLazy();
        }


        #endregion Form Method

        #region MarketWatch Click
        public async Task DefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // ---------------------------------------------
                // UI cleanup (safe)
                // ---------------------------------------------
                fontSizeComboBox.Visible = true;
                savelabel.Visible = false;

                //uiManager?.SetFontSizeComboBoxVisibility(true);

                pnlSearch.Visible = true;
                searchTextLabel.Visible = true;
                txtsearch.Visible = true;
                txtsearch.Text = string.Empty;

                //uiManager?.SetSearchBoxVisibility(true);


                // Close editable grid if open
                var editable = EditableMarketWatchGrid.CurrentInstance;
                if (editable != null)
                {
                    if (editable.IsCurrentCellInEditMode)
                        editable.EndEdit();

                    editable.EditableDispose();
                    editable.Dispose();
                }

                toolsToolStripMenuItem.Enabled = true;
                isLoadedSymbol = false;

                // Switch UI to main grid panel
                thecalcifyGrid();

                saveFileName = null;
                titleLabel.Text = "DEFAULT";
                saveMarketWatchHost.Visible = false;
                refreshMarketWatchHost.Visible = true;
                isEdit = false;

                // ---------------------------------------------
                // STEP 1: Load initial snapshot from API
                // ---------------------------------------------
                //SendRestartSignal();   // fills resultdefault and pastRateTickDTO

                // ---------------------------------------------
                // STEP 2: Prepare identifiers for default screen
                // ---------------------------------------------
                identifiers = symbolMaster?.ToList() ?? new List<string>();

                // ---------------------------------------------
                // STEP 3: Rebuild full grid with 0 data
                // ---------------------------------------------
                InitializeDataGridView(); // Creates rows & applies column prefs

                // ---------------------------------------------
                // STEP 4: Update menu (files)
                // ---------------------------------------------
                await MenuLoadAsync();

                // ---------------------------------------------
                // STEP 5: ENSURE SIGNALR IS CONNECTED
                // ---------------------------------------------
                //await EnsureSignalRConnectedAndSubscribedAsync();

                // ---------------------------------------------
                // STEP 6: APPEAR: Consumer + Grid now continue flowing live ticks
                // ---------------------------------------------
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public async void NewCTRLNToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    //// 6. Clean up current resources before switching
            //    //CleanupBeforeViewSwitch();

            //    //// 1. Set new view mode
            //    //marketWatchViewMode = MarketWatchViewMode.New;

            //    //// 2. Reset state if not in edit mode
            //    //if (!isEdit)
            //    //{
            //    //    selectedSymbols.Clear();
            //    //    saveFileName = null;
            //    //    isLoadedSymbol = false;
            //    //}

            //    //// 3. Create and configure new editable grid
            //    //var editableGrid = new EditableMarketWatchGrid
            //    //{
            //    //    Name = "editableMarketWatchGridView",
            //    //    Dock = DockStyle.Fill,
            //    //    //pastRateTickDTO = pastRateTickDTO,
            //    //    isEditMarketWatch = true,
            //    //    SymbolName = SubscribeSymbol,
            //    //};

            //    //// 4. Handle edit mode specific setup
            //    //if (isEdit && editableGrid.selectedSymbols != null && string.IsNullOrEmpty(saveFileName))
            //    //{
            //    //    //editableGrid.saveFileName = saveFileName;
            //    //}

            //    //// 5. Add to controls and bring to front
            //    //this.Controls.Add(editableGrid);
            //    //editableGrid.BringToFront();
            //    //editableGrid.Focus();

            //    //// 7. Update UI state
            //    //UpdateUIStateForNewMarketWatch();


            //}
            //catch (Exception ex)
            //{
            //    ApplicationLogger.LogException(ex);
            //    MessageBox.Show($"Error switching to new market watch: {ex.Message}");
            //}
            //finally
            //{
            //    if (this.InvokeRequired)
            //    {
            //        this.Invoke(new Action(async () =>
            //        {
            //            notificationSettings.Visible = false;
            //            licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");

            //            RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
            //            if (RemainingDays <= 7)
            //            {
            //                await CheckLicenceLoop();
            //            }
            //            else
            //            {
            //                licenceExpire.Text = $"License Expired On :- {licenceDate}";
            //            }
            //        }));
            //    }
            //    else
            //    {
            //        notificationSettings.Visible = false;
            //        licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");

            //        RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
            //        if (RemainingDays <= 7)
            //        {
            //            await CheckLicenceLoop();
            //        }
            //        else
            //        {
            //            licenceExpire.Text = $"License Expired On :- {licenceDate}";
            //        }
            //    }
            //}

            try
            {
                // 1. Clean up existing NewsControl if already present
                var existingNewMarketWatch = this.Controls.Find("newMarketWatchControlView", true).FirstOrDefault();
                if (existingNewMarketWatch != null)
                {
                    this.Controls.Remove(existingNewMarketWatch);
                    existingNewMarketWatch.Dispose();
                }

                // 2. Create new AboutControl
                var newMarketWatchControl = new EditableMarketWatchControl()
                {
                    Name = "newMarketWatchControlView",
                    Dock = DockStyle.Fill
                };
                saveMarketWatchHost.Visible = true;
                fontSizeComboBox.Visible = false;
                //uiManager?.SetFontSizeComboBoxVisibility(false);

                pnlSearch.Visible = true;
                searchTextLabel.Visible = true;
                txtsearch.Visible = true;
                //uiManager?.SetSearchBoxVisibility(false);

                refreshMarketWatchHost.Visible = false;
                newCTRLNToolStripMenuItem1.Enabled = false;
                // Update status label

                // Update title based on edit mode
                titleLabel.Text = "New Market Watch";

                // 🔥 THIS IS WHERE SetSymbolMaster IS CALLED
                newMarketWatchControl.SetSymbolMaster(SymbolName);

                // 3. Add it to main form
                this.Controls.Add(newMarketWatchControl);
                newMarketWatchControl.BringToFront();
                newMarketWatchControl.Focus();

                licenceDate = LoginInfo.RateExpiredDate.ToString();

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop();
                }
                else
                {
                    licenceExpire.Text = $"Licence Expire At:- {LoginInfo.RateExpiredDate:dd:MM:yyyy}";
                }

            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading News view: {ex.Message}");
            }
            finally
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        notificationSettings.Visible = false;
                    }));
                }
                else
                {
                    notificationSettings.Visible = false;
                }
            }
        }

        public async void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //if (FileLists == null || FileLists.Count == 0)
                //{
                //    MessageBox.Show("No Market Watch available to delete.", "Information",
                //                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}

                using (var selectionForm = new Form())
                {
                    // --- Form Styling ---
                    selectionForm.Text = "Manage Watchlists";
                    selectionForm.Size = new Size(550, 600);
                    selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    selectionForm.StartPosition = FormStartPosition.CenterParent;
                    selectionForm.BackColor = Color.White;
                    selectionForm.MaximizeBox = false;
                    selectionForm.MinimizeBox = false;
                    selectionForm.ShowIcon = false;
                    selectionForm.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular); // Modern Font

                    // --- 1. Header Section ---
                    var headerPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 60,
                        BackColor = Color.White,
                        Padding = new Padding(20, 0, 0, 0)
                    };
                    // Draw subtle bottom border
                    headerPanel.Paint += (s, p) =>
                    {
                        p.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                    };

                    var headerLabel = new Label
                    {
                        Text = "Delete Market Watch",
                        Dock = DockStyle.Fill,
                        ForeColor = Color.FromArgb(30, 41, 59), // Dark Slate Blue
                        TextAlign = ContentAlignment.MiddleLeft,
                        Font = new System.Drawing.Font("Segoe UI", 14F, FontStyle.Bold)
                    };
                    headerPanel.Controls.Add(headerLabel);

                    // --- 2. Search Section ---
                    var searchPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 50,
                        Padding = new Padding(20, 10, 20, 5),
                        BackColor = Color.White
                    };

                    var searchBox = new TextBox
                    {
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("Segoe UI", 11F),
                        ForeColor = Color.Gray,
                        Text = "Search...",
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    // Search Placeholder Logic
                    searchBox.Enter += (s, args) =>
                    {
                        if (searchBox.Text == "Search...") { searchBox.Text = ""; searchBox.ForeColor = Color.Black; }
                    };
                    searchBox.Leave += (s, args) =>
                    {
                        if (string.IsNullOrWhiteSpace(searchBox.Text)) { searchBox.Text = "Search..."; searchBox.ForeColor = Color.Gray; }
                    };

                    searchPanel.Controls.Add(searchBox);

                    // --- 3. ListView Section ---
                    var listPanel = new Panel
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(20, 5, 20, 10), // Padding around the list
                        BackColor = Color.White
                    };

                    var listView = new ListView
                    {
                        Dock = DockStyle.Fill,
                        CheckBoxes = true,
                        View = View.Details,
                        FullRowSelect = true,
                        GridLines = false,
                        HeaderStyle = ColumnHeaderStyle.Nonclickable,
                        BorderStyle = BorderStyle.FixedSingle,
                        Font = new System.Drawing.Font("Segoe UI", 10F),
                        ShowGroups = false
                    };

                    // Row height
                    ImageList imgList = new ImageList();
                    imgList.ImageSize = new Size(1, 22);
                    listView.SmallImageList = imgList;

                    // Only one column
                    listView.Columns.Add("Name", 420);

                    FileLists = (await _marketwatchServerAPI.GetMarketWatchListAsync()).ToList();
                    // Populate Data
                    foreach (string filePath in FileLists)
                    {
                        if (filePath != saveFileName)
                        {
                            var item = new ListViewItem(Path.GetFileName(filePath));
                            item.Tag = filePath;
                            listView.Items.Add(item);
                        }
                    }


                    if (listView.Items.Count == 0)
                    {
                        MessageBox.Show("There is only one MarketWatch and that Open so can't Delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    listPanel.Controls.Add(listView);

                    // --- 4. Footer Buttons Section ---
                    var controlsPanel = new Panel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 70,
                        BackColor = Color.FromArgb(248, 250, 252), // Very light gray background
                        Padding = new Padding(20, 15, 20, 15)
                    };
                    // Top border for footer
                    controlsPanel.Paint += (s, p) =>
                    {
                        p.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, controlsPanel.Width, 0);
                    };

                    // "Select All" Button (Styled as a Link/Secondary)
                    var btnSelectAll = new Button
                    {
                        Text = "Select All",
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Transparent,
                        ForeColor = Color.FromArgb(0, 120, 215), // Blue
                        Size = new Size(100, 40),
                        Dock = DockStyle.Left,
                        Cursor = Cursors.Hand,
                        Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold)
                    };
                    btnSelectAll.FlatAppearance.BorderSize = 0;
                    btnSelectAll.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);

                    // "Delete" Button (Danger Style)
                    var btnDelete = new Button
                    {
                        Text = "Delete Selected",
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(220, 38, 38), // Red
                        ForeColor = Color.White,
                        Size = new Size(130, 40),
                        Dock = DockStyle.Right,
                        Cursor = Cursors.Hand,
                        Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold)
                    };
                    btnDelete.FlatAppearance.BorderSize = 0;

                    // "Cancel" Button
                    var btnCancel = new Button
                    {
                        Text = "Cancel",
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        Size = new Size(90, 40),
                        Dock = DockStyle.Right,
                        Cursor = Cursors.Hand
                    };
                    btnCancel.FlatAppearance.BorderColor = Color.LightGray;

                    // Spacer
                    Panel spacer = new Panel { Width = 10, Dock = DockStyle.Right };

                    // --- Event Handlers ---

                    btnSelectAll.Click += (s, args) =>
                    {
                        bool anyUnchecked = listView.Items.Cast<ListViewItem>().Any(i => !i.Checked);
                        foreach (ListViewItem item in listView.Items)
                        {
                            item.Checked = anyUnchecked;
                        }
                        btnSelectAll.Text = anyUnchecked ? "Unselect All" : "Select All";
                    };

                    btnCancel.Click += (s, args) => selectionForm.DialogResult = DialogResult.Cancel;

                    btnDelete.Click += async (s, args) =>
                    {
                        var selectedFiles = listView.CheckedItems.Cast<ListViewItem>()
                                                    .Select(item => item.Tag.ToString())
                                                    .ToList();

                        if (selectedFiles.Count == 0)
                        {
                            MessageBox.Show("Please select at least one Market Watch to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var confirmResult = MessageBox.Show($"Are you sure you want to delete {selectedFiles.Count} Market Watch(s)?\nThis action cannot be undone.",
                                                            "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (confirmResult == DialogResult.Yes)
                        {
                            int successCount = 0;
                            var failedDeletions = new List<string>();

                            foreach (string filePath in selectedFiles)
                            {
                                if (saveFileName == filePath)
                                {
                                    failedDeletions.Add($"{Path.GetFileName(filePath)} (Currently Open)");
                                    continue;
                                }

                                string fullpath = Path.Combine(AppFolder, username, $"{filePath}.slt");

                                var marketwatchMenuItem = await _marketwatchServerAPI.GetMarketWatchByNameAsync(filePath);
                                try
                                {
                                    DeleteExcelSheet(filePath);
                                    if (File.Exists(fullpath))
                                    {
                                        File.Delete(fullpath);
                                        successCount++;
                                        isdeleted = true;
                                    }
                                    var result = await _marketwatchServerAPI.DeleteMarketWatchAsync(marketwatchMenuItem.MarketWatchId);
                                    if (result) { successCount++; }
                                }
                                catch (Exception ex)
                                {
                                    failedDeletions.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                                    ApplicationLogger.LogException(ex);
                                }
                            }

                            // --- SUCCESS MESSAGE LOGIC ---
                            StringBuilder msg = new StringBuilder();
                            if (successCount > 0)
                            {
                                msg.AppendLine($"✅ Successfully deleted {successCount} watchlist(s).");
                            }

                            if (failedDeletions.Count > 0)
                            {
                                msg.AppendLine("\n❌ Failed to delete:");
                                foreach (var fail in failedDeletions) msg.AppendLine($"- {fail}");
                            }

                            MessageBoxIcon icon = failedDeletions.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
                            MessageBox.Show(msg.ToString(), "Deletion Result", MessageBoxButtons.OK, icon);

                            if (successCount > 0)
                            {
                                selectionForm.DialogResult = DialogResult.OK;
                                await MenuLoadAsync(); // Refresh menu
                            }
                        }
                    };

                    // Search Logic
                    searchBox.TextChanged += (s, args) =>
                    {
                        string txt = searchBox.Text == "Search..." ? "" : searchBox.Text;
                        listView.BeginUpdate();
                        listView.Items.Clear();

                        foreach (string filePath in FileLists.Where(f => Path.GetFileName(f).IndexOf(txt, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            if (filePath != saveFileName)
                            {
                                var item = new ListViewItem(Path.GetFileName(filePath));
                                item.SubItems.Add(filePath);
                                item.Tag = filePath;
                                listView.Items.Add(item);
                            }
                        }
                        listView.EndUpdate();
                    };

                    // Add Controls
                    controlsPanel.Controls.Add(btnDelete);
                    controlsPanel.Controls.Add(spacer);
                    controlsPanel.Controls.Add(btnCancel);
                    controlsPanel.Controls.Add(btnSelectAll);

                    selectionForm.Controls.Add(listPanel);
                    selectionForm.Controls.Add(searchPanel);
                    selectionForm.Controls.Add(controlsPanel);
                    selectionForm.Controls.Add(headerPanel);

                    // Show
                    if (selectionForm.ShowDialog() == DialogResult.OK)
                    {
                        saveFileName = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(async () =>
                    {
                        notificationSettings.Visible = false;
                        licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");

                        RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                        if (RemainingDays <= 7)
                        {
                            await CheckLicenceLoop();
                        }
                        else
                        {
                            licenceExpire.Text = $"License Expired On :- {licenceDate}";
                        }
                    }));
                }
                else
                {
                    notificationSettings.Visible = false;
                    licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");

                    RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                    if (RemainingDays <= 7)
                    {
                        await CheckLicenceLoop();
                    }
                    else
                    {
                        licenceExpire.Text = $"License Expired On :- {licenceDate}";
                    }
                }
            }
        }

        public static void DeleteExcelSheet(string filename)
        {
            try
            {
                // Attempt to connect to Excel (if running)
                var excelApp = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");

                if (excelApp != null)
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string destExcelPath = Path.Combine(desktopPath, "thecalcify.xlsx");

                    // Loop through all open workbooks
                    foreach (Microsoft.Office.Interop.Excel.Workbook wb in excelApp.Workbooks)
                    {
                        if (string.Equals(wb.FullName, destExcelPath, StringComparison.OrdinalIgnoreCase))
                        {
                            // Try to find the worksheet by name
                            Microsoft.Office.Interop.Excel.Worksheet sheetToDelete = null;

                            foreach (Microsoft.Office.Interop.Excel.Worksheet ws in wb.Sheets)
                            {
                                if (string.Equals(ws.Name, filename, StringComparison.OrdinalIgnoreCase))
                                {
                                    sheetToDelete = ws;
                                    break;
                                }
                            }

                            if (sheetToDelete != null)
                            {
                                // If the sheet is active, switch to "Sheet1" before deleting
                                try
                                {
                                    if (sheetToDelete == wb.ActiveSheet)
                                    {
                                        Microsoft.Office.Interop.Excel.Worksheet fallbackSheet = null;
                                        foreach (Microsoft.Office.Interop.Excel.Worksheet ws in wb.Sheets)
                                        {
                                            if (string.Equals(ws.Name, "Sheet1", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fallbackSheet = ws;
                                                break;
                                            }
                                        }

                                        if (fallbackSheet != null)
                                        {
                                            fallbackSheet.Activate();
                                        }
                                    }

                                    // Delete the sheet (suppress confirmation)
                                    excelApp.DisplayAlerts = false;
                                    sheetToDelete.Delete();
                                    excelApp.DisplayAlerts = true;
                                }
                                catch (Exception sheetEx)
                                {
                                    ApplicationLogger.LogException(sheetEx); // Optional log
                                }
                            }

                            break; // Exit loop once workbook is found
                        }
                    }
                }
            }
            catch (COMException)
            {
                // Excel is not running; skip silently
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex); // Log unexpected issues
            }

        }

        //public void MenuLoad()
        //{
        //    try
        //    {
        //        // Final folder path
        //        string finalPath = Path.Combine(AppFolder, username);

        //        // Get all .slt files from the application folder
        //        List<string> fileNames = Directory.GetFiles(finalPath, "*.slt")
        //                                         .Select(Path.GetFileNameWithoutExtension)
        //                                         .ToList();

        //        FileLists = fileNames;

        //        // Clear existing menu items
        //        viewToolStripMenuItem.DropDownItems.Clear();
        //        // Add Default menu item with click handler
        //        ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("👁️‍🗨️ Default");
        //        defaultMenuItem.Click += async (sender, e) =>
        //        {
        //            selectedSymbols.Clear();
        //            identifiers.Clear();
        //            saveFileName = null;
        //            lastOpenMarketWatch = "Default";

        //            var clickedItem = (ToolStripMenuItem)sender;
        //            await DefaultToolStripMenuItem_Click(sender, e);
        //            addEditSymbolsToolStripMenuItem.Enabled = false;
        //            await LoadInitialMarketDataAsync();
        //            isGrid = true;
        //            reloadGrid = true;
        //        };

        //        viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

        //        // Add each file as a menu item with a click handler
        //        foreach (string fileName in fileNames)
        //        {
        //            ToolStripMenuItem menuItem = new ToolStripMenuItem("👁️‍🗨️ " + fileName);
        //            menuItem.Click += async (sender, e) =>
        //            {
        //                selectedSymbols.Clear();
        //                identifiers.Clear();
        //                saveFileName = string.Empty;
        //                //_updateQueue = new ConcurrentQueue<MarketDataDto>();

        //                var clickedItem = (ToolStripMenuItem)sender;

        //                saveFileName = clickedItem.Text.Replace("👁️‍🗨️", "").Trim();
        //                addEditSymbolsToolStripMenuItem.Enabled = true;
        //                lastOpenMarketWatch = saveFileName;

        //                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
        //                if (editableMarketWatchGrid != null)
        //                {
        //                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
        //                    {
        //                        editableMarketWatchGrid.EndEdit();
        //                    }

        //                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
        //                    editableMarketWatchGrid.Dispose();
        //                }

        //                saveMarketWatchHost.Visible = false;
        //                refreshMarketWatchHost.Visible = true;
        //                await LoadSymbol(Path.Combine(saveFileName + ".slt"));

        //                try
        //                {
        //                    if (titleLabel != null)
        //                    {
        //                        titleLabel.Text = !string.IsNullOrWhiteSpace(saveFileName)
        //                            ? saveFileName.ToUpper()
        //                            : "Default";
        //                    }
        //                    else
        //                    {
        //                        ApplicationLogger.Log("titleLabel is null at MenuLoad");
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    ApplicationLogger.LogException(ex);
        //                    ApplicationLogger.Log("saveFileName: " + saveFileName ?? "NULL");
        //                }

        //                isEdit = false;
        //                await LoadInitialMarketDataAsync();
        //                isGrid = true;
        //                reloadGrid = true;
        //            };
        //            viewToolStripMenuItem.DropDownItems.Add(menuItem);
        //        }
        //    }
        //    catch (DirectoryNotFoundException)
        //    {
        //        // Clear existing menu items
        //        viewToolStripMenuItem.DropDownItems.Clear();
        //        // Add Default menu item with click handler
        //        ToolStripMenuItem defaultMenuItem = new ToolStripMenuItem("Default");
        //        defaultMenuItem.Click += async (sender, e) =>
        //        {
        //            selectedSymbols.Clear();
        //            identifiers.Clear();
        //            lastOpenMarketWatch = "Default";

        //            var clickedItem = (ToolStripMenuItem)sender;
        //            await DefaultToolStripMenuItem_Click(sender, e);
        //            MenuLoad();
        //            addEditSymbolsToolStripMenuItem.Enabled = false;
        //            saveFileName = null;
        //            titleLabel.Text = "DEFAULT";
        //            await LoadInitialMarketDataAsync();
        //            isGrid = true;
        //            reloadGrid = true;
        //        };
        //        defaultMenuItem.Enabled = true;
        //        viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger.LogException(ex);
        //    }
        //}

        //public async Task LoadSymbol(string Filename)
        //{
        //    try
        //    {
        //        //savelabel.Visible = false;
        //        //fontSizeComboBox.Visible = true;
        //        //uiManager?.SetFontSizeComboBoxVisibility(true);

        //        pnlSearch.Visible = true;
        //        searchTextLabel.Visible = true;
        //        txtsearch.Clear();
        //        txtsearch.Visible = true;
        //        //uiManager?.SetSearchBoxVisibility(true);


        //        string finalPath = Path.Combine(AppFolder, username);
        //        selectedSymbols.Clear();
        //        Filename = Path.Combine(finalPath, Filename);
        //        string cipherText = File.ReadAllText(Filename);
        //        string json = CryptoHelper.Decrypt(cipherText, "v@d{4NME4sOSywXF");
        //        var symbols = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
        //        selectedSymbols.AddRange(symbols);
        //        identifiers = selectedSymbols.Distinct().ToList();
        //        isLoadedSymbol = true;
        //        marketWatchViewMode = MarketWatchViewMode.Default;
        //        titleLabel.Text = Path.GetFileNameWithoutExtension(Filename).ToUpper();
        //        InitializeDataGridView();          // Configure the grid

        //        //pageSwitched = true;

        //        //await EnsureSignalRConnectedAndSubscribedAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("File Was Never Save Or Moved Please Try Again!", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        ApplicationLogger.LogException(ex);
        //    }

        //    thecalcifyGrid();
        //    MenuLoad();
        //}

        public async Task MenuLoadAsync()
        {
            try
            {
                var marketWatches = await _marketwatchServerAPI.GetMarketWatchAsync();

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        viewToolStripMenuItem.DropDownItems.Clear();

                    // Default
                    var defaultMenuItem = new ToolStripMenuItem("👁️‍🗨️ Default");
                    defaultMenuItem.Click += async (s, e) =>
                    {
                        selectedSymbols.Clear();
                        identifiers.Clear();
                        lastOpenMarketWatch = "Default";

                        await DefaultToolStripMenuItem_Click(s, e);
                        addEditSymbolsToolStripMenuItem.Enabled = false;

                        isGrid = true;
                        reloadGrid = true;

                        SendRestartSignal();


                    };

                    viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

                    // API MarketWatches
                    foreach (var mw in marketWatches)
                    {
                        var menuItem = new ToolStripMenuItem("👁️‍🗨️ " + mw.MarketWatchName);

                        menuItem.Click += async (s, e) =>
                        {

                            await LoadMarketWatchFromApiAsync(mw);
                            marketWatchItem = mw;

                            SendRestartSignal();

                        };

                        viewToolStripMenuItem.DropDownItems.Add(menuItem);
                    }
                    }));
                }
                else 
                {
                    viewToolStripMenuItem.DropDownItems.Clear();

                    // Default
                    var defaultMenuItem = new ToolStripMenuItem("👁️‍🗨️ Default");
                    defaultMenuItem.Click += async (s, e) =>
                    {

                        selectedSymbols.Clear();
                        identifiers?.Clear();
                        lastOpenMarketWatch = "Default";

                        await DefaultToolStripMenuItem_Click(s, e);
                        addEditSymbolsToolStripMenuItem.Enabled = false;

                        isGrid = true;
                        reloadGrid = true;

                        SendRestartSignal();

                    };

                    viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

                    // API MarketWatches
                    foreach (var mw in marketWatches)
                    {
                        var menuItem = new ToolStripMenuItem("👁️‍🗨️ " + mw.MarketWatchName);

                        menuItem.Click += async (s, e) =>
                        {

                            await LoadMarketWatchFromApiAsync(mw);
                            marketWatchItem = mw;

                            SendRestartSignal();

                        };

                        viewToolStripMenuItem.DropDownItems.Add(menuItem);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private async Task LoadMarketWatchFromApiAsync(MarketWatchItem marketWatch)
        {
            try
            {
                savelabel.Visible = false;
                fontSizeComboBox.Visible = true;

                searchTextLabel.Visible = true;
                txtsearch.Clear();
                txtsearch.Visible = true;

                selectedSymbols.Clear();
                identifiers.Clear();
                isLoadedSymbol = true;

                selectedSymbols.AddRange(marketWatch.Symbols);
                selectedSymbols.RemoveAll(s =>
                    string.IsNullOrWhiteSpace(s) || s.Equals("null", StringComparison.OrdinalIgnoreCase)
                );

                identifiers = marketWatch.Symbols.Distinct().ToList();
                identifiers.RemoveAll(s =>
                    string.IsNullOrWhiteSpace(s) || s.Equals("null", StringComparison.OrdinalIgnoreCase)
                );

                lastOpenMarketWatch = marketWatch.MarketWatchName;
                titleLabel.Text = marketWatch.MarketWatchName.ToUpper();

                // Dispose old grid
                var grid = EditableMarketWatchGrid.CurrentInstance;
                if (grid != null)
                {
                    if (grid.IsCurrentCellInEditMode)
                        grid.EndEdit();

                    grid.EditableDispose();
                    grid.Dispose();
                }


                // Switch UI to main grid panel
                thecalcifyGrid();


                saveMarketWatchHost.Visible = false;
                refreshMarketWatchHost.Visible = true;
                addEditSymbolsToolStripMenuItem.Enabled = true;
                marketWatchViewMode = MarketWatchViewMode.Default;


                InitializeDataGridView();


                isEdit = false;
                isGrid = true;
                reloadGrid = true;

                if ((selectedSymbols == null || selectedSymbols.Count == 0) && (identifiers == null || identifiers.Count == 0))
                {
                    var result = MessageBox.Show(
                        "No symbols are currently selected.\n\n" +
                        "Would you like to add symbols to this Market Watch now?",
                        "No Symbols Selected",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        AddEditSymbolsToolStripMenuItem_Click(null, null);
                    }
                }


            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show("Failed to load MarketWatch", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearCollections()
        {
            try
            {
                //lock (_updateQueue)
                //{
                //    while (_updateQueue.TryDequeue(out _)) { }
                //}

                lock (symbolRowMap)
                {
                    symbolRowMap.Clear();
                }

                _prevAskMap.Clear();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void UpdateUIStateForNewMarketWatch()
        {
            try
            {
                ClearCollections();

                // Update menu items
                toolsToolStripMenuItem.Enabled = true;
                newCTRLNToolStripMenuItem1.Enabled = false;

                pnlSearch.Visible = true;
                searchTextLabel.Visible = true;
                txtsearch.Visible = true;

                // Update save button visibility
                saveMarketWatchHost.Visible = true;
                saveMarketWatchHost.Text = "Save MarketWatch";
                refreshMarketWatchHost.Visible = false;

                fontSizeComboBox.Visible = false;
                //uiManager?.SetFontSizeComboBoxVisibility(false);

                // Update title based on edit mode
                titleLabel.Text = isEdit
                    ? $"Edit {saveFileName?.ToUpper() ?? "Unknown"} MarketWatch"
                    : "New MarketWatch";

                // Reset save file name
                saveFileName = null;

                savelabel.Visible = true;

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
            try
            {
                // 1. Dispose SignalR connection properly
                //DisposeSignalRConnection();

                // //2.Stop and dispose timers
                //signalRTimer?.Stop();
                //signalRTimer?.Dispose();
                //signalRTimer = null;


                //if (_watchdogTimer != null)
                //{
                //    _watchdogTimer.Stop();
                //    _watchdogTimer.Dispose();
                //    _watchdogTimer = null;
                //}

                //_updateTimer?.Dispose();
                //_updateTimer = null;

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
                    {
                        editableMarketWatchGrid.EndEdit();
                    }

                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
                    editableMarketWatchGrid.Dispose();
                }


                _latestUpdates.Clear();
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
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }


        private void CleanupDataGridView()
        {
            try
            {
                if (defaultGrid.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        defaultGrid.SuspendLayout();
                        defaultGrid.Visible = false;
                        defaultGrid.DataSource = null; // Unbind data
                        defaultGrid.Rows.Clear();
                        defaultGrid.Columns.Clear();
                        defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
                        defaultGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 1.5f, FontStyle.Bold);
                        defaultGrid.ResumeLayout();
                    }));
                }
                else
                {
                    defaultGrid.SuspendLayout();
                    defaultGrid.Visible = false;
                    defaultGrid.DataSource = null; // Unbind data
                    defaultGrid.Rows.Clear();
                    defaultGrid.Columns.Clear();
                    defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
                    defaultGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 1.5f, FontStyle.Bold);
                    defaultGrid.ResumeLayout();
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public static void KillProcess()
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
                    ApplicationLogger.LogException(ex);
                }
            }
        }

        public async void exportWorksheetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Clean up existing ExportControl if already present
                var existingExport = this.Controls.Find("exportControlView", true).FirstOrDefault();
                if (existingExport != null)
                {
                    this.Controls.Remove(existingExport);
                    existingExport.Dispose();
                }

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
                    {
                        editableMarketWatchGrid.EndEdit();
                    }

                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
                    editableMarketWatchGrid.Dispose();
                }


                // 2. Create new ExportControl
                var exportControl = new UserExcelExportForm(token)
                {
                    Name = "exportControlView",
                    Dock = DockStyle.Fill
                };

                //DisposeSignalRConnection();
                saveMarketWatchHost.Visible = false;
                fontSizeComboBox.Visible = false;
                pnlSearch.Visible = false;
                searchTextLabel.Visible = false;
                txtsearch.Visible = false;
                refreshMarketWatchHost.Visible = false;
                newCTRLNToolStripMenuItem1.Enabled = true;
                // Update status label

                // Update title based on edit mode
                titleLabel.Text = "Export Excel Sheets";

                // 3. Add it to main form
                this.Controls.Add(exportControl);
                exportControl.BringToFront();
                exportControl.Focus();

                licenceDate = LoginInfo.RateExpiredDate.ToString();

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop();
                }
                else
                {
                    licenceExpire.Text = $"Licence Expire At:- {LoginInfo.RateExpiredDate:dd:MM:yyyy}";
                }

            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading Export view: {ex.Message}");
            }
        }


        #endregion

        #region Tools
        public void FullScreenF11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public async void DisconnectESCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ Stop background processes
                await StopBackgroundTasks(); // You define this method

                // 2️⃣ Unsubscribe event handlers
                UnsubscribeAllEvents(); // Optional, but recommended if you manually subscribed

                await LogoutAsync();

                // 3️⃣ Show Login Form
                Login loginForm = new Login();
                loginForm.Show();

                // 4️⃣ Dispose current form
                this.Hide();      // optional: avoid flicker before dispose
                this.Dispose();   // frees unmanaged resources
                this.Close();   // frees unmanaged resources

                // 5️⃣ Kill extra processes if needed (use with caution)
                KillProcess();    // Only if you're absolutely sure it's safe to kill processes
                //await DisconnectESCToolStripMenuItem_ClickAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during disconnect: " + ex.Message);
            }
            finally
            {
                await StopBackgroundTasks();
            }
        }

        #endregion

        #region Column/Symbol - Add/Remove

        public void AddEditColumnsToolStripMenuItem_Click(object sender, EventArgs e)
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

                    CredentialManager.SaveMarketWatchWithColumns(lastOpenMarketWatch, (columnPreferences.Count == 0 || columnPreferences == null) ? columnPreferencesDefault : columnPreferences);

                    panelAddColumns.Visible = false;
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
            }

            panelAddColumns.Visible = true;
            panelAddColumns.BringToFront();
        }

        public void AddEditSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
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
                    checkedListSymbols.Dock = DockStyle.Fill;
                    buttonPanel.Dock = DockStyle.Bottom;

                    buttonPanel.Controls.Add(btnSelectAllSymbols);
                    buttonPanel.Controls.Add(btnConfirmAddSymbols);
                    buttonPanel.Controls.Add(btnCancelAddSymbols);

                    panelAddSymbols.Controls.Add(buttonPanel);
                    panelAddSymbols.Controls.Add(checkedListSymbols);
                    panelAddSymbols.Controls.Add(titleLabel);

                    this.Controls.Add(panelAddSymbols);

                    this.Resize += (s3, e3) =>
                    {
                        panelAddSymbols.Location = new System.Drawing.Point(
                            (this.Width - panelAddSymbols.Width) / 2,
                            (this.Height - panelAddSymbols.Height) / 2
                        );
                    };

                    // Select All click
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

                    // Save click
                    btnConfirmAddSymbols.Click += async (s, e2) =>
                    {
                        // Get the checked display names (SymbolName)
                        var currentlyCheckedNames = checkedListSymbols.CheckedItems.Cast<string>().ToList();

                        if (!currentlyCheckedNames.Any())
                        {
                            MessageBox.Show("Please select at least one symbol to confirm.", "No Selection",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Map checked names back to their symbols
                        var currentlyCheckedSymbols = SymbolName
                            .Where(x => currentlyCheckedNames.Contains(x.SymbolName.Trim()))
                            .Select(x => x.Symbol)
                            .ToList();

                        var previouslySelected = selectedSymbols;

                        var addedSymbols = currentlyCheckedSymbols.Except(previouslySelected).ToList();
                        var removedSymbols = previouslySelected.Except(currentlyCheckedSymbols).ToList();

                        if (!addedSymbols.Any() && !removedSymbols.Any())
                        {
                            MessageBox.Show("No changes made.");
                            return;
                        }

                        //// Save changes
                        //EditableMarketWatchGrid editableMarketWatchGrid =
                        //    EditableMarketWatchGrid.CurrentInstance ?? new EditableMarketWatchGrid();

                        //editableMarketWatchGrid.isGrid = false;
                        ////editableMarketWatchGrid.saveFileName = saveFileName;
                        //editableMarketWatchGrid.username = username;

                        selectedSymbols = currentlyCheckedSymbols;
                        await EditableMarketWatchGrid.SaveMarketWatchAsync(selectedSymbols,marketWatchItem);

                        // identifiers drives the grid; SignalR still uses symbolMaster
                        identifiers = selectedSymbols;

                        SafeInvoke(InitializeDataGridView);
                        SendRestartSignal();

                        //await EnsureSignalRConnectedAndSubscribedAsync();

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
                        checkedListSymbols.Items.Add(item.SymbolName.Trim(), true);
                    }
                }

                // Then unselected symbols
                foreach (var item in SymbolName)
                {
                    if (!identifiers.Contains(item.Symbol))
                    {
                        checkedListSymbols.Items.Add(item.SymbolName.Trim(), false);
                    }
                }

                panelAddSymbols.Visible = true;
                panelAddSymbols.BringToFront();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        #endregion

        #region Helper Method

        public async Task LoadInitialMarketDataAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    var request = new HttpRequestMessage(HttpMethod.Get, $"{APIPath}getInstrument");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.SendAsync(request).ConfigureAwait(false);


                    var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    resultdefault = JsonConvert.DeserializeObject<MarketApiResponse>(jsonString);

                    if (resultdefault?.data == null)
                        return;

                    //SaveInitDataToFile(resultdefault.data);

                    RunOnUIThread(async () =>
                    {
                        pastRateTickDTO = resultdefault.data;

                        InitializeIdentifiers(resultdefault.data);
                        InitializeSymbolMaster(resultdefault.data);

                        // Filter with identifiers
                        resultdefault.data = resultdefault.data
                            .Where(x => identifiers != null && identifiers.Contains(x.i))
                            .ToList();

                        ApplyInitialSnapshotToGrid(resultdefault.data);
                    });
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                RunOnUIThread(async () =>
                {
                    notificationSettings.Visible = false;

                    licenceDate = LoginInfo.RateExpiredDate.ToString("dd:MM:yyyy");
                    RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;

                    if (RemainingDays <= 7)
                        await CheckLicenceLoop();
                    else
                        licenceExpire.Text = $"License Expired On :- {licenceDate}";
                });
            }
        }

        private bool IsSessionExpired(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.Forbidden ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.NotFound ||
                   !response.IsSuccessStatusCode;
        }

        private void HandleSessionExpired(HttpStatusCode statusCode)
        {
            var app = thecalcify.CurrentInstance;
            app.DisconnectESCToolStripMenuItem_Click(null, null);

            MessageBox.Show(
                "Session expired. Please log in again.",
                "Session Expired",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ApplicationLogger.Log(
                $"Session expired or unauthorized access due to {statusCode} from Symbol Load API");
        }

        private void RunOnUIThread(Func<Task> action)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(async () => await action()));
            else
                _ = action();
        }


        private void InitializeIdentifiers(List<MarketDataDto> data)
        {
            if (identifiers != null)
                return;

            identifiers = data
                .Where(x => !string.IsNullOrEmpty(x.i))
                .Select(x => x.i)
                .ToList();

            SymbolName = data
                .Where(x => !string.IsNullOrEmpty(x.i) && !string.IsNullOrEmpty(x.n))
                .Select(x => (Symbol: x.i, SymbolName: x.n))
                .ToList();
        }

        private void InitializeSymbolMaster(List<MarketDataDto> data)
        {
            if (symbolMaster == null || symbolMaster.Count > 0)
                return;

            symbolMaster = data
                .Where(x => !string.IsNullOrEmpty(x.i))
                .Select(x => x.i)
                .ToList();

            UpdateRtwConfig();
        }


        //private static void SaveInitDataToFile(List<MarketDataDto> data)
        //{
        //    try
        //    {
        //        //var dict = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        //        //foreach (var d in data)
        //        //{
        //        //    dict[d.i] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        //        //    {
        //        //        ["Name"] = d.n,
        //        //        ["Bid"] = d.b,
        //        //        ["Ask"] = d.a,
        //        //        ["LTP"] = d.ltp,
        //        //        ["High"] = d.h,
        //        //        ["Low"] = d.l,
        //        //        ["Open"] = d.o,
        //        //        ["Close"] = d.c,
        //        //        ["Net Chng"] = d.d,
        //        //        ["V"] = d.v,
        //        //        ["ATP"] = d.atp,
        //        //        ["Bid Size"] = d.bq,
        //        //        ["Total Bid Size"] = d.tbq,
        //        //        ["Ask Size"] = d.sq,
        //        //        ["Total Ask Size"] = d.tsq,
        //        //        ["Volume"] = d.vt,
        //        //        ["Open Interest"] = d.oi,
        //        //        ["Last Size"] = d.ltq,
        //        //        ["Time"] = Common.TimeStampConvert(d.t)
        //        //    };
        //        //}

        //        var lastTick = LastTickStore.GetAll();

        //        Directory.CreateDirectory(Path.GetDirectoryName(marketInitDataPath));
        //        string json = JsonConvert.SerializeObject(lastTick);
        //        string encryptedJson = CryptoHelper.Encrypt(json, "v@d{4NME4sOSywXF");
        //        File.WriteAllText(marketInitDataPath, encryptedJson);
        //        SaveInitDataPathToRegistry();
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger.Log($"Error writing initdata.dat: {ex.Message} And {ex.StackTrace}");
        //    }
        //}

        //private static void SaveInitDataPathToRegistry()
        //{
        //    try
        //    {
        //        using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
        //        using (var key = baseKey.CreateSubKey(@"SOFTWARE\thecalcify"))
        //        {
        //            key.SetValue("InitDataPath", marketInitDataPath, RegistryValueKind.String);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger.LogException(ex);
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
                // 🔑 Resolve Name from SubscribeSymbol tuple
                string displayName;
                if (!_symbolNameMap.TryGetValue(symbol, out displayName))
                    displayName = symbol; // fallback


                defaultGrid.Rows.Add(new object[]
                {
            symbol, displayName, "--", "--", "--", "--", "--", "--", "--", "--", "--",
            "--", "--", "--", "--", "--", "--", "--", "--", "--"
                });
            }
        }

        private void ApplyColumnPreferences()
        {
            foreach (DataGridViewColumn col in defaultGrid.Columns)
            {
                col.Visible = columnPreferencesDefault.Contains(col.Name);
                col.ReadOnly = true;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.Resizable = DataGridViewTriState.True;
                col.SortMode = DataGridViewColumnSortMode.NotSortable; // disable sort

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

        private async void InitializeDataGridView()
        {
            LiveGridRegistry.Register(this);
            ReplayLastTicksToDefaultGrid();

            defaultGrid.SuspendLayout();

            defaultGrid.DataSource = null;
            defaultGrid.Rows.Clear();
            defaultGrid.Columns.Clear();

            defaultGrid.AllowUserToAddRows = false;
            defaultGrid.ScrollBars = ScrollBars.Both;
            defaultGrid.AutoGenerateColumns = false;

            InitializeGridColumns();
            PopulateGridRows();
            ApplyColumnPreferences();
            BuildSymbolRowMap();

            //if (resultdefault?.data != null && resultdefault.data.Count != 0)
            //{
            //    // ✅ Filter resultdefault.data to keep only symbols in identifiers
            //    resultdefault.data = resultdefault.data
            //        .Where(x => identifiers.Contains(x.i))
            //        .ToList();
            //    ApplyInitialSnapshotToGrid(resultdefault.data);
            //}

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

            defaultGrid.ResumeLayout();
        }

        private void BuildSymbolRowMap()
        {
            symbolRowMap.Clear();
            for (int i = 0; i < defaultGrid.Rows.Count; i++)
            {
                string symbol = defaultGrid.Rows[i].Cells["symbol"].Value?.ToString().Replace(" ▲", "").Replace(" ▼", "").Trim();
                if (!string.IsNullOrEmpty(symbol))
                    symbolRowMap[symbol] = i;
            }
        }

        private async void ApplyInitialSnapshotToGrid(List<MarketDataDto> snapshot)
        {
            if (snapshot == null || snapshot.Count == 0)
                return;

            // Prevent timer/DTO updates during initial snapshot
            _isGridBuilding = true;

            if (defaultGrid.IsDisposed || !defaultGrid.IsHandleCreated)
                return;

            try
            {
                defaultGrid.SuspendLayout();

                foreach (var dto in snapshot)
                {
                    if (dto == null)
                        continue;

                    // Grid may be rebuilding → stop applying snapshot immediately
                    if (defaultGrid.Rows.Count == 0)
                        break;

                    // Symbol missing → skip
                    if (!symbolRowMap.TryGetValue(dto.i, out int rowIndex))
                        continue;

                    // Row index no longer exists
                    if (rowIndex < 0 || rowIndex >= defaultGrid.Rows.Count)
                        continue;

                    dto.n = SymbolName 
                        .Where(x => x.Symbol == dto.i)
                        .Select(x => x.SymbolName)
                        .FirstOrDefault();

                    LastTickStore.ExcelPublish(dto);

                    DataGridViewRow row;

                    try
                    {
                        row = defaultGrid.Rows[rowIndex];
                    }
                    catch
                    {
                        continue;   // Grid refreshing / replaced → skip safely
                    }

                    // SAFE cell updates
                    SafeSet(row, "Name", dto.n ?? "--");
                    SafeSet(row, "Bid", dto.b);
                    SafeSet(row, "Ask", dto.a);
                    SafeSet(row, "LTP", dto.ltp);
                    SafeSet(row, "High", dto.h);
                    SafeSet(row, "Low", dto.l);
                    SafeSet(row, "Open", dto.o);
                    SafeSet(row, "Close", dto.c);
                    SafeSet(row, "Net Chng", dto.d);
                    SafeSet(row, "ATP", dto.atp);
                    SafeSet(row, "Bid Size", dto.bq);
                    SafeSet(row, "Total Bid Size", dto.tbq);
                    SafeSet(row, "Ask Size", dto.sq);
                    SafeSet(row, "Total Ask Size", dto.tsq);
                    SafeSet(row, "Volume", dto.vt);
                    SafeSet(row, "Open Interest", dto.oi);
                    SafeSet(row, "Last Size", dto.ltq);
                    SafeSet(row, "Time", Common.TimeStampConvert(dto.t));

                    // Track timestamp
                    if (long.TryParse(dto.t, out long ts))
                        _rowLastUpdate[dto.i] = ts;
                }
            }
            finally
            {
                defaultGrid.ResumeLayout();
                _isGridBuilding = false;
            }
        }

        private void SafeSet(DataGridViewRow row, string col, object value)
        {
            if (row == null) return;
            var cell = row.Cells[col];
            if (cell != null) cell.Value = value;
        }

        public void HandleLastOpenedMarketWatch()
        {
            try
            {
                if (string.IsNullOrEmpty(lastOpenMarketWatch))
                    return;

                // Find and click the matching menu item
                foreach (ToolStripMenuItem item in viewToolStripMenuItem.DropDownItems)
                {
                    if (item.Text.Replace("👁️‍🗨️", "").Trim() == lastOpenMarketWatch)
                    {
                        item.PerformClick();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }


        private void SafeInvoke(Action action)
        {
            if (!IsDisposed && IsHandleCreated)
            {
                if (InvokeRequired) BeginInvoke((MethodInvoker)(() => action()));
                else action();
            }
        }

        private void UnsubscribeAllEvents()
        {
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            System.Windows.Forms.Application.ThreadException -= Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }

        private async Task StopBackgroundTasks()
        {
            _cts?.Cancel();
            await Task.Delay(50);
        }

        #endregion

        #region Alert

        public void AlertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var alertForm = new AlertCreationPanel(token))
            {
                if (isFullScreen)
                {
                    alertForm.StartPosition = FormStartPosition.CenterParent;
                    alertForm.TopMost = true; // Ensures it stays above the full-screen window
                    alertForm.ShowDialog(this); // Pass the main form as owner
                }
                else
                {
                    alertForm.ShowDialog();
                }
            }
        }

        public async void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    using (var aboutForm = new About(username, password, licenceDate, token))
            //    {
            //        if (isFullScreen)
            //        {
            //            aboutForm.StartPosition = FormStartPosition.CenterParent;
            //            aboutForm.TopMost = true; // Ensures it stays above the full-screen window
            //            aboutForm.ShowDialog(this); // Pass the main form as owner
            //        }
            //        else
            //        {
            //            aboutForm.ShowDialog(this);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ApplicationLogger.LogException(ex);
            //}

            try
            {
                // 1. Clean up existing NewsControl if already present
                var existingAbout = this.Controls.Find("aboutControlView", true).FirstOrDefault();
                if (existingAbout != null)
                {
                    this.Controls.Remove(existingAbout);
                    existingAbout.Dispose();
                }

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
                    {
                        editableMarketWatchGrid.EndEdit();
                    }

                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
                    editableMarketWatchGrid.Dispose();
                }


                // 2. Create new AboutControl
                var aboutControl = new About(username, password, licenceDate, token)
                {
                    Name = "aboutControlView",
                    Dock = DockStyle.Fill
                };
                saveMarketWatchHost.Visible = false;
                fontSizeComboBox.Visible = false;
                //uiManager?.SetFontSizeComboBoxVisibility(false);

                pnlSearch.Visible = false;
                searchTextLabel.Visible = false;
                txtsearch.Visible = false;
                //uiManager?.SetSearchBoxVisibility(false);

                refreshMarketWatchHost.Visible = false;
                newCTRLNToolStripMenuItem1.Enabled = true;
                // Update status label

                // Update title based on edit mode
                titleLabel.Text = "About";

                // 3. Add it to main form
                this.Controls.Add(aboutControl);
                aboutControl.BringToFront();
                aboutControl.Focus();

                licenceDate = LoginInfo.RateExpiredDate.ToString();

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop();
                }
                else
                {
                    licenceExpire.Text = $"Licence Expire At:- {LoginInfo.RateExpiredDate:dd:MM:yyyy}";
                }

            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading News view: {ex.Message}");
            }
            finally
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        notificationSettings.Visible = false;
                    }));
                }
                else
                {
                    notificationSettings.Visible = false;
                }
            }
        }

        public async void RefreshMarketWatchHost_Click(object sender, EventArgs e)
        {
            try
            {
                _isGridBuilding = true;

                // 🚫 Disable refresh button while refreshing
                refreshMarketWatchHost.Enabled = false;

                // 🔄 Reset state
                selectedSymbols.Clear();
                identifiers.Clear();
                isEdit = false;
                isGrid = true;
                reloadGrid = true;
                saveFileName = null;
                //lastOpenMarketWatch = "Default";

                // 🧹 Clear grid and data immediately
                if (defaultGrid != null)
                {
                    defaultGrid.DataSource = null;
                    defaultGrid.Rows.Clear();
                    defaultGrid.Columns.Clear();
                    defaultGrid.Refresh();
                }

                // 🔄 Dispose editable grid if any
                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
                    {
                        editableMarketWatchGrid.EndEdit();
                    }

                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
                    editableMarketWatchGrid.Dispose();
                }

                saveMarketWatchHost.Visible = false;
                refreshMarketWatchHost.Visible = true;

                HandleLastOpenedMarketWatch();

                // 🔄 Reload market data & ensure SignalR
                InitializeDataGridView();
                SendRestartSignal(); //await EnsureSignalRConnectedAndSubscribedAsync();

                ApplicationLogger.Log("Refresh: Switched back to DEFAULT Market Watch.");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                // ✅ Re-enable refresh button after process
                refreshMarketWatchHost.Enabled = true;
                _isGridBuilding = false;
            }
        }

        #endregion

        #region Excel Export

        private void ExportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SplashManager.Show(this, "Exporting to Excel...", "Please wait");
                ExportToExcelToolStripMenuItem.Enabled = false;

                if (Type.GetTypeFromProgID("thecalcify", false) == null)
                    RegisterRtdDll("thecalcifyRTD.dll");

                SetThrottle();
                KillProcess();

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string destExcelPath = Path.Combine(desktopPath, "thecalcify.xlsx");

                if (File.Exists(excelFilePath) && !File.Exists(destExcelPath))
                {
                    File.Copy(excelFilePath, destExcelPath);
                }

                //if (!File.Exists(marketInitDataPath))
                //{
                //    SplashManager.Hide();
                //    MessageBox.Show("initdata.dat not found.");
                //    return;
                //}

                //var dict = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                var getPastData = LastTickStore.GetAll();

                //foreach (var d in resultdefault.data)
                //{
                //    dict[d.i] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                //    {
                //        ["Name"] = d.n,
                //        ["Bid"] = d.b,
                //        ["Ask"] = d.a,
                //        ["LTP"] = d.ltp,
                //        ["High"] = d.h,
                //        ["Low"] = d.l,
                //        ["Open"] = d.o,
                //        ["Close"] = d.c,
                //        ["Net Chng"] = d.d,
                //        ["V"] = d.v,
                //        ["ATP"] = d.atp,
                //        ["Bid Size"] = d.bq,
                //        ["Total Bid Size"] = d.tbq,
                //        ["Ask Size"] = d.sq,
                //        ["Total Ask Size"] = d.tsq,
                //        ["Volume"] = d.vt,
                //        ["Open Interest"] = d.oi,
                //        ["Last Size"] = d.ltq,
                //        ["Time"] = Common.TimeStampConvert(d.t)
                //    };
                //}

                List<ExcelFormulaCell> formulaCells = BuildFormulaCells(getPastData);

                // ✅ Excel attach/create
                Microsoft.Office.Interop.Excel.Application excelApp = null;
                try
                {
                    excelApp = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
                }
                catch
                {
                    excelApp = new Microsoft.Office.Interop.Excel.Application();
                    excelApp.Visible = true;
                }

                // ✅ Workbook open or attach
                Microsoft.Office.Interop.Excel.Workbook wb = null;
                foreach (Microsoft.Office.Interop.Excel.Workbook openWb in excelApp.Workbooks)
                {
                    if (string.Equals(openWb.FullName, destExcelPath, StringComparison.OrdinalIgnoreCase))
                    {
                        wb = openWb;
                        break;
                    }
                }

                if (wb == null)
                {
                    if (!File.Exists(destExcelPath))
                    {
                        if (File.Exists(excelFilePath))
                            File.Copy(excelFilePath, destExcelPath);
                        else
                        {
                            SplashManager.Hide();
                            MessageBox.Show("thecalcify Excel file not found.");
                            return;
                        }
                    }

                    wb = excelApp.Workbooks.Open(destExcelPath);
                }

                // ✅ Worksheet select/create
                Microsoft.Office.Interop.Excel.Worksheet ws;
                if (string.IsNullOrEmpty(saveFileName) || saveFileName == "Default")
                {
                    try
                    {
                        ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets["Sheet1"];
                        ws.Cells.Clear();
                    }
                    catch
                    {
                        ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets.Add();
                        ws.Name = "Sheet1";
                    }

                }
                else
                {
                    try
                    {
                        ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets[saveFileName];
                        ws.Cells.Clear();
                    }
                    catch
                    {
                        ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets.Add();
                        ws.Name = saveFileName;
                    }
                }


                // Try to get the sheet if it already exists
                Microsoft.Office.Interop.Excel.Worksheet GetSheetIfExists(string name)
                {
                    foreach (object sheetObj in wb.Sheets)
                    {
                        if (sheetObj is Microsoft.Office.Interop.Excel.Worksheet sheet)
                        {
                            if (sheet.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                                return sheet;
                        }
                    }
                    return null;
                }


                // Try to find existing sheet
                Microsoft.Office.Interop.Excel.Worksheet costCalcWs = GetSheetIfExists("Cost.Cal");

                if (costCalcWs == null)
                {
                    costCalcWs = (Microsoft.Office.Interop.Excel.Worksheet)wb.Sheets.Add();
                    costCalcWs.Name = "Cost.Cal";
                    ApplyCostCalJsonToSheet(costCalcWs, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "excel_model.json"));
                }


                // ✅ Make bulk 2D array
                int maxRow = formulaCells.Max(c => c.Row);
                int maxCol = formulaCells.Max(c => c.Column);
                object[,] bulkData = new object[maxRow, maxCol];

                foreach (var cell in formulaCells)
                {
                    if (cell.Row == 1 || cell.Column == 1)
                    {
                        // Plain text
                        bulkData[cell.Row - 1, cell.Column - 1] = cell.Formula;
                    }
                    else
                    {
                        // Ensure formula has "="
                        string f = cell.Formula.Trim();
                        if (!f.StartsWith("="))
                            f = "=" + f;

                        bulkData[cell.Row - 1, cell.Column - 1] = f;
                    }
                }

                // ✅ Dump in one shot to Excel Range
                Microsoft.Office.Interop.Excel.Range startCell = (Microsoft.Office.Interop.Excel.Range)ws.Cells[1, 1];
                Microsoft.Office.Interop.Excel.Range endCell = (Microsoft.Office.Interop.Excel.Range)ws.Cells[maxRow, maxCol];
                Microsoft.Office.Interop.Excel.Range writeRange = ws.Range[startCell, endCell];

                const int MAX_RETRIES = 10;
                int retries = 0;
                bool success = false;

                while (!success && retries < MAX_RETRIES)
                {
                    try
                    {
                        writeRange.Value2 = bulkData;
                        success = true;
                    }
                    catch (COMException ex) when ((uint)ex.ErrorCode == 0x800AC472)
                    {
                        retries++;
                        System.Threading.Thread.Sleep(100); // Wait 100ms before retry
                    }
                }
                ws.Activate();
                SplashManager.Hide();
                excelApp.Visible = true;
            }
            catch (Exception ex)
            {
                SplashManager.Hide();
                MessageBox.Show("An error occurred while exporting. Please retry.");
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                ExportToExcelToolStripMenuItem.Enabled = true;
            }
        }

        private void ApplyCostCalJsonToSheet(Microsoft.Office.Interop.Excel.Worksheet ws, string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            var cells = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, CellData>>(json);

            foreach (var kv in cells)
            {
                var cell = kv.Value;
                Microsoft.Office.Interop.Excel.Range rng = ws.Cells[cell.Row, cell.Column];

                // Value / Formula
                if (!string.IsNullOrEmpty(cell.Formula))
                    rng.Formula = cell.Formula;
                else
                    rng.Value2 = cell.Value;

                // Formatting
                var f = cell.Format;
                if (f != null)
                {
                    rng.Font.Name = f.FontName;
                    rng.Font.Size = f.FontSize;
                    rng.Font.Bold = f.Bold;
                    rng.Font.Italic = f.Italic;
                    rng.Font.Color = ColorTranslator.FromHtml(f.FontColor);
                    rng.Interior.Color = ColorTranslator.FromHtml(f.BackgroundColor);
                    rng.NumberFormat = f.NumberFormat;

                    // ALIGNMENT FIX
                    rng.HorizontalAlignment = ConvertAlignment(f.HorizontalAlign);
                    rng.VerticalAlignment = ConvertVerticalAlignment(f.VerticalAlign);

                    // BORDERS
                    ApplyBorders(rng);
                }
            }
        }

        private object ConvertAlignment(string align)
        {
            if (string.IsNullOrEmpty(align))
                return Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignGeneral;

            align = align.Trim().ToLower();

            switch (align)
            {
                case "center":
                    return Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                case "left":
                    return Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;

                case "right":
                    return Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;

                case "justify":
                    return Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignJustify;

                default:
                    int val;
                    if (int.TryParse(align, out val))
                        return val;

                    return Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignGeneral;
            }
        }

        private object ConvertVerticalAlignment(string align)
        {
            if (string.IsNullOrEmpty(align))
                return Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignBottom;

            align = align.Trim().ToLower();

            switch (align)
            {
                case "top":
                    return Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignTop;

                case "center":
                    return Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;

                case "bottom":
                    return Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignBottom;

                default:
                    return Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
            }
        }

        private void ApplyBorders(Microsoft.Office.Interop.Excel.Range rng)
        {
            Microsoft.Office.Interop.Excel.Borders borders = rng.Borders;

            borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            borders.Color = ColorTranslator.FromHtml("#000000");
            borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
        }

        public void RegisterRtdDll(string dllName, params string[] searchPaths)
        {
            try
            {
                // 🔹 Locate DLL
                string dllPath = searchPaths
                    .SelectMany(p => new[] { p, Path.Combine(p, dllName) })
                    .Concat(new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\bin\Debug", dllName),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\bin\Release", dllName)
                    })
                    .Select(Path.GetFullPath)
                    .FirstOrDefault(File.Exists);

                if (dllPath == null)
                {
                    ApplicationLogger.Log($"RTD DLL '{dllName}' not found.");
                    return;
                }

                // 🔹 Pick RegAsm (Excel 32-bit → Framework, Excel 64-bit → Framework64)
                bool excel32 = true;
                try
                {
                    var excelApp = new Microsoft.Office.Interop.Excel.Application();
                    excel32 = !excelApp.OperatingSystem.Contains("64");
                    excelApp.Quit();
                }
                catch { excel32 = true; }



                string officeVersion = GetOfficeVersion(); // e.g., "16.0", "14.0"
                int versionMajor = 16; // default to 16

                if (int.TryParse(officeVersion.Split('.')[0], out int parsed))
                {
                    versionMajor = parsed;
                }

                // Decide RegAsm framework version based on Office version
                string regasmFrameworkVersion = versionMajor >= 15 ? "v4.0.30319" : "v2.0.50727";

                // Determine RegAsm path based on Excel bitness and version compatibility
                string regasm = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    excel32 ? $@"Microsoft.NET\Framework\{regasmFrameworkVersion}\RegAsm.exe"
                            : $@"Microsoft.NET\Framework64\{regasmFrameworkVersion}\RegAsm.exe");



                // 🔹 Run unregister + register
                foreach (var args in new[] { $"/unregister \"{dllPath}\"", $"\"{dllPath}\" /codebase /tlb" })
                {
                    var psi = new ProcessStartInfo(regasm, args)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var proc = Process.Start(psi);
                    string output = proc.StandardOutput.ReadToEnd();
                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    if (proc.ExitCode != 0)
                        ApplicationLogger.Log($"RegAsm failed. Args: {args}\nOutput: {output}\nError: {error}");
                }

                ApplicationLogger.Log($"RTD DLL registered successfully: {dllPath}");
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log("RegisterRtdDll Error: " + ex.Message);
            }
        }

        public static void SetThrottle()
        {
            try
            {
                string officeVersion = GetOfficeVersion();

                string AppIcon = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{{45A18102-1652-4AAA-8C62-4306D49EF5AB}}";
                string excelOptionsPath = $@"Software\Microsoft\Office\{officeVersion}\Excel\Options";
                string graphicsPath = $@"Software\Microsoft\Office\{officeVersion}\Common\Graphics";
                string securityPath = $@"Software\Microsoft\Office\{officeVersion}\Excel\Security";

                //
                // ---- Excel Options (RTD + Disable Animations) ----
                //
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(excelOptionsPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("RTDThrottleInterval", 200, RegistryValueKind.DWord);
                        key.SetValue("EnableAnimations", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(excelOptionsPath))
                        {
                            newKey.SetValue("RTDThrottleInterval", 200, RegistryValueKind.DWord);
                            newKey.SetValue("EnableAnimations", 0, RegistryValueKind.DWord);
                        }
                    }
                }

                //
                // ---- Graphics (DisableAnimations for Excel 2013+) ----
                //
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(graphicsPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("DisableAnimations", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(graphicsPath))
                        {
                            newKey.SetValue("DisableAnimations", 1, RegistryValueKind.DWord);
                        }
                    }
                }

                //
                // ---- VBA Security (AccessVBOM = 1) ----
                //
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(securityPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("AccessVBOM", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(securityPath))
                        {
                            newKey.SetValue("AccessVBOM", 1, RegistryValueKind.DWord);
                        }
                    }
                }


                //
                //---- Add Icon to Winforms Open File Dialog (Optional) ----
                //
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(AppIcon, writable: true))
                {
                    if (key == null || key.GetValue("DisplayIcon") == null)
                    {
                        key.SetValue(
                            "DisplayIcon",
                            $"{AppDomain.CurrentDomain.BaseDirectory}AppIcon.ico,0",
                            RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public static string GetOfficeVersion()
        {
            string defaultComVersion = "16.0"; // Default fallback
            string detectedComVersion = "";
            string[] installedVersions = new string[0];

            try
            {
                // 🔹 Detect the default registered COM version of Excel
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Excel.Application\CurVer"))
                {
                    string curVer = key?.GetValue(null)?.ToString(); // e.g. "Excel.Application.16"
                    if (!string.IsNullOrEmpty(curVer))
                    {
                        detectedComVersion = curVer.Split('.').Last(); // Get "16"
                        defaultComVersion = detectedComVersion + ".0";
                    }
                }

                // 🔹 Detect all installed Excel versions (from registry)
                string[] possibleVersions = { "16.0", "15.0", "14.0", "12.0", "11.0" };
                string[] registryBases = {
                @"SOFTWARE\Microsoft\Office\",
                @"SOFTWARE\WOW6432Node\Microsoft\Office\"
            };

                var foundVersions = possibleVersions
                    .SelectMany(version =>
                        registryBases.Select(basePath =>
                            Registry.LocalMachine.OpenSubKey($"{basePath}{version}\\Excel") != null ? version : null
                        )
                    )
                    .Where(v => v != null)
                    .Distinct()
                    .ToArray();

                installedVersions = foundVersions;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log("Failed to detect Excel version(s): " + ex.Message);
            }

            // 🧾 Log both
            string installedList = installedVersions.Length > 0
                ? string.Join(", ", installedVersions)
                : "None Found";

            ApplicationLogger.Log($"Default COM Excel Version: {defaultComVersion}");
            ApplicationLogger.Log($"Installed Excel Versions Detected: {installedList}");

            // 🔁 Optionally return just the default (for RegAsm decisions)
            return defaultComVersion;
        }

        private List<ExcelFormulaCell> BuildFormulaCells(Dictionary<string, Dictionary<string, object>> dict)
        {
            var formulaCells = new List<ExcelFormulaCell>();

            // Header row
            formulaCells.Add(new ExcelFormulaCell
            {
                Row = 1,
                Column = 1,
                Formula = "Name"
            });

            int startRow = 2;
            int startCol = 1;

            // Collect all unique fields (excluding "V")
            var allFields = dict.Values
                .SelectMany(inner => inner.Keys)
                .Distinct()
                .Where(field => field != "V")
                .ToList();

            // Add field headers
            for (int i = 0; i < allFields.Count; i++)
            {
                if (allFields[i] == "Name")
                    continue;

                formulaCells.Add(new ExcelFormulaCell
                {
                    Row = 1,
                    Column = startCol + i,
                    Formula = allFields[i]
                });
            }

            int currentRow = startRow;

            foreach (var outer in dict)
            {
                string symbol = outer.Key;
                var valueDict = outer.Value;

                // Safely get Name from inner dictionary
                string name = valueDict.ContainsKey("Name") ? valueDict["Name"]?.ToString() ?? symbol : symbol;

                // Add Name (from valueDict) to column A
                formulaCells.Add(new ExcelFormulaCell
                {
                    Row = currentRow,
                    Column = 1,
                    Formula = name
                });

                // Add RTD formulas
                for (int i = 0; i < allFields.Count; i++)
                {
                    string field = allFields[i];
                    int col = startCol + i;

                    if (field == "Name")
                        continue;

                    string formula = $"=RTD(\"thecalcify\", ,\"{symbol}\",\"{field}\")";

                    formulaCells.Add(new ExcelFormulaCell
                    {
                        Row = currentRow,
                        Column = col,
                        Formula = formula
                    });
                }

                currentRow++;
            }

            return formulaCells;
        }

        #endregion Excel Export

        #region News

        public async void NewsListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Clean up existing NewsControl if already present
                var existingNews = this.Controls.Find("newsControlView", true).FirstOrDefault();
                if (existingNews != null)
                {
                    this.Controls.Remove(existingNews);
                    existingNews.Dispose();
                }

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
                    {
                        editableMarketWatchGrid.EndEdit();
                    }

                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
                    editableMarketWatchGrid.Dispose();
                }


                // 2. Create new NewsControl
                var newsControl = new NewsControl(username, password, token, string.Empty)
                {
                    Name = "newsControlView",
                    Dock = DockStyle.Fill
                };

                //DisposeSignalRConnection();
                saveMarketWatchHost.Visible = false;
                fontSizeComboBox.Visible = false;
                //uiManager?.SetFontSizeComboBoxVisibility(false);

                pnlSearch.Visible = false;
                searchTextLabel.Visible = false;
                txtsearch.Visible = false;
                //uiManager?.SetSearchBoxVisibility(false);

                refreshMarketWatchHost.Visible = false;
                newCTRLNToolStripMenuItem1.Enabled = true;
                // Update status label

                // Update title based on edit mode
                titleLabel.Text = "News";

                // 3. Add it to main form
                this.Controls.Add(newsControl);
                newsControl.BringToFront();
                newsControl.Focus();

                licenceDate = LoginInfo.NewsExpiredDate.ToString();

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop();
                }
                else
                {
                    licenceExpire.Text = $"Licence Expire At:- {LoginInfo.NewsExpiredDate:dd:MM:yyyy}";
                }

            }
            catch (Exception ex)
            {
                SplashManager.Hide();
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading News view: {ex.Message}");
            }
            finally
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        notificationSettings.Visible = true;
                    }));
                }
                else
                {
                    notificationSettings.Visible = true;
                }
            }
        }

        public async void NewsHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Clean up existing NewsControl if already present
                var existingNews = this.Controls.Find("newsControlView", true).FirstOrDefault();
                if (existingNews != null)
                {
                    this.Controls.Remove(existingNews);
                    existingNews.Dispose();
                }

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    if (editableMarketWatchGrid.IsCurrentCellInEditMode)
                    {
                        editableMarketWatchGrid.EndEdit();
                    }

                    editableMarketWatchGrid.EditableDispose(); // Dispose the grid
                    editableMarketWatchGrid.Dispose();
                }


                // 2. Create new NewsControl
                var newsControl = new NewsControl(username, password, token, "history")
                {
                    Name = "newsControlView",
                    Dock = DockStyle.Fill
                };

                //DisposeSignalRConnection();
                saveMarketWatchHost.Visible = false;
                fontSizeComboBox.Visible = false;
                //uiManager?.SetFontSizeComboBoxVisibility(false);

                pnlSearch.Visible = false;
                searchTextLabel.Visible = false;
                txtsearch.Visible = false;
                //uiManager?.SetSearchBoxVisibility(false);

                refreshMarketWatchHost.Visible = false;
                // Update status label

                newCTRLNToolStripMenuItem1.Enabled = true;


                // Update title based on edit mode
                titleLabel.Text = "News History";

                // 3. Add it to main form
                this.Controls.Add(newsControl);
                newsControl.BringToFront();
                newsControl.Focus();

                licenceDate = LoginInfo.NewsExpiredDate.ToString();

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop();
                }
                else
                {
                    licenceExpire.Text = $"Licence Expire At:- {licenceDate.Replace("0:00:00", "").Replace("12:00:00 AM", "").Replace("00:00:00", "").Replace("00:00", "").Replace("00:00 AM", "").TrimEnd('0').Trim()}";
                }

            }
            catch (Exception ex)
            {
                SplashManager.Hide();
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading News view: {ex.Message}");
            }
            finally
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        notificationSettings.Visible = true;
                    }));
                }
                else
                {
                    notificationSettings.Visible = true;
                }
            }
        }

        public async void NewsSettingsToolStrip_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Clean up existing NewsControl if already present
                var existingNews = this.Controls.Find("NewsSettingView", true).FirstOrDefault();
                if (existingNews != null)
                {
                    this.Controls.Remove(existingNews);
                    existingNews.Dispose();
                }

                //EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                //if (editableMarketWatchGrid != null)
                //{
                //    if (connection.State != HubConnectionState.Disconnected)
                //    {
                //        await connection.StopAsync(); // ✅ Only stop if not already disconnected
                //    }

                //    await connection.DisposeAsync(); // ✅ Dispose safely
                //    isConnectionDisposed = true;
                //}


                // 2. Create new NewsControl
                var newsControl = new NewsSetting(userId, keywords, topics, isDND, token)
                {
                    Name = "NewsSettingView",
                    Dock = DockStyle.Fill
                };

                //DisposeSignalRConnection();
                saveMarketWatchHost.Visible = false;
                fontSizeComboBox.Visible = false;
                //uiManager?.SetFontSizeComboBoxVisibility(false);

                pnlSearch.Visible = false;
                searchTextLabel.Visible = false;
                txtsearch.Visible = false;
                //uiManager?.SetSearchBoxVisibility(false);

                refreshMarketWatchHost.Visible = false;
                // Update status label

                // Update title based on edit mode
                titleLabel.Text = "News Settings";

                // 3. Add it to main form
                this.Controls.Add(newsControl);
                newsControl.BringToFront();
                newsControl.Focus();

                licenceDate = LoginInfo.NewsExpiredDate.ToString();

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop();
                }
                else
                {
                    licenceExpire.Text = $"Licence Expire At:- {licenceDate.Replace("0:00:00", "").Replace("12:00:00 AM", "").Replace("00:00:00", "").Replace("00:00", "").Replace("00:00 AM", "").TrimEnd('0').Trim()}";
                }

            }
            catch (Exception ex)
            {
                // Catch other unexpected issues
                SplashManager.Hide();
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading News view: {ex.Message}");
            }
            finally
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        notificationSettings.Visible = true;
                    }));
                }
                else
                {
                    notificationSettings.Visible = true;
                }
            }
        }

        #endregion

        #region User Activity Moniter

        public async Task UserInfoSignalREvent(string username)
        {

            bool hasPrevNews = LoginInfo.IsNews;
            bool hasPrevRate = LoginInfo.IsRate;

            var userconnection = new HubConnectionBuilder()
                .WithUrl($"{APIPath}{APIUrl.SignalRConnection}")
                .WithAutomaticReconnect()
                .WithStatefulReconnect()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Critical);
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .Build();

            userconnection.Reconnected += async (connectionId) =>
            {
                await userconnection.InvokeAsync("client", username);
                //await userconnection.InvokeAsync("ClientWithDevice", username, Common.UUIDExtractor());

            };

            userconnection.On<object>("ReceiveMessage", async (base64) =>
            {
                try
                {
                    string base64String = base64?.ToString();
                    if (string.IsNullOrWhiteSpace(base64String))
                    {
                        ApplicationLogger.Log("Received empty message.");
                        return;
                    }

                    var root = JObject.Parse(base64String);
                    bool status = root.Value<bool>("status");

                    if (!status)
                    {
                        await HandleLogoutAsync("Don't Have Active Status");
                        return;
                    }
                    UserDto userDto = new UserDto();

                    try
                    {
                        string resultJson = Common.JsonExtractor(base64String);
                        userDto = System.Text.Json.JsonSerializer.Deserialize<UserDto>(resultJson);

                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.LogException(ex);
                    }



                    if (userDto == null)
                    {
                        ApplicationLogger.Log("Received null UserDto.");
                        return;
                    }

                    if (!userDto.isActive || (!userDto.hasNewsAccess && !userDto.hasRateAccess))
                    {
                        await HandleLogoutAsync("Logged out due to Session Limit.");
                        return;
                    }

                    await UpdateUIBasedOnUserDto(userDto);

                    if (!string.IsNullOrEmpty(userDto.keywords) || !string.IsNullOrEmpty(userDto.topics))
                    {
                        topics = userDto.topics == "string" ? string.Empty : userDto.topics;
                        keywords = userDto.keywords == "string" ? string.Empty : userDto.keywords;
                        isDND = userDto.isDND;
                    }
                    userId = userDto.id;
                    if (notificationSettings.Enabled == false)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                notificationSettings.Enabled = true;
                            }));
                        }
                        else
                        {
                            notificationSettings.Enabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log("Error processing SignalR message: " + ex.Message);
                    ApplicationLogger.LogException(ex);
                }
            });

            userconnection.On<object>("ReceiveNewsNotification", async (data) =>
            {

                try
                {

                    string json = data?.ToString();

                    if (!string.IsNullOrWhiteSpace(json) && !isDND)
                    {
                        var news = System.Text.Json.JsonSerializer.Deserialize<NewsNotificationDTO>(json);

                        Common.ShowWindowsToast(news.headLine, Common.TimeStampConvert(news.sortTimestamp));
                    }
                    else
                    {
                        ApplicationLogger.Log("Warning: Received empty or null data.");
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log("Exception: " + ex.Message);
                }
            });

            userconnection.On<string>("rateAlertNotification", (compressedBase64) =>
            {

                try
                {
                    // Base64 → byte[]
                    byte[] compressedBytes = Convert.FromBase64String(compressedBase64);

                    // GZip Decompress
                    string decompressedJson = DecompressGzip(compressedBytes);

                    //// Parse JSON
                    var alertObj = JsonConvert.DeserializeObject<RateAlertNotificationDto>(decompressedJson);

                    if (alertObj != null && alertObj.Data != null)
                    {
                        if (alertObj.Data.Flag.ToLower().Contains("pop"))
                        {

                            Common.ShowRateAlertWindowsToast(
                                $"Rate Alert Triggered for {alertObj.Data.Symbol}",
                                $"Your rate alert for {alertObj.Data.Symbol} On {AlertCreationPanel.ConvertTypeCodeToLabel(alertObj.Data.Type)} has been triggered at {alertObj.Data.Rate}"
                            );
                        }

                        if (alertObj.Data.Flag.ToLower().Contains("status"))
                        {
                            if (this.InvokeRequired)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    savelabel.Visible = true;
                                    savelabel.Text = $"Alert for {alertObj.Data.Symbol} On {AlertCreationPanel.ConvertTypeCodeToLabel(alertObj.Data.Type)} has been triggered at {alertObj.Data.Rate} At {DateTime.Now:G}";
                                }));
                            }
                            else
                            {
                                savelabel.Visible = true;
                                savelabel.Text = $"Alert for {alertObj.Data.Symbol} On {AlertCreationPanel.ConvertTypeCodeToLabel(alertObj.Data.Type)} has been triggered at {alertObj.Data.Rate} At {DateTime.Now:G}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log("❌ Error processing rate alert: " + ex);
                }
            });

            userconnection.On<bool>("SheetUpdated", async (status) =>
            {

                List<SheetWrapperDto> sheets = await UserExcelExportForm.GetSheetListAsync(token);

                foreach (SheetWrapperDto sheetWrapper in sheets)
                {
                    if (sheetWrapper != null && sheetWrapper.Type == "json")
                    {

                        Dictionary<string, decimal> editedCells = sheetWrapper.Data.EditedCells;
                        var excelApp = ExcelAppManager.GetExcelApp();
                        try
                        {

                            excelApp.app.ScreenUpdating = false;
                            excelApp.app.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;

                            Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.workbook;
                            Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.Sheets[sheetWrapper.SheetName];
                            foreach (var cellEntry in editedCells)
                            {
                                string cellAddress = cellEntry.Key;
                                decimal cellValue = cellEntry.Value;
                                Microsoft.Office.Interop.Excel.Range cell = worksheet.get_Range(cellAddress);
                                //cell.Value = cellValue;
                                //cell.Value2 = cellValue;

                                var rng = worksheet.Range[cellAddress];
                                rng.Value2 = (double)cellValue;
                            }
                            workbook.Save();
                        }
                        finally
                        {
                            excelApp.app.ScreenUpdating = true;
                            excelApp.app.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationAutomatic;
                        }
                    }
                }
            });

            userconnection.On<bool>("MarketWatchUpdated", async (reason) =>
            {

                ApplicationLogger.Log("Market Watch Update Signal Received.");
                await MenuLoadAsync();
            });

            userconnection.On<object>("UserListOfSymbol", data =>
            {

                var parsed = JsonConvert.DeserializeObject<List<SymbolItem>>(data.ToString());
             
                SendRestartSignal();

                // 1️⃣ store tuple list
                SymbolName = parsed
                    .Select(x => (x.i, x.n))
                    .ToList();

                // 2️⃣ populate symbol master
                symbolMaster = SymbolName
                    .Select(x => x.Symbol)
                    .Distinct()
                    .ToList();

                UpdateRtwConfig();

                identifiers = symbolMaster.ToList();

                // 3️⃣ write RTW config
                UpdateRtwConfig();

                // 4️⃣ rebuild grid + row map on UI thread
                SafeInvoke(() =>
                {
                    InitializeDataGridView();

                    symbolRowMap.Clear();
                    for (int i = 0; i < identifiers.Count; i++)
                        symbolRowMap[identifiers[i]] = i;


                    _symbolNameMap.Clear();

                    foreach (var item in SymbolName)
                    {
                        // Symbol → Name
                        if (!_symbolNameMap.ContainsKey(item.Symbol))
                            _symbolNameMap[item.Symbol] = item.SymbolName;
                    }
                });
            });






            await userconnection.StartAsync();
            await userconnection.InvokeAsync("client", username);
            //await userconnection.InvokeAsync("ClientWithDevice", username, Common.UUIDExtractor());

            // Helper to marshal UI updates safely
            async Task InvokeOnUIThread(Func<Task> action)
            {
                // If form is disposed or disposing → just ignore the call
                if (this.IsDisposed || this.Disposing)
                    return;

                if (this.InvokeRequired)
                {
                    try
                    {
                        await (Task)this.Invoke(new Func<Task>(action));
                    }
                    catch (ObjectDisposedException)
                    {
                        // Form was closed between the check and invoke
                    }
                }
                else
                {
                    if (!this.IsDisposed)
                        await action();
                }
            }


            async Task UpdateUIBasedOnUserDto(UserDto userDto)
            {
                // Combination of flags for clearer logic
                bool hasNews = userDto.hasNewsAccess;
                bool hasRate = userDto.hasRateAccess;
                bool active = userDto.isActive;

                if (!active)
                {
                    await HandleLogoutAsync("Account Deactivated By Admin.");
                    return;
                }

                if (hasNews && !hasRate && hasPrevRate)
                {
                    await InvokeOnUIThread(() =>
                    {
                        newCTRLNToolStripMenuItem.Visible = false;
                        newsToolStripMenuItem.Visible = true;
                        NewsListToolStripMenuItem_Click(this, EventArgs.Empty);
                        hasPrevNews = hasNews;
                        hasPrevRate = hasRate;
                        return Task.CompletedTask;
                    });
                }
                else if (!hasNews && hasRate && hasPrevNews)
                {
                    await InvokeOnUIThread(async () =>
                    {
                        newCTRLNToolStripMenuItem.Visible = true;
                        newsToolStripMenuItem.Visible = false;
                        hasPrevNews = hasNews;
                        hasPrevRate = hasRate;
                        await DefaultToolStripMenuItem_Click(this, EventArgs.Empty);
                    });
                }
                else if (hasNews && hasRate && (!hasPrevNews || !hasPrevRate))
                {
                    await InvokeOnUIThread(async () =>
                    {
                        newCTRLNToolStripMenuItem.Visible = true;
                        newsToolStripMenuItem.Visible = true;
                        hasPrevNews = hasNews;
                        hasPrevRate = hasRate;
                        await DefaultToolStripMenuItem_Click(this, EventArgs.Empty);
                    });
                }
            }

            async Task HandleLogoutAsync(string Reason)
            {
                ApplicationLogger.Log("User Logged Out From SignalR...");

                try
                {
                    await InvokeOnUIThread(async () =>
                    {
                        await StopBackgroundTasks();

                        UnsubscribeAllEvents();

                        var loginForm = new Login();
                        loginForm.Show();


                        this.Hide();
                        this.Dispose();
                        this.Close();

                        await userconnection.StopAsync();
                        await userconnection.DisposeAsync();
                        userconnection = null;

                        await LogoutAsync();

                        MessageBox.Show("You have been logged out. " + Reason, "Logged Out", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        KillProcess();
                    });
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                    MessageBox.Show("[HandleLogoutAsync] Error during disconnect: " + ex.Message);
                }
                finally
                {
                    await StopBackgroundTasks();
                }
            }
        }

        public async Task LogoutAsync()
        {
            var payload = new
            {
                userId,
                deviceId = Common.UUIDExtractor()
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var response = await client.PostAsync($"{APIPath}logout", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ApplicationLogger.Log("✅ Logout successful.");
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden ||
                             response.StatusCode == HttpStatusCode.Unauthorized ||
                             response.StatusCode == HttpStatusCode.NotFound)
                    {
                        thecalcify thecalcify = CurrentInstance;
                        thecalcify.DisconnectESCToolStripMenuItem_Click(null, null);
                        MessageBox.Show("Session expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        ApplicationLogger.Log($"❌ Logout failed. Status: {response.StatusCode}, Response: {responseText}");
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"❗ Error during logout: {ex.Message}");
                }
            }
        }

        private static string DecompressGzip(byte[] compressed)
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

        #region RTW

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (defaultGrid.IsDisposed)
                return;

            int count = 0;

            foreach (var key in _latestTicks.Keys)
            {
                if (_latestTicks.TryRemove(key, out var dto))
                {
                    foreach (var grid in LiveGridRegistry.All)
                    {
                        if (grid.IsReady)
                            grid.TryApplyDto(dto);
                    }


                    //ExcelNotifier.NotifyExcel(dto.i,dto)

                    if (++count >= 200)  // limit
                        break;
                }
            }
        }

        private async Task ConsumeTicks(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !defaultGrid.IsDisposed)
            {
                try
                {
                    if (_queue.Read(0, out byte[] buffer))
                    {
                        TickBinary tick = TickBinary.FromBytes(buffer);

                        // Convert to your UI DTO
                        var dto = TickUIConverter.ToUiDto(tick);

                        // Queue it for UI thread
                        _latestTicks[dto.i] = dto;
                    }
                    else
                    {
                        await Task.Delay(1);
                    }
                }
                catch { }
            }
        }

        private void ApplyDtoToGridFast(MarketDataDto dto)
        {
            if (dto == null) return;

            // ======================================
            // 0️⃣ PREVENT CRASH DURING GRID REBUILD
            // ======================================
            if (_isGridBuilding) return;                     // grid is loading / clearing
            if (defaultGrid.IsDisposed) return;
            if (!defaultGrid.IsHandleCreated) return;
            if (defaultGrid.Rows.Count == 0) return;


            // ======================================
            // 1 EXCEL NOTIFIER (is fast; keep as is)
            // ======================================
            dto.n = SymbolName
                        .Where(x => x.Symbol == dto.i)
                        .Select(x => x.SymbolName)
                        .FirstOrDefault();
            LastTickStore.ExcelPublish(dto);

            // ======================================
            // 2 SAFE ROW INDEX FETCH
            // ======================================
            if (!symbolRowMap.TryGetValue(dto.i, out int rowIndex))
                return; // symbol not yet added

            // --- Critical: prevent out-of-range ---
            if (rowIndex < 0 || rowIndex >= defaultGrid.Rows.Count)
                return;

            DataGridViewRow row;
            try
            {
                row = defaultGrid.Rows[rowIndex];
            }
            catch
            {
                return; // Grid is refreshing → ROW INVALID
            }

            // ======================================
            // 3️⃣ READ OLD VALUES (safe)
            // ======================================
            double oldBid = FastParse(row.Cells["Bid"].Value);
            double oldAsk = FastParse(row.Cells["Ask"].Value);
            double oldLTP = FastParse(row.Cells["LTP"].Value);
            double oldChange = FastParse(row.Cells["Net Chng"].Value);
            double oldHigh = FastParse(row.Cells["High"].Value);
            double oldLow = FastParse(row.Cells["Low"].Value);
            double oldOpen = FastParse(row.Cells["Open"].Value);
            double oldClose = FastParse(row.Cells["Close"].Value);
            double oldATP = FastParse(row.Cells["ATP"].Value);
            double oldAskSize = FastParse(row.Cells["Ask Size"].Value);
            double oldTotalAskSize = FastParse(row.Cells["Total Ask Size"].Value);
            double oldBidSize = FastParse(row.Cells["Bid Size"].Value);
            double oldTotalBidSize = FastParse(row.Cells["Total Bid Size"].Value);
            double oldVolume = FastParse(row.Cells["Volume"].Value);
            double oldOpenInterest = FastParse(row.Cells["Open Interest"].Value);
            double oldLastSize = FastParse(row.Cells["Last Size"].Value);

            // ======================================
            // 4️⃣ CELL UPDATES (safe)
            // ======================================
            UpdateIfChanged(row, "Bid", dto.b);
            UpdateIfChanged(row, "Ask", dto.a);
            UpdateIfChanged(row, "LTP", dto.ltp);

            UpdateIfChanged(row, "High", dto.h);
            UpdateIfChanged(row, "Low", dto.l);
            UpdateIfChanged(row, "Open", dto.o);
            UpdateIfChanged(row, "Close", dto.c);

            UpdateIfChanged(row, "Net Chng", dto.d);
            UpdateIfChanged(row, "ATP", dto.atp);

            UpdateIfChanged(row, "Bid Size", dto.bq);
            UpdateIfChanged(row, "Total Bid Size", dto.tbq);
            UpdateIfChanged(row, "Ask Size", dto.sq);
            UpdateIfChanged(row, "Total Ask Size", dto.tsq);

            UpdateIfChanged(row, "Volume", dto.vt);
            UpdateIfChanged(row, "Open Interest", dto.oi);
            UpdateIfChanged(row, "Last Size", dto.ltq);

            UpdateIfChanged(row, "Time", Common.TimeStampConvert(dto.t));

            // ======================================
            // 5️⃣ PRICE COLOR UPDATES (safe)
            // ======================================
            UpdateColorFast(row, "Bid", oldBid, FastParse(dto.b));
            UpdateColorFast(row, "Ask", oldAsk, FastParse(dto.a));
            UpdateColorFast(row, "LTP", oldLTP, FastParse(dto.ltp));
            UpdateColorFast(row, "Net Chng", oldChange, FastParse(dto.d));
            UpdateColorFast(row, "High", oldHigh, FastParse(dto.h));
            UpdateColorFast(row, "Low", oldLow, FastParse(dto.l));
            UpdateColorFast(row, "Open", oldOpen, FastParse(dto.o));
            UpdateColorFast(row, "Close", oldClose, FastParse(dto.c));
            UpdateColorFast(row, "ATP", oldATP, FastParse(dto.atp));
            UpdateColorFast(row, "Ask Size", oldAskSize, FastParse(dto.sq));
            UpdateColorFast(row, "Total Ask Size", oldTotalAskSize, FastParse(dto.tsq));
            UpdateColorFast(row, "Bid Size", oldBidSize, FastParse(dto.bq));
            UpdateColorFast(row, "Total Bid Size", oldTotalBidSize, FastParse(dto.tbq));
            UpdateColorFast(row, "Volume", oldVolume, FastParse(dto.vt));
            UpdateColorFast(row, "Open Interest", oldOpenInterest, FastParse(dto.oi));
            UpdateColorFast(row, "Last Size", oldLastSize, FastParse(dto.ltq));

            // ======================================
            // 6️⃣ ARROW UPDATE (safe)
            // ======================================
            if (double.TryParse(dto.a, out var ask))
            {
                UpdateAskArrow(row, dto.i, ask);

                DateTime tickTime;

                if (long.TryParse(dto.t, out long ts))
                {
                    if (ts > 1000000000000)
                        tickTime = DateTimeOffset.FromUnixTimeMilliseconds(ts).ToOffset(TimeSpan.FromHours(5.5)).DateTime;
                    else
                        tickTime = DateTimeOffset.FromUnixTimeSeconds(ts).ToOffset(TimeSpan.FromHours(5.5)).DateTime;
                }
                else
                {
                    tickTime = DateTime.Now; // IST already
                }


                var tick = new Tick
                {
                    Symbol = dto.i,
                    Time = tickTime,
                    Price = ask,
                    Volume = Convert.ToDouble(dto.vt)
                };

                GlobalTickDispatcher.Publish(tick);
            }
        }

        private double FastParse(object val)
        {
            if (val == null) return 0;
            double.TryParse(val.ToString(), out double r);
            return r;
        }

        private void UpdateIfChanged(DataGridViewRow row, string col, string newVal)
        {
            var cell = row.Cells[col];
            if (cell.Value?.ToString() != newVal)
                cell.Value = newVal;  // only update if changed
        }

        private void UpdateColorFast(DataGridViewRow row, string col, double oldVal, double newVal)
        {
            if (oldVal == newVal) return;

            var cell = row.Cells[col];

            if (newVal > oldVal)
                cell.Style.ForeColor = Color.Green;
            else
                cell.Style.ForeColor = Color.Red;
        }

        private void UpdateAskArrow(DataGridViewRow row, string symbol, double newAsk)
        {
            double prevAsk = 0;
            bool havePrev = _prevAskMap.TryGetValue(symbol, out prevAsk);

            if (havePrev)
            {
                if (newAsk > prevAsk)
                {
                    ApplyArrow(row.Cells["Name"], true);
                    //row.Cells["i"].Style.ForeColor = Color.Green;
                }
                else if (newAsk < prevAsk)
                {
                    ApplyArrow(row.Cells["Name"], false);
                    //row.Cells["i"].Style.ForeColor = Color.Red;
                }
            }

            _prevAskMap[symbol] = newAsk;
        }

        private void ApplyArrow(DataGridViewCell nameCell, bool isUp)
        {
            string clean = (nameCell.Value?.ToString() ?? "")
                .Replace(" ▲", "")
                .Replace(" ▼", "")
                .Trim();

            if (isUp)
            {
                nameCell.Value = clean + " ▲";
                nameCell.Style.ForeColor = Color.Green;
            }
            else
            {
                nameCell.Value = clean + " ▼";
                nameCell.Style.ForeColor = Color.Red;
            }
        }

        private void UpdateRtwConfig()
        {
            try
            {
                ApplicationLogger.Log("[RTW] Updating config file by thecalcify at " + RtwConfigPath);

                Directory.CreateDirectory(Path.GetDirectoryName(RtwConfigPath));
                File.WriteAllText(RtwConfigPath, JsonConvert.SerializeObject(symbolMaster));

                ApplicationLogger.Log("[RTW] Config updated successfully by thecalcify for " + symbolMaster.Count + " symbols.");

                // Restart service so RTW reloads symbols
                //RestartRTWService();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void RestartRTWService()
        {
            try
            {
                string serviceName = "thecalcifyRTW";
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        ApplicationLogger.Log("[RTW] Service stopped successfully by thecalcify.");
                    }
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    ApplicationLogger.Log("[RTW] Service started successfully by thecalcify.");
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }


        private void startRTWService()
        {
            try
            {
                string serviceName = "thecalcifyRTW";

                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.Paused)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));

                        ApplicationLogger.Log("[RTW] Service started successfully by thecalcify.");
                    }

                }

                string updateServiceName = "thecalcifyUpdate";

                using (ServiceController sc = new ServiceController(updateServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.Paused)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));

                        ApplicationLogger.Log("[Update thecalcifyUpdate] Service started successfully by thecalcify.");
                    }

                }


            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        #endregion

        #region Copy Actions
        private void DefaultGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedRowsToClipboard();
                e.Handled = true;
            }
        }

        private void CopySelectedRowsToClipboard()
        {
            if (defaultGrid.SelectedRows.Count == 0)
                return;

            var excelRTD = new StringBuilder();

            var rows = defaultGrid.SelectedRows
                                  .Cast<DataGridViewRow>()
                                  .OrderBy(r => r.Index);

            foreach (var row in rows)
            {
                List<string> cells = new List<string>();

                // Symbol from hidden column
                string symbol = row.Cells[0].Value?.ToString()?.Trim() ?? "";

                // Name (first visible)
                string nameValue = "";
                foreach (DataGridViewCell c in row.Cells)
                {
                    if (c.Visible)
                    {
                        nameValue = c.Value?.ToString()
                            .Replace(" ▲", "")
                            .Replace(" ▼", "")
                            .Trim() ?? "";
                        break;
                    }
                }

                cells.Add(nameValue);

                // RTD formulas
                foreach (DataGridViewColumn col in defaultGrid.Columns)
                {
                    if (!col.Visible) continue;
                    if (col.Index == 0) continue;
                    if (col.HeaderText == "Name") continue;

                    string header = col.HeaderText.Trim();
                    string formula = $"=RTD(\"thecalcify\",,\"{symbol}\",\"{header}\")";
                    cells.Add(formula);
                }

                excelRTD.AppendLine(string.Join("\t", cells));
            }

            DataObject obj = new DataObject();

            // ✔ Excel sees RTD formulas (UnicodeText)
            obj.SetData(DataFormats.UnicodeText, excelRTD.ToString());

            // ✔ All other apps see blank → paste = nothing
            obj.SetData(DataFormats.Text, "");
            obj.SetData(DataFormats.StringFormat, "");

            Clipboard.Clear();
            Clipboard.SetDataObject(obj, true);
        }

        private void CopyRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (defaultGrid.SelectedRows.Count == 0)
            { return; }

            CopySelectedRowsToClipboard();
        }

        #endregion

        #region Chart
        private void ChartWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_rightClickedRowIndex < 0)
                return;

            string symbol = defaultGrid.Rows[_rightClickedRowIndex].Cells["Symbol"].Value.ToString();
            string displaySymbol = defaultGrid.Rows[_rightClickedRowIndex].Cells["Name"].Value.ToString().Replace(" ▲", "").Replace(" ▼", "").Trim();


            var chartForm = new Chart(symbol, displaySymbol);

            if (isFullScreen)
            {
                chartForm.StartPosition = FormStartPosition.CenterParent;
                chartForm.TopMost = true;
            }

            chartForm.Show();
        }

        #endregion

        #region ILiveMarketGrid Implementation

        public bool IsReady =>
            !_isGridBuilding &&
            !defaultGrid.IsDisposed &&
            defaultGrid.IsHandleCreated &&
            defaultGrid.Rows.Count > 0;

        public bool TryApplyDto(MarketDataDto dto)
        {
            ApplyDtoToGridFast(dto);
            return true;
        }

        private void ReplayLastTicksToDefaultGrid()
        {
            var all = LastTickStore.GetAll();

            foreach (var kv in all)
            {
                ApplySnapshotToGrid(kv.Key, kv.Value);
            }
        }


        private void ApplySnapshotToGrid(string symbol, Dictionary<string, object> snapshot)
        {
            if (defaultGrid == null || defaultGrid.IsDisposed)
                return;

            if (!symbolRowMap.TryGetValue(symbol, out int rowIndex))
                return;

            if (rowIndex < 0 || rowIndex >= defaultGrid.Rows.Count)
                return;

            var row = defaultGrid.Rows[rowIndex];

            foreach (var kv in snapshot)
            {
                if (!defaultGrid.Columns.Contains(kv.Key))
                    continue;

                row.Cells[kv.Key].Value = kv.Value;
            }
        }


        //public static void SendRestartSignal()
        //{
        //    using (var evt = EventWaitHandle.OpenExisting(IpcConstants.RestartSignalName))
        //    {
        //        evt.Set(); // 🔥 sends the signal
        //    }
        //}

        public static void SendRestartSignal()
        {
            try
            {
                using (var client = new NamedPipeClientStream(
                    ".",
                    "TheCalcifyPipe",
                    PipeDirection.Out))
                {
                    // Wait max 1 second for service
                    client.Connect(1000);

                    using (var writer = new StreamWriter(client, Encoding.UTF8))
                    {
                        writer.AutoFlush = true;
                        writer.WriteLine("RESTART");
                    }
                }
            }
            catch (System.TimeoutException)
            {
                // Service not running — ignore
            }
            catch (IOException)
            {
                // Pipe not available — ignore
            }
            catch (Exception)
            {
                // Optional logging
            }
        }


        #endregion

    }
}