using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Alert;
using thecalcify.Helper;
using thecalcify.MarketWatch;
using thecalcify.News;
using Button = System.Windows.Forms.Button;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace thecalcify
{
    public partial class thecalcify : Form
    {
        #region Declaration and Initialization

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        // ======================
        // 📌 Config / Constants
        // ======================
        public readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "thecalcify");

        private readonly TimeSpan _reconnectThrottle = TimeSpan.FromSeconds(10); // prevent spam

        // ======================
        // 📌 User / Credentials
        // ======================
        public string token, licenceDate, username, password;

        // ======================
        // 📌 Flags / States
        // ======================
        private bool isConnectionDisposed = false;

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

        private DateTime _lastReconnectAttempt = DateTime.MinValue;
        private DateTime lastUiUpdate = DateTime.MinValue;
        private Rectangle prevBounds;
        private FormWindowState prevState;
        private FormBorderStyle prevStyle;
        public string saveFileName;
        public string lastOpenMarketWatch = string.Empty;

        // ======================
        // 📌 Core Data Collections
        // ======================
        public List<string> identifiers;

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

        public List<string> FileLists = new List<string>();
        public List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();

        // ======================
        // 📌 Dictionaries / Maps
        // ======================
        private readonly Dictionary<string, int> symbolRowMap = new Dictionary<string, int>();

        private Dictionary<string, double> previousAskMap = new Dictionary<string, double>();
        private readonly Dictionary<string, decimal> previousAsks = new Dictionary<string, decimal>();

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
        public HubConnection connection { get; set; }
        private bool isReconnectTimerRunning = false;
        private bool _isReconnectInProgress = false;
        private bool _eventHandlersAttached = false;

        public Common commonClass;
        private ConcurrentQueue<MarketDataDto> _updateQueue = new ConcurrentQueue<MarketDataDto>();
        private readonly object _tableLock = new object();
        private readonly object _reconnectLock = new object();

        // ======================
        // 📌 Timers / Threads
        // ======================
        private System.Threading.Timer _updateTimer;

        private System.Windows.Forms.Timer signalRTimer;
        private Thread licenceThread;

        // ======================
        // 📌 Excel Interop
        // ======================

        private readonly string excelFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "thecalcify.xlsx");
        private static readonly string marketInitDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "initdata.dat");

        // ======================
        // 📌 UI Elements
        // ======================
        private CheckedListBox checkedListColumns;

        private Button btnSelectAllColumns;
        private Button btnConfirmAddColumns;
        private Button btnCancelAddColumns;

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


        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Formatting = Formatting.None
        };

        #endregion Declaration and Initialization

        #region Form Method
        public thecalcify()
        {
            InitializeComponent();
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

                RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
                if (RemainingDays <= 7)
                {
                    await CheckLicenceLoop(RemainingDays);
                }
                else
                {
                    licenceExpire.Text += licenceDate;
                }

                // --- UI SETUP (non-data related) ---
                this.AutoScaleMode = AutoScaleMode.Dpi;

                this.KeyPreview = true;
                this.DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);

                // --- PARALLEL INITIALIZATION ---
                var initializationTasks = new List<Task>();


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

                // Warm up Excel COM server (faster first export)
                var app = new Microsoft.Office.Interop.Excel.Application();
                app.Quit();

                // --- MENU SETUP ---
                if (LoginInfo.IsRate && LoginInfo.IsNews)
                {
                    MenuLoad();
                }
                else if (LoginInfo.IsRate)
                {
                    MenuLoad();
                    newsToolStripMenuItem.Visible = false;
                }
                else if (LoginInfo.IsNews)
                {
                    this.newsToolStripMenuItem_Click(this, EventArgs.Empty);
                    newCTRLNToolStripMenuItem.Visible = false;
                    toolsToolStripMenuItem.Enabled = true;
                }

                // --- LOAD INITIAL DATA ASYNCHRONOUSLY ---
                await LoadInitialMarketDataAsync();
                HandleLastOpenedMarketWatch();

                // --- FORM PROPERTIES ---
                this.WindowState = FormWindowState.Maximized;
                defaultGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

                CurrentInstance = this;

                // Initialize Grid on UI thread
                SafeInvoke(InitializeDataGridView);

                // Start SignalR
                SignalRTimer();
                await SignalREvent();

                // --- GLOBAL EVENTS ---
                NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
                SystemEvents.PowerModeChanged += OnPowerModeChanged;
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                KillProcess();

            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private Task CheckLicenceLoop(int RemainingDays)
        {
            RemainingDays = (Common.ParseToDate(licenceDate) - DateTime.Now.Date).Days;
            if (RemainingDays <= 7)
            {
                // Start a timer instead of thread
                var licenceTimer = new System.Windows.Forms.Timer
                {
                    Interval = 500, // half a second
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
                licenceExpire.Text += licenceDate;
            }

            return Task.CompletedTask;
        }
        private void Home_FormClosed(object sender, FormClosedEventArgs e)
        {
            isRunning = false;

            try
            {
                KillProcess();
                // Correct way to call the static method
                CredentialManager.SaveMarketWatchWithColumns(lastOpenMarketWatch, columnPreferences.Count == 0 ? columnPreferencesDefault : columnPreferences);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

            System.Windows.Forms.Application.Exit();
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
                        MessageBox.Show("Error during disconnect: " + ex.Message);
                    }
                    finally
                    {
                        await StopBackgroundTasks();
                    }
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

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var aboutForm = new About(username, password, licenceDate,token))
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
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
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

        public async Task DefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                fontSizeComboBox.Visible = true;
                savelabel.Visible = false;

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null && editableMarketWatchGrid.IsCurrentCellInEditMode)
                {
                    editableMarketWatchGrid.EndEdit();
                }
                editableMarketWatchGrid?.Dispose();
                toolsToolStripMenuItem.Enabled = true;
                isLoadedSymbol = false;
                thecalcifyGrid();
                txtsearch.Text = string.Empty;
                saveFileName = null;
                await LoadInitialMarketDataAsync();

                MenuLoad();
                titleLabel.Text = "DEFAULT";
                saveMarketWatchHost.Visible = false;
                refreshMarketWatchHost.Visible = true;
                isEdit = false;
                identifiers = symbolMaster;
                InitializeDataGridView();          // Configure the grid
                await SignalREvent();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
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
                if (isEdit && editableGrid.selectedSymbols != null && string.IsNullOrEmpty(saveFileName))
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

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
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
                                    DeleteExcelSheet(filePath);
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
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
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

        private void FullScreenF11ToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void AddEditSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
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
                        SafeInvoke(InitializeDataGridView);
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
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
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
                connection = null;
                _eventHandlersAttached = false;

                SignalRTimer();
                await SignalREvent();
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
                connection = null;
                signalRTimer.Stop();

                _eventHandlersAttached = false;

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
            _ = AttemptReconnectAsync("Network address changed.");
        }

        private async void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                await SignalREvent();
        }

        private async void RefreshMarketWatchHost_Click(object sender, EventArgs e)
        {
            try
            {
                // 🚫 Disable refresh button while refreshing
                refreshMarketWatchHost.Enabled = false;

                // 🔄 Reset state
                selectedSymbols.Clear();
                identifiers.Clear();
                _updateQueue = new ConcurrentQueue<MarketDataDto>();
                isEdit = false;
                isGrid = true;
                reloadGrid = true;
                saveFileName = null;
                lastOpenMarketWatch = "Default";

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
                editableMarketWatchGrid?.Dispose();
                saveMarketWatchHost.Visible = false;
                refreshMarketWatchHost.Visible = true;

                // 🔄 Force Default view
                await DefaultToolStripMenuItem_Click(sender, e);
                titleLabel.Text = "DEFAULT";

                // 🔄 Reload market data & reconnect SignalR
                InitializeDataGridView();
                await LoadInitialMarketDataAsync();
                await SignalREvent();

                ApplicationLogger.Log("Refresh: Switched back to DEFAULT Market Watch.");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                //MessageBox.Show("Failed to refresh and reset to Default view. Please try again.",
                //                "Refresh Error",
                //                MessageBoxButtons.OK,
                //                MessageBoxIcon.Error);
            }
            finally
            {
                // ✅ Re-enable refresh button after process
                refreshMarketWatchHost.Enabled = true;
            }
        }



        #endregion Form Method

        #region SignalR Methods
        public void SignalRTimer()
        {
            if (signalRTimer != null) return; // prevent multiple timers

            signalRTimer = new System.Windows.Forms.Timer { Interval = 10_000 };
            signalRTimer.Tick += async (s, e) =>
            {
                try
                {
                    if (!isReconnectTimerRunning && connection?.State == HubConnectionState.Disconnected)
                    {
                        isReconnectTimerRunning = true;
                        await TryReconnectAsync().ConfigureAwait(false);
                        isReconnectTimerRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                    isReconnectTimerRunning = false;
                }
            };
            signalRTimer.Start();
        }

        private async Task TryReconnectAsync()
        {
            if (_isReconnectInProgress) return; // prevent parallel reconnects
            if (connection?.State != HubConnectionState.Disconnected) return;

            try
            {
                _isReconnectInProgress = true;
                await SignalREvent().ConfigureAwait(false);
            }
            catch (Exception ex) when (
                ex is OperationCanceledException ||
                ex is ObjectDisposedException ||
                ex is TargetInvocationException ||
                ex is InvalidOperationException)
            {
                ApplicationLogger.LogException(ex);

                // 🔄 Instead of immediate retry → small backoff
                await Task.Delay(2000);
                await SignalREvent().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Unexpected errors
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                _isReconnectInProgress = false;
            }
        }

        private HubConnection BuildConnection()
        {
            return new HubConnectionBuilder()
                .WithUrl($"http://api.thecalcify.com/excel?user={username}&auth=Starline@1008&type=desktop", options =>
                {
                    options.Headers.Add("Origin", "http://api.thecalcify.com/");
                    options.Transports = HttpTransportType.LongPolling | HttpTransportType.ServerSentEvents | HttpTransportType.WebSockets | HttpTransportType.None; // try fallback
                })
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task SignalREvent()
        {
            try
            {
                // 🔄 Reuse connection if already exists
                if (connection == null)
                {
                    connection = BuildConnection();

                    if (!_eventHandlersAttached)
                    {
                        // Attach event handlers only once
                        connection.On<string>("excelRate", OnExcelRateReceived);

                        connection.Closed += async (error) =>
                        {
                            ApplicationLogger.Log("Connection closed");

                            await Task.Delay(2000); // backoff instead of random
                            await TryReconnectAsync(); // safe reconnect

                        };

                        connection.Reconnected += async (connectionId) =>
                        {
                            ApplicationLogger.Log("Reconnected to SignalR hub");
                            try
                            {
                                if (selectedSymbols.Count > 0)
                                    identifiers = new List<string>(selectedSymbols);

                                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                                {
                                    await connection.InvokeAsync("SubscribeSymbols", symbolMaster, cts.Token);
                                }
                                ApplicationLogger.Log("Resubscribed after reconnect.");
                            }
                            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
                            {
                                ApplicationLogger.Log("SignalR Reconnecting/subscribe timeout/canceled.");

                                if (savelabel.InvokeRequired)
                                {
                                    savelabel.Invoke(new Action(() =>
                                    {
                                        savelabel.Visible = true;
                                        savelabel.Text = "Client is offline, switch network.";
                                    }));
                                }
                                else
                                {
                                    savelabel.Visible = true;
                                    savelabel.Text = "Client is offline, switch network.";
                                }
                                connection = null;
                                _eventHandlersAttached = false;
                            }
                            catch (Exception ex)
                            {
                                ApplicationLogger.LogException(ex);
                            }

                        };

                        _eventHandlersAttached = true;
                    }
                }

                if (connection.State == HubConnectionState.Disconnected)
                    await connection.StartAsync().ConfigureAwait(false);

                if (connection.State == HubConnectionState.Connected)
                {
                    // identifiers sync only once per connection start
                    if (selectedSymbols.Count > 0)
                        identifiers = new List<string>(selectedSymbols);

                    try
                    {
                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                        {
                            await connection.InvokeAsync("SubscribeSymbols", symbolMaster, cts.Token).ConfigureAwait(false);

                            if (savelabel.InvokeRequired)
                            {
                                savelabel.Invoke(new Action(() =>
                                {
                                    savelabel.Visible = false;
                                    savelabel.Text = string.Empty;
                                }));
                            }
                            else
                            {
                                savelabel.Visible = false;
                                savelabel.Text = string.Empty;
                            }
                        }

                        SetupUpdateTimer();
                    }
                    catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
                    {
                        ApplicationLogger.Log("SignalR subscribe timeout/canceled.");

                        if (savelabel.InvokeRequired)
                        {
                            savelabel.Invoke(new Action(() =>
                            {
                                savelabel.Visible = true;
                                savelabel.Text = "Client is offline, switch network.";
                            }));
                        }
                        else
                        {
                            savelabel.Visible = true;
                            savelabel.Text = "Client is offline, switch network.";
                        }
                        connection = null;
                        _eventHandlersAttached = false;
                    }
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                ApplicationLogger.Log("SignalR subscribe timeout/canceled.");

                if (savelabel.InvokeRequired)
                {
                    savelabel.Invoke(new Action(() =>
                    {
                        savelabel.Visible = true;
                        savelabel.Text = "Client is offline, switch network.";
                    }));
                }
                else
                {
                    savelabel.Visible = true;
                    savelabel.Text = "Client is offline, switch network.";
                }
                connection = null;
                _eventHandlersAttached = false;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void SetupUpdateTimer()
        {
            try
            {
                // Purana timer dispose karo
                if (_updateTimer != null)
                {
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }

                // Background timer setup (100ms interval)
                _updateTimer = new System.Threading.Timer(
                    callback: state =>
                    {
                        try
                        {
                            UpdateTimer_Tick(null, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                            ApplicationLogger.LogException(ex);
                        }
                    },
                    state: null,
                    dueTime: 0,        // start immediately
                    period: 100        // repeat every 100ms
                );
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void OnExcelRateReceived(string base64)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                string json = DecompressGzip(bytes);
                var data = JsonConvert.DeserializeObject<MarketDataDto>(json, _jsonSettings);

                if (data == null || string.IsNullOrEmpty(data.i))
                    return;

                _latestUpdates[data.i] = data; // replace latest update for each symbol
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void AddRowFromDTO(MarketDataDto dto)
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
                Common.TimeStampConvert(dto.t)   // Time
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
            try
            {
                if (_latestUpdates.IsEmpty) return;

                var updates = _latestUpdates.Values.ToList();
                _latestUpdates.Clear(); // clear for next batch


                if (updates.Count == 0) return;

                // If queue has too many records, keep only the newest 1000
                if (updates.Count > 1000)
                {
                    // Sort by Time (assuming MarketDataDto has a Time property)
                    updates = updates
                        .OrderByDescending(x => x.t)  // Newest first
                        .Take(1000)                     // Keep only 1000 newest
                        .OrderBy(x => x.t)           // Restore original order if needed
                        .ToList();
                }

                try
                {
                    updates = updates.Where(x => long.TryParse(x.t, out _)).OrderByDescending(x => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(x.t)).LocalDateTime).ToList();
                }
                catch (Exception)
                {
                }

                if (updates != null)
                {
                    SafeInvoke(() => ApplyBatchUpdates(updates));
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void ApplyBatchUpdates(List<MarketDataDto> updates)
        {
            if (updates == null || updates.Count == 0)
                return;

            try
            {
                defaultGrid.SuspendLayout();

                lock (_tableLock)
                {
                    var now = DateTime.Now;

                    foreach (var newData in updates)
                    {
                        if (newData == null || string.IsNullOrEmpty(newData.i))
                            continue;

                        // Prepare dictionary and notify Excel
                        var symbol = newData.i;
                        var dict = CreateFieldDictionary(newData);
                        ExcelNotifier.NotifyExcel(symbol, dict);

                        if (!identifiers.Contains(symbol))
                            continue;

                        if (!symbolRowMap.TryGetValue(symbol, out int rowIndex))
                        {
                            var dto = pastRateTickDTO?.FirstOrDefault(x => x.i == symbol);
                            if (dto != null)
                                AddRowFromDTO(dto);

                            if (!symbolRowMap.TryGetValue(symbol, out rowIndex))
                                continue;
                        }

                        var row = defaultGrid.Rows[rowIndex];
                        UpdateRow(row, newData);
                    }

                    // Throttle font refresh (once per batch)
                    if ((now - lastUiUpdate).TotalMilliseconds > 120)
                    {
                        var font = new Font("Microsoft Sans Serif", fontSize);
                        defaultGrid.DefaultCellStyle.Font = font;
                        defaultGrid.RowHeadersDefaultCellStyle.Font = new Font(font.FontFamily, fontSize + 1.5f, FontStyle.Bold);
                        lastUiUpdate = now;
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

        private static Dictionary<string, object> CreateFieldDictionary(MarketDataDto data)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["Name"] = data.i,
                ["Bid"] = data.b,
                ["Ask"] = data.a,
                ["LTP"] = data.ltp,
                ["High"] = data.h,
                ["Low"] = data.l,
                ["Open"] = data.o,
                ["Close"] = data.c,
                ["Net Chng"] = data.d,
                ["V"] = data.v,
                ["ATP"] = data.atp,
                ["Bid Size"] = data.bq,
                ["Total Bid Size"] = data.tbq,
                ["Ask Size"] = data.sq,
                ["Total Ask Size"] = data.tsq,
                ["Volume"] = data.vt,
                ["Open Interest"] = data.oi,
                ["Last Size"] = data.ltq,
                ["Time"] = Common.TimeStampConvert(data.t)
            };
        }

        private void UpdateRow(DataGridViewRow row, MarketDataDto data)
        {
            var symbol = data.i;
            var previousValues = new Dictionary<string, string>();

            foreach (var col in numericColumns)
            {
                if (defaultGrid.Columns.Contains(col))
                    previousValues[col] = row.Cells[col].Value?.ToString() ?? "";
            }

            // Update all values
            SetCellValue(row, "Bid", data.b);
            SetCellValue(row, "Ask", data.a);
            SetCellValue(row, "LTP", data.ltp);
            SetCellValue(row, "High", data.h);
            SetCellValue(row, "Low", data.l);
            SetCellValue(row, "Open", data.o);
            SetCellValue(row, "Close", data.c);
            SetCellValue(row, "Net Chng", data.d);
            SetCellValue(row, "V", data.v);
            SetCellValue(row, "ATP", data.atp);
            SetCellValue(row, "Bid Size", data.bq);
            SetCellValue(row, "Total Bid Size", data.tbq);
            SetCellValue(row, "Ask Size", data.sq);
            SetCellValue(row, "Total Ask Size", data.tsq);
            SetCellValue(row, "Volume", data.vt);
            SetCellValue(row, "Open Interest", data.oi);
            SetCellValue(row, "Last Size", data.ltq);
            SetCellValue(row, "Time", Common.TimeStampConvert(data.t));

            // Name cell update
            var nameCell = row.Cells["Name"];
            if ((nameCell.Value?.ToString() ?? "N/A") == "N/A")
                nameCell.Value = pastRateTickDTO?.FirstOrDefault(x => x.i == symbol)?.n ?? "--";

            // Ask price arrow logic
            string askStr = data.a;
            int askDirection = 0;
            bool hasAskChange = false;

            if (!string.IsNullOrEmpty(askStr) && double.TryParse(askStr, out double newAsk))
            {
                if (previousAskMap.TryGetValue(symbol, out double prevAsk))
                {
                    if (newAsk > prevAsk)
                    {
                        askDirection = 1;
                        hasAskChange = true;
                    }
                    else if (newAsk < prevAsk)
                    {
                        askDirection = -1;
                        hasAskChange = true;
                    }
                }

                previousAskMap[symbol] = newAsk;
            }

            // Highlight changed numeric columns
            foreach (string col in numericColumns)
            {
                if (!defaultGrid.Columns.Contains(col)) continue;

                var cell = row.Cells[col];
                var prev = previousValues.TryGetValue(col, out string prevVal) ? prevVal : "";
                var curr = cell.Value?.ToString() ?? "";

                if (IsNumericChange(prev, curr, out int direction))
                {
                    cell.Style.ForeColor = direction == 1 ? Color.Green :
                                           direction == -1 ? Color.Red :
                                           cell.Style.ForeColor;
                }
            }

            // Arrow in name
            if (hasAskChange)
            {
                string baseName = (nameCell.Value?.ToString() ?? "").Replace(" ▲", "").Replace(" ▼", "").Trim();
                nameCell.Value = askDirection == 1 ? baseName + " ▲" :
                                 askDirection == -1 ? baseName + " ▼" : baseName;
                nameCell.Style.ForeColor = askDirection == 1 ? Color.Green :
                                           askDirection == -1 ? Color.Red : nameCell.Style.ForeColor;
            }
        }

        private void SetCellValue(DataGridViewRow row, string columnName, object value)
        {
            if (!defaultGrid.Columns.Contains(columnName)) return;

            var cell = row.Cells[columnName];
            var newVal = value ?? "--";
            if ((cell.Value?.ToString() ?? "--") != newVal.ToString())
            {
                cell.Value = newVal;
            }
        }

        private static bool IsNumericChange(object oldVal, object newVal, out int direction)
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

        private async void DisconnectESCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ Stop background processes
                await StopBackgroundTasks(); // You define this method

                // 2️⃣ Unsubscribe event handlers
                UnsubscribeAllEvents(); // Optional, but recommended if you manually subscribed

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
            try
            {
                if (connection != null && !isConnectionDisposed)
                {
                    if (connection.State != HubConnectionState.Disconnected)
                    {
                        await connection.StopAsync(); // ✅ Only stop if not already disconnected
                    }

                    await connection.DisposeAsync(); // ✅ Dispose safely
                    isConnectionDisposed = true;
                }

                if (signalRTimer != null)
                {
                    signalRTimer.Stop();
                    signalRTimer.Dispose();
                    signalRTimer = null;
                }
            }
            catch (ObjectDisposedException)
            {
                // Already disposed, safe to ignore or log once
                Console.WriteLine("SignalR connection was already disposed.");
            }
            catch (Exception ex)
            {
                // Catch other unexpected issues
                Console.WriteLine("Error stopping background tasks: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
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

        #endregion SignalR Methods

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

                    // ✅ Async call
                    var response = await client.SendAsync(request).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        return;
                    }

                    var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    resultdefault = JsonConvert.DeserializeObject<MarketApiResponse>(jsonString);

                    if (resultdefault != null && resultdefault.data != null)
                    {
                        SaveInitDataToFile(resultdefault.data);

                        if (this.InvokeRequired)
                        {
                            if (!this.IsDisposed && this.IsHandleCreated)
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    pastRateTickDTO = resultdefault.data;

                                    // ✅ Initialize identifiers & SymbolName if needed
                                    if (identifiers == null || string.IsNullOrEmpty(saveFileName))
                                    {
                                        identifiers = resultdefault.data
                                            .Where(x => !string.IsNullOrEmpty(x.i))
                                            .Select(x => x.i)
                                            .ToList();

                                        SymbolName = resultdefault.data
                                            .Where(x => !string.IsNullOrEmpty(x.i) && !string.IsNullOrEmpty(x.n))
                                            .Select(x => (Symbol: x.i, SymbolName: x.n)) // ✅ works in C# 7.3
                                            .ToList();

                                    }

                                    // ✅ Initialize symbolMaster only once
                                    if (symbolMaster != null && symbolMaster.Count == 0)
                                    {
                                        symbolMaster = resultdefault.data
                                            .Where(x => !string.IsNullOrEmpty(x.i))
                                            .Select(x => x.i)
                                            .ToList();
                                    }

                                    // ✅ Filter with identifiers
                                    resultdefault.data = resultdefault.data
                                        .Where(x => identifiers != null && identifiers.Contains(x.i))
                                        .ToList();

                                    ApplyBatchUpdates(resultdefault.data);
                                });
                            }
                        }
                        else
                        {
                            pastRateTickDTO = resultdefault.data;

                            // ✅ Initialize identifiers & SymbolName if needed
                            if (identifiers == null || string.IsNullOrEmpty(saveFileName))
                            {
                                identifiers = resultdefault.data
                                    .Where(x => !string.IsNullOrEmpty(x.i))
                                    .Select(x => x.i)
                                    .ToList();

                                SymbolName = resultdefault.data
                                    .Where(x => !string.IsNullOrEmpty(x.i) && !string.IsNullOrEmpty(x.n))
                                    .Select(x => (Symbol: x.i, SymbolName: x.n)) // ✅ works in C# 7.3
                                    .ToList();

                            }

                            // ✅ Initialize symbolMaster only once
                            if (symbolMaster != null && symbolMaster.Count == 0)
                            {
                                symbolMaster = resultdefault.data
                                    .Where(x => !string.IsNullOrEmpty(x.i))
                                    .Select(x => x.i)
                                    .ToList();
                            }

                            // ✅ Filter with identifiers
                            resultdefault.data = resultdefault.data
                                .Where(x => identifiers != null && identifiers.Contains(x.i))
                                .ToList();

                            ApplyBatchUpdates(resultdefault.data);
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                connection = null;
                _eventHandlersAttached = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading initial market data: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
        }

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

        private async Task AttemptReconnectAsync(string reason)
        {
            try
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
                    //if (connection == null)
                    //{
                    //    connection = BuildConnection();
                    //    connection.On<string>("excelRate", OnExcelRateReceived);
                    //}

                    if (connection != null && connection.State == HubConnectionState.Disconnected)
                    {
                        await connection.StartAsync();
                        await connection.InvokeAsync("SubscribeSymbols", symbolMaster);
                        ApplicationLogger.Log("Reconnected and resubscribed.");
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"Reconnect failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        #endregion SignalR Helper Method

        #region Other Methods

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
                    selectedSymbols.Clear();
                    identifiers.Clear();
                    saveFileName = null;
                    lastOpenMarketWatch = "Default";

                    var clickedItem = (ToolStripMenuItem)sender;
                    await DefaultToolStripMenuItem_Click(sender, e);
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    await LoadInitialMarketDataAsync();
                    isGrid = true;
                    reloadGrid = true;
                };

                viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);

                // Add each file as a menu item with a click handler
                foreach (string fileName in fileNames)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(fileName);
                    menuItem.Click += async (sender, e) =>
                    {
                        selectedSymbols.Clear();
                        identifiers.Clear();
                        saveFileName = string.Empty;
                        _updateQueue = new ConcurrentQueue<MarketDataDto>();

                        var clickedItem = (ToolStripMenuItem)sender;

                        saveFileName = clickedItem.Text;
                        addEditSymbolsToolStripMenuItem.Enabled = true;
                        lastOpenMarketWatch = saveFileName;

                        EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                        editableMarketWatchGrid?.Dispose();
                        saveMarketWatchHost.Visible = false;
                        refreshMarketWatchHost.Visible = true;
                        await LoadSymbol(Path.Combine(saveFileName + ".slt"));

                        try
                        {
                            if (titleLabel != null)
                            {
                                titleLabel.Text = !string.IsNullOrWhiteSpace(saveFileName)
                                    ? saveFileName.ToUpper()
                                    : "Default";
                            }
                            else
                            {
                                ApplicationLogger.Log("titleLabel is null at MenuLoad");
                            }
                        }
                        catch (Exception ex)
                        {
                            ApplicationLogger.LogException(ex);
                            ApplicationLogger.Log("saveFileName: " + saveFileName ?? "NULL");
                        }

                        isEdit = false;
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
                    selectedSymbols.Clear();
                    identifiers.Clear();
                    lastOpenMarketWatch = "Default";

                    var clickedItem = (ToolStripMenuItem)sender;
                    await DefaultToolStripMenuItem_Click(sender, e);
                    MenuLoad();
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    saveFileName = null;
                    titleLabel.Text = "DEFAULT";
                    await LoadInitialMarketDataAsync();
                    isGrid = true;
                    reloadGrid = true;
                };
                defaultMenuItem.Enabled = true;
                viewToolStripMenuItem.DropDownItems.Add(defaultMenuItem);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public async Task LoadSymbol(string Filename)
        {
            try
            {
                savelabel.Visible = false;
                fontSizeComboBox.Visible = true;
                string finalPath = Path.Combine(AppFolder, username);
                selectedSymbols.Clear();
                Filename = Path.Combine(finalPath, Filename);
                string cipherText = File.ReadAllText(Filename);
                string json = CryptoHelper.Decrypt(cipherText, EditableMarketWatchGrid.passphrase);
                var symbols = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                selectedSymbols.AddRange(symbols);
                identifiers = selectedSymbols.Distinct().ToList();
                isLoadedSymbol = true;
                marketWatchViewMode = MarketWatchViewMode.Default;
                titleLabel.Text = Path.GetFileNameWithoutExtension(Filename).ToUpper();
                InitializeDataGridView();          // Configure the grid
                await SignalREvent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Was Never Save Or Moved Please Try Again!", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ApplicationLogger.LogException(ex);
            }

            thecalcifyGrid();
            MenuLoad();
        }

        private void newsToolStripMenuItem_Click(object sender, EventArgs e)
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

                // 2. Create new NewsControl
                var newsControl = new NewsControl(username, password, token)
                {
                    Name = "newsControlView",
                    Dock = DockStyle.Fill
                };

                saveMarketWatchHost.Visible = false;
                refreshMarketWatchHost.Visible = true;
                fontSizeComboBox.Visible = false;
                // Update status label

                // Update title based on edit mode
                titleLabel.Text = "News";

                // 3. Add it to main form
                this.Controls.Add(newsControl);
                newsControl.BringToFront();
                newsControl.Focus();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show($"Error loading News view: {ex.Message}");
            }
        }

        private void ClearCollections()
        {
            try
            {
                lock (_updateQueue)
                {
                    while (_updateQueue.TryDequeue(out _)) { }
                }

                lock (symbolRowMap)
                {
                    symbolRowMap.Clear();
                }

                previousAsks.Clear();
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

                // Update save button visibility
                saveMarketWatchHost.Visible = true;
                saveMarketWatchHost.Text = "Save MarketWatch";
                refreshMarketWatchHost.Visible = false;

                fontSizeComboBox.Visible = false;

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
                DisposeSignalRConnection();

                // 2. Stop and dispose timers
                signalRTimer?.Stop();
                signalRTimer?.Dispose();
                signalRTimer = null;

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
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void AlertToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void DisposeSignalRConnection()
        {
            try
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
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private void CleanupDataGridView()
        {
            try
            {
                defaultGrid.SuspendLayout();
                defaultGrid.Visible = false;

                // Unbind data
                defaultGrid.DataSource = null;

                // Clear the grid only after unbinding
                defaultGrid.Rows.Clear();
                defaultGrid.Columns.Clear();

                // Dispose cell styles and other resources
                defaultGrid.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
                defaultGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 1.5f, FontStyle.Bold);

                defaultGrid.ResumeLayout();
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
                    Console.WriteLine("Error killing Excel process: " + ex.Message);
                    ApplicationLogger.LogException(ex);
                }
            }
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
                    if (item.Text == lastOpenMarketWatch)
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

        private static void SaveInitDataToFile(List<MarketDataDto> data)
        {
            try
            {
                var dict = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

                foreach (var d in data)
                {
                    dict[d.i] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Name"] = d.n,
                        ["Bid"] = d.b,
                        ["Ask"] = d.a,
                        ["LTP"] = d.ltp,
                        ["High"] = d.h,
                        ["Low"] = d.l,
                        ["Open"] = d.o,
                        ["Close"] = d.c,
                        ["Net Chng"] = d.d,
                        ["V"] = d.v,
                        ["ATP"] = d.atp,
                        ["Bid Size"] = d.bq,
                        ["Total Bid Size"] = d.tbq,
                        ["Ask Size"] = d.sq,
                        ["Total Ask Size"] = d.tsq,
                        ["Volume"] = d.vt,
                        ["Open Interest"] = d.oi,
                        ["Last Size"] = d.ltq,
                        ["Time"] = Common.TimeStampConvert(d.t)
                    };
                }

                Directory.CreateDirectory(Path.GetDirectoryName(marketInitDataPath));
                string json = JsonConvert.SerializeObject(dict);
                string encryptedJson = CryptoHelper.Encrypt(json, EditableMarketWatchGrid.passphrase);
                File.WriteAllText(marketInitDataPath, encryptedJson);
                SaveInitDataPathToRegistry();
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"Error writing initdata.dat: {ex.Message} And {ex.StackTrace}");
            }
        }

        private static void SaveInitDataPathToRegistry()
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.CreateSubKey(@"SOFTWARE\thecalcify"))
                {
                    key.SetValue("InitDataPath", marketInitDataPath, RegistryValueKind.String);
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
        #endregion Other Methods

        #region Excel Export

        private void ExportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
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

                if (!File.Exists(marketInitDataPath))
                {
                    MessageBox.Show("initdata.dat not found.");
                    return;
                }

                var dict = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

                foreach (var d in resultdefault.data)
                {
                    dict[d.i] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Name"] = d.n,
                        ["Bid"] = d.b,
                        ["Ask"] = d.a,
                        ["LTP"] = d.ltp,
                        ["High"] = d.h,
                        ["Low"] = d.l,
                        ["Open"] = d.o,
                        ["Close"] = d.c,
                        ["Net Chng"] = d.d,
                        ["V"] = d.v,
                        ["ATP"] = d.atp,
                        ["Bid Size"] = d.bq,
                        ["Total Bid Size"] = d.tbq,
                        ["Ask Size"] = d.sq,
                        ["Total Ask Size"] = d.tsq,
                        ["Volume"] = d.vt,
                        ["Open Interest"] = d.oi,
                        ["Last Size"] = d.ltq,
                        ["Time"] = Common.TimeStampConvert(d.t)
                    };
                }

                List<ExcelFormulaCell> formulaCells = BuildFormulaCells(dict);

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
                        ws = wb.Sheets["Sheet1"];
                        ws.Cells.Clear();
                    }
                    catch
                    {
                        ws = wb.Sheets.Add();
                        ws.Name = "Sheet1";
                    }
                }
                else
                {
                    try
                    {
                        ws = wb.Sheets[saveFileName];
                        ws.Cells.Clear();
                    }
                    catch
                    {
                        ws = wb.Sheets.Add();
                        ws.Name = saveFileName;
                    }
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
                Microsoft.Office.Interop.Excel.Range startCell = ws.Cells[1, 1];
                Microsoft.Office.Interop.Excel.Range endCell = ws.Cells[maxRow, maxCol];
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
                excelApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting. Please retry.");
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                ExportToExcelToolStripMenuItem.Enabled = true;
            }
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
                string excelOptionsPath = $@"Software\Microsoft\Office\{officeVersion}\Excel\Options";
                string graphicsPath = $@"Software\Microsoft\Office\{officeVersion}\Common\Graphics";

                // --- Excel Options (RTD + EnableAnimations) ---
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(excelOptionsPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("RTDThrottleInterval", 200, RegistryValueKind.DWord);
                        key.SetValue("EnableAnimations", 0, RegistryValueKind.DWord);
                        Console.WriteLine("RTDThrottleInterval & EnableAnimations updated.");
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(excelOptionsPath))
                        {
                            newKey.SetValue("RTDThrottleInterval", 200, RegistryValueKind.DWord);
                            newKey.SetValue("EnableAnimations", 0, RegistryValueKind.DWord);
                            Console.WriteLine("Excel Options key created & values set.");
                        }
                    }
                }

                // --- Common Graphics (DisableAnimations for Excel 2013+) ---
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(graphicsPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("DisableAnimations", 1, RegistryValueKind.DWord);
                        Console.WriteLine("DisableAnimations updated.");
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(graphicsPath))
                        {
                            newKey.SetValue("DisableAnimations", 1, RegistryValueKind.DWord);
                            Console.WriteLine("Graphics key created & DisableAnimations set.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                Console.WriteLine("Error setting registry values: " + ex.Message);
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
    }
}