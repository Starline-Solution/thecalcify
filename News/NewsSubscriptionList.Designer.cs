namespace thecalcify.News
{
    partial class NewsSubscriptionList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel userpanel;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewsSubscriptionList));
            this.userpanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // userpanel
            // 
            this.userpanel.AutoScroll = true;
            this.userpanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userpanel.Location = new System.Drawing.Point(0, 0);
            this.userpanel.Name = "userpanel";
            this.userpanel.Size = new System.Drawing.Size(829, 681);
            this.userpanel.TabIndex = 0;
            this.userpanel.WrapContents = false;
            // 
            // NewsSubscriptionList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(829, 681);
            this.Controls.Add(this.userpanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewsSubscriptionList";
            this.Text = "NewsSubscriptionList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
    }
}