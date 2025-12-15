using System.Windows.Forms;

namespace thecalcify.Helper
{
    partial class About
    {
        private System.ComponentModel.IContainer components = null;

        private Panel cardPanel;
        private Label lblHeader;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblExpiry;
        private Label lblVersion;
        private Label lblModified;
        private Button updateButton;
        private Label rightsLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.cardPanel = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblExpiry = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblModified = new System.Windows.Forms.Label();
            this.updateButton = new System.Windows.Forms.Button();
            this.rightsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // 
            // cardPanel
            // 
            this.cardPanel.BackColor = System.Drawing.Color.White;
            this.cardPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardPanel.Location = new System.Drawing.Point(0, 0); // Centered later
            this.cardPanel.Name = "cardPanel";
            this.cardPanel.Padding = new System.Windows.Forms.Padding(20);
            this.cardPanel.Size = new System.Drawing.Size(540, 260);
            this.cardPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cardPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.cardPanel_Paint);

            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeader.Location = new System.Drawing.Point(25, 15);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(199, 37);
            this.lblHeader.Text = "User Information";

            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblUsername.Location = new System.Drawing.Point(25, 70);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(115, 25);
            this.lblUsername.Text = "User Name: ";

            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblPassword.Location = new System.Drawing.Point(25, 105);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(101, 25);
            this.lblPassword.Text = "Password: ";

            // 
            // lblExpiry
            // 
            this.lblExpiry.AutoSize = true;
            this.lblExpiry.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblExpiry.Location = new System.Drawing.Point(25, 140);
            this.lblExpiry.Name = "lblExpiry";
            this.lblExpiry.Size = new System.Drawing.Size(150, 25);
            this.lblExpiry.Text = "License Expires: ";

            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblVersion.Location = new System.Drawing.Point(25, 175);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(87, 25);
            this.lblVersion.Text = "Version: ";

            // 
            // lblModified
            // 
            this.lblModified.AutoSize = true;
            this.lblModified.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblModified.Location = new System.Drawing.Point(25, 210);
            this.lblModified.Name = "lblModified";
            this.lblModified.Size = new System.Drawing.Size(195, 25);
            this.lblModified.Text = "Version Modified Date: ";

            // Add labels into cardPanel
            this.cardPanel.Controls.Add(this.lblHeader);
            this.cardPanel.Controls.Add(this.lblUsername);
            this.cardPanel.Controls.Add(this.lblPassword);
            this.cardPanel.Controls.Add(this.lblExpiry);
            this.cardPanel.Controls.Add(this.lblVersion);
            this.cardPanel.Controls.Add(this.lblModified);

            // 
            // updateButton
            // 
            this.updateButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.updateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.updateButton.ForeColor = System.Drawing.Color.White;
            this.updateButton.Location = new System.Drawing.Point(0, 0); // Centered later
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(220, 45);
            this.updateButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.updateButton.Text = "Check for Update";
            this.updateButton.UseVisualStyleBackColor = false;
            this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);

            // 
            // rightsLabel
            // 
            this.rightsLabel.AutoSize = true;
            this.rightsLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.rightsLabel.ForeColor = System.Drawing.Color.Gray;
            this.rightsLabel.Location = new System.Drawing.Point(10, 520);
            this.rightsLabel.Name = "rightsLabel";
            this.rightsLabel.Size = new System.Drawing.Size(291, 23);
            this.rightsLabel.Text = "© 2025 thecalcify. All rights reserved.";
            this.rightsLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // 
            // About (UserControl)
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.cardPanel);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.rightsLabel);
            this.Name = "About";
            this.Size = new System.Drawing.Size(900, 550);
            this.Load += new System.EventHandler(this.About_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
