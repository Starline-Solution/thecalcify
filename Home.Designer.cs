using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectESCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenF11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newCTRLNToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMarketWatchHost = new System.Windows.Forms.ToolStripMenuItem();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.licenceExpire = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.Panel();
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
            this.defaultGrid.Location = new System.Drawing.Point(0, 60);
            this.defaultGrid.MultiSelect = false;
            this.defaultGrid.Name = "defaultGrid";
            this.defaultGrid.ReadOnly = true;
            this.defaultGrid.RowHeadersVisible = false;
            this.defaultGrid.RowHeadersWidth = 51;
            this.defaultGrid.RowTemplate.Height = 36;
            this.defaultGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.defaultGrid.Size = new System.Drawing.Size(1115, 622);
            this.defaultGrid.TabIndex = 1;
            this.defaultGrid.DataSourceChanged += new System.EventHandler(this.DefaultGrid_DataSourceChanged);
            this.defaultGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DefaultGrid_CellFormatting);
            this.defaultGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DefaultGrid_DataBindingComplete);
            // 
            // Tools
            // 
            this.Tools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.Tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportToExcelToolStripMenuItem,
            this.addEditSymbolsToolStripMenuItem});
            this.Tools.Name = "ClickMenuStrip";
            this.Tools.Size = new System.Drawing.Size(199, 52);
            // 
            // ExportToExcelToolStripMenuItem
            // 
            this.ExportToExcelToolStripMenuItem.Name = "ExportToExcelToolStripMenuItem";
            this.ExportToExcelToolStripMenuItem.Size = new System.Drawing.Size(198, 24);
            this.ExportToExcelToolStripMenuItem.Text = "Export To Excel";
            this.ExportToExcelToolStripMenuItem.Click += new System.EventHandler(this.ExportToExcelToolStripMenuItem_Click);
            // 
            // addEditSymbolsToolStripMenuItem
            // 
            this.addEditSymbolsToolStripMenuItem.Enabled = false;
            this.addEditSymbolsToolStripMenuItem.Name = "addEditSymbolsToolStripMenuItem";
            this.addEditSymbolsToolStripMenuItem.Size = new System.Drawing.Size(198, 24);
            this.addEditSymbolsToolStripMenuItem.Text = "Add/Edit Symbols";
            this.addEditSymbolsToolStripMenuItem.Click += new System.EventHandler(this.AddEditSymbolsToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.menuStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.newCTRLNToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 30);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1115, 30);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnectESCToolStripMenuItem,
            this.fullScreenF11ToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(64, 26);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // disconnectESCToolStripMenuItem
            // 
            this.disconnectESCToolStripMenuItem.Name = "disconnectESCToolStripMenuItem";
            this.disconnectESCToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
            this.disconnectESCToolStripMenuItem.Text = "Disconnect    (ESC)";
            this.disconnectESCToolStripMenuItem.Click += new System.EventHandler(this.disconnectESCToolStripMenuItem_Click);
            // 
            // fullScreenF11ToolStripMenuItem
            // 
            this.fullScreenF11ToolStripMenuItem.Name = "fullScreenF11ToolStripMenuItem";
            this.fullScreenF11ToolStripMenuItem.Size = new System.Drawing.Size(243, 26);
            this.fullScreenF11ToolStripMenuItem.Text = "Full Screen (F11)";
            this.fullScreenF11ToolStripMenuItem.Click += new System.EventHandler(this.fullScreenF11ToolStripMenuItem_Click);
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
            this.newCTRLNToolStripMenuItem1.Click += new System.EventHandler(this.newCTRLNToolStripMenuItem1_Click);
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
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(66, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
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
            this.bottomPanel.Controls.Add(this.licenceExpire);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 682);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(1115, 26);
            this.bottomPanel.TabIndex = 4;
            // 
            // searchTextLabel
            // 
            this.searchTextLabel.AutoSize = true;
            this.searchTextLabel.Location = new System.Drawing.Point(466, 33);
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
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Home_FormClosed);
            this.Load += new System.EventHandler(this.Home_Load);
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
                // Reset all filters
                if (defaultGrid?.DataSource is DataTable dt)
                {
                    dt.DefaultView.RowFilter = "";
                }

                EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
                if (editableMarketWatchGrid != null)
                {
                    editableMarketWatchGrid.serchstring = "";
                    foreach (DataGridViewRow row in editableMarketWatchGrid.Rows)
                    {
                        row.Visible = true;
                    }
                }
            }
            else
            {
                // Build RowFilter for defaultGrid
                if (defaultGrid?.DataSource is DataTable dt)
                {
                    var rowFilter = string.Join(" OR ", keywords
                        .Select(k => $"Name LIKE '%{k.Replace("'", "''")}%'"));
                    dt.DefaultView.RowFilter = rowFilter;
                }

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
            fontSize = Convert.ToInt32(fontSizeComboBox.SelectedItem.ToString());

            //EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance;
            //if (editableMarketWatchGrid != null)
            //{
            //    editableMarketWatchGrid.fontSize = fontSize;
            //    editableMarketWatchGrid.UpdateGridColumnVisibility();
            //}
        }

        private void AddEditSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (panelAddColumns != null && panelAddColumns.Visible)
            //    panelAddColumns.Visible = false;

            //// Create panel if it hasn't been initialized yet
            //if (panelAddSymbols == null)
            //{
            //    // Initialize panel
            //    panelAddSymbols = new Panel
            //    {
            //        Size = new System.Drawing.Size(500, 500),
            //        BackColor = Color.White,
            //        BorderStyle = BorderStyle.None,
            //        Visible = false,
            //        Padding = new Padding(20),
            //    };

            //    panelAddSymbols.Paint += (s2, e2) =>
            //    {
            //        ControlPaint.DrawBorder(e2.Graphics, panelAddSymbols.ClientRectangle,
            //            Color.LightGray, 2, ButtonBorderStyle.Solid,
            //            Color.LightGray, 2, ButtonBorderStyle.Solid,
            //            Color.LightGray, 2, ButtonBorderStyle.Solid,
            //            Color.LightGray, 2, ButtonBorderStyle.Solid);
            //    };

            //    panelAddSymbols.Location = new System.Drawing.Point(
            //        (this.Width - panelAddSymbols.Width) / 2,
            //        (this.Height - panelAddSymbols.Height) / 2
            //    );

            //    // Title label
            //    Label titleLabel = new Label
            //    {
            //        Text = "🔄 Add / Edit Symbols",
            //        Font = new System.Drawing.Font("Microsoft Sans Serif Semibold", 16, FontStyle.Bold),
            //        ForeColor = Color.FromArgb(50, 50, 50),
            //        Dock = DockStyle.Top,
            //        Height = 50,
            //        TextAlign = ContentAlignment.MiddleCenter,
            //        Padding = new Padding(0, 10, 0, 10)
            //    };

            //    // CheckedListBox
            //    checkedListSymbols = new CheckedListBox
            //    {
            //        Height = 320,
            //        Dock = DockStyle.Top,
            //        Font = new System.Drawing.Font("Microsoft Sans Serif", 10),
            //        BorderStyle = BorderStyle.FixedSingle,
            //        CheckOnClick = true,
            //        BackColor = Color.White
            //    };

            //    // Button container
            //    Panel buttonPanel = new Panel
            //    {
            //        Height = 80,
            //        Dock = DockStyle.Bottom,
            //        Padding = new Padding(10),
            //        BackColor = Color.White
            //    };

            //    // Buttons
            //    btnSelectAllSymbols = new Button
            //    {
            //        Text = "Select All",
            //        Height = 40,
            //        Width = 120,
            //        BackColor = Color.FromArgb(0, 122, 204),
            //        ForeColor = Color.White,
            //        FlatStyle = FlatStyle.Flat,
            //        Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
            //        Cursor = Cursors.Hand
            //    };
            //    btnSelectAllSymbols.FlatAppearance.BorderSize = 0;

            //    btnConfirmAddSymbols = new Button
            //    {
            //        Text = "✔ Save",
            //        Height = 40,
            //        Width = 120,
            //        BackColor = Color.FromArgb(0, 122, 204),
            //        ForeColor = Color.White,
            //        FlatStyle = FlatStyle.Flat,
            //        Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
            //        Cursor = Cursors.Hand
            //    };
            //    btnConfirmAddSymbols.FlatAppearance.BorderSize = 0;

            //    btnCancelAddSymbols = new Button
            //    {
            //        Text = "✖ Cancel",
            //        Height = 40,
            //        Width = 120,
            //        BackColor = Color.LightGray,
            //        ForeColor = Color.Black,
            //        FlatStyle = FlatStyle.Flat,
            //        Font = new System.Drawing.Font("Microsoft Sans Serif", 10, FontStyle.Bold),
            //        Cursor = Cursors.Hand
            //    };
            //    btnCancelAddSymbols.FlatAppearance.BorderSize = 0;

            //    // Layout
            //    btnSelectAllSymbols.Location = new Point(30, 35);
            //    btnConfirmAddSymbols.Location = new Point(170, 35);
            //    btnCancelAddSymbols.Location = new Point(310, 35);

            //    titleLabel.Dock = DockStyle.Top;
            //    checkedListSymbols.Dock = DockStyle.Fill; // So it takes remaining space
            //    buttonPanel.Dock = DockStyle.Bottom;


            //    buttonPanel.Controls.Add(btnSelectAllSymbols);
            //    buttonPanel.Controls.Add(btnConfirmAddSymbols);
            //    buttonPanel.Controls.Add(btnCancelAddSymbols);

            //    panelAddSymbols.Controls.Add(buttonPanel);  // bottom first
            //    panelAddSymbols.Controls.Add(checkedListSymbols); // middle
            //    panelAddSymbols.Controls.Add(titleLabel);   // top last


            //    this.Controls.Add(panelAddSymbols);

            //    this.Resize += (s3, e3) =>
            //    {
            //        panelAddSymbols.Location = new Point(
            //            (this.Width - panelAddSymbols.Width) / 2,
            //            (this.Height - panelAddSymbols.Height) / 2
            //        );
            //    };

            //    // Hook up events

            //    btnSelectAllSymbols.Click += (s, e2) =>
            //    {
            //        bool allChecked = true;
            //        for (int i = 0; i < checkedListSymbols.Items.Count; i++)
            //        {
            //            if (!checkedListSymbols.GetItemChecked(i))
            //            {
            //                allChecked = false;
            //                break;
            //            }
            //        }

            //        bool check = !allChecked;
            //        btnSelectAllSymbols.Text = check ? "Unselect All" : "Select All";

            //        for (int i = 0; i < checkedListSymbols.Items.Count; i++)
            //        {
            //            checkedListSymbols.SetItemChecked(i, check);
            //        }
            //    };

            //    btnConfirmAddSymbols.Click += async (s, e2) =>
            //    {
            //        // Get the checked display names (SymbolName)
            //        var currentlyCheckedNames = checkedListSymbols.CheckedItems.Cast<string>().ToList();


            //        // If nothing is selected
            //        if (!currentlyCheckedNames.Any())
            //        {
            //            MessageBox.Show("Please select at least one symbol to confirm.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }


            //        // Map checked names back to their symbols
            //        var currentlyCheckedSymbols = SymbolName
            //            .Where(x => currentlyCheckedNames.Contains(x.SymbolName))
            //            .Select(x => x.Symbol)
            //            .ToList();

            //        // Compare with previous selection
            //        var previouslySelected = selectedSymbols;

            //        var addedSymbols = currentlyCheckedSymbols.Except(previouslySelected).ToList();
            //        var removedSymbols = previouslySelected.Except(currentlyCheckedSymbols).ToList();

            //        if (!addedSymbols.Any() && !removedSymbols.Any())
            //        {
            //            MessageBox.Show("No changes made.");
            //            return;
            //        }


            //        // Save changes
            //        EditableMarketWatchGrid editableMarketWatchGrid = EditableMarketWatchGrid.CurrentInstance ?? new EditableMarketWatchGrid();
            //        editableMarketWatchGrid.isGrid = false;
            //        editableMarketWatchGrid.saveFileName = saveFileName;
            //        editableMarketWatchGrid.username = username;
            //        selectedSymbols = currentlyCheckedSymbols;
            //        editableMarketWatchGrid.SaveSymbols(selectedSymbols);
            //        identifiers = selectedSymbols;
            //        await SignalREvent();

            //        panelAddSymbols.Visible = false;
            //    };

            //    btnCancelAddSymbols.Click += (s, e2) =>
            //    {
            //        panelAddSymbols.Visible = false;
            //    };
            //}

            //// Refresh items before showing
            //checkedListSymbols.Items.Clear();

            //// Add selected symbols first
            //foreach (var item in SymbolName)
            //{
            //    if (identifiers.Contains(item.Symbol))
            //    {
            //        checkedListSymbols.Items.Add(item.SymbolName, true); // Display symbol name
            //    }
            //}

            //// Then unselected symbols
            //foreach (var item in SymbolName)
            //{
            //    if (!identifiers.Contains(item.Symbol))
            //    {
            //        checkedListSymbols.Items.Add(item.SymbolName, false);
            //    }
            //}


            //panelAddSymbols.Visible = true;
            //panelAddSymbols.BringToFront();


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
    }
}