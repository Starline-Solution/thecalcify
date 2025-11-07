namespace thecalcify.Home
{
    partial class HomeUserControlForm
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
            this.home_Usercontrol1 = new Home_Usercontrol();
            this.SuspendLayout();
            // 
            // home_Usercontrol1
            // 
            this.home_Usercontrol1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.home_Usercontrol1.Location = new System.Drawing.Point(0, 0);
            this.home_Usercontrol1.Name = "home_Usercontrol1";
            this.home_Usercontrol1.Size = new System.Drawing.Size(800, 450);
            this.home_Usercontrol1.TabIndex = 0;
            // 
            // HomeUserControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.home_Usercontrol1);
            this.Name = "HomeUserControlForm";
            this.Text = "HomeUserControlForm";
            this.ResumeLayout(false);

        }

        #endregion

        private Home_Usercontrol home_Usercontrol1;
    }
}