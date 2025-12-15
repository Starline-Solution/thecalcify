using System.Drawing;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    partial class About
    {
        private System.ComponentModel.IContainer components = null;
        private Panel cardPanel;
        private Panel separatorLine;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblExpiry;
        private Label lblVersion;
        private Label lblModified;
        private Button btnUpdate;
        private PictureBox pictureBox1;

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
            this.cardPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.separatorLine = new System.Windows.Forms.Panel();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblModified = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.cardPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // cardPanel
            // 
            this.cardPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cardPanel.BackColor = System.Drawing.Color.White;
            this.cardPanel.Controls.Add(this.pictureBox1);
            this.cardPanel.Controls.Add(this.separatorLine);
            this.cardPanel.Controls.Add(this.lblUsername);
            this.cardPanel.Controls.Add(this.lblPassword);
            this.cardPanel.Controls.Add(this.lblExpiry);
            this.cardPanel.Controls.Add(this.lblVersion);
            this.cardPanel.Controls.Add(this.lblModified);
            this.cardPanel.Controls.Add(this.btnUpdate);
            this.cardPanel.Location = new System.Drawing.Point(0, 0);
            this.cardPanel.Margin = new System.Windows.Forms.Padding(4);
            this.cardPanel.Name = "cardPanel";
            this.cardPanel.Size = new System.Drawing.Size(400, 430);
            this.cardPanel.TabIndex = 0;
            this.cardPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.cardPanel_Paint);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::thecalcify.Properties.Resources.starline_solution;
            this.pictureBox1.Location = new System.Drawing.Point(30, 25);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(340, 60);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // separatorLine
            // 
            this.separatorLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.separatorLine.Location = new System.Drawing.Point(30, 95);
            this.separatorLine.Margin = new System.Windows.Forms.Padding(4);
            this.separatorLine.Name = "separatorLine";
            this.separatorLine.Size = new System.Drawing.Size(340, 3);
            this.separatorLine.TabIndex = 1;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblUsername.Location = new System.Drawing.Point(40, 120);
            this.lblUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(194, 25);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "User Name: thecalcify";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblPassword.Location = new System.Drawing.Point(40, 155);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(180, 25);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password: thecalcify";
            // 
            // lblExpiry
            // 
            this.lblExpiry.AutoSize = true;
            this.lblExpiry.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblExpiry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblExpiry.Location = new System.Drawing.Point(40, 190);
            this.lblExpiry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExpiry.Name = "lblExpiry";
            this.lblExpiry.Size = new System.Drawing.Size(241, 25);
            this.lblExpiry.TabIndex = 4;
            this.lblExpiry.Text = "License Expires:  31:12:2025";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblVersion.Location = new System.Drawing.Point(40, 225);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(127, 25);
            this.lblVersion.TabIndex = 5;
            this.lblVersion.Text = "Version:  1.0.0";
            // 
            // lblModified
            // 
            this.lblModified.AutoSize = true;
            this.lblModified.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblModified.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblModified.Location = new System.Drawing.Point(40, 260);
            this.lblModified.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblModified.Name = "lblModified";
            this.lblModified.Size = new System.Drawing.Size(302, 25);
            this.lblModified.TabIndex = 6;
            this.lblModified.Text = "Version Modified Date:  05:12:2025";
            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.btnUpdate.FlatAppearance.BorderSize = 0;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdate.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnUpdate.ForeColor = System.Drawing.Color.White;
            this.btnUpdate.Location = new System.Drawing.Point(95, 330);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(210, 50);
            this.btnUpdate.TabIndex = 7;
            this.btnUpdate.Text = "Check for Update";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            this.btnUpdate.Paint += new System.Windows.Forms.PaintEventHandler(this.btnUpdate_Paint);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Controls.Add(this.cardPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "About";
            this.Size = new System.Drawing.Size(800, 600);
            this.Load += new System.EventHandler(this.About_Load);
            this.Resize += new System.EventHandler(this.About_Resize);
            this.cardPanel.ResumeLayout(false);
            this.cardPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
