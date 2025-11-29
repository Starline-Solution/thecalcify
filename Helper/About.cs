using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public partial class About : Form
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
            usernameLabel.Text = username;
            licenceExpireLabel.Text = licenceExpiryDate;
            string[] parts = Application.ProductVersion.Split('.');
            string result = string.Join(".", parts.Take(3));
            appVersionLabel.Text = result;
            userPasswordLabel.Text = password;
            var filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            lastUpdateDateLabel.Text = File.GetLastWriteTime(filePath).Date.ToString("dd:MM:yyyy");
        }


        private void UpdateButton_Click(object sender, EventArgs e)
        {
            bool isInternetAvailable = Common.InternetAvilable();
            if (!isInternetAvailable)
            {
                MessageBox.Show(
                    "An internet connection is required to perform the update. Please check your connection and try again.",
                    "No Internet Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            var response = MessageBox.Show(
                "Please ensure you are connected to a stable internet network to avoid interruptions.\n\n" +
                "The upgrade may take around 5 minutes to complete. Thank you for your patience and support.",
                "Upgrade thecalcify",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information
            );

            if (response == DialogResult.OK)
            {
                updateButton.Enabled = false;
                _ = new UpdateAgent(token, this);
                updateButton.Enabled = true;
            }
        }
    }
}
