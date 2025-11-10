using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.News
{
    public partial class NewsSetting : UserControl
    {
        private static string apiUrl = APIUrl.ProdUrl;
        private string[] keywords;
        private bool isDND { get; set; }
        private int userId;
        private string token;
        private TextBox keywordInputTextBox; // new input box
        private Panel overlayPanel;
        private bool overlayVisible = false;
        private string[] topics;
        private Dictionary<string, string> _selectedSubTopics = new Dictionary<string, string>();
        private static Dictionary<string, string> _topicTitleLookup;

        public NewsSetting Instance { get; private set; }

        public NewsSetting(int _userid, string _keywords, string _topics, bool _isDND, string _token)
        {
            InitializeComponent();
            this.EnableDoubleBufferRecursive();
            flowTopics.EnableDoubleBuffer();
            flowKeywords.EnableDoubleBuffer();



            Instance = this;

            this.Click += (s, e) =>
            {
                if (overlayPanel != null && overlayPanel.Visible)
                {
                    overlayPanel.Visible = false;
                    overlayVisible = false;
                }
            };

            // Split and sanitize
            topics = _topics.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            keywords = _keywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrWhiteSpace(k)).ToArray();
            isDND = _isDND;
            userId = _userid;
            token = _token;

            InitializeSelectedSubTopics();


            if (_isDND)
                dndOn.Checked = true;
            else
                dndOff.Checked = true;

            GenerateTopicChips();
            GenerateKeywordsChips();
        }

        private void GenerateTopicChips()
        {
            flowTopics.Controls.Clear();

            // -------------------------------
            // 1. Manage Subscription Button (Overlay Trigger)
            // -------------------------------
            var manageButton = new Button
            {
                Text = "▶️ Manage Subscription",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(6, 4, 6, 4),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            manageButton.FlatAppearance.BorderSize = 0;

            manageButton.Click += (s, e) =>
            {
                ToggleOverlayPanel();
            };

            flowTopics.Controls.Add(manageButton);

            // -------------------------------
            // 2. Regular Topic Chips
            // -------------------------------
            foreach (string topic in _selectedSubTopics.Values)
            {
                var chipPanel = new Panel
                {
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Margin = new Padding(6, 4, 6, 4),
                    Padding = new Padding(10, 6, 10, 6),
                    Height = 32
                };

                var label = new Label
                {
                    Text = topic,
                    AutoSize = true,
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                    Margin = new Padding(0, 0, 4, 0),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var closeButton = new Button
                {
                    Text = "×",
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(22, 22),
                    Margin = new Padding(5, 0, 0, 0),
                    BackColor = Color.Transparent,
                    ForeColor = Color.DimGray,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    TabStop = false
                };

                closeButton.FlatAppearance.BorderSize = 0;
                closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 0, 0);
                closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 0, 0);

                closeButton.Click += async (s, e) =>
                {
                    flowTopics.Controls.Remove(chipPanel);
                    string[] updatedTopics = _selectedSubTopics.Where(t => t.Value != topic).Select(t => t.Key).ToArray();
                    var keyToRemove = _selectedSubTopics.FirstOrDefault(t => t.Value == topic).Key;
                    if (!string.IsNullOrEmpty(keyToRemove))
                    {
                        _selectedSubTopics.Remove(keyToRemove);
                        await UpdateTopicOrKeywordAsync(true, string.Join(",", _selectedSubTopics.Keys));
                    }
                };

                chipPanel.Controls.Add(label);
                chipPanel.Controls.Add(closeButton);

                label.Location = new Point(6, (chipPanel.Height - label.Height) / 2);
                closeButton.Location = new Point(label.Right + 8, (chipPanel.Height - closeButton.Height) / 2);
                chipPanel.Width = label.Width + closeButton.Width + 28;

                chipPanel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var rect = chipPanel.ClientRectangle;
                    rect.Inflate(-1, -1);

                    var path = RoundedRect(rect, 14);
                    chipPanel.Region = new Region(path);

                    using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                               rect,
                               Color.FromArgb(240, 240, 240),
                               Color.FromArgb(220, 220, 220),
                               45F))
                    {
                        g.FillPath(brush, path);
                    }

                    using (var pen = new Pen(Color.LightGray, 1))
                    {
                        g.DrawPath(pen, path);
                    }

                    path.Dispose();
                };

                flowTopics.Controls.Add(chipPanel);
            }

            InitializeOverlayPanel(); // Ensure overlay is ready
        }

        private void InitializeOverlayPanel()
        {
            if (overlayPanel != null)
                return;

            // === Base Overlay Panel ===
            overlayPanel = new Panel
            {
                Size = flowTopics.Size,
                Location = flowTopics.Location,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                AutoScroll = true
            };

            // === Header Container ===
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10, 8, 10, 8)
            };

            // Title Label
            var headerLabel = new Label
            {
                Text = "▼ Manage Subscription",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Close Button (✕)
            var closeButton = new Button
            {
                Text = "✕",
                Size = new Size(28, 28),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Red,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) =>
            {
                overlayPanel.Visible = false;
                overlayVisible = false;
            };

            // Add header controls
            headerPanel.Controls.Add(closeButton);
            headerPanel.Controls.Add(headerLabel);

            // === Category Button ===
            var categoryBtn = new Button
            {
                Text = "Category Subscription",
                Size = new Size(200, 30),
                Location = new Point(10, headerPanel.Bottom + 10)
            };
            categoryBtn.Click += (s, e) => CategorywiseSubscription();

            // === Region Button ===
            var regionBtn = new Button
            {
                Text = "Region Subscription",
                Size = new Size(200, 30),
                Location = new Point(10, categoryBtn.Bottom + 10)
            };
            regionBtn.Click += (s, e) => RegionwiseSubscription();

            // === Add everything to overlay ===
            overlayPanel.Controls.Add(headerPanel);
            overlayPanel.Controls.Add(categoryBtn);
            overlayPanel.Controls.Add(regionBtn);

            // Adjust button layout when resized
            overlayPanel.Resize += (s, e) =>
            {
                categoryBtn.Location = new Point(10, headerPanel.Bottom + 10);
                regionBtn.Location = new Point(10, categoryBtn.Bottom + 10);
            };

            // Add overlay to parent
            this.Controls.Add(overlayPanel);
            overlayPanel.BringToFront();
        }


        private void ToggleOverlayPanel()
        {
            overlayVisible = !overlayVisible;
            overlayPanel.Visible = overlayVisible;
            if (overlayVisible)
            {
                overlayPanel.BringToFront();
            }
        }

        private void GenerateKeywordsChips()
        {
            flowKeywords.Controls.Clear();

            // Create input TextBox for new keywords
            keywordInputTextBox = new TextBox
            {
                Width = 150,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Margin = new Padding(5),
                Text = "Add keyword..."
            };
            keywordInputTextBox.KeyDown += KeywordInputTextBox_KeyDown;

            flowKeywords.Controls.Add(keywordInputTextBox);

            // Add existing keyword chips
            foreach (var keyword in keywords)
            {
                var chip = CreateChip(keyword);
                flowKeywords.Controls.Add(chip);
            }
        }

        private async void KeywordInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Back)
            {
                var textBox = sender as TextBox;
                if (textBox == null)
                    return;

                int cursorPos = textBox.SelectionStart;

                if (cursorPos == 0)
                    return;

                // Find index of previous word boundary
                int lastSpace = textBox.Text.LastIndexOf(' ', cursorPos - 1);

                // If no space found, delete from start
                int startDelete = lastSpace >= 0 ? lastSpace + 1 : 0;
                int lengthToRemove = cursorPos - startDelete;

                // Remove the word
                textBox.Text = textBox.Text.Remove(startDelete, lengthToRemove);
                textBox.SelectionStart = startDelete; // Set cursor position

                e.SuppressKeyPress = true; // Prevent default Backspace behavior
            }
            if (e.KeyCode == Keys.Enter)
            {
                string newKeyword = keywordInputTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newKeyword) && !keywords.Contains(newKeyword, StringComparer.OrdinalIgnoreCase))
                {
                    // Update keywords array
                    var newKeywordsList = keywords.ToList();
                    newKeywordsList.Add(newKeyword);
                    keywords = newKeywordsList.ToArray();

                    // Add new chip to flowKeywords panel
                    var chip = CreateChip(newKeyword);
                    flowKeywords.Controls.Add(chip);

                    // Clear input box
                    keywordInputTextBox.Clear();

                    // Update server
                    await UpdateTopicOrKeywordAsync(false, String.Join(",", keywords));
                }
                else
                {
                    MessageBox.Show("Keyword Already Exists!", "Duplication Keyword", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private Panel CreateChip(string text)
        {
            var chipPanel = new Panel
            {
                BackColor = Color.Transparent, // Light pastel tone
                AutoSize = true,
                Margin = new Padding(6, 4, 6, 4),
                Padding = new Padding(10, 6, 10, 6),
                Height = 32,
            };

            var label = new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Margin = new Padding(0, 0, 4, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var closeButton = new Button
            {
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(22, 22),
                Margin = new Padding(5, 0, 0, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.DimGray,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TabStop = false
            };

            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 0, 0);
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 0, 0);
            closeButton.Click += async (s, e) =>
            {
                flowKeywords.Controls.Remove(chipPanel);
                // Update keywords array by removing the keyword
                keywords = keywords.Where(k => !k.Equals(text, StringComparison.OrdinalIgnoreCase)).ToArray();


                // Update server
                await UpdateTopicOrKeywordAsync(false, String.Join(",", keywords));
            };

            chipPanel.Controls.Add(label);
            chipPanel.Controls.Add(closeButton);

            // Layout inside chipPanel
            label.Location = new Point(6, (chipPanel.Height - label.Height) / 2);
            closeButton.Location = new Point(label.Right + 8, (chipPanel.Height - closeButton.Height) / 2);
            chipPanel.Width = label.Width + closeButton.Width + 28;

            chipPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rect = chipPanel.ClientRectangle;
                rect.Inflate(-1, -1); // shrink to prevent edge clipping

                var path = RoundedRect(rect, 14);

                // Set rounded clip region so the panel itself looks rounded
                chipPanel.Region = new Region(path);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect,
                    Color.FromArgb(240, 240, 240),
                    Color.FromArgb(220, 220, 220),
                    45F))
                {
                    g.FillPath(brush, path); // Fills only the rounded area
                }

                using (var pen = new Pen(Color.LightGray, 1))
                {
                    g.DrawPath(pen, path); // Optional border
                }

                path.Dispose();
            };

            return chipPanel;
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            path.StartFigure();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private async void UpdateDNDStatus(object sender, EventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio == null || !radio.Checked)
                return;  // Ignore unchecked events


            bool enabled = false;
            if (dndOn.Checked)
            {
                enabled = true;
                isDND = true;
            }
            else if (dndOff.Checked)
            {
                enabled = false;
                isDND = false;
            }

            var payload = new
            {
                userId,
                deviceId = Common.UUIDExtractor(),
                isDND = enabled
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var response = await client.PostAsync($"{apiUrl}update-status-dnd", httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("DND status updated successfully on server.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        thecalcify thecalcify = thecalcify.CurrentInstance;
                        thecalcify.DisconnectESCToolStripMenuItem_Click(null, null);
                        MessageBox.Show("Session expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to update DND status. Server responded: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating DND status: {ex.Message}");
                }
            }
        }

        public async Task UpdateTopicOrKeywordAsync(bool isTopic, string topicOrKeyword)
        {
            var payload = new
            {
                userId,
                isTopic,
                topicOrKeyword
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                try
                {
                    var response = await client.PostAsync($"{apiUrl}update-topic-keyword", content);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ {(isTopic ? "Topics" : "Keywords")} updated successfully.");
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        thecalcify thecalcify = thecalcify.CurrentInstance;
                        thecalcify.DisconnectESCToolStripMenuItem_Click(null, null);
                        MessageBox.Show("Session expired. Please log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Failed to update {(isTopic ? "topics" : "keywords")}. Status: {response.StatusCode}, Response: {responseText}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❗ Error updating {(isTopic ? "topics" : "keywords")}: {ex.Message}");
                }
            }
        }

        private void CategorywiseSubscription()
        {
            string jsonCategoriesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories.json");
            string jsonContent = File.ReadAllText(jsonCategoriesFilePath);

            // Merge topics[] and _selectedSubTopics
            var mergedSubTopics = new Dictionary<string, string>(_selectedSubTopics);


            foreach (var topic in topics.Where(topic => !mergedSubTopics.ContainsKey(topic)))
            {
                mergedSubTopics[topic] = topic;
            }

            using (var NewsSubscribeForm = new NewsSubscriptionList(jsonContent, "category", mergedSubTopics,Instance))
            {
                NewsSubscribeForm.ShowDialog();
            }
        }

        private void RegionwiseSubscription()
        {
            string jsonRegionsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Regions.json");
            string jsonContent = File.ReadAllText(jsonRegionsFilePath);

            // Merge topics[] and _selectedSubTopics
            var mergedSubTopics = new Dictionary<string, string>(_selectedSubTopics);


            foreach (var topic in topics.Where(topic => !mergedSubTopics.ContainsKey(topic)))
            {
                mergedSubTopics[topic] = topic;
            }

            using (var NewsSubscribeForm = new NewsSubscriptionList(jsonContent, "region", mergedSubTopics, Instance))
            {
                NewsSubscribeForm.ShowDialog();
            }
        }

        public async Task UpdateSelectedSubTopics(Dictionary<string, string> selectedSubTopics)
        {
            _selectedSubTopics = selectedSubTopics;

            await UpdateTopicOrKeywordAsync(true, string.Join(",", selectedSubTopics.Keys));

            //InitializeSelectedSubTopics();
            GenerateTopicChips();
        }

        private void InitializeSelectedSubTopics()
        {
            string categoriesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories.json");
            string regionsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Regions.json");

            if (_topicTitleLookup == null)
            {
                var categories = LoadJsonTopics(categoriesPath);
                var regions = LoadJsonTopics(regionsPath);

                _topicTitleLookup = categories
                    .Concat(regions)
                    .GroupBy(t => t.Code)
                    .ToDictionary(g => g.Key, g => g.First().Title);
            }

            _selectedSubTopics.Clear();

            foreach (var topic in topics)
            {
                if (_topicTitleLookup.TryGetValue(topic, out string title))
                    _selectedSubTopics[topic] = title;
                else
                    _selectedSubTopics[topic] = topic; // Use key if not found
            }
        }

        private List<TopicItem> LoadJsonTopics(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                return new List<TopicItem>();

            var jsonContent = File.ReadAllText(jsonFilePath);

            var categoriesOrRegions = JsonConvert.DeserializeObject<List<CategoryOrRegion>>(jsonContent);

            return categoriesOrRegions.SelectMany(c => c.Topics).ToList();
        }

        public class CategoryOrRegion
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("topics")]
            public List<TopicItem> Topics { get; set; }
        }

        public class TopicItem
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }
        }

        private void NewsSetting_Leave(object sender, EventArgs e)
        {
            thecalcify thecalcify = thecalcify.CurrentInstance;
            thecalcify.isDND = isDND;
            thecalcify.keywords = string.Join(",", keywords);
            thecalcify.topics = string.Join(",", _selectedSubTopics.Keys);
        }
    }
}