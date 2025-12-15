using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace thecalcify.Excel_Helper
{
    public partial class UserExcelExportForm : UserControl
    {
        public UserExcelExportForm()
        {
            InitializeComponent();
        }

        private void UserExcelExportForm_Load(object sender, EventArgs e)
        {
            ApplyRuntimeLayout();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string workbookPath = Path.Combine(desktopPath, "thecalcify.xlsx");

            try
            {
                var sheets = GetSheetNames(workbookPath);

                clbSheets.Items.Clear();
                foreach (var s in sheets)
                    clbSheets.Items.Add(s);

                clbSheets.Items.Remove("Sheet1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading workbook: " + ex.Message);
            }
        }

        private void UserExcelExportForm_Resize(object sender, EventArgs e)
        {
            ApplyRuntimeLayout();
        }

        private void ApplyRuntimeLayout()
        {
            // Center card
            panelCard.Left = (this.Width - panelCard.Width) / 2;

            // Rounded corners (runtime only)
            panelCard.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, panelCard.Width, panelCard.Height, 20, 20)
            );
        }

        public static List<string> GetSheetNames(string filePath)
        {
            var list = new List<string>();

            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            var wb = excelApp.Workbooks.Open(filePath);

            foreach (Microsoft.Office.Interop.Excel.Worksheet sheet in wb.Sheets)
                list.Add(sheet.Name);

            wb.Close(false);
            excelApp.Quit();

            return list;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string workbookPath = Path.Combine(desktopPath, "thecalcify.xlsx");
            string workbookName = "thecalcify.xlsx";

            if (clbSheets.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one worksheet.");
                return;
            }

            foreach (string sheetName in clbSheets.CheckedItems)
            {
                SaveOpenWorkbook(workbookName);
                var json = GetExcelSheetAsModel(workbookPath, sheetName);
                string savePath = Path.Combine(desktopPath, $"{sheetName}.json");

                File.WriteAllText(savePath, json);
            }

            MessageBox.Show("Selected worksheets exported to JSON!");
        }

        public static string GetExcelSheetAsModel(string filePath, string sheetName)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            string tempFile = Path.GetTempFileName() + ".xlsx";

            try
            {
                File.Copy(filePath, tempFile, true);

                excelApp = new  Microsoft.Office.Interop.Excel.Application { Visible = false, DisplayAlerts = false };
                workbook = excelApp.Workbooks.Open(tempFile);

                Microsoft.Office.Interop.Excel.Worksheet sheet = workbook.Sheets[sheetName];
                Microsoft.Office.Interop.Excel.Range usedRange = sheet.UsedRange;

                int rowCount = usedRange.Rows.Count;
                int colCount = usedRange.Columns.Count;

                var model = new workBookInfo
                {
                    sheetName = sheetName,
                    totalRows = rowCount,
                    totalColumns = colCount,
                    cells = new Dictionary<string, CellData>()
                };

                for (int r = 1; r <= rowCount; r++)
                {
                    for (int c = 1; c <= colCount; c++)
                    {
                        Microsoft.Office.Interop.Excel.Range cell = usedRange.Cells[r, c];
                        string address = GetCellAddress(r, c);
                        CellData dto = BuildCellData(cell);

                        model.cells[address] = dto;

                        Marshal.ReleaseComObject(cell);
                    }
                }

                Marshal.ReleaseComObject(usedRange);
                workbook.Close(false);
                Marshal.ReleaseComObject(workbook);
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
                File.Delete(tempFile);

                return JsonConvert.SerializeObject(model);
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        private static CellData BuildCellData(Microsoft.Office.Interop.Excel.Range cell)
        {
            string formula = cell.Formula?.ToString();
            string value = cell.Value2?.ToString();

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

        private static CellFormat ExtractCellFormat(Microsoft.Office.Interop.Excel.Range cell)
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

        public static void SaveOpenWorkbook(string workbookName)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = (Microsoft.Office.Interop.Excel.Application)Marshal.GetActiveObject("Excel.Application");
            Microsoft.Office.Interop.Excel.Workbook wb = excelApp.Workbooks[workbookName];
            wb.Save();
            Marshal.ReleaseComObject(wb);
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


    public class workBookInfo
    {
        public string sheetName { get; set; }
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
}
