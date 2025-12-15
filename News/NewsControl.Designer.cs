using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;
using thecalcify.Properties;
using System.ComponentModel;

namespace thecalcify.News
{
    partial class NewsControl
    {
        private IContainer components = null;

        private DataGridView dgvNews;
        private ComboBox cmbCategory;
        private ComboBox cmbSubCategory;
        private Button btnSearchNews;
        private Label lblCategory;
        private Label lblSubCategory;
        private Panel pnlTop;
        private Panel pnlBottom;
        private Label lblPageInfo;
        private Button btnNextPage;
        private Button btnPrevPage;
        private DateTime? startDate = null;
        private DateTime? endDate = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Logic preserved from original file
                try { PeriodicDispose(); } catch { }
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            dgvNews = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            DGVTitle = new DataGridViewTextBoxColumn();
            DVGCategory = new DataGridViewTextBoxColumn();
            DVGSubCategory = new DataGridViewTextBoxColumn();
            cmbCategory = new ComboBox();
            cmbSubCategory = new ComboBox();
            btnSearchNews = new Button();
            lblCategory = new Label();
            lblSubCategory = new Label();
            pnlTop = new Panel();
            tocalender = new MonthCalendar();
            fromcalender = new MonthCalendar();
            todateTextbox = new TextBox();
            fromTextbox = new TextBox();
            btnRefresh = new Button();
            label1 = new Label();
            newsSearch = new TextBox();
            newsUpdateLable = new Label();
            pnlBottom = new Panel();
            btnPrevPage = new Button();
            lblPageInfo = new Label();
            btnNextPage = new Button();
            ((ISupportInitialize)(dgvNews)).BeginInit();
            pnlTop.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // dgvNews
            // 
            dgvNews.AllowDrop = true;
            dgvNews.AllowUserToAddRows = false;
            dgvNews.AllowUserToDeleteRows = false;
            dgvNews.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            dgvNews.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvNews.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvNews.BackgroundColor = Color.White;
            dgvNews.BorderStyle = BorderStyle.None;
            dgvNews.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvNews.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.Padding = new Padding(10, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvNews.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvNews.ColumnHeadersHeight = 45;
            dgvNews.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvNews.Columns.AddRange(new DataGridViewColumn[] {
            dataGridViewTextBoxColumn1,
            DGVTitle,
            DVGCategory,
            DVGSubCategory});
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 16F);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            dataGridViewCellStyle3.Padding = new Padding(5, 0, 0, 0);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            dataGridViewCellStyle3.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvNews.DefaultCellStyle = dataGridViewCellStyle3;
            dgvNews.Dock = DockStyle.Fill;
            dgvNews.EnableHeadersVisualStyles = false;
            dgvNews.GridColor = Color.WhiteSmoke;
            dgvNews.Location = new Point(0, 306);
            dgvNews.Name = "dgvNews";
            dgvNews.ReadOnly = true;
            dgvNews.RowHeadersVisible = false;
            dgvNews.RowHeadersWidth = 51;
            dgvNews.RowTemplate.Height = 40;
            dgvNews.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNews.Size = new Size(1255, 144);
            dgvNews.TabIndex = 0;
            dgvNews.CellDoubleClick += new DataGridViewCellEventHandler(DgvNews_CellDoubleClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewTextBoxColumn1.HeaderText = "Time";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.Width = 150;
            // 
            // DGVTitle
            // 
            DGVTitle.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DGVTitle.HeaderText = "Title";
            DGVTitle.MinimumWidth = 6;
            DGVTitle.Name = "DGVTitle";
            DGVTitle.ReadOnly = true;
            // 
            // DVGCategory
            // 
            DVGCategory.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            DVGCategory.HeaderText = "Category";
            DVGCategory.MinimumWidth = 6;
            DVGCategory.Name = "DVGCategory";
            DVGCategory.ReadOnly = true;
            DVGCategory.Width = 150;
            // 
            // DVGSubCategory
            // 
            DVGSubCategory.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            DVGSubCategory.HeaderText = "SubCategory";
            DVGSubCategory.MinimumWidth = 6;
            DVGSubCategory.Name = "DVGSubCategory";
            DVGSubCategory.ReadOnly = true;
            DVGSubCategory.Width = 150;
            // 
            // cmbCategory
            // 
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Font = new Font("Segoe UI", 10F);
            cmbCategory.Location = new Point(25, 45);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new Size(180, 31);
            cmbCategory.TabIndex = 1;
            cmbCategory.Visible = false;
            // 
            // cmbSubCategory
            // 
            cmbSubCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSubCategory.Enabled = false;
            cmbSubCategory.Font = new Font("Segoe UI", 10F);
            cmbSubCategory.Location = new Point(220, 45);
            cmbSubCategory.Name = "cmbSubCategory";
            cmbSubCategory.Size = new Size(180, 31);
            cmbSubCategory.TabIndex = 3;
            cmbSubCategory.Visible = false;
            // 
            // btnSearchNews
            // 
            btnSearchNews.BackColor = Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            btnSearchNews.Cursor = Cursors.Hand;
            btnSearchNews.FlatAppearance.BorderSize = 0;
            btnSearchNews.FlatStyle = FlatStyle.Flat;
            btnSearchNews.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSearchNews.ForeColor = Color.White;
            btnSearchNews.Location = new Point(666, 43);
            btnSearchNews.Name = "btnSearchNews";
            btnSearchNews.Size = new Size(110, 32);
            btnSearchNews.TabIndex = 4;
            btnSearchNews.Text = "SEARCH";
            btnSearchNews.UseVisualStyleBackColor = false;
            btnSearchNews.Visible = false;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblCategory.ForeColor = Color.Black;
            lblCategory.Location = new Point(22, 22);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(76, 20);
            lblCategory.TabIndex = 0;
            lblCategory.Text = "Category:";
            lblCategory.Visible = false;
            // 
            // lblSubCategory
            // 
            lblSubCategory.AutoSize = true;
            lblSubCategory.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblSubCategory.ForeColor = Color.DimGray;
            lblSubCategory.Location = new Point(217, 22);
            lblSubCategory.Name = "lblSubCategory";
            lblSubCategory.Size = new Size(102, 20);
            lblSubCategory.TabIndex = 2;
            lblSubCategory.Text = "SubCategory:";
            lblSubCategory.Visible = false;
            // 
            // pnlTop
            // 
            pnlTop.AutoSize = true;
            pnlTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlTop.BackColor = Color.White;
            pnlTop.Controls.Add(tocalender);
            pnlTop.Controls.Add(fromcalender);
            pnlTop.Controls.Add(todateTextbox);
            pnlTop.Controls.Add(fromTextbox);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(label1);
            pnlTop.Controls.Add(newsSearch);
            pnlTop.Controls.Add(newsUpdateLable);
            pnlTop.Controls.Add(lblCategory);
            pnlTop.Controls.Add(cmbCategory);
            pnlTop.Controls.Add(lblSubCategory);
            pnlTop.Controls.Add(cmbSubCategory);
            pnlTop.Controls.Add(btnSearchNews);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(3, 10, 3, 3);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(10);
            pnlTop.Size = new Size(1255, 306);
            pnlTop.TabIndex = 1;
            // 
            // tocalender
            // 
            tocalender.Location = new Point(540, 80);
            tocalender.MaxSelectionCount = 31;
            tocalender.Name = "tocalender";
            tocalender.TabIndex = 12;
            tocalender.Visible = false;
            tocalender.DateSelected += new DateRangeEventHandler(tocalender_DateSelected);
            // 
            // fromcalender
            // 
            fromcalender.Location = new Point(415, 80);
            fromcalender.MaxSelectionCount = 31;
            fromcalender.Name = "fromcalender";
            fromcalender.TabIndex = 10;
            fromcalender.Visible = false;
            fromcalender.DateSelected += new DateRangeEventHandler(fromcalender_DateSelected);
            // 
            // todateTextbox
            // 
            todateTextbox.BackColor = Color.WhiteSmoke;
            todateTextbox.BorderStyle = BorderStyle.FixedSingle;
            todateTextbox.Font = new Font("Segoe UI", 10F);
            todateTextbox.Location = new Point(540, 45);
            todateTextbox.Name = "todateTextbox";
            todateTextbox.Size = new Size(120, 30);
            todateTextbox.TabIndex = 14;
            todateTextbox.Text = "To Date";
            todateTextbox.Visible = false;
            todateTextbox.Click += new EventHandler(todateTextbox_Click);
            todateTextbox.Leave += new EventHandler(todateTextbox_Leave);
            // 
            // fromTextbox
            // 
            fromTextbox.BackColor = Color.WhiteSmoke;
            fromTextbox.BorderStyle = BorderStyle.FixedSingle;
            fromTextbox.Font = new Font("Segoe UI", 10F);
            fromTextbox.Location = new Point(415, 45);
            fromTextbox.Name = "fromTextbox";
            fromTextbox.Size = new Size(120, 30);
            fromTextbox.TabIndex = 13;
            fromTextbox.Text = "From Date";
            fromTextbox.Visible = false;
            fromTextbox.Click += new EventHandler(fromTextbox_Click);
            fromTextbox.Leave += new EventHandler(fromTextbox_Leave);
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI Symbol", 17F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            btnRefresh.Location = new Point(1192, 31);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(50, 46);
            btnRefresh.TabIndex = 11;
            btnRefresh.Text = "↻";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += new EventHandler(BtnRefresh_Click);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(790, 22);
            label1.Name = "label1";
            label1.Size = new Size(100, 20);
            label1.TabIndex = 8;
            label1.Text = "Search News:";
            // 
            // newsSearch
            // 
            newsSearch.BorderStyle = BorderStyle.FixedSingle;
            newsSearch.Font = new Font("Segoe UI", 10F);
            newsSearch.Location = new Point(790, 45);
            newsSearch.Name = "newsSearch";
            newsSearch.Size = new Size(220, 30);
            newsSearch.TabIndex = 7;
            newsSearch.TextChanged += new EventHandler(TextBox1_TextChanged);
            newsSearch.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            // 
            // newsUpdateLable
            // 
            newsUpdateLable.AutoSize = true;
            newsUpdateLable.Font = new Font("Segoe UI", 8F);
            newsUpdateLable.ForeColor = Color.Black;
            newsUpdateLable.Location = new Point(1020, 50);
            newsUpdateLable.Name = "newsUpdateLable";
            newsUpdateLable.Size = new Size(132, 19);
            newsUpdateLable.TabIndex = 5;
            newsUpdateLable.Text = "Last Received At: -- ";
            newsUpdateLable.Visible = false;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.White;
            pnlBottom.Controls.Add(btnPrevPage);
            pnlBottom.Controls.Add(lblPageInfo);
            pnlBottom.Controls.Add(btnNextPage);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 450);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(10, 5, 10, 5);
            pnlBottom.Size = new Size(1255, 50);
            pnlBottom.TabIndex = 2;
            // 
            // btnPrevPage
            // 
            btnPrevPage.BackColor = Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            btnPrevPage.Cursor = Cursors.Hand;
            btnPrevPage.FlatAppearance.BorderSize = 0;
            btnPrevPage.FlatStyle = FlatStyle.Flat;
            btnPrevPage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPrevPage.ForeColor = Color.White;
            btnPrevPage.Location = new Point(13, 8);
            btnPrevPage.Name = "btnPrevPage";
            btnPrevPage.Size = new Size(110, 32);
            btnPrevPage.TabIndex = 2;
            btnPrevPage.Text = "< Home";
            btnPrevPage.UseVisualStyleBackColor = false;
            btnPrevPage.Visible = false;
            btnPrevPage.Click += new EventHandler(BtnPrevPage_Click);
            // 
            // lblPageInfo
            // 
            lblPageInfo.Anchor = AnchorStyles.None;
            lblPageInfo.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblPageInfo.ForeColor = Color.DimGray;
            lblPageInfo.Location = new Point(395, 10);
            lblPageInfo.Name = "lblPageInfo";
            lblPageInfo.Size = new Size(465, 30);
            lblPageInfo.TabIndex = 0;
            lblPageInfo.Text = "Records : 30";
            lblPageInfo.TextAlign = ContentAlignment.MiddleCenter;
            lblPageInfo.Visible = false;
            // 
            // btnNextPage
            // 
            btnNextPage.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            btnNextPage.BackColor = Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            btnNextPage.Cursor = Cursors.Hand;
            btnNextPage.FlatAppearance.BorderSize = 0;
            btnNextPage.FlatStyle = FlatStyle.Flat;
            btnNextPage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNextPage.ForeColor = Color.White;
            btnNextPage.Location = new Point(1133, 8);
            btnNextPage.Name = "btnNextPage";
            btnNextPage.Size = new Size(110, 32);
            btnNextPage.TabIndex = 1;
            btnNextPage.Text = "Next >";
            btnNextPage.UseVisualStyleBackColor = false;
            btnNextPage.Visible = false;
            btnNextPage.Click += new EventHandler(BtnNextPage_Click);
            // 
            // NewsControl
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dgvNews);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            Name = "NewsControl";
            Size = new Size(1255, 500);
            ((ISupportInitialize)(dgvNews)).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn DGVTitle;
        private DataGridViewTextBoxColumn DVGCategory;
        private DataGridViewTextBoxColumn DVGSubCategory;
        private Label newsUpdateLable;
        private TextBox newsSearch;
        private Label label1;
        private Button btnRefresh;
        private MonthCalendar fromcalender;
        private MonthCalendar tocalender;
        private TextBox todateTextbox;
        private TextBox fromTextbox;
    }
}