using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thecalcify.News
{
    public partial class NewsSubscribeList : UserControl
    {
        private readonly string jsonCategoriesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Categories.json");
        private readonly string jsonRegionsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Regions.json");

        public NewsSubscribeList()
        {
            InitializeComponent();
            LoadCategoriesFromFile();
        }

        private void LoadCategoriesFromFile()
        {
            try
            {
                if (!File.Exists(jsonCategoriesFilePath))
                {
                    MessageBox.Show($"JSON file not found: {jsonCategoriesFilePath}");
                    return;
                }

                string jsonContent = File.ReadAllText(jsonCategoriesFilePath);
                var categories = JsonConvert.DeserializeObject<List<NewsCategory>>(jsonContent);

                DisplayCategories(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JSON:\n{ex.Message}");
            }
        }

        private void DisplayCategories(List<NewsCategory> categories)
        {
            scrollPanel.Controls.Clear();
            int top = 10;
            int margin = 10;
            int groupBoxWidth = scrollPanel.ClientSize.Width - 25;

            foreach (var category in categories)
            {
                var groupBox = new GroupBox
                {
                    Text = category.title,
                    Location = new Point(margin, top),
                    Width = groupBoxWidth,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Padding = new Padding(10, 30, 10, 10)
                };

                var contentPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 2
                };
                contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

                var mainCheckBox = new CheckBox
                {
                    Text = category.title,
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular)
                };

                var expandButton = new Button
                {
                    Text = "▼", // down arrow initially
                    AutoSize = true,
                    Anchor = AnchorStyles.Right,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.LightGray,
                    Padding = new Padding(5, 0, 5, 0)
                };

                contentPanel.Controls.Add(mainCheckBox, 0, 0);
                contentPanel.Controls.Add(expandButton, 1, 0);
                groupBox.Controls.Add(contentPanel);

                var topicPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Visible = false,
                    Margin = new Padding(0, 10, 0, 0)
                };

                foreach (var topic in category.topics)
                {
                    var topicContainer = new TableLayoutPanel
                    {
                        AutoSize = true,
                        ColumnCount = 1,
                        Padding = new Padding(5),
                        BackColor = Color.FromArgb(245, 245, 245),
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    var topicCheckBox = new CheckBox
                    {
                        Text = topic.title,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Regular)
                    };

                    var topicDescription = new Label
                    {
                        Text = topic.description,
                        AutoSize = true,
                        MaximumSize = new Size(groupBoxWidth - 50, 0),
                        Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                        ForeColor = Color.DimGray,
                        Margin = new Padding(20, 0, 0, 0)
                    };

                    topicContainer.Controls.Add(topicCheckBox);
                    topicContainer.Controls.Add(topicDescription);
                    topicPanel.Controls.Add(topicContainer);
                }

                expandButton.Click += (s, e) =>
                {
                    topicPanel.Visible = !topicPanel.Visible;
                    expandButton.Text = topicPanel.Visible ? "▲" : "▼";
                };

                groupBox.Controls.Add(topicPanel);
                scrollPanel.Controls.Add(groupBox);

                // Adjust height automatically due to AutoSize
                top += groupBox.Height + margin;
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
