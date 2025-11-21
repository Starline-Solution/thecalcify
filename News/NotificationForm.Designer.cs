using System.Drawing;

namespace thecalcify.News
{
    partial class NotificationForm
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

        #region Designer Code (UI elements)

        private System.Windows.Forms.Label lblHeadline;
        private System.Windows.Forms.Label lblTimestamp;
        private System.Windows.Forms.Button btnClose;

        private void InitializeComponent()
        {
            this.lblHeadline = new System.Windows.Forms.Label();
            this.lblTimestamp = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblHeadline
            // 
            this.lblHeadline.AutoEllipsis = true;
            this.lblHeadline.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblHeadline.ForeColor = System.Drawing.Color.Black;
            this.lblHeadline.Location = new System.Drawing.Point(15, 50);
            this.lblHeadline.MaximumSize = new System.Drawing.Size(320, 0);
            this.lblHeadline.Name = "lblHeadline";
            this.lblHeadline.Size = new System.Drawing.Size(320, 50);
            this.lblHeadline.TabIndex = 1;
            this.lblHeadline.Text = "Sample Headline Text Here";
            this.lblHeadline.DoubleClick += new System.EventHandler(this.lblHeadline_DoubleClick);
            // 
            // lblTimestamp
            // 
            this.lblTimestamp.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblTimestamp.ForeColor = System.Drawing.Color.Gray;
            this.lblTimestamp.Location = new System.Drawing.Point(15, 110);
            this.lblTimestamp.Name = "lblTimestamp";
            this.lblTimestamp.Size = new System.Drawing.Size(320, 25);
            this.lblTimestamp.TabIndex = 2;
            this.lblTimestamp.Text = "Timestamp or Source";
            this.lblTimestamp.DoubleClick += new System.EventHandler(this.lblTimestamp_DoubleClick);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(367, 7);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(30, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            this.btnClose.MouseEnter += new System.EventHandler(this.BtnClose_MouseEnter);
            this.btnClose.MouseLeave += new System.EventHandler(this.BtnClose_MouseLeave);
            // 
            // NotificationForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 160);
            this.Controls.Add(this.lblTimestamp);
            this.Controls.Add(this.lblHeadline);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NotificationForm";
            this.DoubleClick += new System.EventHandler(this.NotificationForm_DoubleClick);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
