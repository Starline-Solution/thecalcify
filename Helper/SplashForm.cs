using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public partial class SplashForm : Form
    {
        private Label lblTitle;
        private Label lblMessage;
        private PictureBox gifLoader;

        public SplashForm(string title = "Please Wait", string message = "Loading ...")
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(350, 80);
            this.BackColor = Color.White;
            this.Opacity = 0.95;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // ---- GIF Loader ----
            gifLoader = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(15, 20),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            // Load GIF from file
            gifLoader.Image = Properties.Resources.loading;

            // Title label
            lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(60, 15),
                BackColor = Color.Transparent
            };

            // Message label
            lblMessage = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(60, 35),
                BackColor = Color.Transparent
            };

            this.Controls.Add(gifLoader);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMessage);

            // Rounded corners
            this.Region = GetRoundedRegion(this.Width, this.Height, 6);

            // Draw border
            this.Paint += (s, e) =>
            {
                using (var path = GetRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 6))
                using (var pen = new Pen(Color.LightGray, 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Region GetRoundedRegion(int width, int height, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(width - radius, 0, radius, radius, 270, 90);
            path.AddArc(width - radius, height - radius, radius, radius, 0, 90);
            path.AddArc(0, height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        public void UpdateMessage(string title, string message)
        {
            lblTitle.Text = title;
            lblMessage.Text = message;
        }

        public void SafeHide()
        {
            this.Close();
        }

        public void CenterToOwner(Form parent)
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

    }
}
