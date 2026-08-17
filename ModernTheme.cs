using System.Drawing;
using System.Windows.Forms;

namespace XV2SaveEditor
{
    public static class ModernTheme
    {
        public static readonly Color Window = Color.FromArgb(8, 11, 20);
        public static readonly Color Sidebar = Color.FromArgb(11, 14, 26);
        public static readonly Color Surface = Color.FromArgb(17, 21, 36);
        public static readonly Color SurfaceRaised = Color.FromArgb(25, 30, 49);
        public static readonly Color Border = Color.FromArgb(48, 55, 80);
        public static readonly Color Text = Color.FromArgb(242, 245, 255);
        public static readonly Color Muted = Color.FromArgb(147, 157, 185);
        public static readonly Color Purple = Color.FromArgb(143, 93, 255);
        public static readonly Color PurpleHover = Color.FromArgb(163, 122, 255);
        public static readonly Color PurpleDark = Color.FromArgb(93, 54, 188);
        public static readonly Color Cyan = Color.FromArgb(54, 214, 231);
        public static readonly Color CyanDim = Color.FromArgb(25, 86, 103);

        public static void Apply(Control root)
        {
            root.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            root.ForeColor = Text;
            if (root is Form) root.BackColor = Window;
            ApplyChildren(root);
        }

        private static void ApplyChildren(Control root)
        {
            foreach (Control control in root.Controls)
            {
                switch (control)
                {
                    case Button button:
                        StyleButton(button, false);
                        break;
                    case GroupBox group:
                        group.ForeColor = Cyan;
                        group.BackColor = Surface;
                        group.Padding = new Padding(12);
                        break;
                    case TabControl tabs when tabs.ItemSize.Height > 2:
                        StyleTabs(tabs);
                        break;
                    case TabPage page:
                        page.BackColor = Window;
                        page.ForeColor = Text;
                        break;
                    case TextBox textBox:
                        textBox.BackColor = SurfaceRaised;
                        textBox.ForeColor = Text;
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ComboBox combo:
                        combo.BackColor = SurfaceRaised;
                        combo.ForeColor = Text;
                        combo.FlatStyle = FlatStyle.Flat;
                        break;
                    case NumericUpDown numeric:
                        numeric.BackColor = SurfaceRaised;
                        numeric.ForeColor = Text;
                        numeric.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case CheckedListBox checkedList:
                        checkedList.BackColor = Surface;
                        checkedList.ForeColor = Text;
                        checkedList.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ListBox list:
                        list.BackColor = Surface;
                        list.ForeColor = Text;
                        list.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ListView view:
                        view.BackColor = Surface;
                        view.ForeColor = Text;
                        view.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case Label label:
                        label.ForeColor = label.ForeColor == SystemColors.ControlText ? Text : label.ForeColor;
                        label.BackColor = Color.Transparent;
                        break;
                    case CheckBox check:
                        check.ForeColor = Text;
                        check.BackColor = Color.Transparent;
                        check.FlatStyle = FlatStyle.Flat;
                        break;
                    case Panel panel:
                        panel.BackColor = panel.BackColor == SystemColors.Control ? Surface : panel.BackColor;
                        break;
                }
                control.Font ??= new Font("Segoe UI", 9.5F, FontStyle.Regular);
                ApplyChildren(control);
            }
        }

        private static void StyleTabs(TabControl tabs)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.ItemSize = new Size(Math.Max(110, tabs.ItemSize.Width), 34);
            tabs.Padding = new Point(14, 4);
            tabs.DrawItem -= DrawTab;
            tabs.DrawItem += DrawTab;
        }

        private static void DrawTab(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tabs || e.Index < 0 || e.Index >= tabs.TabPages.Count) return;
            bool selected = e.Index == tabs.SelectedIndex;
            Rectangle bounds = e.Bounds;
            using Brush background = new SolidBrush(selected ? SurfaceRaised : Sidebar);
            e.Graphics.FillRectangle(background, bounds);
            if (selected)
            {
                using Brush rail = new SolidBrush(Cyan);
                e.Graphics.FillRectangle(rail, bounds.Left, bounds.Bottom - 3, bounds.Width, 3);
            }
            TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text,
                new Font("Segoe UI Semibold", 9F), bounds, selected ? Text : Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        public static void StyleButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = primary ? Purple : Border;
            button.FlatAppearance.MouseOverBackColor = primary ? PurpleHover : SurfaceRaised;
            button.FlatAppearance.MouseDownBackColor = PurpleDark;
            button.BackColor = primary ? Purple : SurfaceRaised;
            button.ForeColor = Color.White;
            button.Cursor = Cursors.Hand;
        }
    }
}
