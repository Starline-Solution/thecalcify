using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.MarketWatch
{
    public partial class NewsDescription : Form
    {
        private readonly NewsCategoryItem _news;

        public NewsDescription(NewsCategoryItem news)
        {
            InitializeComponent();
            _news = news;
            LoadNews();
        }

        private void LoadNews()
        {
            if (_news == null) return;

            lblHeadline.Text = _news.HeadLine;

            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(_news.SortTimestamp.ToUniversalTime(), istZone);
            lblDateSource.Text = $"{istTime:dd-MMM-yyyy HH:mm:ss}";

            // Prepare HTML
            string html = _news.BodyXhtmlRich ?? "";

            // Ensure there's a <head> where we can inject CSS and meta
            if (!html.Contains("<head"))
            {
                html = html.Replace("<html", "<html><head></head>");
            }

            // CSS to style the table and more (you can expand this)
            string css = @"
    <style>
      table { border-collapse: collapse; width: 100%; }
      th, td { border: 1px solid #333; padding: 6px; }
      th { background: #f2f2f2; text-align: left; }
      p { font-family: Segoe UI, Arial, sans-serif; font-size: 12px; }
    </style>";

            // Optional: set a base URL if you have one (makes relative links resolve)
            string baseTag = "<base href=\"https://www.example.com/\">";

            // Inject CSS and base tag into head
            html = html.Replace("<head>", "<head>" + baseTag + css);

            // Optional: Add target="_blank" to anchors (so they call NewWindow or open externally)
            // This is simple regex-ish replace — careful with complex HTML; better parse if needed.
            html = Regex.Replace(
                 html,
                 "<a\\s+([^>]*?)>",
                 match =>
                 {
                     string inner = match.Groups[1].Value;

                     if (inner.IndexOf("target=", StringComparison.OrdinalIgnoreCase) >= 0)
                         return match.Value; // Already has target, return as-is

                     return $"<a {inner} target=\"_blank\">"; // Add target if missing
                 },
                 RegexOptions.IgnoreCase);


            // Hook DocumentCompleted to attach external link  handlers (see section 2)
            webBrowserDescription.DocumentCompleted -= WebBrowserDescription_DocumentCompleted;
            webBrowserDescription.DocumentCompleted += WebBrowserDescription_DocumentCompleted;

            webBrowserDescription.DocumentText = html;
        }

        private void WebBrowserDescription_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var doc = webBrowserDescription.Document;
            if (doc == null) return;

            foreach (HtmlElement link in doc.GetElementsByTagName("a"))
            {
                // Remove existing handler to avoid duplicates
                link.Click -= Link_Click;
                link.Click += Link_Click;
            }
        }

        private void Link_Click(object sender, HtmlElementEventArgs e)
        {
            var link = sender as HtmlElement;
            if (link == null) return;

            string url = link.GetAttribute("href");
            if (string.IsNullOrEmpty(url)) return;

            e.ReturnValue = false; // prevent default navigation in WebBrowser

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link: " + ex.Message);
            }
        }
    }
}
