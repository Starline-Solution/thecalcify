using System.Windows.Forms;

namespace thecalcify.MarketWatch
{
    partial class EditableMarketWatchControl
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
                LiveGridRegistry.Unregister(this);

            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._grid = new System.Windows.Forms.DataGridView();
            this.Tools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addEditColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
            this.Tools.SuspendLayout();
            this.SuspendLayout();
            // 
            // _grid
            // 
            this._grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._grid.ContextMenuStrip = this.Tools;
            this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grid.Location = new System.Drawing.Point(0, 0);
            this._grid.Name = "_grid";
            this._grid.RowHeadersWidth = 51;
            this._grid.RowTemplate.Height = 24;
            this._grid.Size = new System.Drawing.Size(1468, 679);
            this._grid.TabIndex = 0;
            // 
            // Tools
            // 
            this.Tools.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.Tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addEditColumnsToolStripMenuItem});
            this.Tools.Name = "Tools";
            this.Tools.Size = new System.Drawing.Size(225, 28);
            // 
            // addEditColumnsToolStripMenuItem
            // 
            this.addEditColumnsToolStripMenuItem.Name = "addEditColumnsToolStripMenuItem";
            this.addEditColumnsToolStripMenuItem.Size = new System.Drawing.Size(224, 24);
            this.addEditColumnsToolStripMenuItem.Text = "✍️ Add/Edit Columns";
            this.addEditColumnsToolStripMenuItem.Click += new System.EventHandler(this.AddSymbol_Click);
            // 
            // EditableMarketWatchControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._grid);
            this.Name = "EditableMarketWatchControl";
            this.Size = new System.Drawing.Size(1468, 679);
            ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
            this.Tools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridView _grid;
        private ContextMenuStrip Tools;
        private ToolStripMenuItem addEditColumnsToolStripMenuItem;
        public Panel panelAddSymbols;
        public CheckedListBox checkedListSymbols;
        public Button btnSelectAllSymbols;
        public Button btnConfirmAddSymbols;
        public Button btnCancelAddSymbols;

    }
}
