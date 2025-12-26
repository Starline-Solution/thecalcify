using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.Excel_Helper
{
    #region DTOs & Models (COMPLETE IMPLEMENTATION)
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
        public string Type { get; set; }

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

        [JsonProperty("cells")]
        public Dictionary<string, CellDto> Cells { get; set; }
    }

    public class CellDto
    {
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
    #endregion

    #region Main Form Class
    public partial class UserExcelExportForm : UserControl
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public static readonly string ApiBaseUrl = APIUrl.ApplicationURL;
        public readonly string _token;

        public UserExcelExportForm(string token)
        {
            InitializeComponent();
            this._token = token;
        }

        #region Form Events
        private async void UserExcelExportForm_Load(object sender, EventArgs e)
        {
            ConfigureGridColumns();
            await ReloadExcelSheetGridAsync();
        }

        private async void excelSheetGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = excelSheetGrid;
            int sheetId = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["sheetID"].Value);
            string sheetName = grid.Rows[e.RowIndex].Cells["SheetName"].Value?.ToString();
            string type = grid.Rows[e.RowIndex].Cells["type"].Value?.ToString();

            grid.Enabled = false;

            try
            {
                if (grid.Columns[e.ColumnIndex].Name == "SaveSheet")
                {
                    await SheetSyncAsync(sheetName, type);
                    await ReloadExcelSheetGridAsync();
                }
                else if (grid.Columns[e.ColumnIndex].Name == "DeleteSheet" && !IsDeleteDisabled(e.RowIndex))
                {
                    var result = MessageBox.Show("we are not deleting Sheet just Sync Data", "Delete Sync Sheet",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        await DeleteSheetAsync(sheetId);
                        MessageBox.Show($"{sheetName} Sheet Deleted");
                        await ReloadExcelSheetGridAsync();
                    }
                }
            }
            finally
            {
                grid.Enabled = true;
            }
        }

        private void excelSheetGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "SaveSheet")
            {
                e.CellStyle.BackColor = Color.FromArgb(46, 204, 113);
                e.CellStyle.SelectionBackColor = Color.FromArgb(46, 204, 113);
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold);
            }

            if (excelSheetGrid.Columns[e.ColumnIndex].Name == "DeleteSheet")
            {
                if (IsDeleteDisabled(e.RowIndex))
                {
                    e.CellStyle.BackColor = Color.LightGray;
                    e.CellStyle.SelectionBackColor = Color.LightGray;
                    e.CellStyle.ForeColor = Color.DarkGray;
                    e.CellStyle.SelectionForeColor = Color.DarkGray;
                }
                else
                {
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
            if (e.RowIndex < 0 || excelSheetGrid.Columns[e.ColumnIndex].Name != "SheetName") return;

            e.Handled = true;
            e.PaintBackground(e.CellBounds, true);

            string sheetName = e.Value?.ToString() ?? "";
            string type = excelSheetGrid.Rows[e.RowIndex].Cells["type"].Value?.ToString() ?? "";
            Image icon = type.Equals("json", StringComparison.OrdinalIgnoreCase)
                ? Properties.Resources.ApplicationIcon_Excel : Properties.Resources.Excel_Image;

            int iconSize = 16, padding = 6;
            var iconRect = new System.Drawing.Rectangle(e.CellBounds.Left + padding,
                e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2, iconSize, iconSize);
            var textRect = new System.Drawing.Rectangle(iconRect.Right + padding, e.CellBounds.Top,
                e.CellBounds.Width - iconSize - padding * 3, e.CellBounds.Height);

            e.Graphics.DrawImage(icon, iconRect);
            TextRenderer.DrawText(e.Graphics, sheetName, e.CellStyle.Font, textRect, e.CellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            if ((e.State & DataGridViewElementStates.Selected) != 0)
                ControlPaint.DrawFocusRectangle(e.Graphics, e.CellBounds);
        }

        private void excelSheetGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || excelSheetGrid.Columns[e.ColumnIndex].Name != "SheetName") return;

            string type = excelSheetGrid.Rows[e.RowIndex].Cells["type"].Value?.ToString() ?? "";
            excelSheetGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = type.ToUpper();
        }
        #endregion

        #region Grid Configuration
        private void ConfigureGridColumns()
        {
            excelSheetGrid.Columns["SheetName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            excelSheetGrid.Columns["SheetName"].FillWeight = 100;

            excelSheetGrid.Columns["sheetUploaded"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            excelSheetGrid.Columns["sheetUploaded"].Width = 90;

            excelSheetGrid.Columns["SaveSheet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            excelSheetGrid.Columns["SaveSheet"].Width = 120;

            excelSheetGrid.Columns["DeleteSheet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            excelSheetGrid.Columns["DeleteSheet"].Width = 120;

            excelSheetGrid.Columns["ModifiedDate"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            excelSheetGrid.Columns["sheetUploaded"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            excelSheetGrid.Columns["SaveSheet"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            excelSheetGrid.Columns["DeleteSheet"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataGridViewColumn col in excelSheetGrid.Columns)
                col.Resizable = DataGridViewTriState.False;
        }

        private bool IsDeleteDisabled(int rowIndex)
        {
            if (rowIndex < 0) return false;

            string sheetName = excelSheetGrid.Rows[rowIndex].Cells["SheetName"].Value?.ToString();
            string type = excelSheetGrid.Rows[rowIndex].Cells["type"].Value?.ToString();

            return sheetName.Equals("Cost.Cal", StringComparison.OrdinalIgnoreCase) &&
                   type.Equals("json", StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Data Loading
        private async Task ReloadExcelSheetGridAsync()
        {
            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "thecalcify.xlsx")))
            {
                MessageBox.Show("Please make sure thecalcify.xlsx is present on Desktop", "Excel Not found",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            excelSheetGrid.Rows.Clear();
            excelSheetGrid.AutoGenerateColumns = false;

            var sheetWrapperDtos = await GetSheetListAsync(_token);
            List<string> sheetNames;
            string modifiedDate;
            ExcelDataBinder.ExceldataBinder(out sheetNames, out modifiedDate);

            if (sheetWrapperDtos == null) return;

            foreach (var sheet in sheetWrapperDtos)
            {
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

            if (sheetNames != null && sheetNames.Count > 0)
            {
                foreach (string name in sheetNames)
                {
                    if (name == "Sheet1") continue;

                    bool exists = sheetWrapperDtos.Any(s =>
                        string.Equals(Regex.Replace(s.SheetName, @"\.json$|\.html$", "", RegexOptions.IgnoreCase),
                            name, StringComparison.OrdinalIgnoreCase));

                    if (!exists)
                    {
                        excelSheetGrid.Rows.Add(0, name == "Cost.Cal" ? "json" : "html", name, false, "Save", "Delete", modifiedDate);
                    }
                }
            }
        }
        #endregion

        #region API Methods
        public static async Task<List<SheetWrapperDto>> GetSheetListAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.GetAsync($"{ApiBaseUrl}api/SaveExcel/get-sheet-list");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponseDto>(json)?.Data;
            }
            catch (Exception) { }
            return null;
        }

        public async Task<bool> DeleteSheetAsync(int sheetId)
        {
            try
            {
                string url = $"{ApiBaseUrl}api/SaveExcel/delete-html-file/{sheetId}";
                var response = await _httpClient.DeleteAsync(url);
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
                var content = new StringContent($"\"{base64}\"", Encoding.UTF8, "application/json");
                string url = $"{ApiBaseUrl}api/SaveExcel/save-html-base64?fileName={Uri.EscapeDataString(fileName)}";
                var response = await _httpClient.PostAsync(url, content);
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
                var content = new StringContent(editableCellsJson, Encoding.UTF8, "application/json");
                string url = $"{ApiBaseUrl}api/SaveExcel/edited-cells";
                var response = await _httpClient.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return false;
            }
        }
        #endregion

        #region Sheet Sync
        public async Task SheetSyncAsync(string sheetName, string type)
        {
            SplashManager.Show(Form.ActiveForm, $"Syncing {sheetName} Sheet");

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string workbookPath = Path.Combine(desktopPath, "thecalcify.xlsx");
                ExcelDataBinder.SaveOpenWorkbook("thecalcify.xlsx");

                var excelData = ExcelDataReader.GetExcelSheetAsModel(workbookPath, sheetName);

                if (sheetName == "Cost.Cal")
                {
                    var cellAddress = ExcelDataExtractor.CellValueExtractor(excelData);
                    var payload = new { editableCellsJson = JsonConvert.SerializeObject(cellAddress), editableCells = new { } };
                    bool jsonResult = await SaveSheetExcelJsonAsync(JsonConvert.SerializeObject(payload));

                    if (!jsonResult)
                    {
                        MessageBox.Show("Failed to sync JSON sheet.", "failed Sheet Bind", MessageBoxButtons.OK);
                        return;
                    }
                }

                string html = ExcelDataConverter.ExcelModelToHtml(excelData);
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));
                bool base64Result = await SaveSheetExcelBase64Async(base64, sheetName);

                MessageBox.Show(base64Result ? "Sheet synced successfully." : "Failed to sync sheet.",
                    "Sheet Sync", MessageBoxButtons.OK, base64Result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            finally
            {
                SplashManager.Hide();
            }
        }
        #endregion
    }
    #endregion

    #region Static Helper Classes (COMPLETE IMPLEMENTATION)
    public static class ExcelDataBinder
    {
        public static void ExceldataBinder(out List<string> sheetNames, out string modifiedDate)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string workbookPath = Path.Combine(desktopPath, "thecalcify.xlsx");

            try
            {
                var sheets = ExcelDataReader.GetSheetNames(workbookPath);
                sheetNames = sheets;
                modifiedDate = File.GetLastWriteTime(workbookPath).ToString("dd-MM-yyyy HH:mm:ss");
                return;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log("Error loading workbook: " + ex.Message);
                sheetNames = null;
                modifiedDate = null;
            }
        }

        public static void SaveOpenWorkbook(string workbookName)
        {
            var (excelApp, workbook) = ExcelAppManager.GetExcelApp();
            try
            {
                workbook = excelApp.Workbooks[workbookName];
                workbook.Save();
            }
            catch { }
        }
    }

    public static class ExcelDataReader
    {
        public static List<string> GetSheetNames(string filePath)
        {
            var (excelApp, workbooks) = ExcelAppManager.GetExcelApp();
            var list = new List<string>();

            try
            {
                foreach (Worksheet sheet in workbooks.Sheets)
                    list.Add(sheet.Name);
            }
            finally { }
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
                        string address = ExcelUtils.GetCellAddress(r, c);
                        model.cells[address] = CellBuilder.BuildCellData(cell);
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
    }

    public static class ExcelDataExtractor
    {
        public static Dictionary<string, int> CellValueExtractor(ExcelModel excelData)
        {
            var result = new Dictionary<string, int>();
            if (excelData?.cells == null) return result;

            foreach (var cellEntry in excelData.cells)
            {
                string cellAddress = cellEntry.Key;
                CellData cell = cellEntry.Value;

                if (!cell.type.Equals("static", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(cell.value) || !ExcelUtils.IsNumericCell(cell)) continue;

                if (int.TryParse(cell.value, out int numericValue))
                    result[cellAddress] = numericValue;
            }
            return result;
        }
    }

    public static class ExcelDataConverter
    {
        public static string ExcelModelToHtml(ExcelModel model)
        {
            var sb = new StringBuilder();
            var styleCache = new Dictionary<string, string>();
            var classCounter = 0;

            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>table.excel{border-collapse:collapse}table.excel td{border:1px solid #ccc;padding:4px;min-width:60px;white-space:nowrap}");

            for (int r = 1; r <= model.totalRows; r++)
            {
                for (int c = 1; c <= model.totalColumns; c++)
                {
                    string addr = ExcelUtils.GetCellAddress(r, c);
                    if (model.cells.TryGetValue(addr, out var cell))
                    {
                        string css = CellFormatter.CellFormatToCss(cell.format);
                        if (!string.IsNullOrEmpty(css) && !styleCache.ContainsKey(css))
                            styleCache[css] = $"s{classCounter++}";
                    }
                }
            }

            foreach (var kvp in styleCache)
                sb.AppendLine($".{kvp.Value}{{{kvp.Key}}}");

            sb.AppendLine("</style></head><body><table class='excel'>");

            for (int r = 1; r <= model.totalRows; r++)
            {
                sb.AppendLine("<tr>");
                for (int c = 1; c <= model.totalColumns; c++)
                {
                    string addr = ExcelUtils.GetCellAddress(r, c);
                    if (model.cells.TryGetValue(addr, out var cell))
                    {
                        string css = CellFormatter.CellFormatToCss(cell.format);
                        string className = string.IsNullOrEmpty(css) ? "" : styleCache[css];

                        sb.Append("<td id='").Append(addr).Append("'")
                          .Append(!string.IsNullOrEmpty(className) ? $" class='{className}'" : "")
                          .Append(">");
                        sb.Append(System.Net.WebUtility.HtmlEncode(cell.formula ?? ""));
                        sb.AppendLine("</td>");
                    }
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }
    }

    public static class CellBuilder
    {
        public static CellData BuildCellData(Range cell)
        {
            string formula = cell.Formula?.ToString();
            string value = cell.Value2?.ToString();

            var dto = new CellData
            {
                value = value,
                formula = formula,
                type = "static"
            };

            if (!string.IsNullOrEmpty(formula))
            {
                if (formula.StartsWith("=RTD(", StringComparison.OrdinalIgnoreCase))
                    dto.type = "rtd";
                else if (formula.StartsWith("="))
                    dto.type = "formula";
            }

            dto.format = CellFormatter.ExtractCellFormat(cell);
            return dto;
        }
    }

    public static class CellFormatter
    {
        public static CellFormat ExtractCellFormat(Range cell)
        {
            bool bold = cell.Font.Bold != null && (bool)cell.Font.Bold;
            bool italic = cell.Font.Italic != null && (bool)cell.Font.Italic;
            int underlineVal = cell.Font.Underline != null ? Convert.ToInt32(cell.Font.Underline) : -4142;
            bool underline = underlineVal != -4142;
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
                fontColor = ExcelUtils.ColorToHex(cell.Font.Color),
                backgroundColor = ExcelUtils.ColorToHex(cell.Interior.Color),
                numberFormat = cell.NumberFormat?.ToString(),
                horizontalAlign = ExcelUtils.AlignName(cell.HorizontalAlignment),
                verticalAlign = ExcelUtils.AlignName(cell.VerticalAlignment)
            };
        }

        public static string CellFormatToCss(CellFormat f)
        {
            if (f == null) return "";
            var css = new List<string> { $"font-size:{f.fontSize}px" };
            if (f.fontStyle.Contains("b")) css.Add("font-weight:bold");
            if (f.fontStyle.Contains("i")) css.Add("font-style:italic");
            if (f.fontStyle.Contains("u")) css.Add("text-decoration:underline");
            if (!string.IsNullOrEmpty(f.fontColor)) css.Add($"color:{f.fontColor}");
            if (!string.IsNullOrEmpty(f.backgroundColor)) css.Add($"background-color:{f.backgroundColor}");
            if (!string.IsNullOrEmpty(f.horizontalAlign)) css.Add($"text-align:{f.horizontalAlign}");
            if (!string.IsNullOrEmpty(f.verticalAlign)) css.Add($"vertical-align:{f.verticalAlign}");
            return string.Join(";", css);
        }
    }

    public static class ExcelUtils
    {
        public static string GetCellAddress(int row, int col)
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

        public static string ColorToHex(object excelColor)
        {
            if (excelColor == null) return null;
            int color = Convert.ToInt32(excelColor);
            int r = color & 0xFF;
            int g = (color >> 8) & 0xFF;
            int b = (color >> 16) & 0xFF;
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string AlignName(object align)
        {
            if (align == null) return null;
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

        public static bool IsNumericCell(CellData cell)
        {
            if (!string.IsNullOrWhiteSpace(cell.format?.numberFormat) && cell.format.numberFormat != "General")
                return true;
            return double.TryParse(cell.value, out _);
        }
    }
    #endregion

    #region COM & Excel Managers
    public static class ExcelAppManager
    {
        private static Microsoft.Office.Interop.Excel.Application _excelApp;
        private static Workbook _workbook;
        private static bool _createdByUs;

        public static (Microsoft.Office.Interop.Excel.Application app, Workbook workbook) GetExcelApp()
        {
            if (!IsExcelAlive(_excelApp) || !IsWorkbookAlive(_workbook))
                CleanupStaleReferences();

            if (_excelApp != null && _workbook != null)
                return (_excelApp, _workbook);

            try
            {
                _excelApp = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
                _createdByUs = false;
            }
            catch (COMException)
            {
                _excelApp = new Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                _createdByUs = true;
            }

            string workbookPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "thecalcify.xlsx");

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
                    _excelApp.Quit();
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
            if (app == null) return false;
            try
            {
                var _ = app.Workbooks.Count;
                return true;
            }
            catch (COMException) { return false; }
        }

        private static bool IsWorkbookAlive(Workbook wb)
        {
            if (wb == null) return false;
            try
            {
                var _ = wb.Name;
                return true;
            }
            catch (COMException) { return false; }
        }

        private static void CleanupStaleReferences()
        {
            try { if (_workbook != null) Marshal.ReleaseComObject(_workbook); } catch { }
            try { if (_excelApp != null) Marshal.ReleaseComObject(_excelApp); } catch { }
            _workbook = null;
            _excelApp = null;
            _createdByUs = false;
        }
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

        int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
            => 0;

        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2) return 100;
            return -1;
        }

        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
            => 2;

        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);
    }

    public interface IOleMessageFilter
    {
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);
        int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);
        int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
    }
    #endregion
}
