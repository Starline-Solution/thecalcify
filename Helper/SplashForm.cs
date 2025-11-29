using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Timers;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public partial class SplashForm : Form
    {
        private Label lblTitle;
        private Label lblMessage;
        private PictureBox spinner;
        private System.Timers.Timer animationTimer;
        private float angle = 0f;
        private bool isDisposing = false;

        public SplashForm(string title = "Please Wait", string message = "Loading ...")
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(350, 60); // Smaller size
            this.BackColor = Color.White;
            this.Opacity = 0.95;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.ControlBox = false;
            this.Text = "";

            // Spinner
            spinner = new PictureBox
            {
                Size = new Size(24, 24),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(15, 15),
                BackColor = Color.Transparent
            };

            // Animation timer setup
            animationTimer = new System.Timers.Timer(50); // 50ms interval = 20 FPS
            animationTimer.Elapsed += AnimationTimer_Elapsed;
            animationTimer.Start();

            // Spinner paint event for animation
            spinner.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TranslateTransform(12, 12); // Center of the picturebox
                e.Graphics.RotateTransform(angle);
                e.Graphics.TranslateTransform(-12, -12); // Reset translation

                using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                {
                    // Draw animated arc (changing start angle)
                    e.Graphics.DrawArc(pen, 2, 2, 20, 20, angle, 270);
                }
            };

            // Title label - smaller font
            lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), // Smaller font
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(50, 10), // Adjusted position
                BackColor = Color.Transparent
            };

            // Message label - smaller font
            lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 9, FontStyle.Regular), // Smaller font
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(50, 28), // Adjusted position
                BackColor = Color.Transparent
            };

            this.Controls.Add(spinner);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMessage);

            // Add rounded corners and shadow
            this.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw shadow
                for (int i = 0; i < 3; i++)
                {
                    using (var path = GetRoundedPath(new Rectangle(i, i, this.Width - 1, this.Height - 1), 5))
                    using (var pen = new Pen(Color.FromArgb(20 + (i * 15), 0, 0, 0), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                // Draw border
                using (var path = GetRoundedPath(new Rectangle(0, 0, this.Width - 1, this.Height - 1), 5))
                using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // Rounded corners
            this.Region = GetRoundedRegion(this.Width, this.Height, 5);
        }

        private void AnimationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            angle += 12f; // Increase angle for rotation
            if (angle >= 360f) angle = 0f; // Reset after full circle

            // Invoke on UI thread to update the spinner
            if (!isDisposing && spinner != null && !spinner.IsDisposed)
            {
                try
                {
                    spinner.Invoke(new Action(() =>
                    {
                        spinner.Refresh();
                    }));
                }
                catch (ObjectDisposedException)
                {

                }
            }
        }

        private void StopAnimation()
        {
            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
                animationTimer = null;
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private System.Drawing.Region GetRoundedRegion(int width, int height, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(width - radius, 0, radius, radius, 270, 90);
            path.AddArc(width - radius, height - radius, radius, radius, 0, 90);
            path.AddArc(0, height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return new System.Drawing.Region(path);
        }

        public void CenterToParent(Form parent)
        {
            if (parent != null && parent.Visible)
            {
                int x = parent.Location.X + (parent.Width - this.Width) / 2;
                int y = parent.Location.Y + (parent.Height - this.Height) / 2;
                this.Location = new Point(x, y);
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        public void UpdateMessage(string title, string message)
        {
            if (lblTitle.InvokeRequired || lblMessage.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblTitle.Text = title;
                    lblMessage.Text = message;
                }));
            }
            else
            {
                lblTitle.Text = title;
                lblMessage.Text = message;
            }
            this.Refresh();
        }

        public void SafeHide()
        {
            isDisposing = true;
            StopAnimation();

            if (!this.IsDisposed)
            {
                this.Close();
            }
        }
    }
}