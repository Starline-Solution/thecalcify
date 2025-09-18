using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thecalcify.Alert
{
    public partial class AlertPopup : Form
    {
        private Timer closeTimer;
        private Label lblMessage;
        private Button btnClose;

        public AlertPopup(string message)
        {
            InitializeFormComponent();
            btnClose.Location = new Point(this.Width - btnClose.Width - 10, 10);
            btnClose.Click += (s, e) => { this.Close(); this.Dispose(); };
            lblMessage.Size = new Size(this.Width - btnClose.Width - 40, 50);
            // Optional: rounded corners (Windows 10+)
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 15, 15));

            lblMessage.Text = message;
            PositionPopup();

            // Start auto-close timer: 20 seconds
            closeTimer = new Timer();
            closeTimer.Interval = 20_000; // 20,000 ms = 20 seconds
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                this.Close();
                this.Dispose();
            };
            closeTimer.Start();
        }

        private void InitializeFormComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            this.BackColor = Color.FromArgb(40, 40, 40); // dark gray modern bg
            this.Size = new Size(320, 80);
            this.Padding = new Padding(15);

            // Close button
            btnClose = new Button();
            btnClose.Text = "X";
            btnClose.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.FromArgb(60, 60, 60);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Size = new Size(30, 30);
            btnClose.Cursor = Cursors.Hand;
            // Message label — leave space for close button
            lblMessage = new Label();
            lblMessage.ForeColor = Color.White;
            lblMessage.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Regular);
            lblMessage.AutoEllipsis = true;
            lblMessage.Location = new Point(15, 15);
            lblMessage.TextAlign = ContentAlignment.MiddleLeft;


            // Add controls to form
            this.Controls.Add(lblMessage);
            this.Controls.Add(btnClose);

        }

        private void PositionPopup()
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 20, workingArea.Bottom - this.Height - 20);
        }

        // Import for rounded corners
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }

}
