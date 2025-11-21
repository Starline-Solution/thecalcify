using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace thecalcify.News
{
    public partial class NotificationForm : Form
    {
        private Timer _timer;

        public NotificationForm(string headline, string timestamp)
        {
            InitializeComponent();

            // Set the labels' text
            lblHeadline.Text = headline;
            lblTimestamp.Text = timestamp;

            // Adjust form size based on controls
            int formWidth = 400; // Adjusted width
            int formHeight = 160; // Adjusted height

            // Set form size and remove padding that could interfere with the layout
            this.ClientSize = new Size(formWidth, formHeight);
            this.Padding = new Padding(10); // Padding for content separation

            // Set the notification form's appearance
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None; // Removes border

            // Add rounded corners
            this.Region = new Region(CreateRoundedRectanglePath(this.ClientRectangle, 15));

            // Set up the timer to close the form after 5 seconds
            _timer = new Timer
            {
                Interval = 5000 // 5 seconds
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Position the notification form in the bottom-right corner
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10,
                Screen.PrimaryScreen.WorkingArea.Height - this.Height - 60
            );
        }

        // Timer tick event to close the notification
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Close();
            _timer.Stop();
            _timer.Dispose();
        }

        // Method to create rounded corners
        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        // Close button click handler
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close(); // Close the notification form
        }

        // Hover effect for close button
        private void BtnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.FromArgb(255, 72, 72); // Darker red when hovered
        }

        private void BtnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.BackColor = Color.FromArgb(255, 92, 92); // Default red
        }

        private void NotificationForm_DoubleClick(object sender, EventArgs e)
        {
            thecalcify thecalcify = thecalcify.CurrentInstance;
            thecalcify.NewsListToolStripMenuItem_Click_1(this, EventArgs.Empty);
        }

        private void lblHeadline_DoubleClick(object sender, EventArgs e)
        {
            NotificationForm_DoubleClick(sender, e);
        }

        private void lblTimestamp_DoubleClick(object sender, EventArgs e)
        {
            NotificationForm_DoubleClick(sender, e);
        }
    }
}
