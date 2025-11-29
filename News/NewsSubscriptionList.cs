using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.News
{
    public partial class NewsSubscriptionList : Form
    {
        private string jsonData;
        private Dictionary<string, string> _selectedSubTopics = new Dictionary<string, string>();
        public object newsSettingInstance;

        public NewsSubscriptionList(string json, string type, Dictionary<string,string> selectedSubTopics, NewsSetting newsSetting)
        {
            InitializeComponent();
            jsonData = json;
            newsSettingInstance = newsSetting;
            _selectedSubTopics = selectedSubTopics ?? new Dictionary<string, string>();
            Load += NewsSubscriptionList_Load;
            this.Text = type.Equals("region", StringComparison.OrdinalIgnoreCase) ? "News Regions" : "News Categories";
        }

        private void NewsSubscriptionList_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var categories = await Task.Run(() =>
                    System.Text.Json.JsonSerializer.Deserialize<List<NewsCategory>>(jsonData, options)
                );

                if (categories != null && categories.Any())
                {
                    //Console.WriteLine($"Deserialized {categories.Count} categories");
                    DisplayCategories(categories);
                }
                else
                {
                    MessageBox.Show("No data found or list is empty.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void DisplayCategories(List<NewsCategory> categories)
        {
            SplashManager.Show(this, "Loading", $"Working on {this.Text}...");

            Task.Run(() =>
            {
                List<GroupBox> groupBoxes = new List<GroupBox>();
                int panelWidth = 0;

                this.Invoke(new MethodInvoker(() =>
                {
                    panelWidth = userpanel.ClientSize.Width - 25;
                }));

                Font categoryFont = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
                Font topicFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
                Font descriptionFont = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic);

                foreach (var category in categories)
                {
                    GroupBox groupBox = new GroupBox
                    {
                        Text = "",
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Width = panelWidth,
                        Font = categoryFont,
                        Margin = new Padding(10),
                        Padding = new Padding(10)
                    };

                    TableLayoutPanel container = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        AutoSize = true,
                        ColumnCount = 1
                    };

                    TableLayoutPanel header = new TableLayoutPanel
                    {
                        AutoSize = true,
                        ColumnCount = 2,
                        Dock = DockStyle.Top
                    };

                    header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));

                    CheckBox mainCheckBox = new CheckBox
                    {
                        Text = category.title,
                        AutoSize = true,
                        MaximumSize = new Size(panelWidth - 50, 0),
                        Font = categoryFont,
                        Margin = new Padding(25, 0, 3, 0),
                        ThreeState = true
                    };

                    Button expandButton = new Button
                    {
                        Text = "▼",
                        Width = 30,
                        Height = 30,
                        BackColor = Color.LightGray,
                        Dock = DockStyle.Fill
                    };

                    FlowLayoutPanel topicsPanel = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        Dock = DockStyle.Top,
                        Visible = false,
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false,
                        Margin = new Padding(30, 0, 0, 10)
                    };

                    header.Controls.Add(mainCheckBox, 0, 0);
                    header.Controls.Add(expandButton, 1, 0);
                    container.Controls.Add(header, 0, 0);
                    container.Controls.Add(topicsPanel, 0, 1);
                    groupBox.Controls.Add(container);

                    // --- Track and sync ---
                    List<CheckBox> topicBoxes = new List<CheckBox>();
                    bool topicsLoaded = false;

                    expandButton.Click += (s, e) =>
                    {
                        topicsPanel.Visible = !topicsPanel.Visible;
                        expandButton.Text = topicsPanel.Visible ? "▲" : "▼";

                        if (topicsPanel.Visible && !topicsLoaded)
                        {
                            SplashManager.Show(this);

                            // Lazy load topics
                            foreach (var topic in category.topics)
                            {
                                bool isChecked = _selectedSubTopics.ContainsKey(topic.code);

                                CheckBox topicCheckBox = new CheckBox
                                {
                                    Text = topic.title,
                                    AutoSize = true,
                                    Font = topicFont,
                                    Checked = isChecked,
                                    Margin = new Padding(3, 3, 3, 0)
                                };

                                Label desc = new Label
                                {
                                    Text = topic.description,
                                    AutoSize = true,
                                    MaximumSize = new Size(panelWidth - 50, 0),
                                    Font = descriptionFont,
                                    ForeColor = Color.Gray,
                                    Margin = new Padding(25, 0, 3, 0)
                                };

                                topicBoxes.Add(topicCheckBox);
                                topicsPanel.Controls.Add(topicCheckBox);
                                topicsPanel.Controls.Add(desc);

                                if (isChecked)
                                {
                                    _selectedSubTopics[topic.code] = topic.title;
                                }

                                topicCheckBox.CheckedChanged += (sender, ev) =>
                                {
                                    if (topicCheckBox.Checked)
                                    {
                                        _selectedSubTopics[topic.code] = topic.title;
                                    }
                                    else
                                    {
                                        _selectedSubTopics.Remove(topic.code);
                                    }

                                    // Update parent state dynamically
                                    UpdateMainCheckboxState(mainCheckBox, topicBoxes, category);
                                };
                            }

                            topicsLoaded = true;

                            // Initial sync once topics are added
                            UpdateMainCheckboxState(mainCheckBox, topicBoxes, category);

                            SplashManager.Hide();
                        }
                    };

                    mainCheckBox.Click += (s, e) =>
                    {
                        bool selectAll = mainCheckBox.CheckState == CheckState.Checked;

                        if (!topicsLoaded)
                        {
                            // If not expanded yet, pre-load selection into dictionary
                            foreach (var topic in category.topics)
                            {
                                if (selectAll)
                                    _selectedSubTopics[topic.code] = topic.title;
                                else
                                    _selectedSubTopics.Remove(topic.code);
                            }
                        }
                        else
                        {
                            foreach (var cb in topicBoxes)
                            {
                                cb.Checked = selectAll;
                            }
                        }

                        mainCheckBox.CheckState = selectAll ? CheckState.Checked : CheckState.Unchecked;
                    };

                    // Check if any of this category's topics are already selected
                    int selectedCount = category.topics.Count(t => _selectedSubTopics.ContainsKey(t.code));
                    if (selectedCount == 0)
                        mainCheckBox.CheckState = CheckState.Unchecked;
                    else if (selectedCount == category.topics.Count)
                        mainCheckBox.CheckState = CheckState.Checked;
                    else
                        mainCheckBox.CheckState = CheckState.Indeterminate;

                    groupBoxes.Add(groupBox);
                }

                this.Invoke(new MethodInvoker(() =>
                {
                    userpanel.SuspendLayout();
                    userpanel.Controls.Clear();
                    userpanel.FlowDirection = FlowDirection.TopDown;
                    userpanel.WrapContents = false;
                    userpanel.AutoScroll = true;
                    foreach (var gb in groupBoxes)
                        userpanel.Controls.Add(gb);
                    userpanel.ResumeLayout();
                    SplashManager.Hide();
                }));
            });
        }

        private void UpdateMainCheckboxState(CheckBox mainCheckBox, List<CheckBox> topicBoxes, NewsCategory category)
        {
            int checkedCount = topicBoxes.Count(cb => cb.Checked);
            int totalCount = topicBoxes.Count;

            if (checkedCount == 0)
                mainCheckBox.CheckState = CheckState.Unchecked;
            else if (checkedCount == totalCount)
                mainCheckBox.CheckState = CheckState.Checked;
            else
                mainCheckBox.CheckState = CheckState.Indeterminate;
        }


        private async void NewsSubscriptionList_FormClosed(object sender, FormClosedEventArgs e)
        {
            var result = MessageBox.Show("Would you like to Save your news topic subscriptions?", "News Subscription", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                if (newsSettingInstance is NewsSetting newsSetting)
                    await newsSetting.UpdateSelectedSubTopics(_selectedSubTopics);
            }
            else
            {
                return;
            }
            
        }


        public class NewsCategory
        {
            public string title { get; set; }
            public List<NewsTopic> topics { get; set; }
        }

        public class NewsTopic
        {
            public string code { get; set; }
            public string title { get; set; }
            public string description { get; set; }
        }

    }
}