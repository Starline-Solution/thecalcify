using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thecalcify.News
{
    public class Chip : Control
    {
        public event Action<string> OnDelete;

        public string Value { get; }

        public Chip(string text)
        {
            this.Value = text;
            this.Text = text;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(10, 6, 10, 6);
            this.Height = 32;
            this.AutoSize = true;
            this.Cursor = Cursors.Hand;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = RoundedRect(rect, 14))
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                   rect,
                   Color.FromArgb(240, 240, 240),
                   Color.FromArgb(220, 220, 220),
                   45f))
            using (var pen = new Pen(Color.LightGray, 1))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            // Text
            TextRenderer.DrawText(g, Text, Font,
                new Point(10, (Height - Font.Height) / 2),
                Color.FromArgb(50, 50, 50));

            // X button
            TextRenderer.DrawText(g, "×", Font,
                new Point(Width - 20, (Height - Font.Height) / 2),
                Color.DimGray);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            // Detect click on X
            if (e.X > Width - 25)
            {
                OnDelete?.Invoke(Value);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = TextRenderer.MeasureText(Text, Font).Width + 45;
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

}
