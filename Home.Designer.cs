namespace thecalcify
{
    partial class thecalcify
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(thecalcify));
            this.defaultGrid = new System.Windows.Forms.DataGridView();
            this.ClickMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ExportToExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectESCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.licenceExpire = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.defaultGrid)).BeginInit();
            this.ClickMenuStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // defaultGrid
            // 
            this.defaultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.defaultGrid.ContextMenuStrip = this.ClickMenuStrip;
            this.defaultGrid.Location = new System.Drawing.Point(0, 28);
            this.defaultGrid.Name = "defaultGrid";
            this.defaultGrid.RowHeadersWidth = 51;
            this.defaultGrid.RowTemplate.Height = 24;
            this.defaultGrid.Size = new System.Drawing.Size(1115, 651);
            this.defaultGrid.TabIndex = 0;
            this.defaultGrid.DataSourceChanged += new System.EventHandler(this.DefaultGrid_DataSourceChanged);
            this.defaultGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DefaultGrid_CellFormatting);
            this.defaultGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DefaultGrid_DataBindingComplete);
            // 
            // ClickMenuStrip
            // 
            this.ClickMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ClickMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportToExcelToolStripMenuItem});
            this.ClickMenuStrip.Name = "ClickMenuStrip";
            this.ClickMenuStrip.Size = new System.Drawing.Size(180, 28);
            // 
            // ExportToExcelToolStripMenuItem
            // 
            this.ExportToExcelToolStripMenuItem.Name = "ExportToExcelToolStripMenuItem";
            this.ExportToExcelToolStripMenuItem.Size = new System.Drawing.Size(179, 24);
            this.ExportToExcelToolStripMenuItem.Text = "Export To Excel";
            this.ExportToExcelToolStripMenuItem.Click += new System.EventHandler(this.ExportToExcelToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1115, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnectESCToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // disconnectESCToolStripMenuItem
            // 
            this.disconnectESCToolStripMenuItem.Name = "disconnectESCToolStripMenuItem";
            this.disconnectESCToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.disconnectESCToolStripMenuItem.Text = "Disconnect    (ESC)";
            this.disconnectESCToolStripMenuItem.Click += new System.EventHandler(this.disconnectESCToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.licenceExpire});
            this.statusStrip1.Location = new System.Drawing.Point(0, 682);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1115, 26);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // licenceExpire
            // 
            this.licenceExpire.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.licenceExpire.Name = "licenceExpire";
            this.licenceExpire.Size = new System.Drawing.Size(129, 20);
            this.licenceExpire.Text = "Licence Expired :- ";
            this.licenceExpire.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // thecalcify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1115, 708);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.defaultGrid);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "thecalcify";
            this.Text = "thecalcify";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Home_FormClosed);
            this.Load += new System.EventHandler(this.Home_Load);
            //this.Layout += new System.Windows.Forms.LayoutEventHandler(this.thecalcify_Layout);
            ((System.ComponentModel.ISupportInitialize)(this.defaultGrid)).EndInit();
            this.ClickMenuStrip.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView defaultGrid;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectESCToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel licenceExpire;
        private System.Windows.Forms.ContextMenuStrip ClickMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ExportToExcelToolStripMenuItem;
    }
}