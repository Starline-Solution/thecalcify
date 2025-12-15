using System;
using System.Drawing;
using System.Windows.Forms;

namespace thecalcify.Excel_Helper
{
    partial class UserExcelExportForm
    {
        //private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Panel panelCard;
        private System.Windows.Forms.CheckedListBox clbSheets;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelBackground;

        private void InitializeComponent()
        {
            this.panelBackground = new System.Windows.Forms.Panel();
            this.panelCard = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.clbSheets = new System.Windows.Forms.CheckedListBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.panelBackground.SuspendLayout();
            this.panelCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBackground
            // 
            this.panelBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(251)))));
            this.panelBackground.Controls.Add(this.panelCard);
            this.panelBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBackground.Location = new System.Drawing.Point(0, 0);
            this.panelBackground.Name = "panelBackground";
            this.panelBackground.Size = new System.Drawing.Size(150, 150);
            this.panelBackground.TabIndex = 0;
            // 
            // panelCard
            // 
            this.panelCard.BackColor = System.Drawing.Color.White;
            this.panelCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard.Controls.Add(this.lblTitle);
            this.panelCard.Controls.Add(this.clbSheets);
            this.panelCard.Controls.Add(this.btnExport);
            this.panelCard.Location = new System.Drawing.Point(30, 120);
            this.panelCard.Name = "panelCard";
            this.panelCard.Padding = new System.Windows.Forms.Padding(20);
            this.panelCard.Size = new System.Drawing.Size(420, 460);
            this.panelCard.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(10, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(185, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Select Worksheets";
            // 
            // clbSheets
            // 
            this.clbSheets.CheckOnClick = true;
            this.clbSheets.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.clbSheets.Location = new System.Drawing.Point(15, 50);
            this.clbSheets.Name = "clbSheets";
            this.clbSheets.Size = new System.Drawing.Size(350, 279);
            this.clbSheets.TabIndex = 1;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.FromArgb(81, 213, 220);
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExport.ForeColor = System.Drawing.Color.White;
            this.btnExport.Location = new System.Drawing.Point(15, 370);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(160, 40);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "Export Selected";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            this.btnExport.MouseEnter += new System.EventHandler(this.btnExport_MouseEnter);
            this.btnExport.MouseLeave += new System.EventHandler(this.btnExport_MouseLeave);
            // 
            // UserExcelExportForm
            // 
            this.Controls.Add(this.panelBackground);
            this.Name = "UserExcelExportForm";
            this.Load += new System.EventHandler(this.UserExcelExportForm_Load);
            this.Resize += new System.EventHandler(this.UserExcelExportForm_Resize);
            this.panelBackground.ResumeLayout(false);
            this.panelCard.ResumeLayout(false);
            this.panelCard.PerformLayout();
            this.ResumeLayout(false);

        }

        // ---------------- Rounded Corners API ----------------
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);


        private void btnExport_MouseEnter(object sender, EventArgs e)
        {
            btnExport.BackColor = Color.FromArgb(5, 130, 235);
        }

        private void btnExport_MouseLeave(object sender, EventArgs e)
        {
            btnExport.BackColor = Color.FromArgb(0, 120, 215);
        }


    }
}
