using Microsoft.Vbe.Interop;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace thecalcify.Excel_Helper
{
    partial class UserExcelExportForm
    {
        private System.Windows.Forms.Panel panelBackground;

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelBackground = new System.Windows.Forms.Panel();
            this.excelSheetGrid = new System.Windows.Forms.DataGridView();
            this.sheetID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SheetName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sheetUploaded = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.SaveSheet = new System.Windows.Forms.DataGridViewButtonColumn();
            this.DeleteSheet = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ModifiedDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.syncSheetlabel = new System.Windows.Forms.Label();
            this.panelBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.excelSheetGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // panelBackground
            // 
            this.panelBackground.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelBackground.Controls.Add(this.excelSheetGrid);
            this.panelBackground.Controls.Add(this.syncSheetlabel);
            this.panelBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBackground.Location = new System.Drawing.Point(0, 0);
            this.panelBackground.Name = "panelBackground";
            this.panelBackground.Padding = new System.Windows.Forms.Padding(10);
            this.panelBackground.Size = new System.Drawing.Size(1239, 718);
            this.panelBackground.TabIndex = 0;
            // 
            // excelSheetGrid
            // 
            this.excelSheetGrid.AllowUserToAddRows = false;
            this.excelSheetGrid.AllowUserToDeleteRows = false;
            this.excelSheetGrid.AllowUserToResizeRows = false;
            this.excelSheetGrid.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.excelSheetGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.excelSheetGrid.ColumnHeadersHeight = 38;
            this.excelSheetGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sheetID,
            this.type,
            this.SheetName,
            this.sheetUploaded,
            this.SaveSheet,
            this.DeleteSheet,
            this.ModifiedDate});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.excelSheetGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.excelSheetGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.excelSheetGrid.EnableHeadersVisualStyles = false;
            this.excelSheetGrid.Location = new System.Drawing.Point(10, 52);
            this.excelSheetGrid.MultiSelect = false;
            this.excelSheetGrid.Name = "excelSheetGrid";
            this.excelSheetGrid.RowHeadersVisible = false;
            this.excelSheetGrid.RowHeadersWidth = 51;
            this.excelSheetGrid.RowTemplate.Height = 36;
            this.excelSheetGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.excelSheetGrid.Size = new System.Drawing.Size(1219, 656);
            this.excelSheetGrid.TabIndex = 0;
            this.excelSheetGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.excelSheetGrid_CellContentClick);
            this.excelSheetGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.excelSheetGrid_CellFormatting);
            this.excelSheetGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.excelSheetGrid_CellMouseEnter);
            this.excelSheetGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.excelSheetGrid_CellPainting);
            // 
            // sheetID
            // 
            this.sheetID.HeaderText = "SheetId";
            this.sheetID.MinimumWidth = 6;
            this.sheetID.Name = "sheetID";
            this.sheetID.Visible = false;
            this.sheetID.Width = 125;
            // 
            // type
            // 
            this.type.HeaderText = "type";
            this.type.MinimumWidth = 6;
            this.type.Name = "type";
            this.type.ReadOnly = true;
            this.type.Visible = false;
            this.type.Width = 125;
            // 
            // SheetName
            // 
            this.SheetName.HeaderText = "Sheet Name";
            this.SheetName.MinimumWidth = 6;
            this.SheetName.Name = "SheetName";
            this.SheetName.ReadOnly = true;
            this.SheetName.Width = 125;
            // 
            // sheetUploaded
            // 
            this.sheetUploaded.HeaderText = "Uploaded";
            this.sheetUploaded.MinimumWidth = 6;
            this.sheetUploaded.Name = "sheetUploaded";
            this.sheetUploaded.ReadOnly = true;
            this.sheetUploaded.Width = 125;
            // 
            // SaveSheet
            // 
            this.SaveSheet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveSheet.HeaderText = "Sync Sheet";
            this.SaveSheet.MinimumWidth = 6;
            this.SaveSheet.Name = "SaveSheet";
            this.SaveSheet.Text = "Sync";
            this.SaveSheet.UseColumnTextForButtonValue = true;
            this.SaveSheet.Width = 125;
            // 
            // DeleteSheet
            // 
            this.DeleteSheet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteSheet.HeaderText = "Delete Sheet";
            this.DeleteSheet.MinimumWidth = 6;
            this.DeleteSheet.Name = "DeleteSheet";
            this.DeleteSheet.Text = "Delete";
            this.DeleteSheet.UseColumnTextForButtonValue = true;
            this.DeleteSheet.Width = 125;
            // 
            // ModifiedDate
            // 
            this.ModifiedDate.HeaderText = "Modified Date";
            this.ModifiedDate.MinimumWidth = 6;
            this.ModifiedDate.Name = "ModifiedDate";
            this.ModifiedDate.ReadOnly = true;
            this.ModifiedDate.Width = 125;
            // 
            // syncSheetlabel
            // 
            this.syncSheetlabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.syncSheetlabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.syncSheetlabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.syncSheetlabel.Location = new System.Drawing.Point(10, 10);
            this.syncSheetlabel.Name = "syncSheetlabel";
            this.syncSheetlabel.Size = new System.Drawing.Size(1219, 42);
            this.syncSheetlabel.TabIndex = 1;
            this.syncSheetlabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UserExcelExportForm
            // 
            this.Controls.Add(this.panelBackground);
            this.Name = "UserExcelExportForm";
            this.Size = new System.Drawing.Size(1239, 718);
            this.Load += new System.EventHandler(this.UserExcelExportForm_Load);
            this.panelBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.excelSheetGrid)).EndInit();
            this.ResumeLayout(false);

        }


        private DataGridView excelSheetGrid;
        private Label syncSheetlabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ExcelAppManager.ReleaseExcelApp();
            }
            base.Dispose(disposing);
        }

        private DataGridViewTextBoxColumn sheetID;
        private DataGridViewTextBoxColumn type;
        private DataGridViewTextBoxColumn SheetName;
        private DataGridViewCheckBoxColumn sheetUploaded;
        private DataGridViewButtonColumn SaveSheet;
        private DataGridViewButtonColumn DeleteSheet;
        private DataGridViewTextBoxColumn ModifiedDate;


        //private void btnExport_MouseEnter(object sender, EventArgs e)
        //{
        //    btnExport.BackColor = Color.FromArgb(5, 130, 235);
        //}

        //private void btnExport_MouseLeave(object sender, EventArgs e)
        //{
        //    btnExport.BackColor = Color.FromArgb(0, 120, 215);
        //}


    }
}
