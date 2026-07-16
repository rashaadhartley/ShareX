using ShareX.HelpersLib;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShareX
{
    internal sealed class CapXCapturePreviewForm : Form
    {
        private static readonly Color CanvasColor = Color.FromArgb(13, 16, 24);
        private static readonly Color SurfaceColor = Color.FromArgb(22, 27, 39);
        private static readonly Color BorderColor = Color.FromArgb(54, 63, 84);
        private static readonly Color TextColor = Color.FromArgb(241, 245, 249);
        private static readonly Color MutedTextColor = Color.FromArgb(148, 163, 184);
        private static readonly Color VioletColor = Color.FromArgb(139, 92, 246);
        private static readonly Color CyanColor = Color.FromArgb(56, 189, 248);

        private readonly Bitmap capturedImage;
        private readonly TaskSettings taskSettings;
        private readonly Timer closeTimer;

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;
                parameters.ExStyle |= (int)WindowStyles.WS_EX_NOACTIVATE;
                return parameters;
            }
        }

        private CapXCapturePreviewForm(Bitmap image, TaskSettings settings)
        {
            capturedImage = image;
            taskSettings = settings;

            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = CanvasColor;
            ClientSize = new Size(400, 112);
            FormBorderStyle = FormBorderStyle.None;
            Padding = new Padding(1);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;

            BuildInterface();

            closeTimer = new Timer { Interval = 7000 };
            closeTimer.Tick += (_, _) => Close();

            Shown += (_, _) =>
            {
                Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
                Location = new Point(area.Right - Width - 18, area.Top + 18);
                closeTimer.Start();
            };
        }

        public static void ShowPreview(Image image, TaskSettings taskSettings)
        {
            if (image == null || Program.MainForm == null || Program.MainForm.IsDisposed)
            {
                return;
            }

            Bitmap imageCopy = new Bitmap(image);
            TaskSettings settingsCopy = taskSettings.Copy();
            settingsCopy.ShowCapXCapturePreview = false;

            Program.MainForm.BeginInvoke(new Action(() =>
            {
                CapXCapturePreviewForm preview = new CapXCapturePreviewForm(imageCopy, settingsCopy);
                preview.Show();
            }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                closeTimer?.Dispose();
                capturedImage?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void BuildInterface()
        {
            Panel surface = new Panel
            {
                BackColor = SurfaceColor,
                Dock = DockStyle.Fill,
                // Keep only the painted border inset. The previous 14 px dock
                // padding reduced the content height enough to clip the buttons.
                Padding = new Padding(1)
            };
            surface.Paint += (_, e) =>
            {
                using Pen pen = new Pen(BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, surface.Width - 1, surface.Height - 1);
            };

            Panel content = new Panel
            {
                BackColor = SurfaceColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 2, 0, 0)
            };

            Label title = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextColor,
                Location = new Point(14, 4),
                Text = "CAPTURE COPIED"
            };

            Label detail = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = MutedTextColor,
                Location = new Point(15, 29),
                Text = $"{capturedImage.Width} × {capturedImage.Height} px  •  Ready to paste"
            };

            FlowLayoutPanel actions = new FlowLayoutPanel
            {
                AutoSize = true,
                BackColor = SurfaceColor,
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(10, 56),
                Margin = Padding.Empty,
                WrapContents = false
            };

            actions.Controls.Add(CreateActionButton("EDIT", VioletColor, EditCapture));
            actions.Controls.Add(CreateActionButton("SAVE", CyanColor, SaveCapture));
            actions.Controls.Add(CreateActionButton("DISMISS", MutedTextColor, Close));

            Button close = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = SurfaceColor,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                ForeColor = MutedTextColor,
                Location = new Point(342, -2),
                Size = new Size(30, 28),
                Text = "×"
            };
            close.FlatAppearance.BorderSize = 0;
            close.Click += (_, _) => Close();

            content.Controls.Add(title);
            content.Controls.Add(detail);
            content.Controls.Add(actions);
            content.Controls.Add(close);
            surface.Controls.Add(content);
            Controls.Add(surface);
        }

        private Button CreateActionButton(string text, Color accent, Action action)
        {
            Button button = new Button
            {
                AutoSize = true,
                BackColor = CanvasColor,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 8F),
                ForeColor = accent,
                Margin = new Padding(4, 0, 4, 0),
                Padding = new Padding(10, 5, 10, 5),
                Text = text,
                UseMnemonic = false
            };
            button.FlatAppearance.BorderColor = BorderColor;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 37, 53);
            button.Click += (_, _) => action();
            button.MouseEnter += (_, _) => closeTimer?.Stop();
            button.MouseLeave += (_, _) => closeTimer?.Start();
            return button;
        }

        private void EditCapture()
        {
            Bitmap image = capturedImage.CloneSafe();
            TaskSettings editSettings = taskSettings.Copy();
            editSettings.ShowCapXCapturePreview = false;
            editSettings.UseDefaultAfterCaptureJob = false;
            editSettings.AfterCaptureJob = AfterCaptureTasks.CopyImageToClipboard;
            Close();
            TaskHelpers.AnnotateImageAsync(image, null, editSettings);
        }

        private void SaveCapture()
        {
            using Bitmap image = capturedImage.CloneSafe();
            string folder = TaskHelpers.GetScreenshotsFolder(taskSettings);
            Directory.CreateDirectory(folder);
            string fileName = $"CapX_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            ImageHelpers.SaveImageFileDialog(image, Path.Combine(folder, fileName));
            Close();
        }
    }
}
