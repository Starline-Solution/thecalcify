using System.Windows.Forms;

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

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
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
            ((System.ComponentModel.ISupportInitialize)(this.dgvNews)).BeginInit();
            this.pnlTop.SuspendLayout();
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
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.CornflowerBlue;
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
            this.dgvNews.Location = new System.Drawing.Point(0, 78);
            this.dgvNews.Name = "dgvNews";
            this.dgvNews.ReadOnly = true;
            this.dgvNews.RowHeadersVisible = false;
            this.dgvNews.RowHeadersWidth = 51;
            this.dgvNews.RowTemplate.Height = 36;
            this.dgvNews.Size = new System.Drawing.Size(800, 422);
            this.dgvNews.TabIndex = 0;
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
            // 
            // cmbSubCategory
            // 
            this.cmbSubCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSubCategory.Enabled = false;
            this.cmbSubCategory.Location = new System.Drawing.Point(233, 41);
            this.cmbSubCategory.Name = "cmbSubCategory";
            this.cmbSubCategory.Size = new System.Drawing.Size(180, 24);
            this.cmbSubCategory.TabIndex = 3;
            // 
            // btnSearchNews
            // 
            this.btnSearchNews.BackColor = System.Drawing.Color.White;
            this.btnSearchNews.Location = new System.Drawing.Point(430, 36);
            this.btnSearchNews.Name = "btnSearchNews";
            this.btnSearchNews.Size = new System.Drawing.Size(99, 28);
            this.btnSearchNews.TabIndex = 4;
            this.btnSearchNews.Text = "Search";
            this.btnSearchNews.UseVisualStyleBackColor = false;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(22, 21);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(65, 16);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "Category:";
            // 
            // lblSubCategory
            // 
            this.lblSubCategory.AutoSize = true;
            this.lblSubCategory.Location = new System.Drawing.Point(231, 21);
            this.lblSubCategory.Name = "lblSubCategory";
            this.lblSubCategory.Size = new System.Drawing.Size(89, 16);
            this.lblSubCategory.TabIndex = 2;
            this.lblSubCategory.Text = "SubCategory:";
            // 
            // pnlTop
            // 
            this.pnlTop.AutoSize = true;
            this.pnlTop.BackColor = System.Drawing.Color.White;
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
            this.pnlTop.Size = new System.Drawing.Size(800, 78);
            this.pnlTop.TabIndex = 1;
            // 
            // NewsControl
            // 
            this.Controls.Add(this.dgvNews);
            this.Controls.Add(this.pnlTop);
            this.Name = "NewsControl";
            this.Size = new System.Drawing.Size(800, 500);
            ((System.ComponentModel.ISupportInitialize)(this.dgvNews)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn DGVTitle;
        private DataGridViewTextBoxColumn DVGCategory;
        private DataGridViewTextBoxColumn DVGSubCategory;
    }
}
