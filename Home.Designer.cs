using System;
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectESCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenF11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMarketWatchHost = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.alertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.CornflowerBlue;
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
            this.defaultGrid.MultiSelect = false;
            this.defaultGrid.Name = "defaultGrid";
            this.defaultGrid.ReadOnly = true;
            this.defaultGrid.RowHeadersVisible = false;
            this.defaultGrid.RowHeadersWidth = 51;
            this.defaultGrid.RowTemplate.Height = 36;
            this.defaultGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.defaultGrid.Size = new System.Drawing.Size(1115, 624);
            this.defaultGrid.TabIndex = 1;
            this.defaultGrid.DataSourceChanged += new System.EventHandler(this.DefaultGrid_DataSourceChanged);
            this.defaultGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DefaultGrid_CellFormatting);
            this.defaultGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DefaultGrid_CellMouseDown);
            this.defaultGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DefaultGrid_CellMouseEnter);
            this.defaultGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.DefaultGrid_CellMouseLeave);
            this.defaultGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DefaultGrid_DataBindingComplete);
            // 
            // Tools
            // 
            this.Tools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.Tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportToExcelToolStripMenuItem,
            this.addEditSymbolsToolStripMenuItem,
            this.addEditColumnsToolStripMenuItem,
            this.clearExcelToolStripMenuItem});
            this.Tools.Name = "ClickMenuStrip";
            this.Tools.Size = new System.Drawing.Size(200, 100);
            // 
            // ExportToExcelToolStripMenuItem
            // 
            this.ExportToExcelToolStripMenuItem.Name = "ExportToExcelToolStripMenuItem";
            this.ExportToExcelToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.ExportToExcelToolStripMenuItem.Text = "Export To Excel";
            this.ExportToExcelToolStripMenuItem.Click += new System.EventHandler(this.ExportToExcelToolStripMenuItem_Click);
            // 
            // addEditSymbolsToolStripMenuItem
            // 
            this.addEditSymbolsToolStripMenuItem.Enabled = false;
            this.addEditSymbolsToolStripMenuItem.Name = "addEditSymbolsToolStripMenuItem";
            this.addEditSymbolsToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.addEditSymbolsToolStripMenuItem.Text = "Add/Edit Symbols";
            this.addEditSymbolsToolStripMenuItem.Click += new System.EventHandler(this.AddEditSymbolsToolStripMenuItem_Click);
            // 
            // addEditColumnsToolStripMenuItem
            // 
            this.addEditColumnsToolStripMenuItem.Name = "addEditColumnsToolStripMenuItem";
            this.addEditColumnsToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.addEditColumnsToolStripMenuItem.Text = "Add/Edit Columns";
            this.addEditColumnsToolStripMenuItem.Click += new System.EventHandler(this.AddEditColumnsToolStripMenuItem_Click);
            // 
            // clearExcelToolStripMenuItem
            // 
            this.clearExcelToolStripMenuItem.Name = "clearExcelToolStripMenuItem";
            this.clearExcelToolStripMenuItem.Size = new System.Drawing.Size(199, 24);
            this.clearExcelToolStripMenuItem.Text = "Clear Excel";
            this.clearExcelToolStripMenuItem.Visible = false;
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
            this.newCTRLNToolStripMenuItem,
            this.newsToolStripMenuItem,
            this.alertToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 30);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 2, 0, 2);
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
            // newCTRLNToolStripMenuItem
            // 
            this.newCTRLNToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newCTRLNToolStripMenuItem1,
            this.viewToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.newCTRLNToolStripMenuItem.Name = "newCTRLNToolStripMenuItem";
            this.newCTRLNToolStripMenuItem.Size = new System.Drawing.Size(127, 24);
            this.newCTRLNToolStripMenuItem.Text = "Market Watch";
            // 
            // newCTRLNToolStripMenuItem1
            // 
            this.newCTRLNToolStripMenuItem1.Name = "newCTRLNToolStripMenuItem1";
            this.newCTRLNToolStripMenuItem1.Size = new System.Drawing.Size(233, 26);
            this.newCTRLNToolStripMenuItem1.Text = "New      (CTRL+N)";
            this.newCTRLNToolStripMenuItem1.Click += new System.EventHandler(this.NewCTRLNToolStripMenuItem1_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(233, 26);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(233, 26);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // newsToolStripMenuItem
            // 
            this.newsToolStripMenuItem.Name = "newsToolStripMenuItem";
            this.newsToolStripMenuItem.Size = new System.Drawing.Size(65, 24);
            this.newsToolStripMenuItem.Text = "News";
            this.newsToolStripMenuItem.Visible = false;
            this.newsToolStripMenuItem.Click += new System.EventHandler(this.NewsToolStripMenuItem_Click);
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
            this.titleLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.licenceExpire.Location = new System.Drawing.Point(936, 0);
            this.licenceExpire.Name = "licenceExpire";
            this.licenceExpire.Padding = new System.Windows.Forms.Padding(0, 4, 10, 0);
            this.licenceExpire.Size = new System.Drawing.Size(179, 24);
            this.licenceExpire.TabIndex = 0;
            this.licenceExpire.Text = "Licence Expired :- ";
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
            this.savelabel.Location = new System.Drawing.Point(7, 5);
            this.savelabel.Name = "savelabel";
            this.savelabel.Size = new System.Drawing.Size(189, 16);
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
            this.fontSizeComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            // alertToolStripMenuItem
            // 
            this.alertToolStripMenuItem.Name = "alertToolStripMenuItem";
            this.alertToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.alertToolStripMenuItem.Text = "Alert";
            this.alertToolStripMenuItem.Click += new System.EventHandler(this.AlertToolStripMenuItem_Click);
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

        private void SaveMarketWatchHost_Click(object sender, EventArgs e)
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

        private void Txtsearch_TextChanged(object sender, EventArgs e)
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


        private void FontSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFontSizeFromComboBox();
        }

        private void FontSizeComboBox_TextChanged(object sender, EventArgs e)
        {
            ApplyFontSizeFromComboBox();
        }

        private void ApplyFontSizeFromComboBox()
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

                    EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;

                    if (editableMarketWatchGrid != null)
                    {
                        editableMarketWatchGrid.fontSize = _fontSize;
                        editableMarketWatchGrid.UpdateGridColumnVisibility();
                        //editableMarketWatchGrid.UpdateGridFontSize();
                    }
                }

            }

            catch (Exception ex)
            {
                ApplicationLogger.Log($"Error applying font size");
                ApplicationLogger.LogException(ex);
            }

        }

        #endregion

        private DataGridView defaultGrid;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem disconnectESCToolStripMenuItem;
        private Label licenceExpire;
        private Panel bottomPanel;
        private ContextMenuStrip Tools;
        private ToolStripMenuItem ExportToExcelToolStripMenuItem;
        private ToolStripMenuItem addEditSymbolsToolStripMenuItem;
        private ToolStripMenuItem addEditColumnsToolStripMenuItem;
        private ToolStripMenuItem fullScreenF11ToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private Panel panelAddColumns;
        private Panel panelAddSymbols;
        private CheckedListBox checkedListSymbols;
        private Button btnSelectAllSymbols;
        private Button btnConfirmAddSymbols;
        private Button btnCancelAddSymbols;
        private TextBox txtsearch;
        private Label searchTextLabel;
        private ComboBox fontSizeComboBox;
        private System.Windows.Forms.ToolStripMenuItem newMarketWatchMenuItem;
        private System.Windows.Forms.Panel headerPanel;
        public System.Windows.Forms.Label titleLabel;
        private ToolStripMenuItem saveMarketWatchHost;
        private ToolStripMenuItem newCTRLNToolStripMenuItem;
        private ToolStripMenuItem newCTRLNToolStripMenuItem1;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem clearExcelToolStripMenuItem;
        private Label savelabel;
        private ToolStripMenuItem newsToolStripMenuItem;
        private ToolStripMenuItem alertToolStripMenuItem;
    }
}