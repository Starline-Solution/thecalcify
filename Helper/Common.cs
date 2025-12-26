using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly string[] WindowsFormats = BuildWindowsFormats();

        private static string[] BuildWindowsFormats()
        {
            string[] dates =
            {
                // Short
                "M/d/yyyy","MM/dd/yyyy","d/M/yyyy","dd/MM/yyyy","dd-MM-yyyy","yyyy-MM-dd",

                // Long
                "dddd, MMMM d, yyyy",
                "dddd, MMMM dd, yyyy",
                "MMMM d, yyyy",
                "MMMM dd, yyyy",
                "d MMMM yyyy",
                "dd MMMM yyyy",

                        // Colon-based dates (non-standard but used in some apps/logs)
                "dd:MM:yyyy",
                "d:M:yyyy",
                "dd:MM:yy",
                "d:M:yy",
                "yyyy:MM:dd",

            };

            string[] times =
            {
                // 12-hour
                "h:mm tt","hh:mm tt",
                "h:mm:ss tt","hh:mm:ss tt",
                "h:mm:ss.f tt","hh:mm:ss.f tt",
                "h:mm:ss.ff tt","hh:mm:ss.ff tt",
                "h:mm:ss.fff tt","hh:mm:ss.fff tt",

                // 24-hour
                "H:mm","HH:mm",
                "H:mm:ss","HH:mm:ss",
                "H:mm:ss.f","HH:mm:ss.f",
                "H:mm:ss.ff","HH:mm:ss.ff",
                "H:mm:ss.fff","HH:mm:ss.fff",

                // time without seconds but with ms
                "HH:mm:fff","H:mm:fff"
            };

            List<string> list = new List<string>();

            // Combine date + time
            foreach (var d in dates)
                foreach (var t in times)
                    list.Add($"{d} {t}");

            // Add date-only formats
            list.AddRange(dates);

            // Add time-only formats
            list.AddRange(times);

            // Add ISO formats
            list.Add("yyyy-MM-ddTHH:mm:ss");
            list.Add("yyyy-MM-ddTHH:mm:ss.fff");

            // Add ISO with space
            list.Add("yyyy-MM-dd HH:mm:ss");
            list.Add("yyyy-MM-dd HH:mm:ss.fff");

            return list.Distinct().ToArray();
        }

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
                throw new ArgumentException("Empty date input");

            input = input.Trim();

            // Remove trailing timezone like +05:30
            input = Regex.Replace(input, @"\s[+\-]\d{2}:\d{2}$", "");

            // Unix Time
            if (Regex.IsMatch(input, @"^\d{10,13}$") && long.TryParse(input, out long unix))
            {
                if (input.Length == 13) return DateTimeOffset.FromUnixTimeMilliseconds(unix).LocalDateTime;
                if (input.Length == 10) return DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
            }

            // Try Windows formats
            if (DateTime.TryParseExact(
                    input,
                    WindowsFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out DateTime dt))
            {
                return dt;
            }

            // Last fallback
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
                return dt;

            throw new FormatException("Invalid date format: " + input);
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

                //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                //DateTimeOffset dateTimeOffset1 = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                DateTimeOffset dateTimeOffset = ParseUnixTimeWithIstRule(timestamp);

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


        public static DateTimeOffset ParseUnixTimeWithIstRule(long timestamp)
        {
            TimeSpan istOffset = TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(30));

            // Seconds (10 digits) → UTC
            if (timestamp < 10_000_000_000L)
            {
                // interpret as local IST time
                var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToOffset(istOffset);
                return dt;
            }

            // Milliseconds (13 digits) → UTC
            else
            {
                // convert UTC → IST
                return DateTimeOffset
                    .FromUnixTimeMilliseconds(timestamp)
                    .ToOffset(istOffset);
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
                .AddArgument("action", "thecalcifyNotification")  // <-- Add this argument
                .AddButton(activationType: ToastActivationType.Background, content: "Dismiss", arguments: "action=dismiss")
                .SetToastDuration(ToastDuration.Short) // Short duration toast
                .Show();
        }

        public static void ShowRateAlertWindowsToast(string title, string body)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(body)
                //.AddText(DateTime.Now.ToString("G"))  // show seconds and AM/PM
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


    public class MarketDataDto : INotifyPropertyChanged
    {
        private string _i, _n, _b, _a, _ltp, _h, _l, _o, _c, _d, _v, _t, _atp, _bq, _tbq, _sq, _tsq, _vt, _oi, _ltq;
        public string i { get => _i; set { _i = value; OnPropertyChanged(nameof(i)); } }
        public string n { get => _n; set { _n = value; OnPropertyChanged(nameof(n)); } }
        public string b { get => _b; set { _b = value; OnPropertyChanged(nameof(b)); } }
        public string a { get => _a; set { _a = value; OnPropertyChanged(nameof(a)); } }
        public string ltp { get => _ltp; set { _ltp = value; OnPropertyChanged(nameof(ltp)); } }
        public string h { get => _h; set { _h = value; OnPropertyChanged(nameof(h)); } }
        public string l { get => _l; set { _l = value; OnPropertyChanged(nameof(l)); } }
        public string o { get => _o; set { _o = value; OnPropertyChanged(nameof(o)); } }
        public string c { get => _c; set { _c = value; OnPropertyChanged(nameof(c)); } }

        public string d { get => _d; set { _d = value; OnPropertyChanged(nameof(d)); } }
        public string v { get => _v; set { _v = value; OnPropertyChanged(nameof(v)); } }

        public string t { get => _t; set { _t = value; OnPropertyChanged(nameof(t)); } }

        public string atp { get => _atp; set { _atp = value; OnPropertyChanged(nameof(atp)); } }
        public string bq { get => _bq; set { _bq = value; OnPropertyChanged(nameof(bq)); } }
        public string tbq { get => _tbq; set { _tbq = value; OnPropertyChanged(nameof(tbq)); } }
        public string sq { get => _sq; set { _sq = value; OnPropertyChanged(nameof(sq)); } }
        public string tsq { get => _tsq; set { _tsq = value; OnPropertyChanged(nameof(tsq)); } }
        public string vt { get => _vt; set { _vt = value; OnPropertyChanged(nameof(vt)); } }
        public string oi { get => _oi; set { _oi = value; OnPropertyChanged(nameof(oi)); } }
        public string ltq { get => _ltq; set { _ltq = value; OnPropertyChanged(nameof(ltq)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

    public class RateAlertNotificationDto
    {
        public string Username { get; set; }
        public RateAlertDataDto Data { get; set; }
    }

    public class RateAlertDataDto
    {
        public int ClientId { get; set; }
        public string Symbol { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
        public string Condition { get; set; }
        public string Flag { get; set; }
        public decimal Rate { get; set; }
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


    public class CellData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value { get; set; }
        public string Formula { get; set; }

        public string Type { get; set; }
        public string Symbol { get; set; }
        public string Field { get; set; }
        public CellFormat Format { get; set; }
    }

    public class CellFormat
    {
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }

        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
        public string NumberFormat { get; set; }
        public string HorizontalAlign { get; set; }
        public string VerticalAlign { get; set; }
    }


    // Helper classes for RTW config symbol info
    public class SymbolItem
    {
        public string i { get; set; }
        public string n { get; set; }
    }



}
