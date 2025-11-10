using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using thecalcify.Properties;

namespace thecalcify.News
{
    partial class NewsControl
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.DataGridView dgvNews;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.ComboBox cmbSubCategory;
        private System.Windows.Forms.Button btnSearchNews;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Label lblSubCategory;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnPrevPage;
        private DateTime? startDate = null;
        private DateTime? endDate = null;


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PeriodicDispose(); // Cancel and wait for background task
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewsControl));
            this.dgvNews = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGVTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DVGCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DVGSubCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.cmbSubCategory = new System.Windows.Forms.ComboBox();
            this.btnSearchNews = new System.Windows.Forms.Button();
            this.lblCategory = new System.Windows.Forms.Label();
            this.lblSubCategory = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.newsSearch = new System.Windows.Forms.TextBox();
            this.newsUpdateLable = new System.Windows.Forms.Label();
            this.txtDateRange = new System.Windows.Forms.TextBox();
            this.monthCalendar = new System.Windows.Forms.MonthCalendar();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnNextPage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNews)).BeginInit();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnRefresh)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvNews
            // 
            this.dgvNews.AllowDrop = true;
            this.dgvNews.AllowUserToAddRows = false;
            this.dgvNews.AllowUserToDeleteRows = false;
            this.dgvNews.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.dgvNews.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvNews.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNews.BackgroundColor = System.Drawing.Color.White;
            this.dgvNews.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvNews.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvNews.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvNews.ColumnHeadersHeight = 40;
            this.dgvNews.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvNews.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.DGVTitle,
            this.DVGCategory,
            this.DVGSubCategory});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvNews.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvNews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNews.EnableHeadersVisualStyles = false;
            this.dgvNews.GridColor = System.Drawing.Color.Gainsboro;
            this.dgvNews.Location = new System.Drawing.Point(0, 291);
            this.dgvNews.Name = "dgvNews";
            this.dgvNews.ReadOnly = true;
            this.dgvNews.RowHeadersVisible = false;
            this.dgvNews.RowHeadersWidth = 51;
            this.dgvNews.RowTemplate.Height = 36;
            this.dgvNews.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNews.Size = new System.Drawing.Size(1255, 159);
            this.dgvNews.TabIndex = 0;
            this.dgvNews.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvNews_CellDoubleClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.FillWeight = 10F;
            this.dataGridViewTextBoxColumn1.HeaderText = "Time";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // DGVTitle
            // 
            this.DGVTitle.FillWeight = 60F;
            this.DGVTitle.HeaderText = "Title";
            this.DGVTitle.MinimumWidth = 6;
            this.DGVTitle.Name = "DGVTitle";
            this.DGVTitle.ReadOnly = true;
            // 
            // DVGCategory
            // 
            this.DVGCategory.FillWeight = 20F;
            this.DVGCategory.HeaderText = "Category";
            this.DVGCategory.MinimumWidth = 6;
            this.DVGCategory.Name = "DVGCategory";
            this.DVGCategory.ReadOnly = true;
            // 
            // DVGSubCategory
            // 
            this.DVGSubCategory.FillWeight = 20F;
            this.DVGSubCategory.HeaderText = "SubCategory";
            this.DVGSubCategory.MinimumWidth = 6;
            this.DVGSubCategory.Name = "DVGSubCategory";
            this.DVGSubCategory.ReadOnly = true;
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(23, 41);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(180, 24);
            this.cmbCategory.TabIndex = 1;
            this.cmbCategory.Visible = false;
            // 
            // cmbSubCategory
            // 
            this.cmbSubCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubCategory.Enabled = false;
            this.cmbSubCategory.Location = new System.Drawing.Point(233, 41);
            this.cmbSubCategory.Name = "cmbSubCategory";
            this.cmbSubCategory.Size = new System.Drawing.Size(180, 24);
            this.cmbSubCategory.TabIndex = 3;
            this.cmbSubCategory.Visible = false;
            // 
            // btnSearchNews
            // 
            this.btnSearchNews.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            this.btnSearchNews.Location = new System.Drawing.Point(648, 36);
            this.btnSearchNews.Name = "btnSearchNews";
            this.btnSearchNews.Size = new System.Drawing.Size(99, 28);
            this.btnSearchNews.TabIndex = 4;
            this.btnSearchNews.Text = "Search";
            this.btnSearchNews.UseVisualStyleBackColor = false;
            this.btnSearchNews.Visible = false;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(22, 21);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(65, 16);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "Category:";
            this.lblCategory.Visible = false;
            // 
            // lblSubCategory
            // 
            this.lblSubCategory.AutoSize = true;
            this.lblSubCategory.Location = new System.Drawing.Point(231, 21);
            this.lblSubCategory.Name = "lblSubCategory";
            this.lblSubCategory.Size = new System.Drawing.Size(89, 16);
            this.lblSubCategory.TabIndex = 2;
            this.lblSubCategory.Text = "SubCategory:";
            this.lblSubCategory.Visible = false;
            // 
            // pnlTop
            // 
            this.pnlTop.AutoSize = true;
            this.pnlTop.BackColor = System.Drawing.Color.White;
            this.pnlTop.Controls.Add(this.btnRefresh);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.newsSearch);
            this.pnlTop.Controls.Add(this.newsUpdateLable);
            this.pnlTop.Controls.Add(this.lblCategory);
            this.pnlTop.Controls.Add(this.cmbCategory);
            this.pnlTop.Controls.Add(this.lblSubCategory);
            this.pnlTop.Controls.Add(this.cmbSubCategory);
            this.pnlTop.Controls.Add(this.btnSearchNews);
            this.pnlTop.Controls.Add(this.txtDateRange);
            this.pnlTop.Controls.Add(this.monthCalendar);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1255, 291);
            this.pnlTop.TabIndex = 1;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.Location = new System.Drawing.Point(1199, 26);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(29, 24);
            this.btnRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.TabStop = false;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(772, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "Search News :- ";
            // 
            // newsSearch
            // 
            this.newsSearch.Location = new System.Drawing.Point(769, 41);
            this.newsSearch.Name = "newsSearch";
            this.newsSearch.Size = new System.Drawing.Size(185, 22);
            this.newsSearch.TabIndex = 7;
            this.newsSearch.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            this.newsSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // newsUpdateLable
            // 
            this.newsUpdateLable.AutoSize = true;
            this.newsUpdateLable.Location = new System.Drawing.Point(986, 44);
            this.newsUpdateLable.Name = "newsUpdateLable";
            this.newsUpdateLable.Size = new System.Drawing.Size(148, 16);
            this.newsUpdateLable.TabIndex = 5;
            this.newsUpdateLable.Text = "Last News Recived At:- ";
            this.newsUpdateLable.Visible = false;
            // 
            // txtDateRange
            // 
            this.txtDateRange.ForeColor = System.Drawing.Color.Gray;
            this.txtDateRange.Location = new System.Drawing.Point(450, 41);
            this.txtDateRange.Name = "txtDateRange";
            this.txtDateRange.Size = new System.Drawing.Size(180, 22);
            this.txtDateRange.TabIndex = 9;
            this.txtDateRange.Text = "yyyy.MM.dd-yyyy.MM.dd";
            this.txtDateRange.Visible = false;
            this.txtDateRange.Click += new System.EventHandler(this.TxtDateRange_Click);
            this.txtDateRange.GotFocus += new System.EventHandler(this.TxtDateRange_GotFocus);
            this.txtDateRange.LostFocus += new System.EventHandler(this.TxtDateRange_LostFocus);
            // 
            // monthCalendar
            // 
            this.monthCalendar.Location = new System.Drawing.Point(450, 65);
            this.monthCalendar.MaxSelectionCount = 31;
            this.monthCalendar.Name = "monthCalendar";
            this.monthCalendar.TabIndex = 10;
            this.monthCalendar.Visible = false;
            this.monthCalendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.MonthCalendar_DateSelected);
            this.monthCalendar.LostFocus += new System.EventHandler(this.MonthCalendar_LostFocus);
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.White;
            this.pnlBottom.Controls.Add(this.btnPrevPage);
            this.pnlBottom.Controls.Add(this.lblPageInfo);
            this.pnlBottom.Controls.Add(this.btnNextPage);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 450);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlBottom.Size = new System.Drawing.Size(1255, 50);
            this.pnlBottom.TabIndex = 2;
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            this.btnPrevPage.Location = new System.Drawing.Point(13, 10);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(105, 30);
            this.btnPrevPage.TabIndex = 2;
            this.btnPrevPage.Text = "< Home";
            this.btnPrevPage.UseVisualStyleBackColor = false;
            this.btnPrevPage.Visible = false;
            this.btnPrevPage.Click += new System.EventHandler(this.BtnPrevPage_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageInfo.Location = new System.Drawing.Point(592, 15);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(111, 20);
            this.lblPageInfo.TabIndex = 0;
            this.lblPageInfo.Text = "Records :- 30";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPageInfo.Visible = false;
            // 
            // btnNextPage
            // 
            this.btnNextPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNextPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            this.btnNextPage.Location = new System.Drawing.Point(1137, 10);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(105, 30);
            this.btnNextPage.TabIndex = 1;
            this.btnNextPage.Text = "Next >";
            this.btnNextPage.UseVisualStyleBackColor = false;
            this.btnNextPage.Visible = false;
            this.btnNextPage.Click += new System.EventHandler(this.BtnNextPage_Click);
            // 
            // NewsControl
            // 
            this.Controls.Add(this.dgvNews);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Name = "NewsControl";
            this.Size = new System.Drawing.Size(1255, 500);
            ((System.ComponentModel.ISupportInitialize)(this.dgvNews)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnRefresh)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _cts?.Cancel(); // stop previous periodic fetch
                            // Restart periodic fetch after reload
            _cts = new CancellationTokenSource();
            _fetchTask = Task.Run(() => PeriodicFetchAsync(_cts.Token));

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = newsSearch.Text.Trim();

            // Split filter by comma, trim each part, and remove empty strings
            var keywords = filterText.Split(',')
                                     .Select(k => k.Trim())
                                     .Where(k => !string.IsNullOrEmpty(k))
                                     .ToList();

            if (keywords.Count == 0)
            {
                // Reset all rows visible in defaultGrid
                if (dgvNews != null)
                {
                    foreach (DataGridViewRow row in dgvNews.Rows)
                    {
                        if (!row.IsNewRow)
                            row.Visible = true;
                    }
                }
            }
            else
            {
                // Filter rows in defaultGrid based on "Name" column
                if (dgvNews != null)
                {
                    foreach (DataGridViewRow row in dgvNews.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["DGVTitle"].Value != null)
                        {
                            string name = row.Cells["DGVTitle"].Value.ToString();
                            bool match = keywords.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                            row.Visible = match;
                        }
                    }
                }
            }

        }

        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Check if Ctrl + Backspace is pressed
            if (e.Control && e.KeyCode == Keys.Back)
            {
                newsSearch.Clear();  // Clear all text 
                e.SuppressKeyPress = true; // Prevent default backspace behavior 
            }
        }

        private void TxtDateRange_Click(object sender, EventArgs e)
        {
            ShowCalendar();
        }

        private void TxtDateRange_GotFocus(object sender, EventArgs e)
        {
            if (txtDateRange.Text == "yyyy.MM.dd-yyyy.MM.dd")
            {
                txtDateRange.Text = "";
                txtDateRange.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void TxtDateRange_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDateRange.Text))
            {
                txtDateRange.Text = "yyyy.MM.dd-yyyy.MM.dd";
                txtDateRange.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void ShowCalendar()
        {
            // Set these properties when showing calendar or initializing:
            monthCalendar.MinDate = DateTime.Today.AddDays(-30);
            monthCalendar.MaxDate = DateTime.Today;
            monthCalendar.Visible = true;
            monthCalendar.BringToFront();
            monthCalendar.Focus();
        }

        private void MonthCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (startDate == null)
            {
                // First click sets start date
                startDate = e.Start;
                endDate = null; // reset end date
                monthCalendar.SetSelectionRange(startDate.Value, startDate.Value); // highlight just start date
                txtDateRange.Text = $"{startDate:yyyy.MM.dd}-";
                txtDateRange.ForeColor = Color.Black;
            }
            else if (endDate == null)
            {
                // Second click sets end date
                if (e.Start < startDate)
                {
                    // Swap if end date is before start date
                    endDate = startDate;
                    startDate = e.Start;
                }
                else
                {
                    endDate = e.Start;
                }
                monthCalendar.SetSelectionRange(startDate.Value, endDate.Value);
                txtDateRange.Text = $"{startDate:yyyy.MM.dd}-{endDate:yyyy.MM.dd}";
                txtDateRange.ForeColor = Color.Black;

                // Optionally hide calendar now or wait for user action
                monthCalendar.Visible = false;

                // Reset for next selection if you want:
                startDate = null;
                endDate = null;
            }
        }


        private void MonthCalendar_LostFocus(object sender, EventArgs e)
        {
            // Hide calendar if user clicks outside the calendar
            if (!txtDateRange.Focused)
            {
                monthCalendar.Visible = false;
            }
        }

        #endregion

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn DGVTitle;
        private DataGridViewTextBoxColumn DVGCategory;
        private DataGridViewTextBoxColumn DVGSubCategory;
        private Label newsUpdateLable;
        private TextBox newsSearch;
        private Label label1;
        private TextBox txtDateRange;
        private MonthCalendar monthCalendar;
        private PictureBox btnRefresh;
    }
}