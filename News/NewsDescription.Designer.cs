using System;

namespace thecalcify.MarketWatch
{
    partial class NewsDescription
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblHeadline;
        private System.Windows.Forms.Label lblDateSource;
        private System.Windows.Forms.RichTextBox txtDescription;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panelBottom;

        // Import Windows API
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblHeadline = new System.Windows.Forms.Label();
            this.lblDateSource = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.RichTextBox();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblHeadline, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDateSource, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtDescription, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblCopyright, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panelBottom, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(658, 535);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblHeadline
            // 
            this.lblHeadline.AutoSize = true;
            this.lblHeadline.BackColor = System.Drawing.Color.White;
            this.lblHeadline.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeadline.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeadline.Location = new System.Drawing.Point(23, 15);
            this.lblHeadline.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.lblHeadline.Name = "lblHeadline";
            this.lblHeadline.Size = new System.Drawing.Size(612, 37);
            this.lblHeadline.TabIndex = 0;
            this.lblHeadline.Text = "HeadLine";
            // 
            // lblDateSource
            // 
            this.lblDateSource.AutoSize = true;
            this.lblDateSource.BackColor = System.Drawing.Color.White;
            this.lblDateSource.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDateSource.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDateSource.ForeColor = System.Drawing.Color.Gray;
            this.lblDateSource.Location = new System.Drawing.Point(23, 62);
            this.lblDateSource.Margin = new System.Windows.Forms.Padding(3, 0, 3, 15);
            this.lblDateSource.Name = "lblDateSource";
            this.lblDateSource.Size = new System.Drawing.Size(612, 20);
            this.lblDateSource.TabIndex = 1;
            this.lblDateSource.Text = "Time";
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.Color.White;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtDescription.Location = new System.Drawing.Point(23, 97);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(612, 327);
            this.txtDescription.TabIndex = 2;
            this.txtDescription.TabStop = false;
            this.txtDescription.Text = "Description";
            this.txtDescription.Click += new System.EventHandler(this.txtDescription_Enter);
            this.txtDescription.Enter += new System.EventHandler(this.txtDescription_Enter);
            this.txtDescription.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtDescription_Enter);
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.BackColor = System.Drawing.Color.White;
            this.lblCopyright.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCopyright.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblCopyright.ForeColor = System.Drawing.Color.DarkGray;
            this.lblCopyright.Location = new System.Drawing.Point(23, 434);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(3, 0, 3, 15);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(612, 19);
            this.lblCopyright.TabIndex = 3;
            this.lblCopyright.Text = "(c) Copyright Thomson Reuters 2024";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.White;
            this.panelBottom.Controls.Add(this.btnClose);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(23, 471);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(612, 46);
            this.panelBottom.TabIndex = 4;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(504, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(105, 27);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // NewsDescription
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(658, 535);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NewsDescription";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "News Details";
            this.Load += new System.EventHandler(this.NewsDescription_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void txtDescription_Enter(object sender, EventArgs e)
        {
            // Immediately move focus away to prevent cursor blinking
            btnClose.Focus();

            // Hide caret using Windows API
            HideCaret(txtDescription.Handle);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NewsDescription_Load(object sender, EventArgs e)
        {
            // Set initial focus to close button to prevent cursor in textbox
            btnClose.Focus();
        }
    }

}