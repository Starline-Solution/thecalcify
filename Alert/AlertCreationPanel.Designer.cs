using System.Windows.Forms;

namespace thecalcify.Alert
{
    partial class AlertCreationPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlertCreationPanel));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnOpenAlert = new System.Windows.Forms.ToolStripButton();
            this.btnAlertHistory = new System.Windows.Forms.ToolStripButton();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            
            // Create AlertId column first
            this.AlertId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlertId.HeaderText = "ID";
            this.AlertId.Name = "AlertId";
            this.AlertId.Visible = false;

            this.Symbol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Rate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreationTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TriggerTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Edit = new System.Windows.Forms.DataGridViewImageColumn();
            this.btnAddAlert = new System.Windows.Forms.Button();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip.AutoSize = false;
            this.toolStrip.BackColor = System.Drawing.Color.White;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenAlert,
            this.btnAlertHistory});
            this.toolStrip.Location = new System.Drawing.Point(20, 20);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.toolStrip.Size = new System.Drawing.Size(885, 60);
            this.toolStrip.TabIndex = 0;
            // 
            // btnOpenAlert
            // 
            this.btnOpenAlert.AutoSize = false;
            this.btnOpenAlert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOpenAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnOpenAlert.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnOpenAlert.Image = global::thecalcify.Properties.Resources.add;
            this.btnOpenAlert.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOpenAlert.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenAlert.Name = "btnOpenAlert";
            this.btnOpenAlert.Size = new System.Drawing.Size(100, 50);
            this.btnOpenAlert.Text = "Open Alerts";
            this.btnOpenAlert.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnOpenAlert.Click += new System.EventHandler(this.BtnOpenAlert_Click);
            // 
            // btnAlertHistory
            // 
            this.btnAlertHistory.AutoSize = false;
            this.btnAlertHistory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAlertHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnAlertHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnAlertHistory.Image = global::thecalcify.Properties.Resources.history;
            this.btnAlertHistory.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnAlertHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAlertHistory.Name = "btnAlertHistory";
            this.btnAlertHistory.Size = new System.Drawing.Size(100, 50);
            this.btnAlertHistory.Text = "Alert History";
            this.btnAlertHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAlertHistory.Click += new System.EventHandler(this.BtnAlertHistory_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeight = 40;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Symbol,
            this.Column,
            this.Rate,
            this.CreationTime,
            this.TriggerTime,
            this.Edit,
            this.AlertId});
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.dataGridView1.Location = new System.Drawing.Point(20, 80);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 200;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(882, 320);
            this.dataGridView1.TabIndex = 1;
            ////
            ////Id
            ////
            //this.AlertId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            //this.AlertId.HeaderText = "ID";
            //this.AlertId.Name = "AlertId";
            //this.AlertId.Visible = false;
            // 
            // Symbol
            // 
            this.Symbol.HeaderText = "Symbol";
            this.Symbol.MinimumWidth = 6;
            this.Symbol.Name = "Symbol";
            this.Symbol.Width = 93;
            // 
            // Column
            // 
            this.Column.HeaderText = "Column";
            this.Column.MinimumWidth = 6;
            this.Column.Name = "Column";
            this.Column.Width = 95;
            // 
            // Rate
            // 
            this.Rate.HeaderText = "Rate ";
            this.Rate.MinimumWidth = 6;
            this.Rate.Name = "Rate";
            this.Rate.ReadOnly = true;
            this.Rate.Width = 78;
            // 
            // CreationTime
            // 
            this.CreationTime.HeaderText = "Creation Time";
            this.CreationTime.MinimumWidth = 6;
            this.CreationTime.Name = "CreationTime";
            this.CreationTime.Width = 143;
            // 
            // TriggerTime
            // 
            this.TriggerTime.HeaderText = "Trigger Time";
            this.TriggerTime.MinimumWidth = 6;
            this.TriggerTime.Name = "TriggerTime";
            this.TriggerTime.Visible = false;
            this.TriggerTime.Width = 133;
            // 
            // Edit
            // 
            this.Edit.HeaderText = "Edit";
            this.Edit.MinimumWidth = 6;
            this.Edit.Name = "Edit";
            this.Edit.Width = 44;
            // 
            // btnAddAlert
            // 
            this.btnAddAlert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddAlert.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnAddAlert.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddAlert.FlatAppearance.BorderSize = 0;
            this.btnAddAlert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnAddAlert.ForeColor = System.Drawing.Color.White;
            this.btnAddAlert.Location = new System.Drawing.Point(20, 420);
            this.btnAddAlert.Name = "btnAddAlert";
            this.btnAddAlert.Padding = new System.Windows.Forms.Padding(5);
            this.btnAddAlert.Size = new System.Drawing.Size(150, 40);
            this.btnAddAlert.TabIndex = 2;
            this.btnAddAlert.Text = "Add New Alert";
            this.btnAddAlert.UseVisualStyleBackColor = false;
            this.btnAddAlert.Click += new System.EventHandler(this.BtnAlert_Click);
            // 
            // AlertCreationPanel
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(925, 480);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnAddAlert);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "AlertCreationPanel";
            this.Padding = new System.Windows.Forms.Padding(20);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Alert Manager";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnOpenAlert;
        private System.Windows.Forms.ToolStripButton btnAlertHistory;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnAddAlert;
        private DataGridViewTextBoxColumn AlertId;
        private DataGridViewTextBoxColumn Symbol;
        private DataGridViewTextBoxColumn Rate;
        private DataGridViewTextBoxColumn Column;
        private DataGridViewTextBoxColumn CreationTime;
        private DataGridViewTextBoxColumn TriggerTime;
        private DataGridViewImageColumn Edit;
    }
}