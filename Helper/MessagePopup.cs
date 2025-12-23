using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class MessagePopup
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn
    (
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidthEllipse,
        int nHeightEllipse
    );
    public static async void ShowPopup(string message, bool isSuccess = false)
    {
        Color toastColor = isSuccess ? Color.FromArgb(0, 160, 70) : Color.FromArgb(220, 53, 69);
        string iconSymbol = isSuccess ? "✔" : "✖";

        int popupWidth = 420;
        int baseHeight = 80;

        Label temp = new Label
        {
            AutoSize = false,
            MaximumSize = new Size(popupWidth - 80, 0),
            Text = message,
            Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Regular)
        };
        temp.UseCompatibleTextRendering = true;
        temp.Size = temp.GetPreferredSize(new Size(popupWidth - 80, 0));

        int dynamicHeight = Math.Max(baseHeight, temp.Height + 40);

        var popup = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            Size = new Size(popupWidth, dynamicHeight),
            BackColor = Color.White,
            TopMost = true,
            Opacity = 0
        };

        popup.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, popup.Width, popup.Height, 12, 12));

        Screen currentScreen = Screen.FromPoint(Cursor.Position);
        Rectangle area = currentScreen.WorkingArea;

        int marginRight = (int)(area.Width * 0.01);
        int marginTop = (int)(area.Height * 0.09);

        popup.Location = new Point(area.Right - popup.Width - marginRight, area.Top + marginTop);

        var iconLabel = new Label
        {
            Text = iconSymbol,
            Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold),
            ForeColor = toastColor,
            AutoSize = true,
        };
        popup.Controls.Add(iconLabel);

        var lbl = new Label
        {
            Text = message,
            Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Regular),
            MaximumSize = new Size(popupWidth - 80, 0),
            AutoSize = true,
            ForeColor = Color.Black,
        };
        popup.Controls.Add(lbl);

        iconLabel.Location = new Point(15, (popup.Height - iconLabel.Height) / 2);
        lbl.Location = new Point(iconLabel.Right + 10, (popup.Height - lbl.Height) / 2);

        var btnClose = new Label
        {
            Text = "×",
            Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold),
            ForeColor = Color.Gray,
            AutoSize = true,
            Cursor = Cursors.Hand
        };
        btnClose.Location = new Point(popup.Width - 28, 8);
        btnClose.Click += (s, e) => popup.Close();
        popup.Controls.Add(btnClose);

        var progress = new Panel
        {
            BackColor = toastColor,
            Size = new Size(0, 4),
            Location = new Point(0, popup.Height - 4)
        };
        popup.Controls.Add(progress);

        popup.Show();

        await AnimateOpacity(popup, 0, 1, 20);

        for (int w = 0; w <= popup.Width; w += 10)
        {
            progress.Width = w;
            await Task.Delay(8);
        }

        await Task.Delay(2000);

        await AnimateOpacity(popup, 1, 0, 20);

        popup.Close();
    }
    private static async Task AnimateOpacity(Form form, double from, double to, int durationMs)
    {
        int steps = 20;
        double step = (to - from) / steps;

        for (int i = 0; i < steps; i++)
        {
            form.Opacity = from + step * i;
            await Task.Delay(durationMs);
        }
        form.Opacity = to;
    }
}


