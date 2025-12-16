using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        // Center the card panel when the control loads
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
        }

        private void btnUpdate_Click(object sender, EventArgs e)
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
                btnUpdate.Enabled = false;
                Form parent = this.FindForm();
                _ = new UpdateAgent(parent);
                btnUpdate.Enabled = true;
            }
        }
        // Recenter when the control is resized
        private void About_Resize(object sender, EventArgs e)
        {
            CenterCardPanel();
        }

        // Method to center the card panel
        private void CenterCardPanel()
        {
            int x = (this.Width - cardPanel.Width) / 2;
            int y = (this.Height - cardPanel.Height) / 2;
            cardPanel.Location = new Point(x, y);
        }

        // Rounded corners for card panel with shadow effect
        private void cardPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            int radius = 20;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = GetRoundedRectangle(panel.ClientRectangle, radius);

            panel.Region = new Region(path);

            using (Pen borderPen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                e.Graphics.DrawPath(borderPen, path);
            }
        }

        // Rounded button
        private void btnUpdate_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            int radius = 10;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            GraphicsPath path = GetRoundedRectangle(btn.ClientRectangle, radius);
            btn.Region = new Region(path);
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            rect.Width -= 1;
            rect.Height -= 1;

            // Top-left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);

            // Top-right corner
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);

            // Bottom-right corner
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);

            // Bottom-left corner
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();

            return path;
        }


    }
}
