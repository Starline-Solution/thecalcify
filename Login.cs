using System;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify
{
    public partial class Login : Form
    {
        private readonly Common CommonClass;
        private bool passwordVisible = false; // Track password visibility state
        public string token, licenceDate, username, userpassword;
        public static Login CurrentInstance { get; private set; }
        public Login()
        {
            CurrentInstance = this; // Set the instance for later use
            InitializeComponent();
            this.KeyPreview = true; // Allow form to detect key presses
            this.FormClosed += Login_FormClosed;
            this.StartPosition = FormStartPosition.CenterScreen;

            CommonClass = new Common(this);

            // Initialize eye button
            InitializeEyeButton();
            // Initialize Save Credentials
            LoadSavedCredentials();
        }

        private void InitializeEyeButton()
        {
            // Set initial image
            try
            {
                eyePictureBox.Image = Properties.Resources.eye_open;
            }
            catch
            {
                // Fallback if image not found
                eyePictureBox.BackColor = Color.White;
                eyePictureBox.Paint += (s, e) =>
                {
                    e.Graphics.DrawString("👁",
                        new Font("Microsoft Sans Serif Emoji", 12),
                        Brushes.Black, 0, 0);
                };
            }

            eyePictureBox.Click += (s, e) => TogglePasswordVisibility();
            eyePictureBox.BringToFront();
        }

        private void LoadSavedCredentials()
        {
            // Explicitly declare the tuple types
            (string username, string password) = CredentialManager.LoadCredentials();

            if (username != null)
            {
                unameTextBox.Text = username;
                saveCredential.Checked = true;
                passwordtextBox.Text = password;
                loginbutton.Focus();
            }
        }

        private void TogglePasswordVisibility()
        {
            passwordVisible = !passwordVisible;
            passwordtextBox.PasswordChar = passwordVisible ? '\0' : '•';

            try
            {
                eyePictureBox.Image = passwordVisible
                    ? Properties.Resources.eye_close
                    : Properties.Resources.eye_open;
            }
            catch
            {
                // Fallback if images not available
                eyePictureBox.Invalidate(); // Forces repaint of our drawn eye
            }
        }

        private async void Login_Click(object sender, EventArgs e)
        {

            loginbutton.Enabled = false;

            if (CommonClass.InternetAvilable())
            {

                string uname = unameTextBox.Text.Trim();
                string password = passwordtextBox.Text.Trim();

                if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter both username and password.",
                                           "Authentication Failed",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Error);

                    loginbutton.Enabled = true;
                    return;
                }

                username = uname;
                userpassword = password;

                var loginData = new
                {
                    username = uname,
                    password
                };

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string apiUrl = $"{ConfigurationManager.AppSettings["thecalcify"]}login";
                        string jsonData = JsonSerializer.Serialize(loginData);
                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            MessageBox.Show("Temporary Upgrading Server. Login after sometime", "Upgrade Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ApplicationLogger.Log($"{responseContent},{DateTime.Now:dd/MM/yyyy HH:mm:ss:ff}");
                            loginbutton.Enabled = true;
                            return;
                        }

                        using (JsonDocument doc = JsonDocument.Parse(responseContent))
                        {
                            var root = doc.RootElement;

                            if (response.IsSuccessStatusCode)
                            {
                                bool isSuccess = root.GetProperty("isSuccess").GetBoolean();

                                if (isSuccess)
                                {
                                    var dataElement = root.GetProperty("data");
                                    token = dataElement.GetProperty("token").GetString();
                                    string expireTimeStr = dataElement.GetProperty("expireTime").GetString();

                                    try
                                    {
                                        if (!string.IsNullOrEmpty(expireTimeStr) &&
                                                                        DateTime.TryParse(expireTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expireDate))
                                        {
                                            licenceDate = expireDate.ToString("dd/MM/yyyy");
                                        }
                                        else
                                        {
                                            MessageBox.Show("You are unauthorized");
                                            loginbutton.Enabled = true;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error parsing rate value at Login_Click: " + ex.Message);
                                    }
                                    //licenceDate = Convert.ToDateTime(dataElement.GetProperty("expireTime").GetString()).ToString("dd/MM/yyyy");
                                    // Decode JWT token
                                    var payload = DecodeJwtPayload(token);

                                    ApplicationLogger.Log($"User Logged In", "Logon");

                                    thecalcify homeForm = new thecalcify();
                                    homeForm.Show();

                                    SaveCredential(); // Presumably saves token or login info
                                    this.Hide();
                                }
                                else
                                {
                                    string message = root.GetProperty("message").GetString();
                                    MessageBox.Show(message ?? "Login failed.",
                                        "Authentication Failed",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Exclamation);
                                    loginbutton.Enabled = true;
                                }
                            }
                            else
                            {
                                string message = root.GetProperty("message").GetString();
                                MessageBox.Show(message ?? "Login failed.",
                                    "Authentication Failed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                                loginbutton.Enabled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Login failed: " + ex.Message);
                        loginbutton.Enabled = true;
                        ApplicationLogger.LogException(ex);
                    }
                }


            }
            else
            {
                MessageBox.Show("Check your internet connection and try again.",
                          "No Internet",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Warning);
                loginbutton.Enabled = true;
            }
        }

        // Helper method to decode JWT payload
        public static JsonElement DecodeJwtPayload(string jwt)
        {
            string payload = jwt.Split('.')[1];

            // Add padding if required
            int mod = payload.Length % 4;
            if (mod > 0)
                payload += new string('=', 4 - mod);

            byte[] bytes = Convert.FromBase64String(payload);
            string json = Encoding.UTF8.GetString(bytes);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                return doc.RootElement.Clone();
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            ApplicationLogger.Log($"User Shutdown App", "Logon");
            Application.Exit(); // Closes all forms and ends the application
        }

        private void UnameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Backspace → Clear all text
            if (e.Control && e.KeyCode == Keys.Back)
            {
                unameTextBox.Text = "";
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Optional: prevent ding sound
                loginbutton.PerformClick(); // Triggers the Click event
            }
        }

        private void PasswordtextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + Backspace → Clear all text
            if (e.Control && e.KeyCode == Keys.Back)
            {
                passwordtextBox.Text = "";
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Optional: prevent ding sound
                loginbutton.PerformClick(); // Triggers the Click event
            }
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                var result = MessageBox.Show("Do you want to Exit Application?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    this.Close(); // Close the login form
                    Application.Exit(); // Terminate the application
                }
            }


        }

        // Add these event handlers for better UX:

        private void TextBox_Enter(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Tag is Panel underline)
            {
                underline.BackColor = Color.FromArgb(0, 120, 215);
                underline.Height = 2;
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Tag is Panel underline)
            {
                underline.BackColor = Color.FromArgb(200, 200, 200);
                underline.Height = 1;
            }
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void EyePictureBox_Click(object sender, EventArgs e)
        {
            if (this.passwordtextBox.PasswordChar == '•')
            {
                this.passwordtextBox.PasswordChar = '\0';
                this.eyePictureBox.Image = Properties.Resources.eye_open;
            }
            else
            {
                this.passwordtextBox.PasswordChar = '•';
                this.eyePictureBox.Image = Properties.Resources.eye_close;
            }
        }

        private void SaveCredential()
        {
            if (!saveCredential.Checked)
            {
                CredentialManager.DeleteCredentials();
                return;
            }
            CredentialManager.SaveCredentials(unameTextBox.Text, passwordtextBox.Text, saveCredential.Checked ? true : false);
        }

        private void exitLabelButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to Exit Application?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Close(); // Close the login form
                System.Windows.Forms.Application.Exit(); // Terminate the application

            }
        }
    }
}
