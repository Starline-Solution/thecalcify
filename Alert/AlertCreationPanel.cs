using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;
using thecalcify.MarketWatch;
using System.IO;

namespace thecalcify.Alert
{
    public partial class AlertCreationPanel : Form
    {
        // UI Components
        private Panel panelAddAlert;
        private TextBox txtValue;
        private ComboBox cmbColumn, cmbSymbol;
        private CheckBox chkStatusBar, chkPopup;
        private Button btnSaveAlert;

        // Data
        public List<string> ColumnNames { get; private set; } = new List<string>();
        private List<(string Symbol, string SymbolName)> SymbolName = new List<(string Symbol, string SymbolName)>();
        public List<string> SymbolNames { get; private set; } = new List<string>();
        public string token;

        // Modern UI Constants
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        private readonly Color PrimaryColor = Color.FromArgb(0, 122, 204);
        private readonly Color SecondaryColor = Color.FromArgb(240, 240, 240);
        private readonly Color TextColor = Color.FromArgb(64, 64, 64);
        private readonly Font ModernFont = new Font("Microsoft Sans Serif", 10);
        private readonly Font ModernFontBold = new Font("Microsoft Sans Serif Semibold", 10);

        public AlertCreationPanel(string token)
        {
            InitializeComponent();
            InitializeModernUI();
            InitializeData();
            this.token = token;
            ShowOpenAlertView();
        }

