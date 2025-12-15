using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public partial class About : UserControl
    {
        public string username, password, licenceExpiryDate, token;

        public About(string username, string password, string licenceExpired, string token)
        {
            InitializeComponent();

            this.username = username;
            this.password = password;
            this.licenceExpiryDate = licenceExpired;
            this.token = token;
        }

        private void About_Load(object sender, EventArgs e)
        {
            // Fill labels
            lblUsername.Text = $"User Name:   {username}";
            lblPassword.Text = $"Password:   {password}";
            if (DateTime.TryParse(licenceExpiryDate, out var expiryDate))
            {
                lblExpiry.Text = $"License Expires:   {expiryDate:dd:MM:yyyy}";
            }
            else
            {
                lblExpiry.Text = $"License Expires:   {licenceExpiryDate}";
            }

            string[] parts = Application.ProductVersion.Split('.');
            string result = string.Join(".", parts.Take(3));
            lblVersion.Text = $"Version:   {result}";

            var filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            lblModified.Text = $"Version Modified Date:   {File.GetLastWriteTime(filePath):dd:MM:yyyy}";

            // ⭐ Center cardPanel
            cardPanel.Left = (this.Width - cardPanel.Width) / 2;
            cardPanel.Top = (this.Height - cardPanel.Height) / 2 - 40;

            // ⭐ Center update button under card
            updateButton.Left = (this.Width - updateButton.Width) / 2;
            updateButton.Top = cardPanel.Bottom + 20;

            // rightsLabel stays bottom-left automatically
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            bool isInternetAvailable = Common.InternetAvilable();
            if (!isInternetAvailable)
            {
                MessageBox.Show(
                    "An internet connection is required to perform the update. Please check your connection and try again.",
                    "No Internet Connection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var response = MessageBox.Show(
                "Please ensure you are connected to a stable network.\n\n" +
                "The upgrade may take around 5 minutes. Thank you for your patience.",
                "Upgrade thecalcify",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (response == DialogResult.OK)
            {
                updateButton.Enabled = false;
                Form parent = this.FindForm();
                _ = new UpdateAgent(token, parent);
                updateButton.Enabled = true;
            }
        }

        private void cardPanel_Paint(object sender, PaintEventArgs e)
        {
            int radius = 20;
            Panel panel = sender as Panel;

            Rectangle rect = panel.ClientRectangle;
            rect.Inflate(-1, -1);

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Pen pen = new Pen(Color.LightGray, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

    }
}
