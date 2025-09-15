using IWshRuntimeLibrary;
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
        private Timer internetCheckTimer;
        private bool isInternetAvailable = true;

        public Common(Control control)
        {
            //uiContext = control;
            //SystemEvents.PowerModeChanged += OnPowerChange;
            //NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            //NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        }

        public Common()
        {
            
        }

        public bool IsFileLocked(string filePath)
        {
            FileStream stream = null;

            if(!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                return false; // It's a directory, not a file
            }

            if(!System.IO.File.Exists(filePath))
            {
                return false; // File does not exist, so it's not locked
            }

            try
            {
                // Try to open the file with exclusive access
                stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable because it is still being written to
                // or being processed by another thread or process
                return true;
            }
            finally
            {
                stream?.Close();
            }

            // File is not locked
            return false;
        }

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

        public static DateTime ParseToDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input date string is null or empty.");

            string[] formats =
            {
        "dd/MM/yyyy", "d/M/yyyy",
        "MM/dd/yyyy", "M/d/yyyy",
        "yyyy-MM-dd", "yyyy/MM/dd",
        "dd-MM-yyyy", "d-M-yyyy",
        "dd.MM.yyyy", "d.M.yyyy"
    };

            if (DateTime.TryParseExact(
                    input,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                return parsedDate.Date; // ✅ return DateTime (no string conversion)
            }

            throw new FormatException($"Invalid date format: {input}");
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

        public string TimeStampConvert(string value)
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

        public void CreateShortCut(string filepath)
        {
            // Path to the file you want to create a shortcut for
            //string targetPath = filepath; // Change this
            string shortcutName = "thecalcify Excel";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktopPath, shortcutName + ".lnk");

            // Create WSH Shell
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = filepath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(filepath);
            shortcut.WindowStyle = 1;
            shortcut.Description = "Runs thecalcify Excel as Administrator";
            shortcut.IconLocation = filepath;

            // This causes the program to request elevation when run
            shortcut.Arguments = ""; // Optional: add arguments if needed
            shortcut.Save();

            // Now we need to mark the shortcut to always run as admin
            // Unfortunately, IWshShortcut doesn't support setting "RunAsAdministrator" directly
            // Instead, we must manually modify the shortcut file

            byte[] bytes = System.IO.File.ReadAllBytes(shortcutPath);
            // Set the 21st byte (index 0x15) to 0x22 (original value | 0x20)
            // This sets the "RunAs" flag
            if (bytes.Length > 0x15)
            {
                bytes[0x15] |= 0x20;
                System.IO.File.WriteAllBytes(shortcutPath, bytes);
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

    class ExcelFormulaCell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Formula { get; set; }
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
