using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

            string[] formats = new string[]
            {
                "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy",
                "d/M/yy", "MM/dd/yyyy", "M/d/yyyy",
                "yyyy-MM-dd", "yyyy/MM/dd", "dd-MM-yyyy",
                "d-M-yyyy", "dd-MM-yy", "d-M-yy",
                "dd.MM.yyyy", "d.M.yyyy", "dd MMM yyyy",
                "d MMM yyyy", "ddd, dd MMM yyyy", "ddd, d MMM yyyy",
                "yyyyMMdd", "MMMyy", "dd/MM/yyyy HH:mm:ss",
                "d/M/yyyy HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "M/d/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "dd-MM-yyyy HH:mm:ss",
                "d-M-yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "d.M.yyyy HH:mm:ss",
                "dd MMM yyyy HH:mm:ss", "d MMM yyyy HH:mm:ss", "ddd, dd MMM yyyy HH:mm:ss",
                "ddd, d MMM yyyy HH:mm:ss", "yyyyMMdd HH:mm:ss", "MMMyy HH:mm:ss",
                "dd/MM/yyyy hh:mm:ss", "d/M/yyyy hh:mm:ss", "MM/dd/yyyy hh:mm:ss",
                "M/d/yyyy hh:mm:ss", "yyyy-MM-dd hh:mm:ss", "yyyy/MM/dd hh:mm:ss",
                "dd-MM-yyyy hh:mm:ss", "d-M-yyyy hh:mm:ss", "dd.MM.yyyy hh:mm:ss",
                "d.M.yyyy hh:mm:ss", "dd MMM yyyy hh:mm:ss", "d MMM yyyy hh:mm:ss",
                "ddd, dd MMM yyyy hh:mm:ss", "ddd, d MMM yyyy hh:mm:ss", "yyyyMMdd hh:mm:ss",
                "MMMyy hh:mm:ss", "dd/MM/yyyy hh:mm:ss tt", "d/M/yyyy hh:mm:ss tt",
                "MM/dd/yyyy hh:mm:ss tt", "M/d/yyyy hh:mm:ss tt", "yyyy-MM-dd hh:mm:ss tt",
                "yyyy/MM/dd hh:mm:ss tt",
                "dd-MM-yyyy hh:mm:ss tt",
                "d-M-yyyy hh:mm:ss tt",
                "dd.MM.yyyy hh:mm:ss tt",
                "d.M.yyyy hh:mm:ss tt",
                "dd MMM yyyy hh:mm:ss tt",
                "d MMM yyyy hh:mm:ss tt",
                "ddd, dd MMM yyyy hh:mm:ss tt",
                "ddd, d MMM yyyy hh:mm:ss tt",
                "yyyyMMdd hh:mm:ss tt",
                "MMMyy hh:mm:ss tt",
                "dd/MM/yyyy hh:mm",
                "d/M/yyyy hh:mm",
                "MM/dd/yyyy hh:mm",
                "M/d/yyyy hh:mm",
                "yyyy-MM-dd hh:mm",
                "yyyy/MM/dd hh:mm",
                "dd-MM-yyyy hh:mm",
                "d-M-yyyy hh:mm",
                "dd.MM.yyyy hh:mm",
                "d.M.yyyy hh:mm",
                "dd MMM yyyy hh:mm",
                "d MMM yyyy hh:mm",
                "ddd, dd MMM yyyy hh:mm",
                "ddd, d MMM yyyy hh:mm",
                "yyyyMMdd hh:mm",
                "MMMyy hh:mm",
                "dd/MM/yyyy hh:mm tt",
                "d/M/yyyy hh:mm tt",
                "MM/dd/yyyy hh:mm tt",
                "M/d/yyyy hh:mm tt",
                "yyyy-MM-dd hh:mm tt",
                "yyyy/MM/dd hh:mm tt",
                "dd-MM-yyyy hh:mm tt",
                "d-M-yyyy hh:mm tt",
                "dd.MM.yyyy hh:mm tt",
                "d.M.yyyy hh:mm tt",
                "dd MMM yyyy hh:mm tt",
                "d MMM yyyy hh:mm tt",
                "ddd, dd MMM yyyy hh:mm tt",
                "ddd, d MMM yyyy hh:mm tt",
                "yyyyMMdd hh:mm tt",
                "MMMyy hh:mm tt",
                "dd-MM-yyyy H:mm:ss",
                "dd-MM-yyyy h.mm.ss tt",
                "dd-MM-yyyy hh:mm:tt",
                "dd-MM-yyyy HH:mm",
                "dd-MM-yyyy H:mm",
                "dd-MM-yy hh:mm tt",
                "dd-MM-yy hh:mm:ss tt",
                "dd-MM-yy HH:mm:ss",
                "dd-MM-yy H:mm:ss",
                "dd-MM-yy h.mm.ss tt",
                "dd-MM-yy hh:mm:tt",
                "dd-MM-yy HH:mm",
                "dd-MM-yy H:mm",
                "d-M-yy hh:mm tt",
                "d-M-yy hh:mm:ss tt",
                "d-M-yy HH:mm:ss",
                "d-M-yy H:mm:ss",
                "d-M-yy h.mm.ss tt",
                "d-M-yy hh:mm:tt",
                "d-M-yy HH:mm",
                "d-M-yy H:mm",
                "d.M.yy hh:mm tt",
                "d.M.yy hh:mm:ss tt",
                "d.M.yy HH:mm:ss",
                "d.M.yy H:mm:ss",
                "d.M.yy h.mm.ss tt",
                "d.M.yy hh:mm:tt",
                "d.M.yy HH:mm",
                "d.M.yy H:mm",
                "yyyy-MM-dd H:mm:ss",
                "yyyy-MM-dd h.mm.ss tt",
                "yyyy-MM-dd hh:mm:tt",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd H:mm",
                "dd MMMM yyyy hh:mm tt",
                "dd MMMM yyyy hh:mm:ss tt",
                "dd MMMM yyyy HH:mm:ss",
                "dd MMMM yyyy H:mm:ss",
                "dd MMMM yyyy h.mm.ss tt",
                "dd MMMM yyyy hh:mm:tt",
                "dd MMMM yyyy HH:mm",
                "dd MMMM yyyy H:mm",
                "d MMMM yyyy hh:mm tt",
                "d MMMM yyyy hh:mm:ss tt",
                "d MMMM yyyy HH:mm:ss",
                "d MMMM yyyy H:mm:ss",
                "d MMMM yyyy h.mm.ss tt",
                "d MMMM yyyy hh:mm:tt",
                "d MMMM yyyy HH:mm",
                "d MMMM yyyy H:mm",
                "dddd, d MMMM yyyy hh:mm tt",
                "dddd, d MMMM yyyy hh:mm:ss tt",
                "dddd, d MMMM yyyy HH:mm:ss",
                "dddd, d MMMM yyyy H:mm:ss",
                "dddd, d MMMM yyyy h.mm.ss tt",
                "dddd, d MMMM yyyy hh:mm:tt",
                "dddd, d MMMM yyyy HH:mm",
                "dddd, d MMMM yyyy H:mm",
                "dddd, d MMMM, yyyy hh:mm tt",
                "dddd, d MMMM, yyyy hh:mm:ss tt",
                "dddd, d MMMM, yyyy HH:mm:ss",
                "dddd, d MMMM, yyyy H:mm:ss",
                "dddd, d MMMM, yyyy h.mm.ss tt",
                "dddd, d MMMM, yyyy hh:mm:tt",
                "dddd, d MMMM, yyyy HH:mm",
                "dddd, d MMMM, yyyy H:mm",
                "dddd, dd MMMM yyyy hh:mm tt",
                "dddd, dd MMMM yyyy hh:mm:ss tt",
                "dddd, dd MMMM yyyy hh:mm:tt",
                "dddd, dd MMMM yyyy h:mm tt",
                "dddd, dd MMMM yyyy h:mm:ss tt",
                "dddd, dd MMMM yyyy h.mm.ss tt",
                "dddd, dd MMMM yyyy HH:mm",
                "dddd, dd MMMM yyyy HH:mm:ss",
                "dddd, dd MMMM yyyy H:mm",
                "dddd, dd MMMM yyyy H:mm:ss",
                "dd MMMM yyyy hh:mm",
                "dd MMMM yyyy hh:mm:ss",
                "dd MMMM yyyy HH:mm:ss tt",
                "dd MMMM yyyy H:mm:ss tt",
                "d MMMM yyyy HH:mm tt",


                // Licence Expiry format issue fixed
                "dd:MM:yyyy", "d:M:yyyy", "dd:MM:yy",
                "d:M:yy","dd-MM-yyyy'T'HH:mm:ss", "d-M-yyyy'T'HH:mm:ss",

                "dd-MM-yyyy HH:mm:ss.fff",
                "dd-MM-yyyy HH:mm:ss:fff",
                "dd/MM/yyyy HH:mm:ss.fff",
                "dd/MM/yyyy HH:mm:ss:fff"
            };

            string pattern = @"\s[+\-]\d{2}:\d{2}$";  // Matches '+00:00' or '-05:30'
            input = Regex.Replace(input, pattern, "");

            formats = formats.Distinct().ToArray();


            if (DateTime.TryParseExact(
                    input,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                return parsedDate; // ✅ return DateTime (no string conversion)
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
                // If the value is invalid or not a valid timestamp, return "--"
                if (string.IsNullOrWhiteSpace(value) || value == "--" || value == "N/A")
                    return "--";

                try
                {
                    // Case 1: It's already a valid ISO 8601 or date string
                    if (DateTimeOffset.TryParse(value, out var dto))
                    {
                        // Convert UTC to IST if necessary
                        if (dto.Offset == TimeSpan.Zero)
                            dto = dto.ToOffset(TimeSpan.FromHours(5.5)); // IST

                        return dto.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture); // or your custom format
                    }

                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                }

                long timestamp = long.Parse(value); // Parse the value to a long for timestamp

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);

                // If the original time zone is UTC (+00:00), convert it to IST (+05:30)
                if (dateTimeOffset.Offset == TimeSpan.Zero)
                {
                    dateTimeOffset = dateTimeOffset.ToOffset(TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(30))); // IST offset: UTC +5:30
                }

                // Pass the converted DateTimeOffset to ParseToDate method for further formatting
                string formattedDate = ParseToDate(dateTimeOffset.ToString()).ToString();

                return formattedDate;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log(value); // Log the input value for troubleshooting
                ApplicationLogger.LogException(ex); // Log the exception details
                return null; // Return null if any error occurs
            }
        }

        public static string UUIDExtractor()
        {

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct"))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        return mo["UUID"]?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
            return string.Empty;
        }

        public static string JsonExtractor(string json)
        {
            try
            {
                string UUID = UUIDExtractor();

                // Parse JSON
                var root = JObject.Parse(json);

                // Filter deviceAccess inside each item of data array
                foreach (var item in root["data"])
                {
                    if (string.IsNullOrWhiteSpace(item["deviceId"]?.ToString()) && string.IsNullOrWhiteSpace(UUID))
                    {
                        continue;
                    }

                    var filteredDevices = item["deviceAccess"]
                        .Where(d => d["deviceType"]?.ToString().ToLower() == "desktop" && d["deviceId"]?.ToString() == UUID)
                        .ToList();

                    // Replace deviceAccess with filtered list
                    item["deviceAccess"] = new JArray(filteredDevices);

                    break;
                }

                // *** Flatten the JSON ***

                // Take first item from data array
                var dataItem = root["data"]?.First as JObject;
                if (dataItem == null)
                {
                    //Console.WriteLine("No data found to flatten.");
                    ApplicationLogger.Log("No data found to flatten.");
                    return string.Empty;
                }

                // Take first deviceAccess item
                var deviceItem = dataItem["deviceAccess"]?.First as JObject;

                // Create new JObject for flattened JSON
                var flattened = new JObject();

                // Copy all root properties except 'data'
                foreach (var prop in root.Properties())
                {
                    if (prop.Name != "data")
                        flattened[prop.Name] = prop.Value;
                }

                // Copy all dataItem properties except 'deviceAccess'
                foreach (var prop in dataItem.Properties())
                {
                    if (prop.Name != "deviceAccess")
                        flattened[prop.Name] = prop.Value;
                }

                // Copy all deviceAccess properties if deviceItem is not null
                if (deviceItem != null)
                {
                    foreach (var prop in deviceItem.Properties())
                    {
                        flattened[prop.Name] = prop.Value;
                    }
                }

                return flattened.ToString();

            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return string.Empty;
            }

        }

        public static void ShowWindowsToast(string title, string timestamp)
        {
            DateTime parsedTime;

            if (!DateTime.TryParse(timestamp, null, DateTimeStyles.AdjustToUniversal, out parsedTime))
            {
                parsedTime = DateTime.UtcNow;
            }
            else
            {
                // Create IST timezone info
                TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                // Tell system that parsedTime is in IST and convert to UTC (if needed)
                parsedTime = TimeZoneInfo.ConvertTimeToUtc(parsedTime, istZone);
            }

            new ToastContentBuilder()
                .AddText(title)
                .AddText(parsedTime.ToLocalTime().ToString("G"))  // show seconds and AM/PM
                .AddArgument("action", "thecaclcifyNotification")  // <-- Add this argument
                .AddButton(activationType: ToastActivationType.Background, content: "Dismiss", arguments: "action=dismiss")
                .SetToastDuration(ToastDuration.Short) // Short duration toast
                .Show();
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

        //[JsonConverter(typeof(StringOrNumberConverter))]
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
    public class NewsCategoryResponse
    {
        public NewsCategoryData Data { get; set; }
    }

    public class NewsCategoryData
    {
        public NewsCategoryItem Item { get; set; }
    }

    public class NewsCategoryItem
    {
        public string VersionedGuid { get; set; }
        public string HeadLine { get; set; }
        public string Fragment { get; set; }
        public string BodyXhtmlRich { get; set; }
        public DateTime FirstCreated { get; set; }
        public DateTime SortTimestamp { get; set; }
        public DateTime ContentTimestamp { get; set; }
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
        public List<string> keyword { get; set; }
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

    public static class LoginInfo
    {
        public static bool IsNews { get; set; }
        public static bool IsRate { get; set; }

        public static DateTime RateExpiredDate { get; set; } = DateTime.MinValue;
        public static DateTime NewsExpiredDate { get; set; } = DateTime.MinValue;

        public static string topics { get; set; } = string.Empty;
        public static string keywords { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public bool status { get; set; }
        public int id { get; set; }
        public string username { get; set; }
        public bool isActive { get; set; }
        public DateTime newsExpireDate { get; set; }
        public DateTime rateExpireDate { get; set; }
        public string topics { get; set; }
        public string keywords { get; set; }
        public string deviceToken { get; set; }
        public string deviceType { get; set; }
        public string deviceId { get; set; }
        public bool isDND { get; set; }
        public bool hasNewsAccess { get; set; }
        public bool hasRateAccess { get; set; }
    }


    public class NewsNotificationDTO
    {
        public string headLine { get; set; }
        public string fragment { get; set; }
        public string versionedGuid { get; set; }
        public string sortTimestamp { get; set; }
        public string firstCreated { get; set; }
    }

    class SmoothFlowLayoutPanel : FlowLayoutPanel
    {
        public SmoothFlowLayoutPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }
    }

    public class ChipListBox : ListBox
    {
        public ChipListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = 32;
            typeof(ListBox).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)
            .SetValue(this, true, null);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string text = this.Items[e.Index].ToString();
            e.DrawBackground();
            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -4);

            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                   Color.FromArgb(240, 240, 240),
                   Color.FromArgb(220, 220, 220), 45f))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            TextRenderer.DrawText(e.Graphics, text, this.Font, rect,
                Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }

    public static class ControlExtensions
    {
        public static void EnableDoubleBuffer(this Control ctrl)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ctrl, true, null);
        }

        public static void EnableDoubleBufferRecursive(this Control ctrl)
        {
            ctrl.EnableDoubleBuffer();
            foreach (Control child in ctrl.Controls)
                child.EnableDoubleBufferRecursive();
        }
    }


}