        private void InitializeModernUI()
        {
            // Form Styling
            this.BackColor = Color.White;
            this.Font = ModernFont;
            this.ForeColor = TextColor;
            this.Padding = new Padding(20);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Alert Manager";
            this.MinimumSize = new Size(800, 500);

            // ToolStrip Styling
            toolStrip.BackColor = Color.White;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            //toolStrip.Renderer = new ModernToolStripRenderer();
            toolStrip.AutoSize = false;
            toolStrip.Height = 60;
            toolStrip.Padding = new Padding(10, 0, 10, 0);

            // DataGridView Styling
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(230, 230, 230);
            dataGridView1.DefaultCellStyle = new DataGridViewCellStyle()
            {
                BackColor = Color.White,
                ForeColor = TextColor,
                SelectionBackColor = SecondaryColor,
                SelectionForeColor = TextColor,
                Font = ModernFont,
                Padding = new Padding(5)
            };
            dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle()
            {
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                Font = ModernFontBold,
                Padding = new Padding(5, 8, 5, 8),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // Buttons Styling
            btnAddAlert.FlatStyle = FlatStyle.Flat;
            btnAddAlert.FlatAppearance.BorderSize = 0;
            btnAddAlert.BackColor = PrimaryColor;
            btnAddAlert.ForeColor = Color.White;
            btnAddAlert.Font = ModernFontBold;
            btnAddAlert.Cursor = Cursors.Hand;
            dataGridView1.DataError += (s, e) => e.ThrowException = false;
            dataGridView1.CellClick += DataGridView1_CellClick;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void InitializeData()
        {
            try
            {
                var liveRate = thecalcify.CurrentInstance;
                SymbolNames = liveRate.SymbolName.Select(x => x.SymbolName).ToList();
                SymbolName = liveRate.SymbolName;

            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Error initializing data: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }
        }

        #region Modern UI Components

        private void ShowAlertPanel(AlertInfo alert = null, int? rowIndex = null)
        {
            // Create main panel
            panelAddAlert = new Panel
            {
                Size = new Size(500, 310), // Reduced height due to removed message box
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelAddAlert.Location = new Point(
                (this.ClientSize.Width - panelAddAlert.Width) / 2,
                (this.ClientSize.Height - panelAddAlert.Height) / 2
            );

            // Title label
            Label titleLabel = new Label
            {
                Text = alert == null ? "Add New Alert" : "Edit Alert",
                Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            panelAddAlert.Controls.Add(titleLabel);


            // Symbol
            Label lblSymbol = new Label { Text = "Symbol:", Location = new Point(20, 60), AutoSize = true };
            cmbSymbol = new ComboBox { Location = new Point(130, 55), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSymbol.Items.AddRange(SymbolNames.ToArray());
            panelAddAlert.Controls.Add(lblSymbol);
            panelAddAlert.Controls.Add(cmbSymbol);

            // Column
            Label lblColumn = new Label { Text = "Column:", Location = new Point(20, 100), AutoSize = true };
            cmbColumn = new ComboBox { Location = new Point(130, 95), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbColumn.Items.AddRange(new[] { "Bid", "Ask", "LTP" });
            panelAddAlert.Controls.Add(lblColumn);
            panelAddAlert.Controls.Add(cmbColumn);

            // Target Value
            Label lblValue = new Label { Text = "Target Value:", Location = new Point(20, 140), AutoSize = true };
            txtValue = new TextBox { Location = new Point(130, 135), Size = new Size(200, 25) };
            panelAddAlert.Controls.Add(lblValue);
            panelAddAlert.Controls.Add(txtValue);

            // Checkboxes
            chkStatusBar = new CheckBox { Text = "Status Bar Notification", Location = new Point(120, 175), AutoSize = true };
            chkPopup = new CheckBox { Text = "Popup Notification", Location = new Point(120, 205), AutoSize = true, Checked = true };
            panelAddAlert.Controls.Add(chkStatusBar);
            panelAddAlert.Controls.Add(chkPopup);

            // Save button
            btnSaveAlert = new Button
            {
                Text = "Save",
                Size = new Size(100, 30),
                Location = new Point(panelAddAlert.Width - 120, panelAddAlert.Height - 40)
            };
            btnSaveAlert.Click += (s, e) => SaveAlert(alert, rowIndex);
            panelAddAlert.Controls.Add(btnSaveAlert);

            // Close button
            Button btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 30),
                Location = new Point(btnSaveAlert.Left - 110, btnSaveAlert.Top)
            };
            btnClose.Click += (s, e) => panelAddAlert.Hide();
            panelAddAlert.Controls.Add(btnClose);

            // Pre-fill values if editing existing alert
            if (alert != null)
            {
                txtValue.Text = alert.rate.ToString();
                cmbSymbol.SelectedItem = alert.identifier;
                cmbColumn.SelectedItem = alert.type;
                chkStatusBar.Checked = alert.NotifyStatusBar;
                chkPopup.Checked = alert.NotifyPopup;
            }
            else if (cmbSymbol.Items.Count > 0)
            {
                cmbSymbol.SelectedIndex = 0;
                if (cmbColumn.Items.Count > 0) cmbColumn.SelectedIndex = 0;
            }

            // Add to form and show
            this.Controls.Add(panelAddAlert);
            panelAddAlert.BringToFront();
            panelAddAlert.Visible = true;
        }

        #endregion

        #region Event Handlers
        private void BtnOpenAlert_Click(object sender, EventArgs e)
        {
            btnAddAlert.Text = "Add New Alert";
            ShowOpenAlertView();
        }

        private void BtnAlertHistory_Click(object sender, EventArgs e)
        {
            btnAddAlert.Text = "Export to CSV";
            ShowAlertHistoryView();
        }

        private void BtnAlert_Click(object sender, EventArgs e)
        {
            if (btnAddAlert.Text == "Add New Alert")
            {
                BtnAddAlert_Click();
            }
            else if (btnAddAlert.Text == "Export to CSV")
            {
                BtnExportCsv_Click();
            }
        }

        private async void BtnAddAlert_Click()
        {
            await LoadAlertsAsync(triggered: false);
            ShowAlertPanel();
        }

        private async void BtnExportCsv_Click()
        {
            //var alerts = ; // ← API call to get all alerts
            ExportAlertsToCsv(await FetchAlertsFromApiAsync());
        }

        private async void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                var symbol = dataGridView1.Rows[e.RowIndex].Cells["Symbol"].Value?.ToString();
                var column = dataGridView1.Rows[e.RowIndex].Cells["Column"].Value?.ToString();

                var alerts = await FetchAlertsFromApiAsync(); // ✅ Properly awaited
                if (alerts == null) return;


                var alert = alerts.FirstOrDefault(a => a.identifier == symbol && GetTypeLabel(a.Type) == column);

                if (alert != null)
                {
                    ShowAlertPanel(new AlertInfo
                    {
                        identifier = alert.identifier,
                        type = GetTypeLabel(alert.Type),
                        rate = alert.rate,
                        NotifyStatusBar = alert.flag?.Contains("Status") ?? false,
                        NotifyPopup = alert.flag?.Contains("Popup") ?? false,
                    }, e.RowIndex);
                }
            }
        }

        private string GetTypeLabel(string type)
        {
            switch (type)
            {
                case "0": return "Bid";
                case "1": return "Ask";
                case "2": return "LTP";
                default: return "Unknown";
            }
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit") // your image column name
            {
                if (e.Value is string path && !string.IsNullOrEmpty(path))
                {
                    try
                    {
                        e.Value = "✏️";
                        e.FormattingApplied = true;
                    }
                    catch
                    {
                        e.Value = null; // or a default image
                    }
                }
            }
        }
        #endregion

        #region View Management

        private async void ShowOpenAlertView()
        {
            await LoadAlertsAsync(triggered: false);
        }

        private async void ShowAlertHistoryView()
        {
            await LoadAlertsAsync(triggered: true);
        }

        private async Task LoadAlertsAsync(bool triggered)
        {
            try
            {
                dataGridView1.Rows.Clear();

                var alerts = await FetchAlertsFromApiAsync(); // ← API call to get all alerts
                if (alerts == null) return;

                dataGridView1.Columns["Edit"].Visible = !triggered;
                dataGridView1.Columns["TriggerTime"].Visible = triggered;

                foreach (var alert in alerts.Where(a => a.IsPassed == triggered))
                {
                    string typeLabel;
                    switch (alert.Type)
                    {
                        case "0":
                            typeLabel = "Bid";
                            break;
                        case "1":
                            typeLabel = "Ask";
                            break;
                        case "2":
                            typeLabel = "LTP";
                            break;
                        default:
                            typeLabel = "Unknown";
                            break;
                    }

                    TimeZoneInfo indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");


                    dataGridView1.Rows.Add(
                    alert.identifier,
                    typeLabel,
                    alert.rate,
                    alert.CreateDate != null ? TimeZoneInfo.ConvertTimeFromUtc(alert.CreateDate, indianZone).ToString(DateTimeFormat) : string.Empty,
                    alert.AlertDate != null ? TimeZoneInfo.ConvertTimeFromUtc(alert.AlertDate.Value, indianZone).ToString(DateTimeFormat) : string.Empty,
                    triggered ? string.Empty : "✏️"
                  );
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Error loading alerts: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }
        }


        #endregion

        #region Alert Management

        private async void SaveAlert(AlertInfo alert, int? rowIndex)
        {
            try
            {

                if (!ValidateAlertInputs())
                    return;

                var newAlert = CreateAlertFromInputs();


                UpdateDataGridView(newAlert, rowIndex);


                // ✅ Call API instead
                bool success = await SendAlertToApiAsync(newAlert, alert);

                if (success)
                {
                    ShowSuccessDialog("Alert saved and synced to server.");
                    panelAddAlert.Visible = false;
                }
                else
                {
                    ShowErrorDialog("Alert was not saved to server.");
                }

                panelAddAlert.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Error saving alert: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }
        }

        private async Task<bool> SendAlertToApiAsync(AlertInfo alert, AlertInfo existingAlert = null)
        {
            try
            {
                var existingAlerts = await FetchAlertsFromApiAsync(); // ✅ Properly awaited
                Notification existingalert = null;

                try
                {
                    if (!string.IsNullOrEmpty(txtValue.Text) &&
                                decimal.TryParse(txtValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var rateValue))
                    {
                        alert.rate = rateValue;
                    }
                    else
                    {
                        MessageBox.Show("Invalid value entered for Target Value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing rate value at SendAlertToApiAsync: " + ex.Message);
                }

                if (existingAlert != null)
                {
                    if (existingAlerts.Count is 0 || existingAlerts is null)
                    {
                        return false;
                    }

                    existingalert = existingAlerts.FirstOrDefault(a => a.identifier == existingAlert.identifier && GetTypeLabel(a.Type) == existingAlert.type);

                }

                if (existingalert != null)
                {
                    alert.id = existingalert.Id;
                }

                string selectedSymbolName = cmbSymbol.SelectedItem.ToString();
                alert.identifier = SymbolName.FirstOrDefault(x => x.SymbolName == selectedSymbolName).Symbol;


                decimal _RateValue = 0;

                try
                {
                    if (decimal.TryParse(alert.rate.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedRate))
                    {
                        _RateValue = parsedRate;
                    }
                    else
                    {
                        // fallback or logging
                        _RateValue = 0; // or whatever default you want
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing rate value at SendAlertToApiAsync1: " + ex.Message);
                }

                var payload = new
                {
                    alert.id,
                    alert.identifier,
                    alert.type,
                    rate = _RateValue,
                    flag = (alert.NotifyPopup && alert.NotifyStatusBar) ? "Popup,Status" :
                           alert.NotifyPopup ? "Popup" :
                           alert.NotifyStatusBar ? "Status" : string.Empty,
                    alert.condition,
                };

                string apiUrl = $"{ConfigurationManager.AppSettings["thecalcify"]}AlertNotification";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $"{token}");

                    string json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                        return true;

                    string error = await response.Content.ReadAsStringAsync();
                    ShowErrorDialog($"API Error: {response.StatusCode}\n{error}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("API call failed: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }

            return false;
        }

        private async Task<List<Notification>> FetchAlertsFromApiAsync()
        {
            try
            {
                string apiUrl = $"{ConfigurationManager.AppSettings["thecalcify"]}GetNotifications";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}"); // Use your stored token

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<NotificationResponse>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (result?.Data != null)
                        {
                            foreach (var notification in result.Data)
                            {
                                var match = SymbolName.FirstOrDefault(x => x.Symbol == notification.identifier).SymbolName;
                                notification.identifier = match != null ? match : "Unknown";
                            }

                            return result.Data;
                        }

                        return result?.Data;
                    }
                    else
                    {
                        ShowErrorDialog($"API Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"API call failed: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }

            return null;
        }

        private bool ValidateAlertInputs()
        {
            if (string.IsNullOrWhiteSpace(cmbSymbol.SelectedItem?.ToString()) ||
                string.IsNullOrWhiteSpace(cmbColumn.SelectedItem?.ToString()) ||
                string.IsNullOrWhiteSpace(txtValue.Text))
            {
                ShowWarningDialog("Please complete all fields.");
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(txtValue.Text) ||
                        !double.TryParse(txtValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    ShowWarningDialog("Please enter a valid numeric value for the target.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing rate value at ValidateAlertInputs: " + ex.Message);
            }

            return true;
        }

        private AlertInfo CreateAlertFromInputs()
        {
            string symbol = cmbSymbol.SelectedItem.ToString();
            string column = cmbColumn.SelectedItem.ToString();
            int columnIndex = cmbColumn.SelectedIndex;
            string currentValue = GetMarketValue(symbol, column);

            if (string.IsNullOrWhiteSpace(currentValue) ||
             string.IsNullOrWhiteSpace(txtValue.Text) ||
             !decimal.TryParse(currentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal currentVal) ||
             !decimal.TryParse(txtValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal targetVal))
            {
                throw new InvalidOperationException("Can't match your target value with previous value. Please check.");
            }

            string condition = currentVal > targetVal ? "<=" : ">=";

            return new AlertInfo
            {
                identifier = symbol,
                type = columnIndex.ToString(),
                rate = targetVal,
                condition = condition,
                createDate = DateTime.Now,
                NotifyStatusBar = chkStatusBar.Checked,
                NotifyPopup = chkPopup.Checked
            };
        }

        private static string GetMarketValue(string symbol, string column)
        {
            if (!string.IsNullOrWhiteSpace(symbol) && !string.IsNullOrEmpty(column))
            { return "200"; }

            return "N/A";
        }

        private void UpdateDataGridView(AlertInfo alert, int? rowIndex)
        {

            string typeLabel;
            switch (alert.type)
            {
                case "0":
                    typeLabel = "Bid";
                    break;
                case "1":
                    typeLabel = "Ask";
                    break;
                case "2":
                    typeLabel = "LTP";
                    break;
                default:
                    typeLabel = "Unknown";
                    break;
            }

            if (rowIndex.HasValue)
            {
                var row = dataGridView1.Rows[rowIndex.Value];
                row.Cells["Symbol"].Value = alert.identifier;
                row.Cells["Column"].Value = typeLabel;
                row.Cells["CreationTime"].Value = alert.createDate.ToString(DateTimeFormat);
                row.Cells["TriggerTime"].Value = alert.TriggerTime?.ToString(DateTimeFormat) ?? string.Empty;
                row.Cells["Edit"].Value = "✏️";
            }
            else
            {
                dataGridView1.Rows.Add(
                    alert.identifier,
                    typeLabel,
                    alert.rate,
                    alert.createDate.ToString(DateTimeFormat),
                    alert.TriggerTime?.ToString(DateTimeFormat) ?? string.Empty,
                    "✏️"
                );
            }
        }

        #endregion

        #region CSV Export

        private void ExportAlertsToCsv(List<Notification> alerts)
        {
            try
            {
                alerts = alerts.Where(a => a.IsPassed).ToList();

                using (var sfd = new SaveFileDialog()
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"AlertHistory_{DateTime.Now:ddMMyyyyHHmmssff}.csv",
                    Title = "Export Alert History"
                })
                {
                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;

                    using (var sw = new System.IO.StreamWriter(sfd.FileName))
                    {
                        // Write header
                        sw.WriteLine("Alert Symbol,Alert Column,Alert Condition,Target Rate,Alert Created At,Alert Trigger At,Alert Type");

                        // Time zone setup
                        TimeZoneInfo indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                        foreach (var alert in alerts)
                        {
                            // Convert type from string to label
                            string typeLabel = GetTypeLabel(alert.Type);

                            string condition =
                                   alert.Condition == "<=" ? "Alert Hit On Less Then Equal To" :
                                   alert.Condition == ">=" ? "Alert Hit On Greater Then Equal To" :
                                   string.Empty;

                            // Convert UTC times to IST and format
                            string creationTime = TimeZoneInfo.ConvertTimeFromUtc(alert.CreateDate, indianZone).ToString(DateTimeFormat);
                            string triggerTime = alert.AlertDate.HasValue
                                ? TimeZoneInfo.ConvertTimeFromUtc(alert.AlertDate.Value, indianZone).ToString(DateTimeFormat)
                                : string.Empty;


                            // Write CSV line
                            sw.WriteLine(
                                $"\"{alert.identifier}\"," +
                                $"\"{typeLabel}\"," +
                                $"\"{condition}\"," +
                                $"\"{alert.rate}\"," +
                                $"\"{creationTime}\"," +
                                $"\"{triggerTime}\"," +
                                $"\"{alert.flag}\""

                                );
                        }
                    }

                    File.OpenRead(sfd.FileName).Close(); // Optional: force file flush
                    ShowSuccessDialog($"Exported {alerts.Count} alerts successfully.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Error exporting alerts: {ex.Message}");
                ApplicationLogger.LogException(ex);
            }
        }


        #endregion

        #region Modern Dialogs

        private void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarningDialog(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowSuccessDialog(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        public class Notification
        {
            public int Id { get; set; }

#pragma warning disable IDE1006 // Naming Styles
            public string identifier { get; set; }

#pragma warning restore IDE1006 // Naming Styles
            public int ClientId { get; set; }

#pragma warning disable IDE1006 // Naming Styles
            public decimal rate { get; set; }

#pragma warning restore IDE1006 // Naming Styles
#pragma warning disable IDE1006 // Naming Styles

            public string flag { get; set; }

#pragma warning restore IDE1006 // Naming Styles
            public bool IsPassed { get; set; }
            public DateTime? AlertDate { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime MDate { get; set; }           // New
            public string Condition { get; set; }         // New
            public string Type { get; set; }              // New
        }


        public class NotificationResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<Notification> Data { get; set; }
        }

    }
}
