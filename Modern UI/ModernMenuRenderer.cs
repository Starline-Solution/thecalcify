using System.Drawing;
using System.Windows.Forms;

namespace thecalcify.Modern_UI
{
    // This class overrides how Menus are painted to look Flat and Modern
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                // Hover Color (Light Blue background)
                Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(225, 248, 249)), rc); // #f1f5f9
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, rc.Width - 1, rc.Height - 1);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? Color.FromArgb(14, 165, 233) : Color.FromArgb(30, 41, 59); // Blue if selected, Dark Slate if not
            base.OnRenderItemText(e);
        }
    }

    // 🎨 Updated Custom ComboBox
    public class ModernComboBox : ComboBox
    {
        private Color borderColor = Color.LightGray;

        public ModernComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.FlatStyle = FlatStyle.Flat;
            this.Font = new Font("Segoe UI", 10F);
        }

        // Draws the items IN the list
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Draw Background & Text
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                // Hovered: Teal Background, White Text
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(81, 213, 220)), e.Bounds);
                TextRenderer.DrawText(e.Graphics, this.Items[e.Index].ToString(), this.Font, e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            else
            {
                // Normal: White Background, Black Text
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                TextRenderer.DrawText(e.Graphics, this.Items[e.Index].ToString(), this.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }

        // Draws the Border and Placeholder Text (Closed State)
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0xF || m.Msg == 0x85) // WM_PAINT
            {
                using (var g = Graphics.FromHwnd(this.Handle))
                {
                    // 1. Draw Border
                    using (var p = new Pen(borderColor, 1))
                    {
                        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                    }

                    // 2. ✅ Draw Placeholder "Font Size" if nothing is selected
                    if (this.SelectedIndex == -1)
                    {
                        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding;
                        Rectangle rect = this.ClientRectangle;
                        rect.X += 4; // Padding left

                        // Draw the Gray Placeholder Text
                        TextRenderer.DrawText(g, "Font Size", this.Font, rect, Color.Gray, flags);
                    }
                }
            }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(241, 245, 249);
        public override Color MenuItemBorder => Color.FromArgb(226, 232, 240);
        public override Color MenuBorder => Color.FromArgb(226, 232, 240);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(241, 245, 249);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(241, 245, 249);
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
    }
}