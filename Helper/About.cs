using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace thecalcify.Helper
{
    public partial class About : Form
    {
        public string username, password, licenceExpiryDate;

        public About(string username, string password, string licenceExpired)
        {
            InitializeComponent();
            this.username = username;
            this.password = password;
            this.licenceExpiryDate = licenceExpired;
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
            UpdateAgent updateAgent = new UpdateAgent();
            updateButton.Enabled = true;
        }
    }
}
