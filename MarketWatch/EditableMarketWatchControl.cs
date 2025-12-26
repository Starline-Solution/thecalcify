using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;
using thecalcify.Properties;

namespace thecalcify.MarketWatch
{
    public partial class EditableMarketWatchControl : UserControl, ILiveMarketGrid
    {
        private readonly Dictionary<string, int> _symbolRowMap =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private List<(string Symbol, string Name)> _symbolMaster =
            new List<(string, string)>();

        public List<string> selectedSymbols = new List<string>();

        private static int? currentMarketWatchId = null; // null = new
        private static string currentMarketWatchName = null;
        // Tracks cycling state per row
        private readonly Dictionary<int, (string Prefix, int Index)> _rowSearchState
            = new Dictionary<int, (string, int)>();

        private int fontSize = 10;

        public EditableMarketWatchControl()
        {
            InitializeComponent();
            InitializeLayout();
            BuildColumns();

            // 🔥 Ensure user can add new symbol
            EnsureOneEmptyRowAtEnd();

            _grid.EditingControlShowing += Grid_EditingControlShowing;

            LiveGridRegistry.Register(this);
        }

        // ===============================
        // PUBLIC API
        // ===============================
        public void SetSymbolMaster(List<(string Symbol, string Name)> symbols)
        {
            _symbolMaster = symbols;

            // ✅ Bind ComboBox column ONCE
            var comboCol = (DataGridViewComboBoxColumn)_grid.Columns["Name"];
            comboCol.DataSource = _symbolMaster.Select(x => x.Name).ToList();
        }

        // ===============================
        // UI INITIALIZATION
        // ===============================
        private void InitializeLayout()
        {
            Dock = DockStyle.Fill;

            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.RowHeadersVisible = false;
            _grid.AutoGenerateColumns = false;

            Controls.Add(_grid);
        }

        private void BuildColumns()
        {
            _grid.Columns.Clear();

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "symbol",
                Visible = false
            });

