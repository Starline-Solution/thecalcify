using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.News
{
    public partial class NewsSubscriptionList : Form
    {
        private string jsonData;

        public NewsSubscriptionList(string json, string type)
        {
            InitializeComponent();
            jsonData = json;
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
            // Show the splash screen
            SplashManager.Show(this, "Loading", $"Working on {this.Text}...");

            Task.Run(new Action(() =>
            {
                // Collect controls on background thread
                List<GroupBox> groupBoxes = new List<GroupBox>();

                int panelWidth = 0;

                // Get panel width from UI thread
                this.Invoke(new MethodInvoker(() =>
                {
                    panelWidth = userpanel.ClientSize.Width - 25;
                }));

                int groupBoxMargin = 10;

                Font categoryFont = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
                Font topicFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
                Font descriptionFont = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic);

                foreach (NewsCategory category in categories)
                {
                    GroupBox groupBox = new GroupBox
                    {
                        Text = "",
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Width = panelWidth,
                        Font = categoryFont,
                        Margin = new Padding(groupBoxMargin),
                        Padding = new Padding(10),
                    };

                    groupBox.SuspendLayout();

                    TableLayoutPanel mainContainer = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        AutoSize = true,
                        ColumnCount = 1,
                    };

                    TableLayoutPanel headerPanel = new TableLayoutPanel
                    {
                        AutoSize = true,
                        ColumnCount = 2,
                        Dock = DockStyle.Top,
                        Margin = new Padding(0),
                    };

                    headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));

                    CheckBox mainCheckBox = new CheckBox
                    {
                        Text = category.title,
                        AutoSize = true,
                        MaximumSize = new Size(panelWidth - 50, 0),
                        Font = categoryFont,
                        ForeColor = Color.Black,
                        Margin = new Padding(25, 0, 3, 0),
                        TextAlign = ContentAlignment.TopLeft,
                        ThreeState = true
                    };

                    Button expandButton = new Button
                    {
                        Text = "▼",
                        Width = 30,
                        Height = 30,
                        BackColor = Color.LightGray,
                        ForeColor = Color.Black,
                        Dock = DockStyle.Fill
                    };

                    headerPanel.Controls.Add(mainCheckBox, 0, 0);
                    headerPanel.Controls.Add(expandButton, 1, 0);

                    FlowLayoutPanel topicsPanel = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        Dock = DockStyle.Top,
                        Visible = false,
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false,
                        Margin = new Padding(30, 0, 0, 10)
                    };

                    List<CheckBox> topicCheckBoxes = new List<CheckBox>();

                    foreach (var topic in category.topics)
                    {
                        CheckBox topicCheckBox = new CheckBox
                        {
                            Text = topic.title,
                            AutoSize = true,
                            Font = topicFont,
                            Margin = new Padding(3, 3, 3, 0)
                        };

                        Label descriptionLabel = new Label
                        {
                            Text = topic.description,
                            AutoSize = true,
                            MaximumSize = new Size(panelWidth - 50, 0),
                            Font = descriptionFont,
                            ForeColor = Color.Gray,
                            Margin = new Padding(25, 0, 3, 0),
                            TextAlign = ContentAlignment.TopLeft
                        };

                        topicCheckBoxes.Add(topicCheckBox);

                        topicCheckBox.CheckedChanged += delegate (object sender, EventArgs e)
                        {
                            int checkedCount = 0;
                            foreach (CheckBox cb in topicCheckBoxes)
                            {
                                if (cb.Checked) checkedCount++;
                            }

                            if (checkedCount == 0)
                                mainCheckBox.CheckState = CheckState.Unchecked;
                            else if (checkedCount == topicCheckBoxes.Count)
                                mainCheckBox.CheckState = CheckState.Checked;
                            else
                                mainCheckBox.CheckState = CheckState.Indeterminate;
                        };

                        topicsPanel.Controls.Add(topicCheckBox);
                        topicsPanel.Controls.Add(descriptionLabel);
                    }

                    mainCheckBox.Click += delegate (object sender, EventArgs e)
                    {
                        bool newCheckedState = false;

                        foreach (CheckBox cb in topicCheckBoxes)
                        {
                            if (!cb.Checked)
                            {
                                newCheckedState = true;
                                break;
                            }
                        }

                        foreach (CheckBox cb in topicCheckBoxes)
                        {
                            cb.CheckedChanged -= delegate { };
                            cb.Checked = newCheckedState;
                            cb.CheckedChanged += delegate { };
                        }

                        mainCheckBox.CheckState = newCheckedState ? CheckState.Checked : CheckState.Unchecked;
                    };

                    expandButton.Click += delegate (object sender, EventArgs e)
                    {
                        topicsPanel.Visible = !topicsPanel.Visible;
                        expandButton.Text = topicsPanel.Visible ? "▲" : "▼";
                    };

                    mainContainer.Controls.Add(headerPanel, 0, 0);
                    mainContainer.Controls.Add(topicsPanel, 0, 1);
                    groupBox.Controls.Add(mainContainer);
                    groupBox.ResumeLayout();

                    groupBoxes.Add(groupBox);
                }

                // Update UI on UI thread
                this.Invoke(new MethodInvoker(delegate
                {
                    userpanel.SuspendLayout();
                    userpanel.Controls.Clear();
                    userpanel.FlowDirection = FlowDirection.TopDown;
                    userpanel.WrapContents = false;
                    userpanel.AutoScroll = true;
                    userpanel.AutoSize = true;

                    foreach (GroupBox gb in groupBoxes)
                    {
                        userpanel.Controls.Add(gb);
                    }

                    userpanel.ResumeLayout();

                    // Hide the splash after UI update
                    SplashManager.Hide();
                }));
            }));
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