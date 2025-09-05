using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public class Common
    {
        private System.Windows.Forms.Timer internetCheckTimer;
        private bool isInternetAvailable = true;
        private readonly Control uiContext; // store a reference to the UI thread control

        public Common(Control control)
        {
            uiContext = control;
        }

        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false; // File is not locked
                }
            }
            catch (IOException)
            {
                return true; // File is locked by Excel or another process
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                ApplicationLogger.LogException(ex);
                return true;
            }
        }


        //public bool IsFileLocked(string filePath)
        //{
        //    try
        //    {
        //        // Try to get a running Excel instance
        //        var excelApp = (Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
        //        if (excelApp != null)
        //        {
        //            // Kill any EXCEL processes without a main window (ghost/background instances)
        //            foreach (var process in Process.GetProcessesByName("EXCEL"))
        //            {
        //                try
        //                {
        //                    if (string.IsNullOrEmpty(process.MainWindowTitle))
        //                    {
        //                        process.Kill();
        //                        process.WaitForExit(); // ensure it's gone
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("Error killing Excel process: " + ex.Message);
        //                    ApplicationLogger.LogException(ex);
        //                }
        //            }

        //            // Check if the given workbook is open in Excel
        //            foreach (Workbook wb in excelApp.Workbooks)
        //            {
        //                if (string.Equals(wb.FullName, filePath, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    return true; // File is open in Excel
        //                }
        //            }

        //            return false; // File not open in Excel
        //        }

        //        return false; // Excel not running
        //    }
        //    catch (System.Runtime.InteropServices.COMException)
        //    {
        //        //ApplicationLogger.LogException(comEx);
        //        // Excel is not running
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log or handle unexpected errors
        //        Console.WriteLine("Error: " + ex.Message);
        //        ApplicationLogger.LogException(ex);
        //        return false;
        //    }
        //}


        public bool InternetAvilable()
        {
            try
            {
                // Quick check using NetworkInterface
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    // More thorough check by pinging a reliable server
                    using (var ping = new Ping())
                    {
                        var reply = ping.Send("8.8.8.8", 3000); // Google DNS
                        return reply.Status == IPStatus.Success;

                    }

                }
                return false;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return false;
            }
        }

        public void StartInternetMonitor()
        {
            internetCheckTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // check every 1 seconds
            };
            internetCheckTimer.Tick += InternetCheckTimer_Tick;
            internetCheckTimer.Start();
        }

        private void InternetCheckTimer_Tick(object sender, EventArgs e)
        {
            bool currentlyAvailable = InternetAvilable();

            if (currentlyAvailable && !isInternetAvailable)
            {
                isInternetAvailable = true;
                ResumeAppLogic();
            }
            else if (!currentlyAvailable && isInternetAvailable)
            {
                isInternetAvailable = false;
            }
        }

        private void ResumeAppLogic()
        {
            try
            {
                ApplicationLogger.Log($"Internet Is Down {DateTime.Now:dd/MM/yyyy HH:mm:ss:ff}");

                //if (live_Rate != null && live_Rate.socket.Disconnected == true)
                //{
                //    await live_Rate.SafeConnectAsync();
                //    if (live_Rate.socket.Disconnected == true)
                //    {
                //        MessageBox.Show("Real time Data stop due to unexpected Network change!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //    }
                //}
            }
            catch (TargetInvocationException)
            {
                //Live_Rate live_Rate = new Live_Rate();
                //await live_Rate.socket.DisconnectAsync();
            }
        }


        // Helper method for safe decimal conversion
        public decimal SafeConvertToDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("NaN", StringComparison.OrdinalIgnoreCase))
            {
                return 0m;
            }

            try
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing rate value at SafeConvertToDecimal: " + ex.Message);
            }

            return 0m; // Default fallback value
        }

        public string timeStampConvert(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value) || value == "--" || value == "N/A")
                    return "--";

                try
                {
                    // Check if the value is already a date or time in string format
                    if (DateTime.TryParse(value, out var dt)) // It's a valid date or time format
                        return value;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing rate value at timeStampConvert: " + ex.Message);
                }

                long timestamp = long.Parse(value);

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                string formattedDate = dateTimeOffset.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss:fff");

                return formattedDate;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log(value);
                ApplicationLogger.LogException(ex);
                return null;
            }
        }
    }


    public class MarketApiResponse
    {
        public bool isSuccess { get; set; }
        public string message { get; set; }
        public List<MarketDataDTO> data { get; set; }
    }


    // DTO to map JSON data
    public class MarketDataDTO
    {
        public string i { get; set; }
        public string n { get; set; }
        public string b { get; set; }
        public string a { get; set; }
        public string ltp { get; set; }
        public string h { get; set; }
        public string l { get; set; }
        public string o { get; set; }
        public string c { get; set; }

        [JsonConverter(typeof(StringOrNumberConverter))]
        public string d { get; set; } = "--";
        public string v { get; set; }
        public string t { get; set; }
        public string atp { get; set; } = "--";   // Ask traded price "98695.47"
        public string bq { get; set; } = "--";   // Bid quantity "1"
        public string tbq { get; set; } = "--";    // Total bid quantity "486"
        public string sq { get; set; } = "--";        // Sell quantity "1"
        public string tsq { get; set; } = "--";    // Total sell quantity "393"
        public string vt { get; set; } = "--";    // Volume traded "2734"
        public string oi { get; set; } = "--";    // Open interest "14129"
        public string ltq { get; set; } = "--";    // Last Traded Quantity "5"

    }

    public class WinApi
    {
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        public static void SetFullScreen(IntPtr handle)
        {
            int width = GetSystemMetrics(SM_CXSCREEN);
            int height = GetSystemMetrics(SM_CYSCREEN);
            SetWindowPos(handle, HWND_TOP, 0, 0, width, height, SWP_SHOWWINDOW);
        }
    }
}