            _grid.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                Width = 220,
                FlatStyle = FlatStyle.Flat
            });

            string[] numericCols =
            {
                "Bid","Ask","LTP","High","Low","Open","Close","Net Chng","ATP",
                "Bid Size","Total Bid Size","Ask Size","Total Ask Size",
                "Volume","Open Interest","Last Size","Time","V"
            };

            foreach (var col in numericCols)
            {
                _grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = col,
                    ReadOnly = true,
                    Visible = col != "V"
                });
            }
        }

        private void AddEmptyRow()
        {
            int rowIndex = _grid.Rows.Add();

            var row = _grid.Rows[rowIndex];
            foreach (DataGridViewCell cell in row.Cells)
                cell.Style.ForeColor = Color.Black;
        }


        // ===============================
        // COMBOBOX HANDLING
        // ===============================
        private void Grid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_grid.CurrentCell == null)
                return;

            if (_grid.CurrentCell.ColumnIndex != _grid.Columns["Name"].Index)
                return;

            if (e.Control is ComboBox combo)
            {
                combo.DropDownStyle = ComboBoxStyle.DropDown;
                combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                combo.AutoCompleteSource = AutoCompleteSource.ListItems;

                // 🔁 detach first (VERY IMPORTANT)
                combo.KeyDown -= Combo_KeyDown;
                combo.SelectionChangeCommitted -= Combo_SelectionCommitted;

                // ✅ attach both
                combo.KeyDown += Combo_KeyDown;                       // bulk bind (Enter)
                combo.SelectionChangeCommitted += Combo_SelectionCommitted; // mouse bind
            }
        }

        private void Combo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            if (!(sender is ComboBox combo)) return;
            if (_grid.CurrentCell == null) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            int rowIndex = _grid.CurrentCell.RowIndex;
            string typedText = combo.Text?.Trim();

            if (string.IsNullOrEmpty(typedText)) return;

            var matches = _symbolMaster
                .Where(x => x.Name.StartsWith(typedText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matches.Any()) return;

            // 🔁 GET OR RESET SEARCH STATE
            if (!_rowSearchState.TryGetValue(rowIndex, out var state) ||
                !state.Prefix.Equals(typedText, StringComparison.OrdinalIgnoreCase))
            {
                state = (typedText, 0);
            }

            var match = matches[state.Index % matches.Count];

            // 🚫 Prevent duplicate symbols
            if (_symbolRowMap.ContainsKey(match.Symbol))
            {
                state.Index++;
                _rowSearchState[rowIndex] = state;
                return;
            }

            BindSymbolToRow(rowIndex, match.Symbol, match.Name);

            // 🔁 Advance index
            state.Index++;
            _rowSearchState[rowIndex] = state;

            EnsureOneEmptyRowAtEnd();
            _grid.CurrentCell = _grid.Rows[_grid.Rows.Count - 1].Cells["Name"];
        }

        private void Combo_SelectionCommitted(object sender, EventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            if (_grid.CurrentCell == null) return;

            int rowIndex = _grid.CurrentCell.RowIndex;
            string selectedName = combo.Text;

            var info = _symbolMaster
                .FirstOrDefault(x => x.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(info.Symbol)) return;

            // 🔥 OVERRIDE EXISTING SYMBOL SAFELY
            ClearExistingSymbolFromRow(rowIndex);
            BindSymbolToRow(rowIndex, info.Symbol, info.Name);

            _rowSearchState.Remove(rowIndex); // reset cycling
        }

        // ===============================
        // CORE BINDING
        // ===============================
        private void BindSymbolToRow(int rowIndex, string symbol, string name)
        {
            if (rowIndex < 0 || rowIndex >= _grid.Rows.Count)
                return;

            var row = _grid.Rows[rowIndex];

            ClearExistingSymbolFromRow(rowIndex);

            row.Cells["symbol"].Value = symbol;
            row.Cells["Name"].Value = name;

            _symbolRowMap[symbol] = rowIndex;

            ApplySnapshot(symbol, rowIndex);

            EnsureOneEmptyRowAtEnd();
        }


        private void ApplySnapshot(string symbol, int rowIndex)
        {
            var all = LastTickStore.GetAll();

            if (!all.TryGetValue(symbol, out var snapshot))
                return;

            foreach (var kv in snapshot)
            {
                if (_grid.Columns.Contains(kv.Key))
                    _grid.Rows[rowIndex].Cells[kv.Key].Value = kv.Value;
            }
        }

        // ===============================
        // LIVE DATA (ILiveMarketGrid)
        // ===============================
        public bool IsReady =>
             !_grid.IsDisposed &&
             _grid.IsHandleCreated &&
             _grid.Visible &&
             _grid.Rows.Count > 0;


        public bool TryApplyDto(MarketDataDto dto)
        {
            if (dto == null) return false;

            if (!_symbolRowMap.TryGetValue(dto.i, out int rowIndex))
                return false;

            if (rowIndex < 0 || rowIndex >= _grid.Rows.Count)
                return false;

            var row = _grid.Rows[rowIndex];

            UpdateCellWithColor(row.Cells["Bid"], dto.b);
            UpdateCellWithColor(row.Cells["Ask"], dto.a);
            UpdateCellWithColor(row.Cells["LTP"], dto.ltp);
            UpdateCellWithColor(row.Cells["High"], dto.h);
            UpdateCellWithColor(row.Cells["Low"], dto.l);
            row.Cells["Open"].Value = dto.o;
            row.Cells["Close"].Value = dto.c;
            row.Cells["Net Chng"].Value = dto.d;
            row.Cells["ATP"].Value = dto.atp;
            row.Cells["Bid Size"].Value = dto.bq;
            row.Cells["Total Bid Size"].Value = dto.tbq;
            row.Cells["Ask Size"].Value = dto.sq;
            row.Cells["Total Ask Size"].Value = dto.tsq;
            row.Cells["Volume"].Value = dto.vt;
            row.Cells["Open Interest"].Value = dto.oi;
            row.Cells["Last Size"].Value = dto.ltq;
            row.Cells["Time"].Value = Common.TimeStampConvert(dto.t);

            return true;
        }

        private static double ParseDouble(object value)
        {
            if (value == null) return double.NaN;
            return double.TryParse(value.ToString(), out var d) ? d : double.NaN;
        }

        private void UpdateCellWithColor(
            DataGridViewCell cell,
            string newValue)
        {
            if (cell == null) return;

            double oldVal = ParseDouble(cell.Value);
            double newVal = ParseDouble(newValue);

            // Update value first
            cell.Value = newValue;

            // If new value is invalid → do nothing
            if (double.IsNaN(newVal))
                return;

            // First tick OR previous invalid → set color once
            if (double.IsNaN(oldVal))
            {
                cell.Style.ForeColor = Color.Black; // initial state
                return;
            }

            // Compare
            if (newVal > oldVal)
                cell.Style.ForeColor = Color.Green;
            else if (newVal < oldVal)
                cell.Style.ForeColor = Color.Red;
            // equal → keep previous color
        }


        private void AddSymbol_Click(object sender, EventArgs e)
        {
            InitializeAddSymbolPanel();
            ShowAddSymbolPanel();
        }

        private void InitializeAddSymbolPanel()
        {

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

        private void ShowAddSymbolPanel()
        {
            try
            {
                // Clear existing items
                checkedListSymbols.Items.Clear();

                // Get current symbols from grid (if any)
                var currentSymbols = new List<string>();
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    if (!row.IsNewRow && row.Cells["symbol"].Value != null)
                    {
                        currentSymbols.Add(row.Cells["symbol"].Value.ToString());
                    }
                }

                // Populate the checklist with ALL available symbols
                foreach (var symbolInfo in _symbolMaster)
                {
                    bool isChecked = currentSymbols.Contains(symbolInfo.Symbol);
                    checkedListSymbols.Items.Add(symbolInfo.Name.Trim(), isChecked);
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


        private async void BtnConfirmAddSymbols_Click(object sender, EventArgs e)
        {
            try
            {
                // 1️⃣ Get checked symbol names
                var checkedNames = checkedListSymbols.CheckedItems.Cast<string>().ToList();

                if (!checkedNames.Any())
                {
                    MessageBox.Show("Please select at least one symbol.");
                    return;
                }

                // 2️⃣ Convert names → symbols
                var newSymbols = _symbolMaster
                    .Where(x => checkedNames.Contains(x.Name.Trim()))
                    .Select(x => x.Symbol)
                    .ToList();

                // 3️⃣ Get current symbols from grid
                var existingSymbols = GetSymbolsFromGrid();

                // 4️⃣ Compute DELTAS
                var addedSymbols = newSymbols.Except(existingSymbols).ToList();
                var removedSymbols = existingSymbols.Except(newSymbols).ToList();

                // 5️⃣ Map added symbols to (Symbol, Name)
                var addedSymbolInfos = _symbolMaster
                    .Where(x => addedSymbols.Contains(x.Symbol))
                    .Select(x => (x.Symbol, x.Name))
                    .ToList();

                // 6️⃣ APPLY TO GRID (🔥 CORE FIX)
                AddSymbols(addedSymbolInfos);
                RemoveSymbols(removedSymbols);

                // 7️⃣ Save MarketWatch
                await SaveMarketWatchAsync(newSymbols);

                // 8️⃣ Close panel
                panelAddSymbols.Visible = false;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show("Error confirming symbols.");
            }
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

        private void BtnCancelAddSymbols_Click(object sender, EventArgs e)
        {
            panelAddSymbols.Visible = false;
        }

        public async Task SaveMarketWatchAsync(List<string> symbol = null, MarketWatchItem existingMarketWatchItem = null)
        {
            try
            {
                var marketWatchService = new MarketwatchServerAPI(thecalcify.token);

                if (symbol == null)
                {
                    symbol = GetSymbolsFromGrid();
                }

                if (!symbol.Any())
                {
                    MessageBox.Show("Please select at least one symbol.");
                    return;
                }

                if (existingMarketWatchItem != null)
                {
                    currentMarketWatchName = existingMarketWatchItem.MarketWatchName;
                    currentMarketWatchId = existingMarketWatchItem.MarketWatchId;
                }

                // Ask name only if first time
                if (string.IsNullOrWhiteSpace(currentMarketWatchName))
                {
                    currentMarketWatchName = PromptMarketWatchName();

                    if (string.IsNullOrWhiteSpace(currentMarketWatchName))
                        return;

                    // Check for duplicate names
                    var existingWatch = await marketWatchService.GetMarketWatchByNameAsync(currentMarketWatchName);
                    if (existingWatch != null)
                    {
                        MessageBox.Show("A MarketWatch with this name already exists. Please choose a different name.");
                        currentMarketWatchName = null;
                        return;
                    }
                }

                var marketWatchItem = new MarketWatchItem
                {
                    MarketWatchId = currentMarketWatchId ?? 0,
                    MarketWatchName = currentMarketWatchName,
                    Symbols = symbol
                };

                if (currentMarketWatchId != 0)
                {
                    var savedItem = await marketWatchService.UpdateMarketWatchAsync(marketWatchItem);


                    if (savedItem == null)
                    {
                        MessageBox.Show("Failed to save MarketWatch.");
                        return;
                    }

                    // ✅ Update state from server
                    currentMarketWatchId = savedItem?.MarketWatchId ?? 0;
                    currentMarketWatchName = savedItem?.MarketWatchName ?? currentMarketWatchName;

                }
                else
                {
                    var savedItem = await marketWatchService.SaveMarketWatchAsync(marketWatchItem);


                    if (savedItem == null)
                    {
                        MessageBox.Show("Failed to save MarketWatch.");
                        return;
                    }

                    // ✅ Update state from server
                    currentMarketWatchId = savedItem?.MarketWatchId ?? 0;
                    currentMarketWatchName = savedItem?.MarketWatchName ?? currentMarketWatchName;

                }

                thecalcify.CurrentInstance.titleLabel.Text =
                    $"Edit {currentMarketWatchName} MarketWatch";

                MessageBox.Show("MarketWatch saved successfully.");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                MessageBox.Show("Error while saving MarketWatch.");
            }
        }

        private List<string> GetSymbolsFromGrid()
        {
            return _grid.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow && r.Cells["symbol"].Value != null)
                .Select(r => r.Cells["symbol"].Value.ToString())
                .Distinct()
                .ToList();
        }

        private static string PromptMarketWatchName()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 150;
                prompt.Text = "Save MarketWatch";
                prompt.Icon = Resources.ApplicationIcon;
                //prompt.Location = Windows.Center

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "MarketWatch Name:" };
                TextBox inputBox = new TextBox() { Left = 20, Top = 45, Width = 340 };
                Button confirmation = new Button() { Text = "Save", Left = 260, Width = 100, Top = 75 };

                confirmation.Click += (sender, e) => { prompt.DialogResult = DialogResult.OK; };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK
                    ? inputBox.Text.Trim()
                    : null;
            }
        }


        // ===============================
        // EXTERNAL SYMBOL MANAGEMENT
        // ===============================

        public void AddSymbols(IEnumerable<(string Symbol, string Name)> symbols)
        {
            foreach (var info in symbols)
            {
                if (_symbolRowMap.ContainsKey(info.Symbol))
                    continue;

                EnsureOneEmptyRowAtEnd();

                int rowIndex = _grid.Rows.Count - 1;

                BindSymbolToRow(rowIndex, info.Symbol, info.Name);
            }
        }



        public void RemoveSymbols(IEnumerable<string> symbols)
        {
            foreach (var symbol in symbols)
            {
                if (!_symbolRowMap.TryGetValue(symbol, out int rowIndex))
                    continue;

                _grid.Rows.RemoveAt(rowIndex);
                _symbolRowMap.Remove(symbol);
            }

            // rebuild map after removals
            RebuildSymbolRowMap();


            // 🔥 Ensure user can add new symbol
            EnsureOneEmptyRowAtEnd();
        }

        private void RebuildSymbolRowMap()
        {
            _symbolRowMap.Clear();

            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                var sym = _grid.Rows[i].Cells["symbol"].Value?.ToString();
                if (!string.IsNullOrEmpty(sym))
                    _symbolRowMap[sym] = i;
            }
        }

        private void EnsureOneEmptyRowAtEnd()
        {
            if (_grid.Rows.Count == 0)
            {
                AddEmptyRow();
                return;
            }

            var lastRow = _grid.Rows[_grid.Rows.Count - 1];
            bool hasSymbol = lastRow.Cells["symbol"].Value != null;

            if (hasSymbol)
                AddEmptyRow();
        }

        private void ClearExistingSymbolFromRow(int rowIndex)
        {
            var oldSymbol = _grid.Rows[rowIndex].Cells["symbol"].Value?.ToString();
            if (!string.IsNullOrEmpty(oldSymbol))
                _symbolRowMap.Remove(oldSymbol);
        }

    }
}
