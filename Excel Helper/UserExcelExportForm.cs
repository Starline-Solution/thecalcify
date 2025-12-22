using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.Excel_Helper
{
    public partial class UserExcelExportForm : UserControl
    {
        public Dictionary<string, CellDto> Cells { get; set; }
        public SheetJsonDto SheetJSON { get; set; }
        private static readonly HttpClient _httpClient = new HttpClient();
        public static readonly string ApiBaseUrl = APIUrl.ProdUrl;
        public readonly string _token;

        #region  Form Methods
        public UserExcelExportForm(string _token)
        {
            InitializeComponent();
            this._token = _token;
        }

        private async void UserExcelExportForm_Load(object sender, EventArgs e)
        {
            ConfigureGridColumns();
            await ReloadExcelSheetGridAsync();
        }

        private async void excelSheetGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = excelSheetGrid;

            // Ignore header clicks
            if (e.RowIndex < 0)
                return;

            // Get SheetId (hidden column)
            int sheetId = Convert.ToInt32(
                excelSheetGrid.Rows[e.RowIndex].Cells["sheetID"].Value
            );

            // Get SheetName
            string sheetName = excelSheetGrid
                .Rows[e.RowIndex]
                .Cells["SheetName"]
                .Value
                ?.ToString();

            string type = excelSheetGrid
                .Rows[e.RowIndex]
                .Cells["type"]
                .Value
                ?.ToString();

            grid.Enabled = false;

            // Save button clicked
            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "SaveSheet")
            {
                await SheetSyncAsync(sheetName, type);
                await ReloadExcelSheetGridAsync();
            }

            // Delete button clicked
            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "DeleteSheet" && !IsDeleteDisabled(e.RowIndex))
            {
                var result = MessageBox.Show($"we are not deleting Sheet just Sync Data", "Delete Sync Sheet", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    await DeleteSheetAsync(sheetId);

                    MessageBox.Show($"{sheetName} Sheet Deleted");

                    await ReloadExcelSheetGridAsync();
                }
            }

            grid.Enabled = true;
        }

        private void excelSheetGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            // SAVE button styling (unchanged)
            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "SaveSheet")
            {
                e.CellStyle.BackColor = Color.FromArgb(46, 204, 113);
                e.CellStyle.SelectionBackColor = Color.FromArgb(46, 204, 113);
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);
            }

            // DELETE button logic
            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "DeleteSheet")
            {
                string sheetName = excelSheetGrid.Rows[e.RowIndex]
                    .Cells["SheetName"].Value?.ToString();

                string type = excelSheetGrid.Rows[e.RowIndex]
                    .Cells["type"].Value?.ToString();

                bool disableDelete =
                    string.Equals(sheetName, "Cost.Cal", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(type, "json", StringComparison.OrdinalIgnoreCase);

                if (disableDelete)
                {
                    // 🔒 Disabled look
                    e.CellStyle.BackColor = Color.LightGray;
                    e.CellStyle.SelectionBackColor = Color.LightGray;
                    e.CellStyle.ForeColor = Color.DarkGray;
                    e.CellStyle.SelectionForeColor = Color.DarkGray;
                    e.CellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular);
                }
                else
                {
                    // 🔴 Enabled look
                    e.CellStyle.BackColor = Color.FromArgb(231, 76, 60);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(231, 76, 60);
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
        }

        private void excelSheetGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (excelSheetGrid.Columns[e.ColumnIndex].Name != "SheetName")
                return;

            e.Handled = true;

            // Paint default background & selection
            e.PaintBackground(e.CellBounds, true);

            string sheetName = e.Value != null ? e.Value.ToString() : string.Empty;

            string type = excelSheetGrid.Rows[e.RowIndex]
                .Cells["type"].Value != null
                    ? excelSheetGrid.Rows[e.RowIndex].Cells["type"].Value.ToString()
                    : string.Empty;

            Image icon;

            if (string.Equals(type, "json", StringComparison.OrdinalIgnoreCase))
                icon = Properties.Resources.AppIcon;   // your app icon
            else
                icon = Properties.Resources.excel_icon; // excel icon

            int iconSize = 16;
            int padding = 6;

            System.Drawing.Rectangle iconRect = new System.Drawing.Rectangle(
                e.CellBounds.Left + padding,
                e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2,
                iconSize,
                iconSize
            );

            System.Drawing.Rectangle textRect = new System.Drawing.Rectangle(
                iconRect.Right + padding,
                e.CellBounds.Top,
                e.CellBounds.Width - iconSize - padding * 3,
                e.CellBounds.Height
            );

            // Draw icon
            e.Graphics.DrawImage(icon, iconRect);

            // Draw text
            TextRenderer.DrawText(
                e.Graphics,
                sheetName,
                e.CellStyle.Font,
                textRect,
                e.CellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );

            // Draw focus rectangle when selected
            if ((e.State & DataGridViewElementStates.Selected) != 0)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, e.CellBounds);
            }

            excelSheetGrid.Columns["SheetName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        }

        private void excelSheetGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "SheetName")
            {
                string type = excelSheetGrid.Rows[e.RowIndex]
                    .Cells["type"].Value != null
                        ? excelSheetGrid.Rows[e.RowIndex].Cells["type"].Value.ToString()
                        : "";

                excelSheetGrid.Rows[e.RowIndex]
                    .Cells[e.ColumnIndex]
                    .ToolTipText = type.ToUpper();
            }
        }

        private bool IsDeleteDisabled(int rowIndex)
        {
            if (rowIndex < 0)
                return false;

            string sheetName = excelSheetGrid.Rows[rowIndex]
                .Cells["SheetName"].Value?.ToString();

            string type = excelSheetGrid.Rows[rowIndex]
                .Cells["type"].Value?.ToString();

            return
                string.Equals(sheetName, "Cost.Cal", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(type, "json", StringComparison.OrdinalIgnoreCase);
        }

        private void ConfigureGridColumns()
        {
            // Sheet Name (main expandable column)
            excelSheetGrid.Columns["SheetName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            excelSheetGrid.Columns["SheetName"].FillWeight = 100;

            // Uploaded (checkbox)
            excelSheetGrid.Columns["sheetUploaded"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            excelSheetGrid.Columns["sheetUploaded"].Width = 90;

            // Sync Sheet (button)
            excelSheetGrid.Columns["SaveSheet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            excelSheetGrid.Columns["SaveSheet"].Width = 120;

            // Delete Sheet (button)
            excelSheetGrid.Columns["DeleteSheet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            excelSheetGrid.Columns["DeleteSheet"].Width = 120;

            // Modified Date
            excelSheetGrid.Columns["ModifiedDate"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            excelSheetGrid.Columns["sheetUploaded"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            excelSheetGrid.Columns["SaveSheet"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            excelSheetGrid.Columns["DeleteSheet"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataGridViewColumn col in excelSheetGrid.Columns)
            {
                col.Resizable = DataGridViewTriState.False;
            }
        }

        private async Task ReloadExcelSheetGridAsync()
        { 
            if(!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "thecalcify.xlsx")))
            {
                MessageBox.Show("Please make sure thecalcify.xlsx is present on Desktop","Excel Not found",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }

            excelSheetGrid.Rows.Clear();
            excelSheetGrid.AutoGenerateColumns = false;

            List<SheetWrapperDto> sheetWrapperDtos = await GetSheetListAsync(_token);
            (List<string> sheetNames, string ModifiedDate) = exceldataBinder();

            if (sheetWrapperDtos == null)
                return;


            // Load synced sheets
            foreach (var sheet in sheetWrapperDtos)
            {
                int editedCellCount = sheet.Data?.EditedCells?.Count ?? 0;

                excelSheetGrid.Rows.Add(
                    sheet.SheetId,
                    sheet.Type,
                    Regex.Replace(sheet.SheetName, @"\.json$|\.html$", "", RegexOptions.IgnoreCase),
                    true,
                    "Save",
                    "Delete",
                    sheet.ModifiedDate
                );
            }

            // Load unsynced sheets
            if (sheetNames == null || sheetNames.Count == 0)
                return;

            // Load local-only sheets
            foreach (string name in sheetNames)
            {
                if (name == "Sheet1") // skip default
                    continue;

                bool exists = sheetWrapperDtos.Any(s =>
                    string.Equals(
                        Regex.Replace(s.SheetName, @"\.json$|\.html$", "", RegexOptions.IgnoreCase),
                        name,
                        StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    excelSheetGrid.Rows.Add(
                        0,
                        name == "Cost.Cal" ? "json" : "html",
                        name,
                        false,
                        "Save",
                        "Delete",
                        ModifiedDate
                    );
                }
            }
        }

        #endregion

        #region API 
        public static async Task<List<SheetWrapperDto>> GetSheetListAsync(string _token)
        {

            try
            {

                // Configure HttpClient once
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await _httpClient.GetAsync($"{ApiBaseUrl}api/SaveExcel/get-sheet-list");
                string jsonData = await response.Content.ReadAsStringAsync();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponseDto>(json).Data;
            }
            catch (Exception) { }
            return null;
        }

        public async Task<bool> DeleteSheetAsync(int sheetId)
        {
            try
            {
                string url =
                    $"{ApiBaseUrl}api/SaveExcel/delete-html-file/{sheetId}";

                HttpResponseMessage response =
                    await _httpClient.DeleteAsync(url);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return false;
            }
        }

        public async Task<bool> SaveSheetExcelBase64Async(string base64, string fileName)
        {
            try
            {
                var jsonBody = $"\"{base64}\"";

                var content = new StringContent(
                    jsonBody,
                    Encoding.UTF8,
                    "application/json"
                );

                string url =
                    $"{ApiBaseUrl}api/SaveExcel/save-html-base64?fileName={Uri.EscapeDataString(fileName)}";

                HttpResponseMessage response =
                    await _httpClient.PostAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return false;
            }
        }

        public async Task<bool> SaveSheetExcelJsonAsync(string editableCellsJson)
        {
            try
            {

                var content = new StringContent(editableCellsJson,Encoding.UTF8,"application/json");

                // URL with query parameter (same as curl)
                string url =$"{ApiBaseUrl}api/SaveExcel/edited-cells";

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // log ex if needed
                ApplicationLogger.LogException(ex);
                return false;
            }
        }

        #endregion

        #region Excel Save Method
        public async Task SheetSyncAsync(string sheetName, string type)
        {
            SplashManager.Show(Form.ActiveForm, $"Syncing {sheetName} Sheet");

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string workbookPath = Path.Combine(desktopPath, "thecalcify.xlsx");

            SaveOpenWorkbook("thecalcify.xlsx");

            var excelData = GetExcelSheetAsModel(workbookPath, sheetName);

            if (sheetName == "Cost.Cal")
            {
                Dictionary<string, int> cellAddress = CellValueExtractor(excelData);

                var payload = new
                {
                    editableCellsJson = JsonConvert.SerializeObject(cellAddress),
                    editableCells = new { }
                };

                bool jsonResult = await SaveSheetExcelJsonAsync(JsonConvert.SerializeObject(payload));

                if (!jsonResult)
                {
                    SplashManager.Hide();
                    MessageBox.Show("Failed to sync JSON sheet.","failed Sheet Bind",MessageBoxButtons.OK);
                    return;
                }
            }

            string html = ExcelModelToHtml(excelData);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));

            bool base64Result =
                await SaveSheetExcelBase64Async(base64, sheetName);

            SplashManager.Hide();
            MessageBox.Show(
                base64Result ? "Sheet synced successfully." : "Failed to sync sheet.","Sheet Sync",MessageBoxButtons.OK, base64Result? MessageBoxIcon.Information : MessageBoxIcon.Error
            );
        }

        public static void SaveOpenWorkbook(string workbookName)
        {
            var (excelApp, workbook) = ExcelAppManager.GetExcelApp();

            try
            {
                try
                {
                    workbook = excelApp.Workbooks[workbookName];
                    workbook.Save();

                }
                catch 
                {
                }

            }
            finally
            {
            }
        }

        #endregion

        #region Excel Methods

        public static (List<string>,string) exceldataBinder()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string workbookPath = Path.Combine(desktopPath, "thecalcify.xlsx");

            try
            {
                var sheets = GetSheetNames(workbookPath);
                return (sheets, File.GetLastWriteTime(workbookPath).ToString("dd-MM-yyyy HH:mm:ss"));
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log("Error loading workbook: " + ex.Message);
            }
            return (null,null);
        }

        public static List<string> GetSheetNames(string filePath)
        {
            var (excelApp, workbooks) = ExcelAppManager.GetExcelApp();
            var list = new List<string>();

            try
            {

                foreach (Worksheet sheet in workbooks.Sheets)
                    list.Add(sheet.Name);
            }
            finally
            {
            }

            return list;
        }


        public static ExcelModel GetExcelSheetAsModel(string filePath, string sheetName)
        {
            string tempFile = Path.GetTempFileName() + ".xlsx";

            try
            {
                File.Copy(filePath, tempFile, true);

                var (excelApp, workbook) = ExcelAppManager.GetExcelApp();

                Worksheet sheet = workbook.Sheets[sheetName];
                Range usedRange = sheet.UsedRange;

                int rowCount = usedRange.Rows.Count;
                int colCount = usedRange.Columns.Count;

                var model = new ExcelModel
                {
                    sheetname = sheetName,
                    totalRows = rowCount,
                    totalColumns = colCount,
                    cells = new Dictionary<string, CellData>()
                };

                for (int r = 1; r <= rowCount; r++)
                {
                    for (int c = 1; c <= colCount; c++)
                    {
                        Range cell = usedRange.Cells[r, c];
                        string address = GetCellAddress(r, c);
                        CellData dto = BuildCellData(cell);

                        model.cells[address] = dto;

                        Marshal.ReleaseComObject(cell);
                    }
                }

                Marshal.ReleaseComObject(usedRange);
                Marshal.ReleaseComObject(sheet);
                File.Delete(tempFile);

                return model;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return null;
            }
        }

        private Dictionary<string, int> CellValueExtractor(ExcelModel excelData)
        {
            var result = new Dictionary<string, int>();

            if (excelData?.cells == null)
                return result;

            foreach (var cellEntry in excelData.cells)
            {
                string cellAddress = cellEntry.Key;
                CellData cell = cellEntry.Value;

                // 1. Check type is static
                if (!string.Equals(cell.type, "static", StringComparison.OrdinalIgnoreCase))
                    continue;

                // 2. Check value exists
                if (string.IsNullOrWhiteSpace(cell.value))
                    continue;

                if (cellEntry.Key == "D4") { }
                // 3. Check number format OR numeric value
                if (IsNumericCell(cell))
                {
                    if (int.TryParse(cell.value, out int numericValue))
                    {
                        result[cellAddress] = numericValue;
                    }
                }
            }

            return result;
        }

        private bool IsNumericCell(CellData cell)
        {
            // If number format explicitly says numeric
            if (!string.IsNullOrWhiteSpace(cell.format?.numberFormat) &&
                cell.format.numberFormat != "General")
            {
                return true;
            }

            // Fallback: try parsing number
            return double.TryParse(cell.value, out _);
        }

        private static CellData BuildCellData(Range cell)
        {
            string formula = cell.Formula?.ToString();
            string value = cell.Value2?.ToString();
            string testvale = cell.Value?.ToString();

            var dto = new CellData
            {
                value = value,
                formula = formula,
                type = "static"
            };

            if (!string.IsNullOrEmpty(formula) &&
                formula.StartsWith("=RTD(", StringComparison.OrdinalIgnoreCase))
            {
                dto.type = "rtd";
            }
            else if (!string.IsNullOrEmpty(formula) && formula.StartsWith("="))
            {
                dto.type = "formula";
            }

            dto.format = ExtractCellFormat(cell);

            return dto;
        }

        private static CellFormat ExtractCellFormat(Range cell)
        {
            bool bold = cell.Font.Bold != null && (bool)cell.Font.Bold;
            bool italic = cell.Font.Italic != null && (bool)cell.Font.Italic;
            int underlineVal = cell.Font.Underline != null ? Convert.ToInt32(cell.Font.Underline) : -4142;  // xlUnderlineStyleNone
            bool underline = underlineVal != -4142;  // underline exists if NOT "None"
            bool regular = !bold && !italic && !underline;

            string fontStyle = regular ? "r" :
                               bold ? "b" :
                               italic ? "i" :
                               underline ? "u" :
                               bold && italic ? "bi" :
                               bold && underline ? "bu" :
                               italic && underline ? "iu" :
                               bold && italic && underline ? "biu" : "r";


            return new CellFormat
            {
                fontStyle = fontStyle,
                fontSize = cell.Font.Size != null ? Convert.ToDouble(cell.Font.Size) : 0,
                fontColor = ColorToHex(cell.Font.Color),
                backgroundColor = ColorToHex(cell.Interior.Color),
                numberFormat = cell.NumberFormat?.ToString(),
                horizontalAlign = AlignName(cell.HorizontalAlignment),
                verticalAlign = AlignName(cell.VerticalAlignment)
            };
        }

        private static string ColorToHex(object excelColor)
        {
            if (excelColor == null) return null;

            int color = Convert.ToInt32(excelColor);
            int r = color & 0xFF;
            int g = (color >> 8) & 0xFF;
            int b = (color >> 16) & 0xFF;

            return $"#{r:X2}{g:X2}{b:X2}";
        }

        private static string AlignName(object align)
        {
            if (align == null)
                return null;

            int val = Convert.ToInt32(align);

            switch (val)
            {
                case -4108: return "Center";
                case -4131: return "Left";
                case -4152: return "Right";
                case -4105: return "General";
                case -4160: return "Top";
                case -4107: return "Bottom";
                case 5: return "Justify";
                case 7: return "Distributed";
                default: return "";
            }
        }

        private static string GetCellAddress(int row, int col)
        {
            string colName = "";
            while (col > 0)
            {
                int mod = (col - 1) % 26;
                colName = (char)(65 + mod) + colName;
                col = (col - mod) / 26;
            }
            return colName + row;
        }

        public static string ExcelModelToHtml(ExcelModel model)
        {
            var sb = new StringBuilder();
            var styleCache = new Dictionary<string, string>(); // Maps CSS string to class name
            var classCounter = 0;

            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("table.excel{border-collapse:collapse}");
            sb.AppendLine("table.excel td{border:1px solid #ccc;padding:4px;min-width:60px;white-space:nowrap}");

            // First pass: collect all unique styles and generate classes
            for (int r = 1; r <= model.totalRows; r++)
            {
                for (int c = 1; c <= model.totalColumns; c++)
                {
                    string addr = GetCellAddress(r, c);
                    model.cells.TryGetValue(addr, out var cell);
                    string css = CellFormatToCss(cell?.format);

                    if (!string.IsNullOrEmpty(css) && !styleCache.ContainsKey(css))
                    {
                        styleCache[css] = $"s{classCounter++}";
                    }
                }
            }

            // Output CSS classes
            foreach (var kvp in styleCache)
            {
                sb.AppendLine($".{kvp.Value}{{{kvp.Key}}}");
            }

            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<table class='excel'>");

            // Second pass: render table with class references
            for (int r = 1; r <= model.totalRows; r++)
            {
                sb.AppendLine("<tr>");
                for (int c = 1; c <= model.totalColumns; c++)
                {
                    string addr = GetCellAddress(r, c);
                    model.cells.TryGetValue(addr, out var cell);
                    string css = CellFormatToCss(cell?.format);
                    string className = string.IsNullOrEmpty(css) ? "" : styleCache[css];

                    sb.Append("<td id='").Append(addr).Append("'");
                    if (!string.IsNullOrEmpty(className))
                        sb.Append(" class='").Append(className).Append("'");
                    sb.Append(">");
                    sb.Append(System.Net.WebUtility.HtmlEncode(cell?.formula ?? ""));
                    sb.AppendLine("</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }

        private static string CellFormatToCss(CellFormat f)
        {
            if (f == null) return "";
            var css = new List<string>
                {
                    $"font-size:{f.fontSize}px"
                };
            if (f.fontStyle.Contains("b")) css.Add("font-weight:bold");
            if (f.fontStyle.Contains("i")) css.Add("font-style:italic");
            if (f.fontStyle.Contains("u")) css.Add("text-decoration:underline");
            if (!string.IsNullOrEmpty(f.fontColor)) css.Add($"color:{f.fontColor}");
            if (!string.IsNullOrEmpty(f.backgroundColor)) css.Add($"background-color:{f.backgroundColor}");
            if (!string.IsNullOrEmpty(f.horizontalAlign)) css.Add($"text-align:{f.horizontalAlign}");
            if (!string.IsNullOrEmpty(f.verticalAlign)) css.Add($"vertical-align:{f.verticalAlign}");
            return string.Join(";", css);
        }

        #endregion


    }

    public class MessageFilter : IOleMessageFilter
    {
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            CoRegisterMessageFilter(newFilter, out _);
        }

        public static void Revoke()
        {
            CoRegisterMessageFilter(null, out _);
        }

        int IOleMessageFilter.HandleInComingCall(int dwCallType,
            IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
        {
            return 0; // SERVERCALL_ISHANDLED
        }

        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee,
            int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2) // SERVERCALL_RETRYLATER
                return 100;        // retry after 100 ms
            return -1;
        }

        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee,
            int dwTickCount, int dwPendingType)
        {
            return 2; // PENDINGMSG_WAITDEFPROCESS
        }

        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(
            IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);
    }

    public interface IOleMessageFilter
    {
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller,
            int dwTickCount, IntPtr lpInterfaceInfo);

        int RetryRejectedCall(IntPtr hTaskCallee,
            int dwTickCount, int dwRejectType);

        int MessagePending(IntPtr hTaskCallee,
            int dwTickCount, int dwPendingType);
    }


    public class ExcelModel
    {
        public string sheetname { get; set; }
        public int totalRows { get; set; }
        public int totalColumns { get; set; }
        public Dictionary<string, CellData> cells { get; set; }
    }

    public class CellData
    {
        public string value { get; set; }
        public string formula { get; set; }
        public string type { get; set; }
        public CellFormat format { get; set; }
    }

    public class CellFormat
    {
        public double fontSize { get; set; }
        public string fontStyle { get; set; }
        public string fontColor { get; set; }
        public string backgroundColor { get; set; }
        public string numberFormat { get; set; }
        public string horizontalAlign { get; set; }
        public string verticalAlign { get; set; }
    }

    public class ApiResponseDto
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<SheetWrapperDto> Data { get; set; }
    }

    public class SheetWrapperDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }   // "json" | "html"

        [JsonProperty("data")]
        public SheetDataDto Data { get; set; }

        [JsonProperty("sheetName")]
        public string SheetName { get; set; }

        [JsonProperty("sheetId")]
        public int SheetId { get; set; }

        [JsonProperty("lastUpdated")]
        public string ModifiedDate { get; set; }
    }

    public class SheetDataDto
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        // Example: { "B4": 0.05, "D4": 0.05 }
        [JsonProperty("editedCells")]
        public Dictionary<string, decimal> EditedCells { get; set; }

        [JsonProperty("sheetJSON")]
        public SheetJsonDto SheetJSON { get; set; }
    }
    public class SheetJsonDto
    {
        [JsonProperty("totalRows")]
        public int TotalRows { get; set; }

        [JsonProperty("totalColumns")]
        public int TotalColumns { get; set; }

        // Example: "A1", "B1", etc.
        [JsonProperty("cells")]
        public Dictionary<string, CellDto> Cells { get; set; }
    }
    public class CellDto
    {
        // Can be string | number | null
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("formula")]
        public string Formula { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public CellFormatDto Format { get; set; }
    }
    public class CellFormatDto
    {
        [JsonProperty("fontSize")]
        public int FontSize { get; set; }

        [JsonProperty("fontStyle")]
        public string FontStyle { get; set; }

        [JsonProperty("fontColor")]
        public string FontColor { get; set; }

        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty("numberFormat")]
        public string NumberFormat { get; set; }

        [JsonProperty("horizontalAlign")]
        public string HorizontalAlign { get; set; }

        [JsonProperty("verticalAlign")]
        public string VerticalAlign { get; set; }
    }
    public enum SheetType
    {
        json,
        html
    }


    public static class ExcelAppManager
    {
        private static Microsoft.Office.Interop.Excel.Application _excelApp;
        private static Workbook _workbook;
        private static bool _createdByUs;

        public static (Microsoft.Office.Interop.Excel.Application app, Workbook workbook) GetExcelApp()
        {
            // 🔴 Excel was closed by user → reset everything
            if (!IsExcelAlive(_excelApp) || !IsWorkbookAlive(_workbook))
            {
                CleanupStaleReferences();
            }

            if (_excelApp != null && _workbook != null)
                return (_excelApp, _workbook);

            try
            {
                _excelApp = (Microsoft.Office.Interop.Excel.Application)
                    Marshal.GetActiveObject("Excel.Application");

                _createdByUs = false;
            }
            catch (COMException)
            {
                _excelApp = new Microsoft.Office.Interop.Excel.Application
                {
                    Visible = false,
                    DisplayAlerts = false
                };
                _createdByUs = true;
            }

            string workbookPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "thecalcify.xlsx"
            );

            foreach (Workbook wb in _excelApp.Workbooks)
            {
                if (string.Equals(wb.Name, "thecalcify.xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    _workbook = wb;
                    return (_excelApp, _workbook);
                }
            }

            _workbook = _excelApp.Workbooks.Open(workbookPath);

            return (_excelApp, _workbook);
        }

        public static void ReleaseExcelApp()
        {
            try
            {
                if (_excelApp != null && _createdByUs)
                {
                    _excelApp.Quit();
                }
            }
            catch { }
            finally
            {
                if (_excelApp != null)
                    Marshal.ReleaseComObject(_excelApp);

                _excelApp = null;
                _createdByUs = false;
            }
        }

        private static bool IsExcelAlive(Microsoft.Office.Interop.Excel.Application app)
        {
            if (app == null)
                return false;

            try
            {
                // Lightweight COM call – throws if Excel is closed
                var _ = app.Workbooks.Count;
                return true;
            }
            catch (COMException)
            {
                return false;
            }
        }

        private static bool IsWorkbookAlive(Workbook wb)
        {
            if (wb == null)
                return false;

            try
            {
                var _ = wb.Name;
                return true;
            }
            catch (COMException)
            {
                return false;
            }
        }

        private static void CleanupStaleReferences()
        {
            try
            {
                if (_workbook != null)
                    Marshal.ReleaseComObject(_workbook);
            }
            catch { }

            try
            {
                if (_excelApp != null)
                    Marshal.ReleaseComObject(_excelApp);
            }
            catch { }

            _workbook = null;
            _excelApp = null;
            _createdByUs = false;
        }



    }


}
