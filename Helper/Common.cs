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

        public static bool InternetAvilable()
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

        private static void ResumeAppLogic()
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

        public static string TimeStampConvert(string value)
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
        public List<MarketDataDto> data { get; set; }
    }


    // DTO to map JSON data
    public class MarketDataDto
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

    public class CategoriesResponse
    {
        public CategoriesData Data { get; set; }
    }

    public class CategoriesData
    {
        public FilterOptions FilterOptions { get; set; }
    }

    public class FilterOptions
    {
        public List<Category> Categories { get; set; }
    }

    public class Category
    {
        public string Code { get; set; }
        public string Literal { get; set; }
        public string Uri { get; set; }

        [Newtonsoft.Json.JsonProperty("children")]
        public List<Category> SubCategories { get; set; } = new List<Category>();
    }

    public class ReutersResponse
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("search")]
        public Search Search { get; set; }
    }

    public class Search
    {
        [JsonPropertyName("totalHits")]
        public int TotalHits { get; set; }

        [JsonPropertyName("items")]
        public List<NewsItem> Items { get; set; }

        [JsonPropertyName("pageInfo")]
        public PageInfo PageInfo { get; set; }
    }

    public class NewsItem
    {
        [JsonPropertyName("headLine")]
        public string HeadLine { get; set; }

        [JsonPropertyName("versionedGuid")]
        public string VersionedGuid { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("credit")]
        public string Credit { get; set; }

        [JsonPropertyName("firstCreated")]
        public string FirstCreated { get; set; }

        [JsonPropertyName("sortTimestamp")]
        public string SortTimestamp { get; set; }

        [JsonPropertyName("contentTimestamp")]
        public string ContentTimestamp { get; set; }

        [JsonPropertyName("productLabel")]
        public string ProductLabel { get; set; }

        [JsonPropertyName("urgency")]
        public int Urgency { get; set; }
    }

    public class PageInfo
    {
        [JsonPropertyName("endCursor")]
        public string EndCursor { get; set; }

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }
    }

    public class RootDto
    {
        public DataDto data { get; set; }
    }

    public class DataDto
    {
        public ItemDto item { get; set; }
    }

    public class ItemDto
    {
        public string byLine { get; set; }
        public string copyrightNotice { get; set; }
        public DateTime versionCreated { get; set; }
        public string fragment { get; set; }
        public string headLine { get; set; }
        public string versionedGuid { get; set; }
        public string uri { get; set; }
        public string language { get; set; }
        public string type { get; set; }
        public string profile { get; set; }
        public string slug { get; set; }
        public string usageTerms { get; set; }
        public string usageTermsRole { get; set; }
        public string version { get; set; }
        public string credit { get; set; }
        public DateTime firstCreated { get; set; }
        public string productLabel { get; set; }
        public string pubStatus { get; set; }
        public int urgency { get; set; }
        public string usn { get; set; }
        public string intro { get; set; }
        public string caption { get; set; }
        public string keyword { get; set; }
        public List<string> channels { get; set; }
        public List<SubjectLocationDto> subjectLocation { get; set; }
        public List<RenditionDto> renditions { get; set; }
    }

    public class SubjectLocationDto
    {
        public string city { get; set; }
        public string countryCode { get; set; }
        public string countryName { get; set; }
    }

    public class RenditionDto
    {
        public string mimeType { get; set; }
        public string uri { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public string code { get; set; }
    }
}
