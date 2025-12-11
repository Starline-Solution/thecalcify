using System.Drawing;
using System.Windows.Forms;

namespace thecalcify
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