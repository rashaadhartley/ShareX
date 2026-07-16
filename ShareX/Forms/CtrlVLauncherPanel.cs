using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShareX
{
    internal sealed class CtrlVLauncherPanel : Panel
    {
        private static readonly Color CanvasColor = Color.FromArgb(10, 12, 18);
        private static readonly Color SurfaceColor = Color.FromArgb(20, 24, 35);
        private static readonly Color SurfaceHoverColor = Color.FromArgb(27, 32, 47);
        private static readonly Color BorderColor = Color.FromArgb(48, 56, 76);
        private static readonly Color TextColor = Color.FromArgb(241, 245, 249);
        private static readonly Color MutedTextColor = Color.FromArgb(139, 151, 174);
        private static readonly Color VioletColor = Color.FromArgb(139, 92, 246);
        private static readonly Color CyanColor = Color.FromArgb(56, 189, 248);

        public CtrlVLauncherPanel(Action quickCapture, Action openEditor, Action openSettings, Action showAdvanced)
        {
            Dock = DockStyle.Fill;
            BackColor = CanvasColor;
            Padding = new Padding(42, 34, 42, 28);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = CanvasColor,
                ColumnCount = 1,
                RowCount = 5,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            layout.Controls.Add(CreateHeader(openSettings), 0, 0);
            layout.Controls.Add(CreateIntro(), 0, 1);
            layout.Controls.Add(CreateActions(quickCapture, openEditor), 0, 2);
            layout.Controls.Add(CreateHint(), 0, 3);
            layout.Controls.Add(CreateFooter(showAdvanced), 0, 4);

            Controls.Add(layout);
        }

        private static Control CreateHeader(Action openSettings)
        {
            Panel header = new Panel { Dock = DockStyle.Fill, BackColor = CanvasColor };

            Label brand = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextColor,
                Location = new Point(0, 4),
                Text = "CTRLV"
            };

            Label product = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = CyanColor,
                Location = new Point(0, 28),
                Text = "CAPTURE WORKSPACE"
            };

            Button settings = CreateLinkButton("SETTINGS", openSettings);
            settings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            settings.Location = new Point(header.Width - settings.Width, 4);
            header.Resize += (_, _) => settings.Left = header.ClientSize.Width - settings.Width;

            header.Controls.Add(brand);
            header.Controls.Add(product);
            header.Controls.Add(settings);
            return header;
        }

        private static Control CreateIntro()
        {
            Panel intro = new Panel { Dock = DockStyle.Fill, BackColor = CanvasColor };
            intro.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold),
                ForeColor = TextColor,
                Location = new Point(-2, 5),
                Text = "Capture what matters."
            });
            intro.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = MutedTextColor,
                Location = new Point(1, 54),
                Text = "One shortcut. Copy instantly. Edit only when you need to."
            });
            return intro;
        }

        private static Control CreateActions(Action quickCapture, Action openEditor)
        {
            TableLayoutPanel actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = CanvasColor,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = new Padding(0, 8, 0, 8)
            };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            actions.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            actions.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            CtrlVActionButton quick = new CtrlVActionButton(
                "CAPTURE REGION",
                "Select an area. It is copied immediately, then a small preview lets you edit, save or dismiss.",
                "CTRL + PRINT SCREEN",
                VioletColor,
                quickCapture)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };

            Button editor = new Button
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = TextColor,
                Margin = new Padding(0, 8, 0, 0),
                Text = "OPEN EXISTING IMAGE"
            };
            editor.FlatAppearance.BorderColor = BorderColor;
            editor.FlatAppearance.MouseOverBackColor = SurfaceHoverColor;
            editor.Click += (_, _) => openEditor();
            actions.Controls.Add(quick, 0, 0);
            actions.Controls.Add(editor, 0, 1);
            return actions;
        }

        private static Control CreateHint()
        {
            Panel hint = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(14, 17, 25),
                Margin = new Padding(0, 12, 0, 10),
                Padding = new Padding(16, 12, 16, 10)
            };
            hint.Paint += (_, e) =>
            {
                using Pen pen = new Pen(BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, hint.Width - 1, hint.Height - 1);
            };
            hint.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedTextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "AFTER CAPTURE   Your image is already copied. Choose Edit or Save from the preview only when needed."
            });
            return hint;
        }

        private static Control CreateFooter(Action showAdvanced)
        {
            Panel footer = new Panel { Dock = DockStyle.Fill, BackColor = CanvasColor };
            Label status = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(101, 113, 137),
                Location = new Point(0, 7),
                Text = "READY"
            };
            Button advanced = CreateLinkButton("ADVANCED", showAdvanced);
            advanced.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            advanced.Location = new Point(footer.Width - advanced.Width, 0);
            footer.Resize += (_, _) => advanced.Left = footer.ClientSize.Width - advanced.Width;
            footer.Controls.Add(status);
            footer.Controls.Add(advanced);
            return footer;
        }

        private static Button CreateLinkButton(string text, Action action)
        {
            Button button = new Button
            {
                AutoSize = true,
                BackColor = CanvasColor,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8F),
                ForeColor = MutedTextColor,
                Padding = new Padding(8, 3, 8, 3),
                Text = text
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = SurfaceColor;
            button.Click += (_, _) => action();
            return button;
        }

        private sealed class CtrlVActionButton : Control
        {
            private readonly string title;
            private readonly string description;
            private readonly string shortcut;
            private readonly Color accent;
            private readonly Action action;
            private bool hovered;

            public CtrlVActionButton(string title, string description, string shortcut, Color accent, Action action)
            {
                this.title = title;
                this.description = description;
                this.shortcut = shortcut;
                this.accent = accent;
                this.action = action;
                Cursor = Cursors.Hand;
                DoubleBuffered = true;
                TabStop = true;
                SetStyle(ControlStyles.Selectable, true);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                hovered = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                hovered = false;
                Invalidate();
                base.OnMouseLeave(e);
            }

            protected override void OnClick(EventArgs e)
            {
                action();
                base.OnClick(e);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    action();
                    e.Handled = true;
                }
                base.OnKeyDown(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);

                using SolidBrush background = new SolidBrush(hovered ? SurfaceHoverColor : SurfaceColor);
                using Pen border = new Pen(hovered ? accent : BorderColor, hovered ? 1.5F : 1F);
                using GraphicsPath path = CreateRoundedRectangle(bounds, 10);
                g.FillPath(background, path);
                g.DrawPath(border, path);

                using SolidBrush accentBrush = new SolidBrush(accent);
                g.FillRectangle(accentBrush, 18, 20, 4, Math.Max(30, Height - 40));

                using Font titleFont = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
                using Font bodyFont = new Font("Segoe UI", 9F);
                using Font shortcutFont = new Font("Segoe UI Semibold", 7.5F);
                TextRenderer.DrawText(g, title, titleFont, new Point(38, 22), TextColor,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, description, bodyFont, new Rectangle(38, 53, Width - 62, 46), MutedTextColor,
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, shortcut, shortcutFont, new Point(38, Height - 30), accent,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                if (Focused)
                {
                    Rectangle focus = Rectangle.Inflate(bounds, -4, -4);
                    ControlPaint.DrawFocusRectangle(g, focus, accent, SurfaceColor);
                }
            }

            private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
            {
                int diameter = radius * 2;
                GraphicsPath path = new GraphicsPath();
                path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
                path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
                path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}
