using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.MarketWatch
{
    public class EditableMarketWatchGrid : DataGridView
    {
        public readonly DataTable marketWatchDatatable = new DataTable();
        private List<string> symbolMaster = new List<string>();
        public List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();
        private bool isSymbolMasterInitialized = false;
        public List<string> selectedSymbols = new List<string>();
        public int fontSize = 12; // Default font size
        private readonly Helper.Common CommonClass;
        private HubConnection connection;
        public List<string> identifiers;
        public static EditableMarketWatchGrid CurrentInstance { get; private set; }
        public bool isEditMarketWatch = false;
        private DataGridView editableMarketWatchGridView;
        public static readonly string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "thecalcify");
        public static readonly string SymbolListFile = Path.Combine(AppFolder, "symbols.slt");
        public static readonly string passphrase = "v@d{4NME4sOSywXF";
        public string saveFileName, serchstring, username;
        public bool isDelete = false;
        public List<string> columnPreferences = new List<string>();
        public List<string> columnPreferencesDefault = new List<string>();
        private ContextMenuStrip rightClickMenu;
        private Panel panelAddSymbols;
        private CheckedListBox checkedListSymbols;
        private Button btnConfirmAddSymbols;
        private Button btnCancelAddSymbols;
        private Button btnSelectAllSymbols;  // declare this with other buttons
        public bool isGrid = true; // Flag to check if this is a grid or not
        private Panel panelAddColumns;
        private CheckedListBox checkedListColumns;
        private Button btnSelectAllColumns;
        private Button btnConfirmAddColumns;
        private Button btnCancelAddColumns;
        public List<MarketDataDTO> pastRateTickDTO = new List<MarketDataDTO>();
        private SynchronizationContext _uiContext;



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose custom UI elements
                panelAddSymbols?.Dispose();
                checkedListSymbols?.Dispose();
                btnConfirmAddSymbols?.Dispose();
                btnCancelAddSymbols?.Dispose();
                btnSelectAllSymbols?.Dispose();

                panelAddColumns?.Dispose();
                checkedListColumns?.Dispose();
                btnConfirmAddColumns?.Dispose();
                btnCancelAddColumns?.Dispose();
                btnSelectAllColumns?.Dispose();

                rightClickMenu?.Dispose();

                // Clear references to managed resources
                symbolMaster?.Clear();
                SymbolName?.Clear();
                selectedSymbols?.Clear();
                identifiers?.Clear();
                pastRateTickDTO?.Clear();

                // Stop and dispose SignalR connection
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
                        connection = null;
                    }
                }

                // Clear event handlers to prevent painting exceptions
                this.DataSource = null;
                this.Rows.Clear();
                this.Columns.Clear();
            }

            base.Dispose(disposing);
        }

        public EditableMarketWatchGrid()
        {
            _uiContext = SynchronizationContext.Current;
            CommonClass = new Helper.Common(this);
            CurrentInstance = this;
            LoadIdentifier();
            InitializeDataTable();
            InitializeGrid();
            InitializeAddSymbolPanel();
            this.KeyDown += EditableMarketWatchGrid_KeyDown;
            InitializeToolTip();
        }

        public async void LoadIdentifier()
        {
            thecalcify live_Rate = thecalcify.CurrentInstance;
            identifiers = live_Rate?.identifiers;
            symbolMaster = identifiers;
            //identifiers.Clear();
            await SignalREventAsync();
            AddManualEditableRow();
        }

        public void InitializeToolTip()
        {
            rightClickMenu = new ContextMenuStrip();

            var addItem = new ToolStripMenuItem("Add/Edit Symbol");
            var addColumn = new ToolStripMenuItem("Add/Edit Column");
            addItem.Click += AddSymbol_Click;
            addColumn.Click += AddColumn_Click;

            rightClickMenu.Items.Add(addItem);
            rightClickMenu.Items.Add(addColumn);

            this.CellMouseClick += EditableMarketWatchGrid_CellMouseClick;
        }

        private void EditableMarketWatchGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveSymbols(selectedSymbols);
            }
        }

        private void InitializeDataTable()
        {
            marketWatchDatatable.Columns.Add("symbol", typeof(string));
            marketWatchDatatable.Columns.Add("Name", typeof(string));
            marketWatchDatatable.Columns.Add("Bid", typeof(string));
            marketWatchDatatable.Columns.Add("Ask", typeof(string));
            marketWatchDatatable.Columns.Add("LTP", typeof(string));
            marketWatchDatatable.Columns.Add("High", typeof(string));
            marketWatchDatatable.Columns.Add("Low", typeof(string));
            marketWatchDatatable.Columns.Add("Open", typeof(string));
            marketWatchDatatable.Columns.Add("Close", typeof(string));
            marketWatchDatatable.Columns.Add("Net Chng", typeof(string));
            marketWatchDatatable.Columns.Add("ATP", typeof(string));
            marketWatchDatatable.Columns.Add("Bid Size", typeof(string));
            marketWatchDatatable.Columns.Add("Total Bid Size", typeof(string));
            marketWatchDatatable.Columns.Add("Ask Size", typeof(string));
            marketWatchDatatable.Columns.Add("Total Ask Size", typeof(string));
            marketWatchDatatable.Columns.Add("Volume", typeof(string));
            marketWatchDatatable.Columns.Add("Open Interest", typeof(string));
            marketWatchDatatable.Columns.Add("Last Size", typeof(string));
            marketWatchDatatable.Columns.Add("V", typeof(string));
            marketWatchDatatable.Columns.Add("Time", typeof(string));
        }

        private void InitializeGrid()
        {
            thecalcify defaultGridInstance = thecalcify.CurrentInstance;
            this.Name = "editableMarketWatchGridView";
            this.Dock = DockStyle.Fill;
            this.ReadOnly = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize);
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.ColumnHeadersHeight = 40;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.AllowUserToResizeRows = false;
            this.AutoGenerateColumns = true;

            this.AllowUserToResizeColumns = true; // Prevent user from resizing columns
            this.AutoGenerateColumns = false; // Set to false to control column creation

            this.ScrollBars = ScrollBars.Both;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.RowTemplate.Height = 30; // or any height you want
            this.ApplyColumnStyles();
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize + 2, FontStyle.Bold)
            };
            this.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            this.CellValueChanged += EditableMarketWatchGrid_CellValueChanged;
            this.CurrentCellDirtyStateChanged += EditableMarketWatchGrid_CurrentCellDirtyStateChanged;
            //this.EditingControlShowing += DataGridView_EditingControlShowing;
            this.DataError += DataErrorHandle;
            this.DataBindingComplete += (s, e) => ApplyFixedColumnWidths(this);

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.SetProperty,
                    null, this, new object[] { true });



            rightClickMenu = new ContextMenuStrip();

            var addItem = new ToolStripMenuItem("Add/Edit Symbol");
            var addColumn = new ToolStripMenuItem("Add/Edit Column");
            addItem.Click += AddSymbol_Click;
            addColumn.Click += AddColumn_Click;

            rightClickMenu.Items.Add(addItem);
            rightClickMenu.Items.Add(addColumn);

            this.CellMouseClick += EditableMarketWatchGrid_CellMouseClick;
            this.EditingControlShowing += EditableMarketWatchGrid_EditingControlShowing;



            editableMarketWatchGridView = this;
        }

        private void EditableMarketWatchGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox comboBox)
            {
                comboBox.Font = new Font(this.Font.FontFamily, this.fontSize);
            }
        }


        private void ApplyFixedColumnWidths(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in this.Columns)
            {
                if (col.Name != "Name")
                    col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.Automatic;
                col.Resizable = DataGridViewTriState.True;

                switch (col.Name)
                {
                    case "Time":
                        col.Width = 250;
                        break;
                    case "Name":
                        col.Width = 210;
                        break;
                    case "Bid":
                    case "Ask":
                    case "LTP":
                    case "High":
                    case "Low":
                    case "Open":
                    case "Close":
                    case "ATP":
                    case "Total Bid Size":
                    case "Ask Size":
                    case "Open Interest":
                    case "Last Size":
                        col.Width = 170;
                        break;
                    case "Total Ask Size":
                        col.Width = 100;
                        break;
                    case "Volume":
                    case "Bid Size":
                    case "Net Chng":
                        col.Width = 120;
                        break;
                    default:
                        col.Width = 100;
                        break;
                }
            }
        }

        private void DataErrorHandle(object sender, DataGridViewDataErrorEventArgs e)
        {
            ApplicationLogger.Log("DataGridView error: " + e.Exception?.Message);
            e.ThrowException = false; // prevent exception from bubbling
        }

        private void InitializeAddSymbolPanel()
        {

            if (panelAddColumns != null && panelAddColumns.Visible)
                panelAddColumns.Visible = false;


            // Container panel (with padding and rounded look)
            panelAddSymbols = new Panel
            {
                Size = new Size(500, 500),
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                Visible = false,
                Padding = new Padding(20),
            };

            // Shadow effect (optional - mimic with a border or external lib if needed)
            panelAddSymbols.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panelAddSymbols.ClientRectangle,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                    System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid);
            };

            // Center panel
            panelAddSymbols.Location = new Point(
                (this.Width - panelAddSymbols.Width) / 2,
                (this.Height - panelAddSymbols.Height) / 2
            );

            // Select All button
            btnSelectAllSymbols = new Button
            {
                Text = "Select All",
                Height = 40,
                Width = 120,
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSelectAllSymbols.FlatAppearance.BorderSize = 0;
            btnSelectAllSymbols.Click += BtnSelectAllSymbols_Click;


            // Title label
            Label titleLabel = new Label
            {
                Text = "🔄 Add / Edit Symbols",
                Font = new System.Drawing.Font("Microsoft Sans Serif Semibold", 16, FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                Dock = DockStyle.Top,
                Height = 80,
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
                BackColor = System.Drawing.Color.White
            };

            // Button container (for spacing)
            Panel buttonPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10),
                BackColor = System.Drawing.Color.White
            };

            btnConfirmAddSymbols = new Button
            {
                Text = "✔ Save",
                Height = 40,
                Width = 120,
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirmAddSymbols.FlatAppearance.BorderSize = 0;
            btnConfirmAddSymbols.Click += BtnConfirmAddSymbols_Click;

            btnCancelAddSymbols = new Button
            {
                Text = "✖ Cancel",
                Height = 40,
                Width = 120,
                BackColor = System.Drawing.Color.LightGray,
                ForeColor = System.Drawing.Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelAddSymbols.FlatAppearance.BorderSize = 0;
            btnCancelAddSymbols.Click += BtnCancelAddSymbols_Click;

            // Add buttons side by side
            // Position buttons side by side with spacing
            btnSelectAllSymbols.Location = new Point(30, 35);
            btnConfirmAddSymbols.Location = new Point(170, 35);
            btnCancelAddSymbols.Location = new Point(310, 35);

            buttonPanel.Controls.Add(btnSelectAllSymbols);
            buttonPanel.Controls.Add(btnConfirmAddSymbols);
            buttonPanel.Controls.Add(btnCancelAddSymbols);

            // Add controls to panel
            panelAddSymbols.Controls.Add(checkedListSymbols);
            panelAddSymbols.Controls.Add(buttonPanel);
            panelAddSymbols.Controls.Add(titleLabel);

            // Add panel to the main control
            this.Controls.Add(panelAddSymbols);

            // Keep panel centered on resize
            this.Resize += (s, e) =>
            {
                panelAddSymbols.Location = new Point(
                    (this.Width - panelAddSymbols.Width) / 2,
                    (this.Height - panelAddSymbols.Height) / 2
                );
            };
        }

        private void BtnSelectAllSymbols_Click(object sender, EventArgs e)
        {
            bool allChecked = true;

            // Check if all items are already checked
            for (int i = 0; i < checkedListSymbols.Items.Count; i++)
            {
                if (!checkedListSymbols.GetItemChecked(i))
                {
                    allChecked = false;
                    break;
                }
            }

            // If all checked, uncheck all; else check all
            bool check = !allChecked;
            if (!check)
                btnSelectAllSymbols.Text = "Select All"; // Change button text to "Select All"
            else
                btnSelectAllSymbols.Text = "Unselect All"; // Change button text to "Unselect All"

            for (int i = 0; i < checkedListSymbols.Items.Count; i++)
            {
                checkedListSymbols.SetItemChecked(i, check);
            }
        }

        private void EditableMarketWatchGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ClearSelection();
                if (e.RowIndex >= 0)
                    this.Rows[e.RowIndex].Selected = true;

                rightClickMenu.Show(Cursor.Position);
            }
        }

        private void AddSymbol_Click(object sender, EventArgs e)
        {
            ShowAddSymbolPanel();
        }

        private void AddColumn_Click(object sender, EventArgs e)
        {
            ShowAddColumnPanel();
        }

        private void ShowAddSymbolPanel()
        {
            try
            {
                // Clear existing items
                checkedListSymbols.Items.Clear();

                // Get current symbols from grid (if any)
                var currentSymbols = new List<string>();
                foreach (DataGridViewRow row in editableMarketWatchGridView.Rows)
                {
                    if (!row.IsNewRow && row.Cells["symbol"].Value != null)
                    {
                        currentSymbols.Add(row.Cells["symbol"].Value.ToString());
                    }
                }

                // Populate the checklist with ALL available symbols
                foreach (var symbolInfo in SymbolName)
                {
                    bool isChecked = currentSymbols.Contains(symbolInfo.Symbol);
                    checkedListSymbols.Items.Add(symbolInfo.SymbolName, isChecked);
                }

                // Update the Select All button state
                UpdateSelectAllButtonState();

                // Show the panel
                panelAddSymbols.Visible = true;
                panelAddSymbols.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing symbol panel: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }
        }

        private void UpdateSelectAllButtonState()
        {
            bool allChecked = checkedListSymbols.Items.Count > 0 &&
                             checkedListSymbols.CheckedItems.Count == checkedListSymbols.Items.Count;
            btnSelectAllSymbols.Text = allChecked ? "Unselect All" : "Select All";
        }

        private void BtnConfirmAddSymbols_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected symbol names from checklist
                var selectedSymbolNames = checkedListSymbols.CheckedItems.Cast<string>().ToList();

                // Convert symbol names back to symbols
                var newSymbols = SymbolName
                    .Where(sn => selectedSymbolNames.Contains(sn.SymbolName))
                    .Select(sn => sn.Symbol)
                    .ToList();

                // Validate selection
                if (newSymbols.Count == 0)
                {
                    MessageBox.Show("Please select at least one symbol.");
                    return;
                }

                this.Rows.Clear(); // Clear existing rows before adding new ones

                // Add each selected symbol as a new row
                foreach (var symbolName in selectedSymbolNames)
                {
                    // Add a new empty row
                    int rowIndex = Rows.Add();
                    Rows[rowIndex].Height = (int)Math.Ceiling(fontSize * 2.8);

                    // Set the symbol name and trigger update
                    SetSymbolAndTriggerUpdate(rowIndex, symbolName);
                }

                // Update the grid
                UpdateGridWithLatestData();

                // Save the changes
                SaveSymbols(newSymbols);

                // Hide the panel
                panelAddSymbols.Visible = false;

                //AddManualRow();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error confirming symbols: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }
        }

        private void BtnCancelAddSymbols_Click(object sender, EventArgs e)
        {
            panelAddSymbols.Visible = false;
        }

        private void ShowAddColumnPanel()
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
                    BackColor = System.Drawing.Color.White,
                    BorderStyle = BorderStyle.None,
                    Visible = false,
                    Padding = new Padding(20),
                };

                panelAddColumns.Paint += (s2, e2) =>
                {
                    ControlPaint.DrawBorder(e2.Graphics, panelAddColumns.ClientRectangle,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid,
                        System.Drawing.Color.LightGray, 2, ButtonBorderStyle.Solid);
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
                    ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                    Dock = DockStyle.Top,
                    Height = 50,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(0, 10, 0, 10)
                };

                // Add spacer panel for spacing
                Panel spacerPanel = new Panel
                {
                    Height = 10, // Adjust height as needed
                    Dock = DockStyle.Top
                };

                // CheckedListBox
                checkedListColumns = new CheckedListBox
                {
                    Height = 320,
                    Dock = DockStyle.Top,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    CheckOnClick = true,
                    BackColor = System.Drawing.Color.White
                };

                // Button container
                Panel buttonPanel = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(10),
                    BackColor = System.Drawing.Color.White
                };

                // Buttons
                btnSelectAllColumns = new Button
                {
                    Text = "Select All",
                    Height = 40,
                    Width = 120,
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
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
                    BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                    ForeColor = System.Drawing.Color.White,
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
                    BackColor = System.Drawing.Color.LightGray,
                    ForeColor = System.Drawing.Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnCancelAddColumns.FlatAppearance.BorderSize = 0;

                // Layout
                btnSelectAllColumns.Location = new Point(30, 25);
                btnConfirmAddColumns.Location = new Point(170, 25);
                btnCancelAddColumns.Location = new Point(310, 25);

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

                    // Update DataTable column visibility
                    foreach (DataColumn column in marketWatchDatatable.Columns)
                    {
                        column.ColumnMapping = columnPreferences.Contains(column.ColumnName)
                            ? MappingType.Element
                            : MappingType.Hidden;

                        if (column.ColumnName == "symbol" || column.ColumnName == "V")
                            column.ColumnMapping = MappingType.Hidden;
                    }

                    // Update grid column visibility
                    UpdateGridColumnVisibility();



                    panelAddColumns.Visible = false;
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

            // Update DataTable column visibility to ensure Symbol is always visible
            foreach (DataColumn column in marketWatchDatatable.Columns)
            {
                if (column.ColumnName == "symbol" && column.ColumnName != "V")
                {
                    column.ColumnMapping = MappingType.Element;
                }
                else
                {
                    column.ColumnMapping = columnPreferences.Contains(column.ColumnName)
                        ? MappingType.Element
                        : MappingType.Hidden;
                }
            }

            panelAddColumns.Visible = true;
            panelAddColumns.BringToFront();
        }

        public void UpdateGridColumnVisibility()
        {
            this.SuspendLayout();

            try
            {

                // Store current column widths before making changes
                var currentWidths = new Dictionary<string, int>();
                foreach (DataGridViewColumn col in this.Columns)
                {
                    currentWidths[col.Name] = col.Width;
                }


                //var columnsToAdd = marketWatchDatatable?.Columns
                //      .Cast<DataColumn>()
                //      .Where(col => col.ColumnMapping != MappingType.Hidden)
                //      .ToList();

                var columnsToAdd = marketWatchDatatable?.Columns.Cast<DataColumn>().ToList();


                if (columnsToAdd == null) return;

                var existingColumnsDict = this.Columns
                    .Cast<DataGridViewColumn>()
                    .ToDictionary(c => c.Name, c => c);

                var desiredColumnNames = columnsToAdd.Select(c => c.ColumnName).ToList();


                // Step 1: Add missing columns
                foreach (var dataColumn in columnsToAdd)
                {
                    if (!existingColumnsDict.ContainsKey(dataColumn.ColumnName))
                    {
                        var gridColumn = new DataGridViewTextBoxColumn
                        {
                            Name = dataColumn.ColumnName,
                            HeaderText = dataColumn.ColumnName,
                            ValueType = dataColumn.DataType,
                            ReadOnly = false
                        };

                        if (dataColumn.DataType == typeof(decimal) ||
                            dataColumn.DataType == typeof(double) ||
                            dataColumn.DataType == typeof(float))
                        {
                            gridColumn.DefaultCellStyle.Format = "N2";
                            gridColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        }
                        else if (dataColumn.DataType == typeof(DateTime))
                        {
                            gridColumn.DefaultCellStyle.Format = "HH:mm:ss:fff";
                        }

                        Columns.Add(gridColumn);
                    }
                }

                // Step 2: Remove columns not in the desired list
                var columnsToRemove = this.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(c => !desiredColumnNames.Contains(c.Name))
                    .ToList();

                foreach (var col in columnsToRemove)
                {
                    this.Columns.Remove(col);
                }

                // Step 3: Reorder columns to match DataTable order
                for (int i = 0; i < desiredColumnNames.Count; i++)
                {
                    string colName = desiredColumnNames[i];
                    var gridColumn = this.Columns[colName];
                    if (gridColumn.Name != "symbol" && gridColumn.Name != "V")
                    {
                        if (gridColumn.DisplayIndex != i)
                        {
                            gridColumn.DisplayIndex = i;
                        }
                    }
                    if (gridColumn is DataGridViewComboBoxColumn comboBoxCol)
                    {
                        comboBoxCol.DefaultCellStyle.Font = new Font(this.Font.FontFamily, this.fontSize);
                    }
                }

                // Step 4: Apply visibility settings
                foreach (DataGridViewColumn column in this.Columns)
                {
                    // Only hide/show columns that exist in our preferences list
                    if (columnPreferencesDefault.Contains(column.Name))
                    {
                        column.Visible = columnPreferences.Contains(column.Name);
                    }
                    if (column.Name == "symbol" || column.Name == "V")
                        column.Visible = false;
                }

                //Step 5: Add Column to thecalcifyGrid
                thecalcify live_Rate = thecalcify.CurrentInstance;
                if (live_Rate != null)
                {
                    live_Rate.columnPreferences = columnPreferences;
                }

                // Restore widths for columns that still exist
                foreach (DataGridViewColumn col in this.Columns)
                {
                    if (currentWidths.ContainsKey(col.Name))
                    {
                        col.Width = currentWidths[col.Name];
                        col.MinimumWidth = col.Width; // Keep it fixed
                    }
                }

                // Apply fixed widths again to ensure consistency
                ApplyFixedColumnWidths(this);
            }
            finally
            {
                this.ResumeLayout();
            }
        }

        private void ApplyColumnStyles()
        {
            foreach (DataGridViewColumn column in this.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (column.ValueType == typeof(decimal))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.ValueType == typeof(DateTime))
                {
                    column.DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
                    column.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss:fff";
                }
            }
        }

        public async Task SignalREventAsync()
        {
            try
            {
                // Initialize identifiers from Live_Rate
                thecalcify live_Rate = thecalcify.CurrentInstance;
                identifiers = live_Rate?.identifiers ?? new List<string>();

                // Configure SignalR connection
                connection = new HubConnectionBuilder()
                    .WithUrl("http://api.thecalcify.com/excel?user=calcify&auth=Starline@1008&type=mobile", options =>
                    {
                        options.Headers.Add("Origin", "http://api.thecalcify.com/");
                    })
                    .WithAutomaticReconnect(new[]
                    {
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    })
                    .Build();

                // Handle incoming data
                connection.On<string>("excelRate", (base64) =>
                {
                    try
                    {
                        Console.WriteLine("Received data from SignalR");

                        var json = DecompressGzip(Convert.FromBase64String(base64));
                        //Console.WriteLine($"Decompressed JSON: {json}");

                        var data = JsonSerializer.Deserialize<MarketDataDTO>(json);

                        if (data != null)
                        {
                            _uiContext?.Post(_ =>
                            {
                                try
                                {
                                    SuspendLayout();

                                    // Try to find existing row by symbol
                                    var existingRow = marketWatchDatatable.AsEnumerable()
                                        .FirstOrDefault(r => r.Field<string>("symbol") == data.i);

                                    DataRow row;

                                    if (existingRow != null)
                                    {
                                        // Update existing row
                                        row = existingRow;
                                    }
                                    else
                                    {
                                        // Create new row only if not exists
                                        row = marketWatchDatatable.NewRow();
                                        row["symbol"] = data.i ?? "--";
                                        marketWatchDatatable.Rows.Add(row);
                                    }



                                    row["Name"] = data.n;
                                    row["Bid"] = data.b ?? "--";
                                    row["Ask"] = data.a ?? "--";
                                    row["High"] = data.h ?? "--";
                                    row["Low"] = data.l ?? "--";
                                    row["Open"] = data.o ?? "--";
                                    row["Close"] = data.c ?? "--";
                                    row["LTP"] = data.ltp ?? "--";
                                    row["Net Chng"] = data.d ?? "--";
                                    row["ATP"] = data.atp ?? "--";
                                    row["Bid Size"] = data.bq ?? "--";
                                    row["Total Bid Size"] = data.tbq ?? "--";
                                    row["Ask Size"] = data.sq ?? "--";
                                    row["Total Ask Size"] = data.tsq ?? "--";
                                    row["Volume"] = data.vt ?? "--";
                                    row["Open Interest"] = data.oi ?? "--";
                                    row["Last Size"] = data.ltq ?? "--";
                                    row["V"] = data.v ?? "--";
                                    row["Time"] = CommonClass.TimeStampConvert(data.t);

                                    if (!isSymbolMasterInitialized)
                                    {
                                        symbolMaster = identifiers;
                                        AddManualEditableRow();
                                        isSymbolMasterInitialized = true;
                                    }

                                    UpdateGridWithLatestData();
                                    //UpdateGridColumnVisibility();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"UI Update Error: {ex}");
                                    ApplicationLogger.LogException(ex);
                                }
                                finally
                                {
                                    ResumeLayout();
                                }
                            }, null);

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing data: {ex}");
                        ApplicationLogger.LogException(ex);
                    }
                });


                // Handle connection events
                connection.Closed += async (error) =>
                {
                    Console.WriteLine($"Connection closed: {error?.Message}");
                    await Task.Delay(new Random().Next(1000, 5000));
                    try
                    {
                        if (connection != null)
                        {
                            if (connection.State == HubConnectionState.Disconnected)
                            {
                                await connection.StartAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.LogException(ex);
                        Console.WriteLine($"Reconnection failed: {ex.Message}");
                    }
                };

                connection.Reconnected += async (connectionId) =>
                {
                    Console.WriteLine($"Reconnected with ID: {connectionId}");
                    await connection.InvokeAsync("SubscribeSymbols", identifiers);
                };

                //if (connection.State == HubConnectionState.Disconnected)
                //{
                // Start connection
                await connection.StartAsync();
                //}
                await connection.InvokeAsync("SubscribeSymbols", identifiers);
                //UpdateGridColumnVisibility();
                Console.WriteLine("Successfully connected to SignalR hub");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ApplicationLogger.LogException(ex);
                Console.WriteLine($"Connection failed: {ex}");
            }
        }

        private static object TryParseDecimal(string input)
        {
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out var result))
                return result;

            return input;
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

        private void EditableMarketWatchGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.IsCurrentCellDirty)
            {
                this.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void EditableMarketWatchGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = sender as DataGridView;

            // Trigger only when "Name" column changes
            if (grid.Columns[e.ColumnIndex].Name == "Name")
            {
                var selectedName = grid.Rows[e.RowIndex].Cells["Name"].Value?.ToString();

                if (!string.IsNullOrEmpty(selectedName))
                {
                    // Map selected Name -> Symbol using SymbolName list
                    var matchedSymbol = SymbolName
                        .FirstOrDefault(sn => sn.SymbolName == selectedName).Symbol;

                    if (!string.IsNullOrEmpty(matchedSymbol))
                    {
                        // Store symbol in hidden column for backend processing
                        if (grid.Columns["symbol"] != null &&
                            !grid.Columns["symbol"].Visible &&
                            e.RowIndex >= 0)
                        {
                            grid.Rows[e.RowIndex].Cells["symbol"].Value = matchedSymbol;
                        }
                        // Add Symbol to List for Saving in Future
                        if (!selectedSymbols.Contains(matchedSymbol))
                            selectedSymbols.Add(matchedSymbol);

                        // Search in DataTable
                        DataRow[] foundRows = marketWatchDatatable.Select($"Symbol = '{matchedSymbol}'");

                        if (foundRows.Length == 0)
                        {
                            // Search in DTO if not found
                            var foundInDto = pastRateTickDTO.Where(x => x.i == matchedSymbol).ToList();

                            if (foundInDto.Count > 0)
                            {
                                foundRows = foundInDto.Select(dto =>
                                {
                                    var row = marketWatchDatatable.NewRow();
                                    row["symbol"] = dto.i;
                                    row["Name"] = dto.n;
                                    row["Bid"] = dto.b;
                                    row["Ask"] = dto.a;
                                    row["LTP"] = dto.ltp;
                                    row["High"] = dto.h;
                                    row["Low"] = dto.l;
                                    row["Open"] = dto.o;
                                    row["Close"] = dto.c;
                                    row["Net Chng"] = dto.d;
                                    row["ATP"] = dto.atp;
                                    row["Bid Size"] = dto.bq;
                                    row["Total Bid Size"] = dto.tbq;
                                    row["Ask Size"] = dto.sq;
                                    row["Total Ask Size"] = dto.tsq;
                                    row["Volume"] = dto.vt;
                                    row["Open Interest"] = dto.oi;
                                    row["Last Size"] = dto.ltq;
                                    row["V"] = dto.v;
                                    row["Time"] = CommonClass.TimeStampConvert(dto.t);
                                    return row;
                                }).ToArray();
                            }
                        }

                        // Populate UI columns from found row
                        if (foundRows.Length > 0)
                        {
                            DataRow row = foundRows[0];

                            foreach (DataColumn column in marketWatchDatatable.Columns)
                            {
                                if (column.ColumnMapping != MappingType.Hidden &&
                                    grid.Columns.Contains(column.ColumnName) &&
                                    column.ColumnName != "symbol" &&
                                    column.ColumnName != "Name") // Skip these
                                {
                                    grid.Rows[e.RowIndex].Cells[column.ColumnName].Value = row[column];
                                }
                            }
                        }

                        // Add a new empty row if last row was just filled
                        if (e.RowIndex == grid.Rows.Count - 1)
                        {
                            int newRowIndex = grid.Rows.Add();
                            grid.Rows[newRowIndex].Cells["Name"] = new DataGridViewComboBoxCell
                            {
                                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                                FlatStyle = FlatStyle.Flat,
                                DataSource = SymbolName.Select(sn => sn.SymbolName).ToList(),
                            };
                            grid.Rows[newRowIndex].Cells["Name"].Style.Font =
                                new System.Drawing.Font("Microsoft Sans Serif", fontSize, FontStyle.Regular);
                        }

                        UpdateGridWithLatestData();
                    }
                }
            }
        }

        private void UpdateGridWithLatestData()
        {


            // Create a dictionary for faster lookup of market data
            var marketDataDict = new Dictionary<string, DataRow>();
            foreach (DataRow row in marketWatchDatatable.Rows)
            {
                var symbol = row["symbol"].ToString();
                //var symbolInfo = SymbolName.FirstOrDefault(sn => sn.SymbolName == symbol).Symbol;

                if (!marketDataDict.ContainsKey(symbol))
                {
                    marketDataDict.Add(symbol, row);
                }
            }

            //this.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize,FontStyle.Regular);
            //this.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize,FontStyle.Bold);

            // Process rows in bulk
            foreach (DataGridViewRow gridRow in editableMarketWatchGridView.Rows)
            {
                if (gridRow.IsNewRow) continue;

                var symbolCell = gridRow.Cells["symbol"];

                if (symbolCell.Value == null) continue;

                string symbol = symbolCell.Value.ToString();
                // Try to get data from market data first
                if (!marketDataDict.TryGetValue(symbol, out DataRow dataRow))
                {
                    // Fall back to DTO if not found in market data
                    var dto = pastRateTickDTO.FirstOrDefault(x => x.i == symbol);
                    //if (IsRowDataDifferent(gridRow, dataRow))
                    //{
                    //    // Update all cells at once for this row
                    //    UpdateRowCells(gridRow, dataRow);

                    //}
                    if (IsRowDataDifferent(gridRow, dto))
                    {
                        // Update from DTO
                        gridRow.Cells["Name"].Value = dto.n;
                        gridRow.Cells["Bid"].Value = dto.b;
                        gridRow.Cells["Ask"].Value = dto.a;
                        gridRow.Cells["LTP"].Value = dto.ltp;
                        gridRow.Cells["High"].Value = dto.h;
                        gridRow.Cells["Low"].Value = dto.l;
                        gridRow.Cells["Open"].Value = dto.o;
                        gridRow.Cells["Close"].Value = dto.c;
                        gridRow.Cells["Net Chng"].Value = dto.d;
                        gridRow.Cells["ATP"].Value = dto.atp;
                        gridRow.Cells["Bid Size"].Value = dto.bq;
                        gridRow.Cells["Total Bid Size"].Value = dto.tbq;
                        gridRow.Cells["Ask Size"].Value = dto.sq;
                        gridRow.Cells["Total Ask Size"].Value = dto.tsq;
                        gridRow.Cells["Volume"].Value = dto.vt;
                        gridRow.Cells["Open Interest"].Value = dto.oi;
                        gridRow.Cells["Last Size"].Value = dto.ltq;
                        gridRow.Cells["V"].Value = dto.v;
                        gridRow.Cells["Time"].Value = CommonClass.TimeStampConvert(dto.t);

                        //isFirstSet = true;
                    }
                }
                else
                {
                    // Update all cells at once for this row
                    UpdateRowCells(gridRow, dataRow);
                }
            }
        }

        private void UpdateRowCells(DataGridViewRow gridRow, DataRow dataRow)
        {
            isDelete = false;

            // ✅ Adjust row height based on font size
            int rowHeight = (int)Math.Ceiling(fontSize * 2.8); // tweak multiplier as needed
            gridRow.Height = rowHeight;

            var valueSymbol = dataRow[0].ToString();

            // ✅ Store previous values for comparison (rounded to 2 decimal places)
            var previousValues = new Dictionary<string, decimal?>();
            foreach (DataGridViewCell cell in gridRow.Cells)
            {
                if (cell.Value == null || cell.OwningColumn.Name == "symbol") continue;

                if (cell.OwningColumn.Name == "symbol")
                {
                    valueSymbol = cell.Value.ToString();
                }

                try
                {
                    if (cell.Value != null &&
                           cell.Value is string s &&
                           decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal decimalValue))
                    {
                        previousValues[cell.OwningColumn.Name] = Math.Round(decimalValue, 2); // Ensures precision consistency
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing rate value at UpdateRowCells: " + ex.Message);
                }
            }


            // ✅ Column header style
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", fontSize + 2, FontStyle.Bold)
            };
            this.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            this.ColumnHeadersHeight = (int)Math.Ceiling(fontSize * 3.0);

            // ✅ Predefined styles
            var symbolStyle = new DataGridViewCellStyle
            {
                ForeColor = Color.Black,
                Font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Regular)
            };

            var defaultStyle = new DataGridViewCellStyle
            {
                ForeColor = Color.Black,
                Font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Regular)
            };

            // ✅ Update each cell
            foreach (DataGridViewCell cell in gridRow.Cells)
            {
                var columnName = cell.OwningColumn.Name;

                if (!columnPreferences.Contains(columnName)) continue;

                if (columnName == "symbol")
                {
                    cell.Style = symbolStyle;
                    continue;
                }

                try
                {
                    object value = TryParseDecimal(dataRow[columnName].ToString());

                    if (value == DBNull.Value)
                    {
                        cell.Value = string.Empty;
                        cell.Style = defaultStyle;
                        continue;
                    }

                    // ✅ Handle numeric values
                    if (value is decimal || value is double || value is float || value is int)
                    {
                        decimal newDecimal = Convert.ToDecimal(value);

                        cell.Value = newDecimal.ToString("N2");

                        // ✅ Decide ForeColor
                        Color foreColor = Color.Black;

                        if (previousValues.TryGetValue(columnName, out decimal? previousValue) && previousValue.HasValue)
                        {
                            if (newDecimal > previousValue.Value)
                            {
                                foreColor = Color.Green;
                            }
                            else if (newDecimal < previousValue.Value)
                            {
                                foreColor = Color.Red;
                            }
                            else
                            {
                                // ✅ Value is same — keep existing cell.ForeColor
                                foreColor = cell.Style?.ForeColor ?? Color.Black;
                            }
                        }


                        // ✅ Apply fresh style with correct color
                        cell.Style = new DataGridViewCellStyle
                        {
                            Font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Regular),
                            ForeColor = foreColor
                        };
                    }
                    // ✅ Handle datetime values
                    else if (value is string stringValue
                         && !string.IsNullOrWhiteSpace(stringValue)
                         && DateTime.TryParse(stringValue, out var parsedDateTime))
                    {
                        cell.Value = parsedDateTime;

                        cell.Style = new DataGridViewCellStyle
                        {
                            Format = "HH:mm:ss:fff",
                            FormatProvider = CultureInfo.InvariantCulture,
                            Font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Regular),
                            ForeColor = Color.Black,
                        };
                    }
                    else if (columnName == "Name")
                    {
                        var symbolName = SymbolName.FirstOrDefault(x => x.Symbol == valueSymbol.ToString()).SymbolName;
                        cell.Value = symbolName;
                        cell.Style = defaultStyle;
                    }
                    // ✅ Handle all other values
                    else
                    {
                        cell.Value = value.ToString();
                        cell.Style = defaultStyle;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing rate value at UpdateRowCell1: " + ex.Message);
                }
            }
        }


        public void SaveSymbols(List<string> SymbolList)
        {
            try
            {
                // Convert symbol names back to symbols if needed
                var symbolsToSave = SymbolList
                    .Select(s =>
                    {
                        var found = SymbolName.FirstOrDefault(sn => sn.SymbolName == s);
                        return !found.Equals(default) ? found.Symbol : s;
                    })
                    .Distinct()
                    .ToList();

                // Rest of your existing save logic...
                if (symbolsToSave.Count == 0)
                {
                    MessageBox.Show("Please select at least one symbol.");
                    return;
                }


                int symbolCount = SymbolList.Count;
                int rowCount = editableMarketWatchGridView.NewRowIndex >= 0
                    ? editableMarketWatchGridView.Rows.Count - 1
                    : editableMarketWatchGridView.Rows.Count;

                rowCount--;

                if (symbolCount != rowCount && isGrid)
                {
                    // Clear the selectedSymbols list
                    SymbolList.Clear();

                    // Iterate through each row in the gridview
                    foreach (DataGridViewRow row in editableMarketWatchGridView.Rows)
                    {
                        // Skip if the row is the new row (if applicable)
                        if (!row.IsNewRow)
                        {
                            // Get the value from the "Symbol" column
                            var symbolValue = row.Cells["symbol"].Value;

                            // Add to selectedSymbols if the value is not null
                            if (symbolValue != null)
                            {
                                SymbolList.Add(symbolValue.ToString());
                            }
                        }
                    }
                }

                if (SymbolList.Count == 0)
                {
                    MessageBox.Show("Please Select Atleast one Symbol for Save", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (saveFileName == null)
                {// Show save file dialog
                    using (var saveDialog = new SaveFileDialog())
                    {
                        string basePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "thecalcify");

                        //Live_Rate live_Rate = Live_Rate.CurrentInstance;

                        // Final folder path
                        string finalPath = Path.Combine(basePath, thecalcify.CurrentInstance.username.Trim());

                        saveDialog.InitialDirectory = finalPath;  // Set default directory
                        saveDialog.Filter = "Symbol List Files (*.slt)|*.slt|All files (*.*)|*.*";
                        saveDialog.Title = "Save Symbol List";
                        saveDialog.DefaultExt = ".slt";
                        saveDialog.AddExtension = true;

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        // If user selected a file
                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            if (Path.GetFileNameWithoutExtension(saveDialog.FileName).ToLower() == "default")
                            {
                                MessageBox.Show("You can not file name Default");
                                return;
                            }
                            string json = JsonSerializer.Serialize(SymbolList);
                            string encryptedJson = CryptoHelper.Encrypt(json, passphrase);

                            // Ensure directory exists (should already exist from AppFolder)
                            if (!Directory.Exists(finalPath))
                                Directory.CreateDirectory(finalPath);


                            // Save to the user-selected filename
                            File.WriteAllText(saveDialog.FileName, encryptedJson);

                            if (isGrid)
                            {
                                SymbolList.Clear();
                            }

                            saveFileName = Path.GetFileNameWithoutExtension(saveDialog.FileName);

                            MessageBox.Show($"{Path.GetFileNameWithoutExtension(saveDialog.FileName)} MarketWatch Save Successfully", "MarketWatch Save", MessageBoxButtons.OK);

                        }
                    }
                }
                else
                {
                    string json = JsonSerializer.Serialize(SymbolList);
                    string encryptedJson = CryptoHelper.Encrypt(json, passphrase);

                    // Ensure directory exists (should already exist from AppFolder)
                    if (!Directory.Exists(AppFolder))
                        Directory.CreateDirectory(AppFolder);

                    //Live_Rate live_Rate = Live_Rate.CurrentInstance;

                    saveFileName = Path.Combine(AppFolder, thecalcify.CurrentInstance.username.Trim(), $"{saveFileName}.slt");
                    // Save to the user-selected filename
                    File.WriteAllText(saveFileName, encryptedJson);

                    if (isGrid)
                    {
                        SymbolList.Clear();
                    }

                    MessageBox.Show($"{Path.GetFileNameWithoutExtension(saveFileName)} Marketwatch Update Successfully", "MarketWatch Save", MessageBoxButtons.OK);

                    saveFileName = Path.GetFileNameWithoutExtension(saveFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem While Saving File: {ex.Message}", "Saving Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                ApplicationLogger.LogException(ex);
            }
            finally
            {
                selectedSymbols = SymbolList;
                thecalcify live_Rate = thecalcify.CurrentInstance;
                if (live_Rate == null)
                    live_Rate.LiveRateGrid();
                if (saveFileName != null)
                {
                    live_Rate.titleLabel.Text = $"{saveFileName}";
                }
                live_Rate.isdeleted = false;
                live_Rate.MenuLoad();
            }
        }

        private void AddManualEditableRow()
        {
            Columns?.Clear();


            var columnsToAdd = marketWatchDatatable?.Columns
                  .Cast<DataColumn>()
                  .Where(col => col.ColumnMapping != MappingType.Hidden)
                  .ToList();

            // 1️⃣ Create dropdown column for "Name"
            var nameColumn = new DataGridViewComboBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                FlatStyle = FlatStyle.Standard, // More native look
                Width = 200,
                DataSource = SymbolName.Select(sn => sn.SymbolName).ToList(),
                ReadOnly = false,
            };

            nameColumn.CellTemplate.Style.Font = new System.Drawing.Font("Microsoft Sans Serif", fontSize, FontStyle.Regular);
            Columns.Add(nameColumn);

            // 2️⃣ Add all other columns from DataTable except "symbol" (hidden)
            foreach (var dataColumn in columnsToAdd)
            {
                if (dataColumn.ColumnName.Equals("symbol", StringComparison.OrdinalIgnoreCase) ||
                    dataColumn.ColumnName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    continue; // symbol column will be hidden, backend only

                var gridColumn = new DataGridViewTextBoxColumn
                {
                    Name = dataColumn.ColumnName,
                    HeaderText = dataColumn.ColumnName,
                    ValueType = dataColumn.DataType,
                    ReadOnly = false
                };

                if (dataColumn.DataType == typeof(decimal) ||
                    dataColumn.DataType == typeof(double) ||
                    dataColumn.DataType == typeof(float))
                {
                    gridColumn.DefaultCellStyle.Format = "N2";
                    gridColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (dataColumn.DataType == typeof(DateTime))
                {
                    gridColumn.DefaultCellStyle.Format = "HH:mm:ss:fff";
                }

                Columns.Add(gridColumn);
            }

            // 3️⃣ Add the hidden backend "symbol" column
            var hiddenSymbolColumn = new DataGridViewTextBoxColumn
            {
                Name = "symbol",
                HeaderText = "symbol",
                Visible = false // hide from user
            };
            Columns.Add(hiddenSymbolColumn);

            // 4️⃣ Hide V column if it exists
            if (this.Columns.Contains("V"))
                this.Columns["V"].Visible = false;


            // After adding all columns, apply fixed widths
            ApplyFixedColumnWidths(this);


            // 5️⃣ Add an empty row
            int rowIndex = Rows.Add();
            Rows[rowIndex].Height = (int)Math.Ceiling(fontSize * 2.8);
        }

        private void SetSymbolAndTriggerUpdate(int rowIndex, string symbolName, bool isNewRow = false)
        {
            // Validate row index
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

            // Validate symbol name
            if (string.IsNullOrEmpty(symbolName))
            {
                Rows[rowIndex].Cells["Name"].Value = null;
                return;
            }

            // Only add new row if explicitly requested
            if (isNewRow)
            {
                rowIndex = Rows.Add();
                Rows[rowIndex].Height = (int)Math.Ceiling(fontSize * 2.8);
            }

            // Set the value in the "Name" combobox column
            if (Columns["Name"] is DataGridViewComboBoxColumn)
            {
                //// Set the display value
                //Rows[rowIndex].Cells["Name"].Value = symbolName;

                // Find the corresponding Symbol object
                var symbolInfo = SymbolName.FirstOrDefault(sn =>
                    string.Equals(sn.SymbolName, symbolName, StringComparison.OrdinalIgnoreCase));

                if (symbolInfo.SymbolName != null)
                {
                    // Set the hidden symbol column value
                    Rows[rowIndex].Cells["symbol"].Value = symbolInfo.Symbol;
                    //Rows[rowIndex].Cells["Name"].Value = symbolInfo.SymbolName;

                    // Trigger update
                    OnCellValueChanged(new DataGridViewCellEventArgs(
                        Columns["Name"].Index,
                        rowIndex));
                }
                else
                {
                    // Clear if symbol not found
                    Rows[rowIndex].Cells["Name"].Value = null;
                    //Rows[rowIndex].Cells["symbol"].Value = null;
                }
            }
        }

        private bool IsRowDataDifferent(DataGridViewRow gridRow, DataRow dataRow)
        {
            foreach (DataColumn col in marketWatchDatatable.Columns)
            {
                string colName = col.ColumnName;
                if (colName == "symbol") continue; // skip symbol column

                var gridValue = gridRow.Cells[colName].Value;
                var dataValue = dataRow[colName];

                // Handle DBNull and null equivalence
                if (dataValue == DBNull.Value) dataValue = null;

                if ((gridValue == null && dataValue != null) ||
                    (gridValue != null && !gridValue.Equals(dataValue)))
                {
                    return true; // values differ
                }
            }
            return false; // all values equal
        }

        private bool IsRowDataDifferent(DataGridViewRow gridRow, MarketDataDTO dto)
        {
            // Replace YourDtoType with the actual DTO type and compare relevant fields
            bool differs = false;

            differs |= !Equals(gridRow.Cells["Name"].Value?.ToString() ?? "--", dto.n);
            differs |= !Equals(gridRow.Cells["Bid"].Value ?? "--", dto.b);
            differs |= !Equals(gridRow.Cells["Ask"].Value ?? "--", dto.a);
            differs |= !Equals(gridRow.Cells["LTP"].Value ?? "--", dto.ltp);
            differs |= !Equals(gridRow.Cells["High"].Value ?? "--", dto.h);
            differs |= !Equals(gridRow.Cells["Low"].Value ?? "--", dto.l);
            differs |= !Equals(gridRow.Cells["Open"].Value ?? "--", dto.o);
            differs |= !Equals(gridRow.Cells["Close"].Value ?? "--", dto.c);
            differs |= !Equals(gridRow.Cells["Net Chng"].Value ?? "--", dto.d);
            differs |= !Equals(gridRow.Cells["ATP"].Value ?? "--", dto.atp);
            differs |= !Equals(gridRow.Cells["Bid Size"].Value ?? "--", dto.bq);
            differs |= !Equals(gridRow.Cells["Total Bid Size"].Value ?? "--", dto.tbq);
            differs |= !Equals(gridRow.Cells["Ask Size"].Value ?? "--", dto.sq);
            differs |= !Equals(gridRow.Cells["Total Ask Size"].Value ?? "--", dto.tsq);
            differs |= !Equals(gridRow.Cells["Volume"].Value ?? "--", dto.vt);
            differs |= !Equals(gridRow.Cells["Open Interest"].Value ?? "--", dto.oi);
            differs |= !Equals(gridRow.Cells["Last Size"].Value ?? "--", dto.ltq);
            differs |= !Equals(gridRow.Cells["V"].Value?.ToString() ?? "--", dto.v);
            differs |= !Equals(gridRow.Cells["Time"].Value?.ToString() ?? "--", CommonClass.TimeStampConvert(dto.t));

            return differs;
        }

        public bool IsValidSymbolName(string inputName)
        {
            if (string.IsNullOrWhiteSpace(inputName))
                return false;

            var validSymbolNames = new HashSet<string>(
                SymbolName.Select(sn => sn.SymbolName),
                StringComparer.OrdinalIgnoreCase
            );

            return validSymbolNames.Contains(inputName);
        }

    }

}