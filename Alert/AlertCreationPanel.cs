using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.Alert
{
    public partial class AlertCreationPanel : Form
    {
        #region Fields & Constants

        private Panel panelAddAlert;
        private TextBox txtValue;
        private ComboBox cmbColumn, cmbSymbol;
        private CheckBox chkStatusBar, chkPopup;
        private Button btnSaveAlert;

        public List<string> ColumnNames { get; private set; } = new List<string>();
        private List<Tuple<string, string>> SymbolName = new List<Tuple<string, string>>();
        public List<string> SymbolNames { get; private set; } = new List<string>();
        public string token;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        private readonly Color PrimaryColor = Color.FromArgb(0, 122, 204);
        private readonly Color SecondaryColor = Color.FromArgb(240, 240, 240);
        private readonly Color TextColor = Color.FromArgb(64, 64, 64);
        private readonly Font ModernFont = new Font("Microsoft Sans Serif", 10);
        private readonly Font ModernFontBold = new Font("Microsoft Sans Serif Semibold", 10);
        private string baseUrl = APIUrl.ProdUrl;

        #endregion

        #region Constructor & Initialization

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
            this.BackColor = Color.White;
            this.Font = ModernFont;
            this.ForeColor = TextColor;
            this.Padding = new Padding(20);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Alert Manager";
            this.MinimumSize = new Size(800, 500);

            ConfigureDataGridView();
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(230, 230, 230);
            dataGridView1.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = TextColor,
                SelectionBackColor = SecondaryColor,
                SelectionForeColor = TextColor,
                Font = ModernFont,
                Padding = new Padding(5)
            };
            dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
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
            dataGridView1.DataError += delegate (object s, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
            dataGridView1.CellClick += DataGridView1_CellClick;
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;
        }

        private void InitializeData()
        {
            try
            {
                var liveRate = thecalcify.CurrentInstance;
                SymbolNames = liveRate.SymbolName.Select(x => x.SymbolName).ToList();
                SymbolName = liveRate.SymbolName
                    .Select(x => Tuple.Create(x.Symbol, x.SymbolName))
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error initializing data: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
        }

        #endregion

        #region Event Handlers

        private async void BtnAddAlert_Click(object sender, EventArgs e)
        {
            await LoadAlertsAsync(false);
            ShowAlertPanel();
        }

        private async void BtnExportCsv_Click(object sender, EventArgs e)
        {
            var alerts = await FetchAlertsFromApiAsync();
            ExportAlertsToCsv(alerts);
        }

        private async void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                var idObj = dataGridView1.Rows[e.RowIndex].Cells["AlertId"].Value;
                if (idObj == null) return;

                int alertId = (int)idObj;
                var alerts = await FetchAlertsFromApiAsync();
                if (alerts == null) return;

                Notification alert = alerts.FirstOrDefault(a => a.Id == alertId);
                if (alert == null) return;

                ShowAlertPanel(new AlertInfo
                {
                    identifier = alert.identifier,
                    type = ConvertTypeCodeToLabel(alert.Type),
                    rate = alert.rate,
                    NotifyStatusBar = alert.flag != null && alert.flag.Contains("Status"),
                    NotifyPopup = alert.flag != null && alert.flag.Contains("Popup")
                }, e.RowIndex);
            }
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit")
            {
                e.Value = "✏️";
                e.FormattingApplied = true;
            }
        }

        #endregion

        #region Button Event Wrappers (for Designer Compatibility)

        private void BtnOpenAlert_Click(object sender, EventArgs e)
        {
            // restore open alert view
            ShowOpenAlertView();
        }

        private void BtnAlertHistory_Click(object sender, EventArgs e)
        {
            // switch to alert history (triggered alerts)
            ShowAlertHistoryView();
        }

        private void BtnAlert_Click(object sender, EventArgs e)
        {
            // handle "Add Alert" or "Export CSV" depending on button text
            if (btnAddAlert.Text == "Add New Alert")
            {
                BtnAddAlert_Click(sender, e);
            }
            else if (btnAddAlert.Text == "Export to CSV")
            {
                BtnExportCsv_Click(sender, e);
            }
        }

        #endregion

        #region Alert Views

        private async void ShowOpenAlertView()
        {
            btnAddAlert.Text = "Add New Alert";
            await LoadAlertsAsync(false);
        }

        private async void ShowAlertHistoryView()
        {
            btnAddAlert.Text = "Export to CSV";
            await LoadAlertsAsync(true);
        }

        #endregion

        #region API Calls

        private async Task<List<Notification>> FetchAlertsFromApiAsync()
        {
            try
            {
                HttpResponseMessage response = await SendApiRequestAsync("GetNotifications", HttpMethod.Get);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    NotificationResponse result = JsonSerializer.Deserialize<NotificationResponse>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result != null && result.Data != null)
                    {
                        foreach (Notification n in result.Data)
                        {
                            var match = SymbolName.FirstOrDefault(x => x.Item1 == n.identifier);
                            n.identifier = match != null ? match.Item2 : "Unknown";
                        }
                        return result.Data;
                    }
                }
                else
                {
                    ShowErrorDialog("API Error: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error fetching alerts: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
            return new List<Notification>();
        }

        private async Task<bool> SendAlertToApiAsync(AlertInfo alert, AlertInfo existingAlert)
        {
            try
            {
                string selectedSymbolName = cmbSymbol.SelectedItem.ToString();
                alert.identifier = SymbolName.FirstOrDefault(x => x.Item2 == selectedSymbolName)?.Item1 ?? "";

                decimal parsedRate;
                if (!decimal.TryParse(txtValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedRate))
                {
                    ShowErrorDialog("Invalid rate value.");
                    return false;
                }
                alert.rate = parsedRate;

                var payload = new
                {
                    alert.id,
                    alert.identifier,
                    type = ConvertLabelToTypeCode(cmbColumn.SelectedItem.ToString()),
                    rate = alert.rate,
                    flag = (alert.NotifyPopup && alert.NotifyStatusBar) ? "Popup,Status" :
                           alert.NotifyPopup ? "Popup" :
                           alert.NotifyStatusBar ? "Status" : string.Empty,
                    alert.condition
                };

                HttpResponseMessage response = await SendApiRequestAsync("AlertNotification", HttpMethod.Post, payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ShowErrorDialog("API call failed: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
            return false;
        }

        private async Task<HttpResponseMessage> SendApiRequestAsync(string endpoint, HttpMethod method, object payload = null)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                if (method == HttpMethod.Get)
                    return await client.GetAsync(baseUrl + endpoint);

                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await client.PostAsync(baseUrl + endpoint, content);
            }
        }

        #endregion

        #region Alert Management

        private async void SaveAlert(AlertInfo alert, int? rowIndex)
        {
            try
            {
                if (!ValidateAlertInputs()) return;

                AlertInfo newAlert = CreateAlertFromInputs();
                UpdateDataGridView(newAlert, rowIndex);

                bool success = await SendAlertToApiAsync(newAlert, alert);
                if (success)
                    ShowSuccessDialog("Alert saved and synced.");
                else
                    ShowErrorDialog("Failed to save alert.");

                panelAddAlert.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error saving alert: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
        }

        private bool ValidateAlertInputs()
        {
            if (cmbSymbol.SelectedItem == null ||
                cmbColumn.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtValue.Text))
            {
                ShowWarningDialog("Please complete all fields.");
                return false;
            }

            double test;
            if (!double.TryParse(txtValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out test))
            {
                ShowWarningDialog("Invalid numeric target value.");
                return false;
            }

            return true;
        }

        private AlertInfo CreateAlertFromInputs()
        {
            string symbol = cmbSymbol.SelectedItem.ToString();
            string column = cmbColumn.SelectedItem.ToString();
            int columnIndex = cmbColumn.SelectedIndex;
            string currentValue = "200"; // stub for real value

            decimal currentVal, targetVal;
            if (!decimal.TryParse(currentValue, NumberStyles.Any, CultureInfo.InvariantCulture, out currentVal) ||
                !decimal.TryParse(txtValue.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out targetVal))
                throw new InvalidOperationException("Invalid target or market value.");

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

        #endregion

        #region Data Binding & UI

        private async Task LoadAlertsAsync(bool triggered)
        {
            try
            {
                dataGridView1.Rows.Clear();
                var alerts = await FetchAlertsFromApiAsync();
                BindAlertsToGrid(alerts, triggered);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error loading alerts: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
        }

        private void BindAlertsToGrid(List<Notification> alerts, bool triggered)
        {
            dataGridView1.Columns["Edit"].Visible = !triggered;
            dataGridView1.Columns["TriggerTime"].Visible = triggered;

            foreach (var alert in alerts.Where(a => a.IsPassed == triggered))
            {
                dataGridView1.Rows.Add(
                    alert.identifier,
                    ConvertTypeCodeToLabel(alert.Type),
                    alert.rate,
                    FormatDateToIST(alert.CreateDate),
                    FormatDateToIST(alert.AlertDate),
                    triggered ? string.Empty : "✏️",
                    alert.Id
                );
            }
        }

        private void UpdateDataGridView(AlertInfo alert, int? rowIndex)
        {
            string typeLabel = ConvertTypeCodeToLabel(alert.type);

            if (rowIndex.HasValue)
            {
                var row = dataGridView1.Rows[rowIndex.Value];
                row.Cells["Symbol"].Value = alert.identifier;
                row.Cells["Column"].Value = typeLabel;
                row.Cells["Rate"].Value = alert.rate;
                row.Cells["CreationTime"].Value = alert.createDate.ToString(DateTimeFormat);
            }
            else
            {
                dataGridView1.Rows.Add(alert.identifier, typeLabel, alert.rate,
                    alert.createDate.ToString(DateTimeFormat), "", "✏️", alert.id);
            }
        }

        private void ShowAlertPanel(AlertInfo alert = null, int? rowIndex = null)
        {
            panelAddAlert = new Panel
            {
                Size = new Size(500, 310),
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelAddAlert.Location = new Point(
                (ClientSize.Width - panelAddAlert.Width) / 2,
                (ClientSize.Height - panelAddAlert.Height) / 2);

            Label title = new Label
            {
                Text = alert == null ? "Add New Alert" : "Edit Alert",
                Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            panelAddAlert.Controls.Add(title);

            Label lblSymbol = new Label { Text = "Symbol:", Location = new Point(20, 60), AutoSize = true };
            cmbSymbol = new ComboBox { Location = new Point(130, 55), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSymbol.Items.AddRange(SymbolNames.ToArray());
            panelAddAlert.Controls.Add(lblSymbol);
            panelAddAlert.Controls.Add(cmbSymbol);

            Label lblColumn = new Label { Text = "Column:", Location = new Point(20, 100), AutoSize = true };
            cmbColumn = new ComboBox { Location = new Point(130, 95), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbColumn.Items.AddRange(new[] { "Bid", "Ask", "LTP" });
            panelAddAlert.Controls.Add(lblColumn);
            panelAddAlert.Controls.Add(cmbColumn);

            Label lblValue = new Label { Text = "Target Value:", Location = new Point(20, 140), AutoSize = true };
            txtValue = new TextBox { Location = new Point(130, 135), Size = new Size(200, 25) };
            panelAddAlert.Controls.Add(lblValue);
            panelAddAlert.Controls.Add(txtValue);

            chkStatusBar = new CheckBox { Text = "Status Bar Notification", Location = new Point(120, 175), AutoSize = true };
            chkPopup = new CheckBox { Text = "Popup Notification", Location = new Point(120, 205), AutoSize = true, Checked = true };
            panelAddAlert.Controls.Add(chkStatusBar);
            panelAddAlert.Controls.Add(chkPopup);

            btnSaveAlert = new Button { Text = "Save", Size = new Size(100, 30), Location = new Point(panelAddAlert.Width - 120, panelAddAlert.Height - 40) };
            btnSaveAlert.Click += delegate { SaveAlert(alert, rowIndex); };
            panelAddAlert.Controls.Add(btnSaveAlert);

            Button btnClose = new Button { Text = "Close", Size = new Size(100, 30), Location = new Point(btnSaveAlert.Left - 110, btnSaveAlert.Top) };
            btnClose.Click += delegate { panelAddAlert.Hide(); };
            panelAddAlert.Controls.Add(btnClose);

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
                cmbColumn.SelectedIndex = 0;
            }

            Controls.Add(panelAddAlert);
            panelAddAlert.BringToFront();
            panelAddAlert.Visible = true;
        }

        #endregion

        #region CSV Export

        private void ExportAlertsToCsv(List<Notification> alerts)
        {
            try
            {
                alerts = alerts.Where(a => a.IsPassed).ToList();
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = "AlertHistory_" + DateTime.Now.ToString("ddMMyyyyHHmmssff") + ".csv",
                    Title = "Export Alert History"
                };

                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var sw = new StreamWriter(sfd.FileName))
                {
                    sw.WriteLine("Symbol,Column,Condition,Rate,Created,Triggered,Type");

                    foreach (var alert in alerts)
                    {
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                            alert.identifier,
                            ConvertTypeCodeToLabel(alert.Type),
                            alert.Condition,
                            alert.rate,
                            FormatDateToIST(alert.CreateDate),
                            FormatDateToIST(alert.AlertDate),
                            alert.flag));
                    }
                }

                ShowSuccessDialog("Exported successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error exporting CSV: " + ex.Message);
                ApplicationLogger.LogException(ex);
            }
        }

        #endregion

        #region Helpers

        private string ConvertTypeCodeToLabel(string code)
        {
            switch (code)
            {
                case "0": return "Bid";
                case "1": return "Ask";
                case "2": return "LTP";
                default: return "Unknown";
            }
        }

        private string ConvertLabelToTypeCode(string label)
        {
            switch (label)
            {
                case "Bid": return "0";
                case "Ask": return "1";
                case "LTP": return "2";
                default: return "0";
            }
        }

        private string FormatDateToIST(DateTime? utcDate)
        {
            if (utcDate == null) return string.Empty;
            try
            {
                TimeZoneInfo ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDate.Value, ist).ToString(DateTimeFormat);
            }
            catch { return utcDate.Value.ToString(DateTimeFormat); }
        }

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

        #region Models

        public class Notification
        {
            public int Id { get; set; }
            public string identifier { get; set; }
            public int ClientId { get; set; }
            public decimal rate { get; set; }
            public string flag { get; set; }
            public bool IsPassed { get; set; }
            public DateTime? AlertDate { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime MDate { get; set; }
            public string Condition { get; set; }
            public string Type { get; set; }
        }

        public class NotificationResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<Notification> Data { get; set; }
        }

        public class AlertInfo
        {
            public int id { get; set; }
            public string identifier { get; set; }
            public string type { get; set; }
            public decimal rate { get; set; }
            public string condition { get; set; }
            public DateTime createDate { get; set; }
            public DateTime? TriggerTime { get; set; }
            public bool NotifyStatusBar { get; set; }
            public bool NotifyPopup { get; set; }
        }

        #endregion
    }
}
