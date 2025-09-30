using System;
using System.Drawing;
using System.Windows.Forms;

namespace thecalcify
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.uname = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.Label();
            this.unameTextBox = new System.Windows.Forms.TextBox();
            this.passwordtextBox = new System.Windows.Forms.TextBox();
            this.loginbutton = new System.Windows.Forms.Button();
            this.eyePictureBox = new System.Windows.Forms.PictureBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.formPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.exitLabelButton = new System.Windows.Forms.Label();
            this.saveCredential = new System.Windows.Forms.CheckBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.unameUnderline = new System.Windows.Forms.Panel();
            this.passwordUnderline = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.eyePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.formPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // uname
            // 
            resources.ApplyResources(this.uname, "uname");
            this.uname.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.uname.Name = "uname";
            // 
            // password
            // 
            resources.ApplyResources(this.password, "password");
            this.password.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.password.Name = "password";
            // 
            // unameTextBox
            // 
            this.unameTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.unameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.unameTextBox, "unameTextBox");
            this.unameTextBox.Name = "unameTextBox";
            this.unameTextBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.unameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UnameTextBox_KeyDown);
            this.unameTextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // passwordtextBox
            // 
            this.passwordtextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.passwordtextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.passwordtextBox, "passwordtextBox");
            this.passwordtextBox.Name = "passwordtextBox";
            this.passwordtextBox.Enter += new System.EventHandler(this.TextBox_Enter);
            this.passwordtextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PasswordtextBox_KeyDown);
            this.passwordtextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // loginbutton
            // 
            this.loginbutton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.loginbutton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.loginbutton.FlatAppearance.BorderSize = 0;
            this.loginbutton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(175)))));
            this.loginbutton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(100)))), ((int)(((byte)(195)))));
            resources.ApplyResources(this.loginbutton, "loginbutton");
            this.loginbutton.ForeColor = System.Drawing.Color.White;
            this.loginbutton.Name = "loginbutton";
            this.loginbutton.UseVisualStyleBackColor = false;
            this.loginbutton.Click += new System.EventHandler(this.Login_Click);
            this.loginbutton.MouseEnter += new System.EventHandler(this.Button_MouseEnter);
            this.loginbutton.MouseLeave += new System.EventHandler(this.Button_MouseLeave);
            // 
            // eyePictureBox
            // 
            this.eyePictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.eyePictureBox, "eyePictureBox");
            this.eyePictureBox.Name = "eyePictureBox";
            this.eyePictureBox.TabStop = false;
            this.eyePictureBox.Click += new System.EventHandler(this.EyePictureBox_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // formPanel
            // 
            this.formPanel.BackColor = System.Drawing.Color.White;
            this.formPanel.Controls.Add(this.pictureBox1);
            this.formPanel.Controls.Add(this.exitLabelButton);
            this.formPanel.Controls.Add(this.saveCredential);
            this.formPanel.Controls.Add(this.titleLabel);
            this.formPanel.Controls.Add(this.uname);
            this.formPanel.Controls.Add(this.password);
            this.formPanel.Controls.Add(this.unameTextBox);
            this.formPanel.Controls.Add(this.passwordtextBox);
            this.formPanel.Controls.Add(this.loginbutton);
            this.formPanel.Controls.Add(this.eyePictureBox);
            this.formPanel.Controls.Add(this.unameUnderline);
            this.formPanel.Controls.Add(this.passwordUnderline);
            resources.ApplyResources(this.formPanel, "formPanel");
            this.formPanel.Name = "formPanel";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::thecalcify.Properties.Resources.starline_solution;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // exitLabelButton
            // 
            resources.ApplyResources(this.exitLabelButton, "exitLabelButton");
            this.exitLabelButton.BackColor = System.Drawing.Color.Red;
            this.exitLabelButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.exitLabelButton.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.exitLabelButton.ForeColor = System.Drawing.Color.White;
            this.exitLabelButton.Name = "exitLabelButton";
            this.exitLabelButton.Click += new System.EventHandler(this.exitLabelButton_Click);
            // 
            // saveCredential
            // 
            resources.ApplyResources(this.saveCredential, "saveCredential");
            this.saveCredential.Name = "saveCredential";
            this.saveCredential.UseVisualStyleBackColor = true;
            // 
            // titleLabel
            // 
            resources.ApplyResources(this.titleLabel, "titleLabel");
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.titleLabel.Name = "titleLabel";
            // 
            // unameUnderline
            // 
            this.unameUnderline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            resources.ApplyResources(this.unameUnderline, "unameUnderline");
            this.unameUnderline.Name = "unameUnderline";
            // 
            // passwordUnderline
            // 
            this.passwordUnderline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            resources.ApplyResources(this.passwordUnderline, "passwordUnderline");
            this.passwordUnderline.Name = "passwordUnderline";
            // 
            // Login
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.Controls.Add(this.formPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Login";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Login_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.eyePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.formPanel.ResumeLayout(false);
            this.formPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Label uname;
        private Label password;
        private TextBox unameTextBox;
        private TextBox passwordtextBox;
        private Button loginbutton;
        private PictureBox eyePictureBox;
        private ErrorProvider errorProvider;
        private Panel formPanel;
        private Label titleLabel;
        private CheckBox saveCredential;
        private Panel unameUnderline;
        private Panel passwordUnderline;
        private Label exitLabelButton;
        private PictureBox pictureBox1;
    }
}