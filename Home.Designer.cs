using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using thecalcify.Helper;
using thecalcify.MarketWatch;


namespace thecalcify
{
    partial class thecalcify
    {
        //public int rowIndexFromMouseDown = -1;
        //public int rowIndexOfItemUnderMouseToDrop = -1;
        public List<DataGridViewRow> draggedRows = new List<DataGridViewRow>();
        public int dragSourceIndex = -1;


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
        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(thecalcify));
            this.defaultGrid = new System.Windows.Forms.DataGridView();
            this.Tools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ExportToExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEditSymbolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEditColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectESCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenF11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMarketWatchHost = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshMarketWatchHost = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportWorksheetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newswatchListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notificationSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.alertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.licenceExpire = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.savelabel = new System.Windows.Forms.Label();
            this.searchTextLabel = new System.Windows.Forms.Label();
            this.fontSizeComboBox = new System.Windows.Forms.ComboBox();
            this.newMarketWatchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtsearch = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.defaultGrid)).BeginInit();
            this.Tools.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // defaultGrid
            // 
            this.defaultGrid.AllowDrop = true;
            this.defaultGrid.AllowUserToAddRows = false;
            this.defaultGrid.AllowUserToDeleteRows = false;
            this.defaultGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.defaultGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.defaultGrid.BackgroundColor = System.Drawing.Color.White;
            this.defaultGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.defaultGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.defaultGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.defaultGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.defaultGrid.ColumnHeadersHeight = 40;
            this.defaultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.defaultGrid.ContextMenuStrip = this.Tools;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.defaultGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.defaultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defaultGrid.EnableHeadersVisualStyles = false;
            this.defaultGrid.GridColor = System.Drawing.Color.Gainsboro;
            this.defaultGrid.Location = new System.Drawing.Point(0, 58);
            this.defaultGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.defaultGrid.Name = "defaultGrid";
            this.defaultGrid.ReadOnly = true;
            this.defaultGrid.RowHeadersVisible = false;
            this.defaultGrid.RowHeadersWidth = 51;
            this.defaultGrid.RowTemplate.Height = 36;
            this.defaultGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.defaultGrid.Size = new System.Drawing.Size(1115, 624);
            this.defaultGrid.TabIndex = 1;
            this.defaultGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DefaultGrid_CellFormatting);
            this.defaultGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DefaultGrid_CellMouseDown);
            this.defaultGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DefaultGrid_CellMouseEnter);
            this.defaultGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.DefaultGrid_CellMouseLeave);
            this.defaultGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DefaultGrid_DataBindingComplete);
            this.defaultGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.DefaultGrid_DragDrop);
            this.defaultGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.DefaultGrid_DragOver);
            this.defaultGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DefaultGrid_KeyDown);
            this.defaultGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DefaultGrid_MouseDown);
            this.defaultGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DefaultGrid_MouseMove);
            // 
            // Tools
            // 
            this.Tools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.Tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportToExcelToolStripMenuItem,
            this.addEditSymbolsToolStripMenuItem,
            this.addEditColumnsToolStripMenuItem,
            this.clearExcelToolStripMenuItem,
            this.copyRowToolStripMenuItem});
            this.Tools.Name = "ClickMenuStrip";
            this.Tools.Size = new System.Drawing.Size(225, 124);
            // 
            // ExportToExcelToolStripMenuItem
            // 
            this.ExportToExcelToolStripMenuItem.Name = "ExportToExcelToolStripMenuItem";
            this.ExportToExcelToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.ExportToExcelToolStripMenuItem.Text = "📗 Export To Excel";
            this.ExportToExcelToolStripMenuItem.Click += new System.EventHandler(this.ExportToExcelToolStripMenuItem_Click);
            // 
            // addEditSymbolsToolStripMenuItem
            // 
            this.addEditSymbolsToolStripMenuItem.Enabled = false;
            this.addEditSymbolsToolStripMenuItem.Name = "addEditSymbolsToolStripMenuItem";
            this.addEditSymbolsToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.addEditSymbolsToolStripMenuItem.Text = "🏷️ Add/Edit Symbols";
            this.addEditSymbolsToolStripMenuItem.Click += new System.EventHandler(this.AddEditSymbolsToolStripMenuItem_Click);
            // 
            // addEditColumnsToolStripMenuItem
            // 
            this.addEditColumnsToolStripMenuItem.Name = "addEditColumnsToolStripMenuItem";
            this.addEditColumnsToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.addEditColumnsToolStripMenuItem.Text = "✍️ Add/Edit Columns";
            this.addEditColumnsToolStripMenuItem.Click += new System.EventHandler(this.AddEditColumnsToolStripMenuItem_Click);
            // 
            // clearExcelToolStripMenuItem
            // 
            this.clearExcelToolStripMenuItem.Name = "clearExcelToolStripMenuItem";
            this.clearExcelToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.clearExcelToolStripMenuItem.Text = "🧹 Clear Excel";
            this.clearExcelToolStripMenuItem.Visible = false;
            // 
            // copyRowToolStripMenuItem
            // 
            this.copyRowToolStripMenuItem.Name = "copyRowToolStripMenuItem";
            this.copyRowToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.copyRowToolStripMenuItem.Text = "📑 Copy Row";
            this.copyRowToolStripMenuItem.Click += new System.EventHandler(this.CopyRowToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.menuStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.saveMarketWatchHost,
            this.refreshMarketWatchHost,
            this.newCTRLNToolStripMenuItem,
            this.newsToolStripMenuItem,
            this.alertToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 30);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1115, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnectESCToolStripMenuItem,
            this.fullScreenF11ToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // disconnectESCToolStripMenuItem
            // 
            this.disconnectESCToolStripMenuItem.Name = "disconnectESCToolStripMenuItem";
            this.disconnectESCToolStripMenuItem.Size = new System.Drawing.Size(287, 26);
            this.disconnectESCToolStripMenuItem.Text = "Disconnect  (Shift + ESC)";
            this.disconnectESCToolStripMenuItem.Click += new System.EventHandler(this.DisconnectESCToolStripMenuItem_Click);
            // 
            // fullScreenF11ToolStripMenuItem
            // 
            this.fullScreenF11ToolStripMenuItem.Name = "fullScreenF11ToolStripMenuItem";
            this.fullScreenF11ToolStripMenuItem.Size = new System.Drawing.Size(287, 26);
            this.fullScreenF11ToolStripMenuItem.Text = "Full Screen (ESC)";
            this.fullScreenF11ToolStripMenuItem.Click += new System.EventHandler(this.FullScreenF11ToolStripMenuItem_Click);
            // 
            // saveMarketWatchHost
            // 
            this.saveMarketWatchHost.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.saveMarketWatchHost.BackColor = System.Drawing.Color.Transparent;
            this.saveMarketWatchHost.ForeColor = System.Drawing.Color.Black;
            this.saveMarketWatchHost.Margin = new System.Windows.Forms.Padding(5, 0, 10, 0);
            this.saveMarketWatchHost.Name = "saveMarketWatchHost";
            this.saveMarketWatchHost.Size = new System.Drawing.Size(164, 24);
            this.saveMarketWatchHost.Text = "Save MarketWatch";
            this.saveMarketWatchHost.Visible = false;
            this.saveMarketWatchHost.Click += new System.EventHandler(this.SaveMarketWatchHost_Click);
            // 
            // refreshMarketWatchHost
            // 
            this.refreshMarketWatchHost.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.refreshMarketWatchHost.BackColor = System.Drawing.Color.Transparent;
            this.refreshMarketWatchHost.ForeColor = System.Drawing.Color.Black;
            this.refreshMarketWatchHost.Image = ((System.Drawing.Image)(resources.GetObject("refreshMarketWatchHost.Image")));
            this.refreshMarketWatchHost.Margin = new System.Windows.Forms.Padding(5, 0, 10, 0);
            this.refreshMarketWatchHost.Name = "refreshMarketWatchHost";
            this.refreshMarketWatchHost.Size = new System.Drawing.Size(34, 24);
            this.refreshMarketWatchHost.ToolTipText = "Refresh MarketWatch";
            this.refreshMarketWatchHost.Click += new System.EventHandler(this.RefreshMarketWatchHost_Click);
            // 
            // newCTRLNToolStripMenuItem
            // 
            this.newCTRLNToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newCTRLNToolStripMenuItem1,
            this.viewToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.exportWorksheetsToolStripMenuItem});
            this.newCTRLNToolStripMenuItem.Name = "newCTRLNToolStripMenuItem";
            this.newCTRLNToolStripMenuItem.Size = new System.Drawing.Size(127, 24);
            this.newCTRLNToolStripMenuItem.Text = "Market Watch";
            // 
            // newCTRLNToolStripMenuItem1
            // 
            this.newCTRLNToolStripMenuItem1.Name = "newCTRLNToolStripMenuItem1";
            this.newCTRLNToolStripMenuItem1.Size = new System.Drawing.Size(234, 26);
            this.newCTRLNToolStripMenuItem1.Text = "New      (CTRL+N)";
            this.newCTRLNToolStripMenuItem1.Click += new System.EventHandler(this.NewCTRLNToolStripMenuItem1_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(234, 26);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(234, 26);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // exportWorksheetsToolStripMenuItem
            // 
            this.exportWorksheetsToolStripMenuItem.Name = "exportWorksheetsToolStripMenuItem";
            this.exportWorksheetsToolStripMenuItem.Size = new System.Drawing.Size(234, 26);
            this.exportWorksheetsToolStripMenuItem.Text = "Export Worksheets";
            this.exportWorksheetsToolStripMenuItem.Click += new System.EventHandler(this.exportWorksheetsToolStripMenuItem_Click);
            // 
            // newsToolStripMenuItem
            // 
            this.newsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newswatchListToolStripMenuItem,
            this.notificationSettings});
            this.newsToolStripMenuItem.Name = "newsToolStripMenuItem";
            this.newsToolStripMenuItem.Size = new System.Drawing.Size(65, 24);
            this.newsToolStripMenuItem.Text = "News";
            // 
            // newswatchListToolStripMenuItem
            // 
            this.newswatchListToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newsListToolStripMenuItem,
            this.newsHistoryToolStripMenuItem});
            this.newswatchListToolStripMenuItem.Name = "newswatchListToolStripMenuItem";
            this.newswatchListToolStripMenuItem.Size = new System.Drawing.Size(242, 26);
            this.newswatchListToolStripMenuItem.Text = "News";
            // 
            // newsListToolStripMenuItem
            // 
            this.newsListToolStripMenuItem.Name = "newsListToolStripMenuItem";
            this.newsListToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.newsListToolStripMenuItem.Text = "News List";
            this.newsListToolStripMenuItem.Click += new System.EventHandler(this.NewsListToolStripMenuItem_Click);
            // 
            // newsHistoryToolStripMenuItem
            // 
            this.newsHistoryToolStripMenuItem.Name = "newsHistoryToolStripMenuItem";
            this.newsHistoryToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.newsHistoryToolStripMenuItem.Text = "News History";
            this.newsHistoryToolStripMenuItem.Click += new System.EventHandler(this.NewsHistoryToolStripMenuItem_Click);
            // 
            // notificationSettings
            // 
            this.notificationSettings.Name = "notificationSettings";
            this.notificationSettings.Size = new System.Drawing.Size(242, 26);
            this.notificationSettings.Text = "Notification Settings";
            this.notificationSettings.Visible = false;
            this.notificationSettings.Click += new System.EventHandler(this.NewsSettingsToolStrip_Click);
            // 
            // alertToolStripMenuItem
            // 
            this.alertToolStripMenuItem.Name = "alertToolStripMenuItem";
            this.alertToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.alertToolStripMenuItem.Text = "Alert";
            this.alertToolStripMenuItem.Visible = false;
            this.alertToolStripMenuItem.Click += new System.EventHandler(this.AlertToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.ToolTipText = "Click CTRL + U";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(1115, 30);
            this.headerPanel.TabIndex = 3;
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.titleLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.Black;
            this.titleLabel.Location = new System.Drawing.Point(0, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(1115, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "DEFAULT";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.titleLabel.TextChanged += new System.EventHandler(this.TitleLabel_TextChanged);
            // 
            // licenceExpire
            // 
            this.licenceExpire.AutoSize = true;
            this.licenceExpire.Dock = System.Windows.Forms.DockStyle.Right;
            this.licenceExpire.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.licenceExpire.Location = new System.Drawing.Point(1105, 0);
            this.licenceExpire.Name = "licenceExpire";
            this.licenceExpire.Padding = new System.Windows.Forms.Padding(0, 4, 10, 0);
            this.licenceExpire.Size = new System.Drawing.Size(10, 24);
            this.licenceExpire.TabIndex = 0;
            this.licenceExpire.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.savelabel);
            this.bottomPanel.Controls.Add(this.licenceExpire);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 682);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1115, 26);
            this.bottomPanel.TabIndex = 4;
            // 
            // savelabel
            // 
            this.savelabel.AutoSize = true;
            this.savelabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.savelabel.Location = new System.Drawing.Point(7, 5);
            this.savelabel.Name = "savelabel";
            this.savelabel.Size = new System.Drawing.Size(242, 20);
            this.savelabel.TabIndex = 1;
            this.savelabel.Text = "Save MarketWatch (CTRL + S)";
            this.savelabel.Visible = false;
            // 
            // searchTextLabel
            // 
            this.searchTextLabel.AutoSize = true;
            this.searchTextLabel.Location = new System.Drawing.Point(464, 33);
            this.searchTextLabel.Name = "searchTextLabel";
            this.searchTextLabel.Size = new System.Drawing.Size(89, 16);
            this.searchTextLabel.TabIndex = 7;
            this.searchTextLabel.Text = "Search Text :-";
            // 
            // fontSizeComboBox
            // 
            this.fontSizeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fontSizeComboBox.FormattingEnabled = true;
            this.fontSizeComboBox.Items.AddRange(new object[] {
            "10",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "24",
            "26",
            "28",
            "30"});
            this.fontSizeComboBox.Location = new System.Drawing.Point(757, 29);
            this.fontSizeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.fontSizeComboBox.Name = "fontSizeComboBox";
            this.fontSizeComboBox.Size = new System.Drawing.Size(160, 24);
            this.fontSizeComboBox.TabIndex = 5;
            this.fontSizeComboBox.Text = "Font Size";
            this.fontSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.FontSizeComboBox_SelectedIndexChanged);
            this.fontSizeComboBox.TextChanged += new System.EventHandler(this.FontSizeComboBox_TextChanged);
            // 
            // newMarketWatchMenuItem
            // 
            this.newMarketWatchMenuItem.Name = "newMarketWatchMenuItem";
            this.newMarketWatchMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // txtsearch
            // 
            this.txtsearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtsearch.Location = new System.Drawing.Point(561, 27);
            this.txtsearch.Name = "txtsearch";
            this.txtsearch.Size = new System.Drawing.Size(176, 27);
            this.txtsearch.TabIndex = 6;
            this.txtsearch.TextChanged += new System.EventHandler(this.Txtsearch_TextChanged);
            this.txtsearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Txtsearch_KeyDown);
            // 
            // thecalcify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1115, 708);
            this.Controls.Add(this.searchTextLabel);
            this.Controls.Add(this.txtsearch);
            this.Controls.Add(this.fontSizeComboBox);
            this.Controls.Add(this.defaultGrid);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.headerPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "thecalcify";
            this.Text = "thecalcify";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Home_FormClosed);
            this.Load += new System.EventHandler(this.Home_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Thecalcify_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.defaultGrid)).EndInit();
            this.Tools.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.headerPanel.ResumeLayout(false);
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void SaveMarketWatchHost_Click(object sender, EventArgs e)
        {
            if (saveMarketWatchHost.Text == "Save MarketWatch")
            {
                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;

                if (editableMarketWatchGrid != null && editableMarketWatchGrid.selectedSymbols != null)
                {
                    selectedSymbols = editableMarketWatchGrid.selectedSymbols;
                    editableMarketWatchGrid.SaveSymbols(selectedSymbols);
                }
                else
                {
                    MessageBox.Show("No active market watch grid found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void Txtsearch_TextChanged(object sender, EventArgs e)
        {
            string filterText = txtsearch.Text.Trim();

            // Split filter by comma, trim each part, and remove empty strings
            var keywords = filterText.Split(',')
                                     .Select(k => k.Trim())
                                     .Where(k => !string.IsNullOrEmpty(k))
                                     .ToList();

            if (keywords.Count == 0)
            {
                // Reset all rows visible in defaultGrid
                if (defaultGrid != null)
                {
                    foreach (DataGridViewRow row in defaultGrid.Rows)
                    {
                        if (!row.IsNewRow)
                            row.Visible = true;
                    }
                }

                // Reset all rows visible in EditableMarketWatchGrid instance
                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    editableMarketWatchGrid.serchstring = "";
                    foreach (DataGridViewRow row in editableMarketWatchGrid.Rows)
                    {
                        if (!row.IsNewRow)
                            row.Visible = true;
                    }
                }
            }
            else
            {
                // Filter rows in defaultGrid based on "Name" column
                if (defaultGrid != null)
                {
                    foreach (DataGridViewRow row in defaultGrid.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["Name"].Value != null)
                        {
                            string name = row.Cells["Name"].Value.ToString();
                            bool match = keywords.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                            row.Visible = match;
                        }
                    }
                }

                // Filter rows in EditableMarketWatchGrid instance
                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    editableMarketWatchGrid.serchstring = filterText;

                    if (editableMarketWatchGrid.Rows.Count == 1)
                    {
                        MessageBox.Show("Please Select Value Before Search", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtsearch.Text = string.Empty;
                        return;
                    }

                    foreach (DataGridViewRow row in editableMarketWatchGrid.Rows)
                    {
                        if (!row.IsNewRow && row.Cells["Name"].Value != null)
                        {
                            string name = row.Cells["Name"].Value.ToString();
                            bool match = keywords.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                            row.Visible = match;
                        }
                    }
                }
            }
        }

        public void FontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFontSizeFromComboBox();
        }

        public void FontSizeComboBox_TextChanged(object sender, EventArgs e)
        {
            ApplyFontSizeFromComboBox();
        }

        public void ApplyFontSizeFromComboBox()
        {
            int _fontSize;

            try
            {
                if (int.TryParse(fontSizeComboBox.Text, out _fontSize))

                {

                    if (_fontSize < 10 || _fontSize > 30)

                    {

                        _fontSize = 12;

                    }



                    fontSize = _fontSize;

                    if (fontSize != defaultGrid.DefaultCellStyle.Font.Size)
                    {
                        defaultGrid.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Regular);
                        defaultGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", fontSize, FontStyle.Bold);
                    }

                    //EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;

                    //if (editableMarketWatchGrid != null)
                    //{
                    //    editableMarketWatchGrid.fontSize = _fontSize;
                    //    editableMarketWatchGrid.UpdateGridColumnVisibility();
                    //    //editableMarketWatchGrid.UpdateGridFontSize();
                    //}
                }

            }

            catch (Exception ex)
            {
                ApplicationLogger.Log($"Error applying font size");
                ApplicationLogger.LogException(ex);
            }

        }

        public string GetCellValue(string symbol, string columnName)
        {
            string cleanSymbol = symbol
                .Replace("▲", "")
                .Replace("▼", "")
                .Trim();

            foreach (DataGridViewRow row in this.defaultGrid.Rows)
            {
                string rowSymbol = row.Cells["name"].Value?.ToString()
                    .Replace("▲", "")
                    .Replace("▼", "")
                    .Trim();

                if (rowSymbol == cleanSymbol)
                    return row.Cells[columnName].Value?.ToString();
            }

            return string.Empty;
        }

        public void DefaultGrid_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = defaultGrid.HitTest(e.X, e.Y);

            if (hit.RowIndex < 0)
                return;

            dragSourceIndex = hit.RowIndex;
            draggedRows = defaultGrid.SelectedRows
                                     .Cast<DataGridViewRow>()
                                     .OrderBy(r => r.Index)
                                     .ToList();

            // ensure row is selected
            if (!defaultGrid.Rows[hit.RowIndex].Selected)
            {
                defaultGrid.ClearSelection();
                defaultGrid.Rows[hit.RowIndex].Selected = true;
                draggedRows = new List<DataGridViewRow> { defaultGrid.Rows[hit.RowIndex] };
            }
        }

        public void DefaultGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left && dragSourceIndex >= 0)
            {
                defaultGrid.DoDragDrop(draggedRows, DragDropEffects.Move);
            }
        }


        public void DefaultGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        public void DefaultGrid_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = defaultGrid.PointToClient(new Point(e.X, e.Y));
            int dropIndex = defaultGrid.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If drop position is OUTSIDE any row → force drop at end
            if (dropIndex < 0 || dropIndex >= defaultGrid.Rows.Count)
                dropIndex = defaultGrid.Rows.Count;

            // 1️⃣ Remove dragged rows (remove bottom-first to avoid shift issues)
            foreach (var row in draggedRows.OrderByDescending(r => r.Index))
            {
                defaultGrid.Rows.RemoveAt(row.Index);
            }

            // 2️⃣ Insert rows at final drop index
            int insertIndex = dropIndex;

            foreach (var row in draggedRows)
            {
                // IMPORTANT: InsertIndex may exceed row count after removals
                if (insertIndex > defaultGrid.Rows.Count)
                    insertIndex = defaultGrid.Rows.Count;

                defaultGrid.Rows.Insert(insertIndex, row);
                insertIndex++;
            }

            // 3️⃣ Reselect dragged rows
            foreach (var row in draggedRows)
                row.Selected = true;

            // 4️⃣ Rebuild symbol index mapping
            RebuildSymbolRowMap();
        }

        public void RebuildSymbolRowMap()
        {
            symbolRowMap.Clear();

            for (int i = 0; i < defaultGrid.Rows.Count; i++)
            {
                var row = defaultGrid.Rows[i];
                string symbol = row.Cells["Symbol"].Value?.ToString();  // your unique symbol key

                if (!string.IsNullOrEmpty(symbol))
                    symbolRowMap[symbol] = i;
            }
        }



        #endregion

        public DataGridView defaultGrid;
        public MenuStrip menuStrip1;
        public ToolStripMenuItem toolsToolStripMenuItem;
        public ToolStripMenuItem disconnectESCToolStripMenuItem;
        public Label licenceExpire;
        public Panel bottomPanel;
        public ContextMenuStrip Tools;
        public ToolStripMenuItem ExportToExcelToolStripMenuItem;
        public ToolStripMenuItem addEditSymbolsToolStripMenuItem;
        public ToolStripMenuItem addEditColumnsToolStripMenuItem;
        public ToolStripMenuItem fullScreenF11ToolStripMenuItem;
        public ToolStripMenuItem aboutToolStripMenuItem;
        public Panel panelAddColumns;
        public Panel panelAddSymbols;
        public CheckedListBox checkedListSymbols;
        public Button btnSelectAllSymbols;
        public Button btnConfirmAddSymbols;
        public Button btnCancelAddSymbols;
        public TextBox txtsearch;
        public Label searchTextLabel;
        public ComboBox fontSizeComboBox;
        public System.Windows.Forms.ToolStripMenuItem newMarketWatchMenuItem;
        public System.Windows.Forms.Panel headerPanel;
        public System.Windows.Forms.Label titleLabel;
        public ToolStripMenuItem saveMarketWatchHost;
        public ToolStripMenuItem newCTRLNToolStripMenuItem;
        public ToolStripMenuItem newCTRLNToolStripMenuItem1;
        public ToolStripMenuItem viewToolStripMenuItem;
        public ToolStripMenuItem deleteToolStripMenuItem;
        public ToolStripMenuItem clearExcelToolStripMenuItem;
        public Label savelabel;
        public ToolStripMenuItem newsToolStripMenuItem;
        public ToolStripMenuItem alertToolStripMenuItem;
        public ToolStripMenuItem refreshMarketWatchHost;
        public ToolStripMenuItem newswatchListToolStripMenuItem;
        public ToolStripMenuItem notificationSettings;
        public ToolStripMenuItem newsListToolStripMenuItem;
        public ToolStripMenuItem newsHistoryToolStripMenuItem;
        public ToolStripMenuItem copyRowToolStripMenuItem;
        public ToolStripMenuItem exportWorksheetsToolStripMenuItem;
        private System.ComponentModel.IContainer components;
    }
}