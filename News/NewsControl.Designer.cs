using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using thecalcify.Helper;
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
                // Logic preserved from original file
                try { PeriodicDispose(); } catch { }
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
            this.todateTextbox = new System.Windows.Forms.TextBox();
            this.fromTextbox = new System.Windows.Forms.TextBox();
            this.tocalender = new System.Windows.Forms.MonthCalendar();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.newsSearch = new System.Windows.Forms.TextBox();
            this.newsUpdateLable = new System.Windows.Forms.Label();
            this.fromcalender = new System.Windows.Forms.MonthCalendar();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnNextPage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNews)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvNews
            // 
            dgvNews.DefaultCellStyle.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            dgvNews.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            this.dgvNews.AllowDrop = true;
            this.dgvNews.AllowUserToAddRows = false;
            this.dgvNews.AllowUserToDeleteRows = false;
            this.dgvNews.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.dgvNews.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;  // KEEP THIS ONE
            this.dgvNews.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNews.BackgroundColor = System.Drawing.Color.White;
            this.dgvNews.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvNews.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvNews.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // REMOVE THE DUPLICATE LINE HERE
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvNews.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvNews.ColumnHeadersHeight = 45;
            this.dgvNews.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvNews.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.DGVTitle,
            this.DVGCategory,
            this.DVGSubCategory});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            // Increased Cell Font Size to 12F
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 16F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(225, 248, 249);
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvNews.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvNews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNews.EnableHeadersVisualStyles = false;
            this.dgvNews.GridColor = System.Drawing.Color.WhiteSmoke;
            this.dgvNews.Location = new System.Drawing.Point(0, 150);
            this.dgvNews.Name = "dgvNews";
            this.dgvNews.ReadOnly = true;
            this.dgvNews.RowHeadersVisible = false;
            this.dgvNews.RowHeadersWidth = 51;
            this.dgvNews.RowTemplate.Height = 40;
            this.dgvNews.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNews.Size = new System.Drawing.Size(1255, 300);
            this.dgvNews.TabIndex = 0;
            this.dgvNews.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvNews_CellDoubleClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn1.HeaderText = "Time";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 150;
            // 
            // DGVTitle
            // 
            this.DGVTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DGVTitle.HeaderText = "Title";
            this.DGVTitle.MinimumWidth = 6;
            this.DGVTitle.Name = "DGVTitle";
            this.DGVTitle.ReadOnly = true;
            // 
            // DVGCategory
            // 
            this.DVGCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DVGCategory.HeaderText = "Category";
            this.DVGCategory.MinimumWidth = 6;
            this.DVGCategory.Name = "DVGCategory";
            this.DVGCategory.ReadOnly = true;
            this.DVGCategory.Width = 150;
            // 
            // DVGSubCategory
            // 
            this.DVGSubCategory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DVGSubCategory.HeaderText = "SubCategory";
            this.DVGSubCategory.MinimumWidth = 6;
            this.DVGSubCategory.Name = "DVGSubCategory";
            this.DVGSubCategory.ReadOnly = true;
            this.DVGSubCategory.Width = 150;
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCategory.Location = new System.Drawing.Point(25, 45);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(180, 31);
            this.cmbCategory.TabIndex = 1;
            this.cmbCategory.Visible = false;
            // 
            // cmbSubCategory
            // 
            this.cmbSubCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubCategory.Enabled = false;
            this.cmbSubCategory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbSubCategory.Location = new System.Drawing.Point(220, 45);
            this.cmbSubCategory.Name = "cmbSubCategory";
            this.cmbSubCategory.Size = new System.Drawing.Size(180, 31);
            this.cmbSubCategory.TabIndex = 3;
            this.cmbSubCategory.Visible = false;
            // 
            // btnSearchNews
            // 
            this.btnSearchNews.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            this.btnSearchNews.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchNews.FlatAppearance.BorderSize = 0;
            this.btnSearchNews.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchNews.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearchNews.ForeColor = System.Drawing.Color.White;
            this.btnSearchNews.Location = new System.Drawing.Point(666, 43);
            this.btnSearchNews.Name = "btnSearchNews";
            this.btnSearchNews.Size = new System.Drawing.Size(110, 32);
            this.btnSearchNews.TabIndex = 4;
            this.btnSearchNews.Text = "SEARCH";
            this.btnSearchNews.UseVisualStyleBackColor = false;
            this.btnSearchNews.Visible = false;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.Black;
            this.lblCategory.Location = new System.Drawing.Point(22, 22);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(76, 20);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "Category:";
            this.lblCategory.Visible = false;
            // 
            // lblSubCategory
            // 
            this.lblSubCategory.AutoSize = true;
            this.lblSubCategory.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubCategory.ForeColor = System.Drawing.Color.DimGray;
            this.lblSubCategory.Location = new System.Drawing.Point(217, 22);
            this.lblSubCategory.Name = "lblSubCategory";
            this.lblSubCategory.Size = new System.Drawing.Size(100, 20);
            this.lblSubCategory.TabIndex = 2;
            this.lblSubCategory.Text = "SubCategory:";
            this.lblSubCategory.Visible = false;
            // 
            // pnlTop
            // 
            this.pnlTop.AutoSize = true;
            this.pnlTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlTop.BackColor = System.Drawing.Color.White;
            this.pnlTop.Controls.Add(this.tocalender);
            this.pnlTop.Controls.Add(this.fromcalender);
            this.pnlTop.Controls.Add(this.todateTextbox);
            this.pnlTop.Controls.Add(this.fromTextbox);
            this.pnlTop.Controls.Add(this.btnRefresh);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.newsSearch);
            this.pnlTop.Controls.Add(this.newsUpdateLable);
            this.pnlTop.Controls.Add(this.lblCategory);
            this.pnlTop.Controls.Add(this.cmbCategory);
            this.pnlTop.Controls.Add(this.lblSubCategory);
            this.pnlTop.Controls.Add(this.cmbSubCategory);
            this.pnlTop.Controls.Add(this.btnSearchNews);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(1255, 150);
            this.pnlTop.TabIndex = 1;
            // 
            // todateTextbox
            // 
            this.todateTextbox.BackColor = System.Drawing.Color.WhiteSmoke;
            this.todateTextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.todateTextbox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.todateTextbox.Location = new System.Drawing.Point(540, 45);
            this.todateTextbox.Name = "todateTextbox";
            this.todateTextbox.Size = new System.Drawing.Size(120, 30);
            this.todateTextbox.TabIndex = 14;
            this.todateTextbox.Text = "To Date";
            this.todateTextbox.Visible = false;
            this.todateTextbox.Click += new System.EventHandler(this.todateTextbox_Click);
            this.todateTextbox.Leave += new System.EventHandler(this.todateTextbox_Leave);
            // 
            // fromTextbox
            // 
            this.fromTextbox.BackColor = System.Drawing.Color.WhiteSmoke;
            this.fromTextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fromTextbox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.fromTextbox.Location = new System.Drawing.Point(415, 45);
            this.fromTextbox.Name = "fromTextbox";
            this.fromTextbox.Size = new System.Drawing.Size(120, 30);
            this.fromTextbox.TabIndex = 13;
            this.fromTextbox.Text = "From Date";
            this.fromTextbox.Visible = false;
            this.fromTextbox.Click += new System.EventHandler(this.fromTextbox_Click);
            this.fromTextbox.Leave += new System.EventHandler(this.fromTextbox_Leave);
            // 
            // tocalender
            // 
            this.tocalender.Location = new System.Drawing.Point(540, 80);
            this.tocalender.MaxSelectionCount = 31;
            this.tocalender.Name = "tocalender";
            this.tocalender.TabIndex = 12;
            this.tocalender.Visible = false;
            this.tocalender.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.tocalender_DateSelected);
            this.tocalender.Leave += new System.EventHandler(this.tocalender_LostFocus);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI Symbol", 17F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(1193, 38);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(50, 46);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Text = "↻";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(790, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "Search News:";
            // 
            // newsSearch
            // 
            this.newsSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.newsSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.newsSearch.Location = new System.Drawing.Point(790, 45);
            this.newsSearch.Name = "newsSearch";
            this.newsSearch.Size = new System.Drawing.Size(220, 30);
            this.newsSearch.TabIndex = 7;
            this.newsSearch.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            this.newsSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // newsUpdateLable
            // 
            this.newsUpdateLable.AutoSize = true;
            this.newsUpdateLable.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.newsUpdateLable.ForeColor = System.Drawing.Color.Black;
            this.newsUpdateLable.Location = new System.Drawing.Point(1020, 50);
            this.newsUpdateLable.Name = "newsUpdateLable";
            this.newsUpdateLable.Size = new System.Drawing.Size(134, 19);
            this.newsUpdateLable.TabIndex = 5;
            this.newsUpdateLable.Text = "Last Received At: -- ";
            this.newsUpdateLable.Visible = false;
            // 
            // fromcalender
            // 
            this.fromcalender.Location = new System.Drawing.Point(415, 80);
            this.fromcalender.MaxSelectionCount = 31;
            this.fromcalender.Name = "fromcalender";
            this.fromcalender.TabIndex = 10;
            this.fromcalender.Visible = false;
            this.fromcalender.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.fromcalender_DateSelected);
            this.fromcalender.Leave += new System.EventHandler(this.fromcalender_LostFocus);
            this.fromcalender.MouseDown += new System.Windows.Forms.MouseEventHandler(this.fromcalender_LostFocus);
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
            this.btnPrevPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevPage.FlatAppearance.BorderSize = 0;
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPrevPage.ForeColor = System.Drawing.Color.White;
            this.btnPrevPage.Location = new System.Drawing.Point(13, 8);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(110, 32);
            this.btnPrevPage.TabIndex = 2;
            this.btnPrevPage.Text = "< Home";
            this.btnPrevPage.UseVisualStyleBackColor = false;
            this.btnPrevPage.Visible = false;
            this.btnPrevPage.Click += new System.EventHandler(this.BtnPrevPage_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblPageInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageInfo.ForeColor = System.Drawing.Color.DimGray;
            this.lblPageInfo.Location = new System.Drawing.Point(395, 10);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(465, 30);
            this.lblPageInfo.TabIndex = 0;
            this.lblPageInfo.Text = "Records : 30";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPageInfo.Visible = false;
            // 
            // btnNextPage
            // 
            this.btnNextPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNextPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            this.btnNextPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPage.FlatAppearance.BorderSize = 0;
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnNextPage.ForeColor = System.Drawing.Color.White;
            this.btnNextPage.Location = new System.Drawing.Point(1133, 8);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(110, 32);
            this.btnNextPage.TabIndex = 1;
            this.btnNextPage.Text = "Next >";
            this.btnNextPage.UseVisualStyleBackColor = false;
            this.btnNextPage.Visible = false;
            this.btnNextPage.Click += new System.EventHandler(this.BtnNextPage_Click);
            // 
            // NewsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvNews);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "NewsControl";
            this.Size = new System.Drawing.Size(1255, 500);
            ((System.ComponentModel.ISupportInitialize)(this.dgvNews)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

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