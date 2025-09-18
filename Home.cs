using ClosedXML.Excel;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public bool isDisconnecting = false, isConnectionDisposed = false;

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
        public HubConnection connection;

        public Common commonClass;
        private ConcurrentQueue<MarketDataDto> _updateQueue = new ConcurrentQueue<MarketDataDto>();
        private readonly object _tableLock = new object();
        private readonly object _reconnectLock = new object();

        // ======================
        // 📌 Timers / Threads
        // ======================
        private System.Windows.Forms.Timer _updateTimer;

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
        public enum MarketWatchViewMode
        {
            Default,
            New
        }

        public MarketWatchViewMode marketWatchViewMode = MarketWatchViewMode.Default;

        public enum ConnectionViewMode
        {
            Connect,
            Disconnect
        }

        public ConnectionViewMode connectionViewMode = ConnectionViewMode.Connect;

        // ======================
        // 📌 API Responses
        // ======================
        public MarketApiResponse resultdefault;

        #endregion Declaration and Initialization

        #region Form Method
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

            DateTime txtlicenceDate = Common.ParseToDate(licenceDate);
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

            // At app startup, spin up Excel hidden, then close it. This warms up the COM server so the real export is fast:
            var app = new Microsoft.Office.Interop.Excel.Application();
            app.Quit();

            MenuLoad();

            // --- LOAD INITIAL DATA ASYNCHRONOUSLY ---
            await LoadInitialMarketDataAsync();
            HandleLastOpenedMarketWatch();

            // --- FORM PROPERTIES ---
            this.WindowState = FormWindowState.Maximized;
            defaultGrid.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

            CurrentInstance = this;

            // --- INITIALIZE DATA STRUCTURES ---
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
                // Correct way to call the static method
                CredentialManager.SaveMarketWatchWithColumns(lastOpenMarketWatch, columnPreferences.Count == 0 ? columnPreferencesDefault : columnPreferences);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

            System.Windows.Forms.Application.Exit();
        }

        private void CheckLicenceLoop()
        {
            try
            {
                while (isRunning)
                {
                    DateTime txtlicenceDate = Common.ParseToDate(licenceDate);
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

        public async Task DefaultToolStripMenuItem_Click(object sender, EventArgs e)
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
            isEdit = false;
            identifiers = symbolMaster;
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

        public static void DeleteExcelSheet(string filename) {
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

            if (e.KeyCode == Keys.Escape)
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
            if (titleLabel != null)
            {
                if (titleLabel.Text.ToLower() == "new marketwatch")
                {
                    saveMarketWatchHost.Visible = true;
                    saveMarketWatchHost.Text = "Save MarketWatch";
                }
                else
                {
                    saveMarketWatchHost.Visible = false;
                }

                txtsearch.Text = null;
            }
        }

        private void DefaultGrid_DataSourceChanged(object sender, EventArgs e)
        {
            try
            { }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"[DefaultGrid_DataSourceChanged] Stuck On : {ex.Message}");
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

        #endregion Form Method

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
                connection = BuildConnection();

                connection.On<string>("excelRate", OnExcelRateReceived);

                connection.Closed += async (error) =>
                {
                    Console.WriteLine("Connection closed");
                    if (!isDisconnecting)
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        // Possibly try reconnect manually if needed
                    }
                };

                connection.Reconnected += async (connectionId) =>
                {
                    if (!isDisconnecting)
                    {
                        Console.WriteLine("Reconnected to SignalR hub");

                        try
                        {
                            if (selectedSymbols.Count != 0)
                                identifiers = new List<string>(selectedSymbols);

                            await connection.InvokeAsync("SubscribeSymbols", symbolMaster);
                            Console.WriteLine("Resubscribed after reconnect.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to resubscribe after reconnect.");
                            ApplicationLogger.LogException(ex);
                        }
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
                    if (connection != null && connection.State == HubConnectionState.Connected)
                    {
                        if (selectedSymbols.Count != 0)
                            identifiers = new List<string>(selectedSymbols);

                        if (currentIdentifiers.Count() != identifiers.Count())
                            identifiers = currentIdentifiers;

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await connection.InvokeAsync("SubscribeSymbols", identifiers, cts.Token);
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
                var data = JsonConvert.DeserializeObject<MarketDataDto>(json);
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

        private static bool IsNullOrEmptyOrPlaceholder(object val)
        {
            return val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString()) || val.ToString() == "--";
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
            if (_updateQueue.IsEmpty) return;

            var updates = new List<MarketDataDto>();
            while (_updateQueue.TryDequeue(out var data))
            {
                updates.Add(data);
            }

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

        private void ApplyBatchUpdates(List<MarketDataDto> updates)
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

                        // Prepare dictionary of field values for this symbol
                        // Assuming 'row' is a DataGridViewRow (not DataRow)
                        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["Name"] = newData.i,
                            ["Bid"] = newData.b,
                            ["Ask"] = newData.a,
                            ["LTP"] = newData.ltp,
                            ["High"] = newData.h,
                            ["Low"] = newData.l,
                            ["Open"] = newData.o,
                            ["Close"] = newData.c,
                            ["Net Chng"] = newData.d,
                            ["V"] = newData.v,
                            ["ATP"] = newData.atp,
                            ["Bid Size"] = newData.bq,
                            ["Total Bid Size"] = newData.tbq,
                            ["Ask Size"] = newData.sq,
                            ["Total Ask Size"] = newData.tsq,
                            ["Volume"] = newData.vt,
                            ["Open Interest"] = newData.oi,
                            ["Last Size"] = newData.ltq,
                            ["Time"] = Common.TimeStampConvert(newData.t)
                        };

                        // After Every updates are applied:
                        ExcelNotifier.NotifyExcel(newData.i, dict);

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
                        SetCellValue(row, "Time", Common.TimeStampConvert(newData.t));

                        // Set name if still default
                        var nameCell = row.Cells["Name"];
                        if ((nameCell.Value?.ToString() ?? "N/A") == "N/A")
                        {
                            var name = pastRateTickDTO?.FirstOrDefault(x => x.i == newData.i)?.n ?? "--";
                            nameCell.Value = name;
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

                    //UpdateExcelDataEfficiently(defaultGrid);

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
                        SaveInitDataToFile(resultdefault.data);

                        // Filter out instruments not in the valid list
                        this.Invoke((MethodInvoker)delegate
                        {
                            pastRateTickDTO = resultdefault.data;

                            if (identifiers == null || saveFileName == null)
                            {
                                // Extract all non-null, non-empty "i" values into identifiers list
                                identifiers = resultdefault.data
                                    .Where(x => !string.IsNullOrEmpty(x.i))
                                    .Select(x => x.i)
                                    .ToList();

                                SymbolName = resultdefault.data
                                     .Where(x => !string.IsNullOrEmpty(x.i) && !string.IsNullOrEmpty(x.n))
                                     .Select(x => (Symbol: x.i, SymbolName: x.n)).ToList();

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
                //col.SortMode = DataGridViewColumnSortMode.Automatic;
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

                if (connection.State == HubConnectionState.Disconnected)
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

        #endregion SignalR Helper Method

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

                //string cipherText = File.ReadAllText(marketInitDataPath);
                //string json = CryptoHelper.Decrypt(cipherText, EditableMarketWatchGrid.passphrase);
                //var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

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
                writeRange.Value2 = bulkData;

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
                    symbolMaster.Clear();
                    saveFileName = null;
                    //StopBackgroundTasks();
                    lastOpenMarketWatch = "Default";

                    var clickedItem = (ToolStripMenuItem)sender;
                    await DefaultToolStripMenuItem_Click(sender, e);
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    //SetActiveMenuItem(clickedItem);
                    //saveMarketWatchHost.Visible = false;
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
                        symbolMaster.Clear();
                        saveFileName = null;
                        _updateQueue = new ConcurrentQueue<MarketDataDto>();


                        var clickedItem = (ToolStripMenuItem)sender;

                        saveFileName = clickedItem.Text;
                        addEditSymbolsToolStripMenuItem.Enabled = true;
                        lastOpenMarketWatch = saveFileName;

                        EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                        editableMarketWatchGrid?.Dispose();
                        saveMarketWatchHost.Visible = false;
                        await LoadSymbol(Path.Combine(saveFileName + ".slt"));

                        titleLabel.Text = saveFileName.ToUpper();
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
                    symbolMaster.Clear();
                    //StopBackgroundTasks();
                    lastOpenMarketWatch = "Default";

                    var clickedItem = (ToolStripMenuItem)sender;
                    await DefaultToolStripMenuItem_Click(sender, e);
                    MenuLoad();
                    addEditSymbolsToolStripMenuItem.Enabled = false;
                    saveFileName = null;
                    //SetActiveMenuItem(clickedItem);
                    //saveMarketWatchHost.Visible = false;
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
            finally
            {
                DefaultGrid_DataSourceChanged(this, EventArgs.Empty);
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
                newCTRLNToolStripMenuItem1.Enabled = false;

                // Update save button visibility
                saveMarketWatchHost.Visible = true;
                saveMarketWatchHost.Text = "Save MarketWatch";

                fontSizeComboBox.Visible = false;
                // Update status label

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
            //CleanupExcel();
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

        private void CleanupDataGridView()
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
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = baseKey.CreateSubKey(@"SOFTWARE\thecalcify"))
            {
                key.SetValue("InitDataPath", marketInitDataPath, RegistryValueKind.String);
            }
        }

        #endregion Other Methods
    }
}