using System;
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
        }


        private void UpdateButton_Click(object sender, EventArgs e)
        {
            updateButton.Enabled = false;
            _ = new UpdateAgent(token, this);
            updateButton.Enabled = true;
        }
    }
}
