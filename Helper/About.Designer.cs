namespace thecalcify.Helper
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.userGroupBox = new System.Windows.Forms.GroupBox();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.userLabel = new System.Windows.Forms.Label();
            this.userPasswordLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.licenceExpireLabel = new System.Windows.Forms.Label();
            this.licenceLabel = new System.Windows.Forms.Label();
            this.appVersionLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.updateButton = new System.Windows.Forms.Button();
            this.userGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // userGroupBox
            // 
            this.userGroupBox.Controls.Add(this.usernameLabel);
            this.userGroupBox.Controls.Add(this.userLabel);
            this.userGroupBox.Controls.Add(this.userPasswordLabel);
            this.userGroupBox.Controls.Add(this.passwordLabel);
            this.userGroupBox.Controls.Add(this.licenceExpireLabel);
            this.userGroupBox.Controls.Add(this.licenceLabel);
            this.userGroupBox.Controls.Add(this.appVersionLabel);
            this.userGroupBox.Controls.Add(this.versionLabel);
            this.userGroupBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.userGroupBox.Location = new System.Drawing.Point(40, 30);
            this.userGroupBox.Name = "userGroupBox";
            this.userGroupBox.Size = new System.Drawing.Size(520, 230);
            this.userGroupBox.TabIndex = 0;
            this.userGroupBox.TabStop = false;
            this.userGroupBox.Text = "User Information";
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.usernameLabel.Location = new System.Drawing.Point(180, 40);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(0, 23);
            this.usernameLabel.TabIndex = 0;
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.userLabel.Location = new System.Drawing.Point(20, 40);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(99, 23);
            this.userLabel.TabIndex = 1;
            this.userLabel.Text = "User Name:";
            // 
            // userPasswordLabel
            // 
            this.userPasswordLabel.AutoSize = true;
            this.userPasswordLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.userPasswordLabel.Location = new System.Drawing.Point(180, 75);
            this.userPasswordLabel.Name = "userPasswordLabel";
            this.userPasswordLabel.Size = new System.Drawing.Size(0, 23);
            this.userPasswordLabel.TabIndex = 2;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.passwordLabel.Location = new System.Drawing.Point(20, 75);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(84, 23);
            this.passwordLabel.TabIndex = 3;
            this.passwordLabel.Text = "Password:";
            // 
            // licenceExpireLabel
            // 
            this.licenceExpireLabel.AutoSize = true;
            this.licenceExpireLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.licenceExpireLabel.Location = new System.Drawing.Point(180, 110);
            this.licenceExpireLabel.Name = "licenceExpireLabel";
            this.licenceExpireLabel.Size = new System.Drawing.Size(0, 23);
            this.licenceExpireLabel.TabIndex = 4;
            // 
            // licenceLabel
            // 
            this.licenceLabel.AutoSize = true;
            this.licenceLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.licenceLabel.Location = new System.Drawing.Point(20, 110);
            this.licenceLabel.Name = "licenceLabel";
            this.licenceLabel.Size = new System.Drawing.Size(127, 23);
            this.licenceLabel.TabIndex = 5;
            this.licenceLabel.Text = "License Expires:";
            // 
            // appVersionLabel
            // 
            this.appVersionLabel.AutoSize = true;
            this.appVersionLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.appVersionLabel.Location = new System.Drawing.Point(180, 145);
            this.appVersionLabel.Name = "appVersionLabel";
            this.appVersionLabel.Size = new System.Drawing.Size(0, 23);
            this.appVersionLabel.TabIndex = 6;
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.versionLabel.Location = new System.Drawing.Point(20, 145);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(70, 23);
            this.versionLabel.TabIndex = 7;
            this.versionLabel.Text = "Version:";
            // 
            // updateButton
            // 
            this.updateButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.updateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.updateButton.ForeColor = System.Drawing.Color.White;
            this.updateButton.Location = new System.Drawing.Point(200, 300);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(200, 45);
            this.updateButton.TabIndex = 1;
            this.updateButton.Text = "Check for Update";
            this.updateButton.UseVisualStyleBackColor = false;
            this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.userGroupBox);
            this.Controls.Add(this.updateButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "About";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About - thecalcify";
            this.Load += new System.EventHandler(this.About_Load);
            this.userGroupBox.ResumeLayout(false);
            this.userGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox userGroupBox;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Label userPasswordLabel;
        private System.Windows.Forms.Label licenceLabel;
        private System.Windows.Forms.Label licenceExpireLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label appVersionLabel;
        private System.Windows.Forms.Button updateButton;
    }
}