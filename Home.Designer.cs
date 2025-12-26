using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using thecalcify.Helper;
using thecalcify.MarketWatch;
using thecalcify.Modern_UI;
using thecalcify.Properties;
using Windows.ApplicationModel.Resources.Core;


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
            this.defaultGrid = new System.Windows.Forms.DataGridView();
            this.Tools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ExportToExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEditSymbolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEditColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chartWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectESCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenF11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportWorksheetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newswatchListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notificationSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.alertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMarketWatchHost = new System.Windows.Forms.Button();
            this.refreshMarketWatchHost = new System.Windows.Forms.Button();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.txtsearch = new System.Windows.Forms.TextBox();
            this.searchTextLabel = new System.Windows.Forms.Label();
            this.licenceExpire = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.savelabel = new System.Windows.Forms.Label();
            this.fontSizeComboBox = new ModernComboBox();
            this.newMarketWatchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.defaultGrid)).BeginInit();
            this.Tools.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.pnlSearch.SuspendLayout();
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
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.defaultGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.defaultGrid.ColumnHeadersHeight = 40;
            this.defaultGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.defaultGrid.ContextMenuStrip = this.Tools;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(213)))), ((int)(((byte)(220)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.defaultGrid.DefaultCellStyle = dataGridViewCellStyle3;
            this.defaultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.defaultGrid.EnableHeadersVisualStyles = false;
            this.defaultGrid.GridColor = System.Drawing.Color.Gainsboro;
            this.defaultGrid.Location = new System.Drawing.Point(0, 60);
            this.defaultGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.defaultGrid.Name = "defaultGrid";
            this.defaultGrid.ReadOnly = true;
            this.defaultGrid.RowHeadersVisible = false;
            this.defaultGrid.RowHeadersWidth = 51;
            this.defaultGrid.RowTemplate.Height = 36;
            this.defaultGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.defaultGrid.Size = new System.Drawing.Size(991, 480);
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
            this.copyRowToolStripMenuItem,
            this.chartWindowToolStripMenuItem});
            this.Tools.Name = "ClickMenuStrip";
            this.Tools.Size = new System.Drawing.Size(225, 148);
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
            // chartWindowToolStripMenuItem
            // 
            this.chartWindowToolStripMenuItem.Name = "chartWindowToolStripMenuItem";
            this.chartWindowToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.chartWindowToolStripMenuItem.Text = "📊 Chart Window";
            this.chartWindowToolStripMenuItem.ToolTipText = "Open Chart";
            this.chartWindowToolStripMenuItem.Visible = false;
            this.chartWindowToolStripMenuItem.Click += new System.EventHandler(this.ChartWindowToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.newCTRLNToolStripMenuItem,
            this.newsToolStripMenuItem,
            this.alertToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 32);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(991, 28);
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
            this.disconnectESCToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.disconnectESCToolStripMenuItem.Text = "❌ Disconnect  (Shift + ESC)";
            this.disconnectESCToolStripMenuItem.Click += new System.EventHandler(this.DisconnectESCToolStripMenuItem_Click);
            // 
            // fullScreenF11ToolStripMenuItem
            // 
            this.fullScreenF11ToolStripMenuItem.Name = "fullScreenF11ToolStripMenuItem";
            this.fullScreenF11ToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.fullScreenF11ToolStripMenuItem.Text = "🔲 Full Screen (ESC)";
            this.fullScreenF11ToolStripMenuItem.Click += new System.EventHandler(this.FullScreenF11ToolStripMenuItem_Click);
            // 
            // newCTRLNToolStripMenuItem
            // 
            this.newCTRLNToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newCTRLNToolStripMenuItem1,
            this.viewToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exportWorksheetsToolStripMenuItem});
            this.newCTRLNToolStripMenuItem.Name = "newCTRLNToolStripMenuItem";
            this.newCTRLNToolStripMenuItem.Size = new System.Drawing.Size(127, 24);
            this.newCTRLNToolStripMenuItem.Text = "Market Watch";
            // 
            // newCTRLNToolStripMenuItem1
            // 
            this.newCTRLNToolStripMenuItem1.Name = "newCTRLNToolStripMenuItem1";
            this.newCTRLNToolStripMenuItem1.Size = new System.Drawing.Size(255, 26);
            this.newCTRLNToolStripMenuItem1.Text = "➕ New      (CTRL+N)";
            this.newCTRLNToolStripMenuItem1.Click += new System.EventHandler(this.NewCTRLNToolStripMenuItem1_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(255, 26);
            this.viewToolStripMenuItem.Text = "📈 View Watchlist";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(255, 26);
            this.deleteToolStripMenuItem.Text = "🗑 Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(252, 6);
            // 
            // exportWorksheetsToolStripMenuItem
            // 
            this.exportWorksheetsToolStripMenuItem.Name = "exportWorksheetsToolStripMenuItem";
            this.exportWorksheetsToolStripMenuItem.Size = new System.Drawing.Size(255, 26);
            this.exportWorksheetsToolStripMenuItem.Text = "🗃️ Export Worksheets";
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
            this.newswatchListToolStripMenuItem.Size = new System.Drawing.Size(263, 26);
            this.newswatchListToolStripMenuItem.Text = "📰 News Watchlist";
            // 
            // newsListToolStripMenuItem
            // 
            this.newsListToolStripMenuItem.Name = "newsListToolStripMenuItem";
            this.newsListToolStripMenuItem.Size = new System.Drawing.Size(214, 26);
            this.newsListToolStripMenuItem.Text = "📋 News List";
            this.newsListToolStripMenuItem.Click += new System.EventHandler(this.NewsListToolStripMenuItem_Click);
            // 
            // newsHistoryToolStripMenuItem
            // 
            this.newsHistoryToolStripMenuItem.Name = "newsHistoryToolStripMenuItem";
            this.newsHistoryToolStripMenuItem.Size = new System.Drawing.Size(214, 26);
            this.newsHistoryToolStripMenuItem.Text = "📜 News History";
            this.newsHistoryToolStripMenuItem.Click += new System.EventHandler(this.NewsHistoryToolStripMenuItem_Click);
            // 
            // notificationSettings
            // 
            this.notificationSettings.Name = "notificationSettings";
            this.notificationSettings.Size = new System.Drawing.Size(263, 26);
            this.notificationSettings.Text = "🔔 Notification Settings";
            this.notificationSettings.Visible = false;
            this.notificationSettings.Click += new System.EventHandler(this.NewsSettingsToolStrip_Click);
            // 
            // alertToolStripMenuItem
            // 
            this.alertToolStripMenuItem.Name = "alertToolStripMenuItem";
            this.alertToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.alertToolStripMenuItem.Text = "Alert";
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
            // saveMarketWatchHost
            // 
            this.saveMarketWatchHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.saveMarketWatchHost.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveMarketWatchHost.FlatAppearance.BorderSize = 0;
            this.saveMarketWatchHost.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveMarketWatchHost.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.saveMarketWatchHost.ForeColor = System.Drawing.Color.Black;
            this.saveMarketWatchHost.Location = new System.Drawing.Point(804, 24);
            this.saveMarketWatchHost.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.saveMarketWatchHost.Name = "saveMarketWatchHost";
            this.saveMarketWatchHost.Size = new System.Drawing.Size(161, 24);
            this.saveMarketWatchHost.TabIndex = 4;
            this.saveMarketWatchHost.Text = "Save MarketWatch";
            this.saveMarketWatchHost.UseVisualStyleBackColor = false;
            this.saveMarketWatchHost.Visible = false;
            this.saveMarketWatchHost.Click += new System.EventHandler(this.SaveMarketWatchHost_Click);
            // 
            // refreshMarketWatchHost
            // 
            this.refreshMarketWatchHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshMarketWatchHost.Cursor = System.Windows.Forms.Cursors.Hand;
            this.refreshMarketWatchHost.FlatAppearance.BorderSize = 0;
            this.refreshMarketWatchHost.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshMarketWatchHost.Font = new System.Drawing.Font("Segoe UI Symbol", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refreshMarketWatchHost.Location = new System.Drawing.Point(950, 24);
            this.refreshMarketWatchHost.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.refreshMarketWatchHost.Name = "refreshMarketWatchHost";
            this.refreshMarketWatchHost.Size = new System.Drawing.Size(31, 32);
            this.refreshMarketWatchHost.TabIndex = 5;
            this.refreshMarketWatchHost.Text = "↻";
            this.refreshMarketWatchHost.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.refreshMarketWatchHost.UseVisualStyleBackColor = false;
            this.refreshMarketWatchHost.Click += new System.EventHandler(this.RefreshMarketWatchHost_Click);
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = System.Drawing.Color.White;
            this.headerPanel.Controls.Add(this.logoPictureBox);
            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(13, 0, 0, 0);
            this.headerPanel.Size = new System.Drawing.Size(991, 32);
            this.headerPanel.TabIndex = 3;
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.logoPictureBox.Image = global::thecalcify.Properties.Resources.starline_solution;
            this.logoPictureBox.InitialImage = global::thecalcify.Properties.Resources.starline_solution;
            this.logoPictureBox.Location = new System.Drawing.Point(13, 0);
            this.logoPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.Padding = new System.Windows.Forms.Padding(27, 0, 0, 0);
            this.logoPictureBox.Size = new System.Drawing.Size(133, 32);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoPictureBox.TabIndex = 1;
            this.logoPictureBox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.BackColor = System.Drawing.Color.White;
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.Black;
            this.titleLabel.Location = new System.Drawing.Point(13, 0);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(9, 0, 3, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(978, 32);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "DEFAULT";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.titleLabel.TextChanged += new System.EventHandler(this.TitleLabel_TextChanged);
            // 
            // pnlSearch
            // 
            this.pnlSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.pnlSearch.Controls.Add(this.txtsearch);
            this.pnlSearch.Controls.Add(this.searchTextLabel);
            this.pnlSearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.pnlSearch.Location = new System.Drawing.Point(587, 22);
            this.pnlSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(196, 24);
            this.pnlSearch.TabIndex = 6;
            this.pnlSearch.Click += new System.EventHandler(this.PnlSearch_Click);
            this.pnlSearch.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlSearch_Paint);
            // 
            // txtsearch
            // 
            this.txtsearch.BackColor = System.Drawing.Color.White;
            this.txtsearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtsearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtsearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtsearch.Location = new System.Drawing.Point(0, 0);
            this.txtsearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtsearch.Name = "txtsearch";
            this.txtsearch.Size = new System.Drawing.Size(196, 20);
            this.txtsearch.TabIndex = 0;
            this.txtsearch.TextChanged += new System.EventHandler(this.Txtsearch_TextChanged);
            this.txtsearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Txtsearch_KeyDown);
            // 
            // searchTextLabel
            // 
            this.searchTextLabel.AutoSize = true;
            this.searchTextLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.searchTextLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.searchTextLabel.ForeColor = System.Drawing.Color.Gray;
            this.searchTextLabel.Location = new System.Drawing.Point(7, 6);
            this.searchTextLabel.Name = "searchTextLabel";
            this.searchTextLabel.Size = new System.Drawing.Size(87, 20);
            this.searchTextLabel.TabIndex = 1;
            this.searchTextLabel.Text = "Search Text:";
            this.searchTextLabel.Click += new System.EventHandler(this.PnlSearch_Click);
            // 
            // licenceExpire
            // 
            this.licenceExpire.AutoSize = true;
            this.licenceExpire.Dock = System.Windows.Forms.DockStyle.Right;
            this.licenceExpire.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.licenceExpire.Location = new System.Drawing.Point(981, 0);
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
            this.bottomPanel.Location = new System.Drawing.Point(0, 540);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(991, 26);
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
            // fontSizeComboBox
            // 
            this.fontSizeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fontSizeComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.fontSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontSizeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fontSizeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fontSizeComboBox.FormattingEnabled = true;
            this.fontSizeComboBox.ItemHeight = 22;
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
            this.fontSizeComboBox.Location = new System.Drawing.Point(802, 26);
            this.fontSizeComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.fontSizeComboBox.Name = "fontSizeComboBox";
            this.fontSizeComboBox.Size = new System.Drawing.Size(143, 28);
            this.fontSizeComboBox.TabIndex = 5;
            this.fontSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.FontSizeComboBox_SelectedIndexChanged);
            this.fontSizeComboBox.TextChanged += new System.EventHandler(this.FontSizeComboBox_TextChanged);
            // 
            // newMarketWatchMenuItem
            // 
            this.newMarketWatchMenuItem.Name = "newMarketWatchMenuItem";
            this.newMarketWatchMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // thecalcify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(991, 566);
            this.Controls.Add(this.saveMarketWatchHost);
            this.Controls.Add(this.refreshMarketWatchHost);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.fontSizeComboBox);
            this.Controls.Add(this.defaultGrid);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.headerPanel);
            this.Icon = global::thecalcify.Properties.Resources.ApplicationIcon;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "thecalcify";
            this.Text = "thecalcify";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Home_FormClosed);
            this.Load += new System.EventHandler(this.Home_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Thecalcify_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.defaultGrid)).EndInit();
            this.Tools.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public async void SaveMarketWatchHost_Click(object sender, EventArgs e)
        {
            if (saveMarketWatchHost.Text == "Save MarketWatch")
            {
                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;

                if (editableMarketWatchGrid != null)
                {
                    await EditableMarketWatchGrid.SaveMarketWatchAsync();
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
            if (fontSizeComboBox.SelectedIndex == -1) return;

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

            defaultGrid.ClearSelection();


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
        public ModernComboBox fontSizeComboBox;
        public System.Windows.Forms.ToolStripMenuItem newMarketWatchMenuItem;
        public System.Windows.Forms.Panel headerPanel;
        public System.Windows.Forms.Label titleLabel;
        public Button saveMarketWatchHost;
        public ToolStripMenuItem newCTRLNToolStripMenuItem;
        public ToolStripMenuItem newCTRLNToolStripMenuItem1;
        public ToolStripMenuItem viewToolStripMenuItem;
        public ToolStripMenuItem deleteToolStripMenuItem;
        public ToolStripMenuItem clearExcelToolStripMenuItem;
        public Label savelabel;
        public ToolStripMenuItem newsToolStripMenuItem;
        public ToolStripMenuItem alertToolStripMenuItem;
        public Button refreshMarketWatchHost;
        public ToolStripMenuItem newswatchListToolStripMenuItem;
        public ToolStripMenuItem notificationSettings;
        public ToolStripMenuItem newsListToolStripMenuItem;
        public ToolStripMenuItem newsHistoryToolStripMenuItem;
        public ToolStripMenuItem copyRowToolStripMenuItem;
        public ToolStripMenuItem chartWindowToolStripMenuItem;
        public ToolStripMenuItem exportWorksheetsToolStripMenuItem;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private Panel pnlSearch;
        private ToolStripSeparator toolStripMenuItem1;
    }
}