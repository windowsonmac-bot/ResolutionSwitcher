using System;
using System.Drawing;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public class StatusWindow : Form
    {
        private Label _gameLabel = null!;
        private Label _progressLabel = null!;
        private ProgressBar _progressBar = null!;
        private RichTextBox _stepsBox = null!;
        private Button _minimizeButton = null!;
        private Button _closeButton = null!;
        private Panel _titlePanel = null!;
        private Panel _stepsPanel = null!;
        private static readonly Logger _logger = Logger.Instance;

        public StatusWindow()
        {
            InitializeComponent();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "ResolutionSwitcher Status";
            Width = 550;
            Height = 400;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Font = new Font("Tahoma", 8f);

            _titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(8, 0, 8, 0)
            };

            var bannerLabel = new Label
            {
                Text = "ResolutionSwitcher Status",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            _titlePanel.Controls.Add(bannerLabel);

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 2f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 2f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _gameLabel = new Label
            {
                Text = "Counter-Strike 2",
                Dock = DockStyle.Fill,
                Height = 25,
                Font = new Font("Tahoma", 12f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8)
            };

            var separatorTop = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Height = 24,
                Value = 0,
                Style = ProgressBarStyle.Continuous,
                Margin = new Padding(0, 0, 0, 8)
            };

            _progressLabel = new Label
            {
                Text = "0% - Initializing...",
                Dock = DockStyle.Fill,
                Height = 20,
                Font = new Font("Tahoma", 9f),
                Margin = new Padding(0, 0, 0, 8)
            };

            var separatorBottom = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };

            _stepsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.Fixed3D,
                Padding = new Padding(6),
                Margin = new Padding(0, 0, 0, 12)
            };

            _stepsBox = new RichTextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 9f),
                Text = "STEPS:\n[✓] Monitor detected: ASUS VP28UQG (Primary)\n[✓] Default saved: 2560x1440 @ 165 Hz\n[→] Changing resolution...",
                WordWrap = false,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            _stepsPanel.Controls.Add(_stepsBox);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0)
            };

            _closeButton = new Button
            {
                Text = "Done",
                Width = 100,
                Height = 30,
                Margin = new Padding(6, 0, 0, 0),
                Font = new Font("Tahoma", 8f)
            };
            _closeButton.Click += (s, e) => Close();

            _minimizeButton = new Button
            {
                Text = "Minimize",
                Width = 100,
                Height = 30,
                Margin = new Padding(6, 0, 0, 0),
                Font = new Font("Tahoma", 8f)
            };
            _minimizeButton.Click += (s, e) => WindowState = FormWindowState.Minimized;

            buttonPanel.Controls.Add(_closeButton);
            buttonPanel.Controls.Add(_minimizeButton);

            layout.Controls.Add(_gameLabel, 0, 0);
            layout.Controls.Add(separatorTop, 0, 1);
            layout.Controls.Add(_progressBar, 0, 2);
            layout.Controls.Add(_progressLabel, 0, 3);
            layout.Controls.Add(separatorBottom, 0, 4);
            layout.Controls.Add(_stepsPanel, 0, 5);
            layout.Controls.Add(buttonPanel, 0, 6);

            contentPanel.Controls.Add(layout);
            Controls.Add(contentPanel);
            Controls.Add(_titlePanel);

            ResumeLayout(false);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            base.OnFormClosed(e);
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var theme = ThemeManager.Palette;

            BackColor = theme.FormBackground;
            ForeColor = theme.TextColor;
            _titlePanel.BackColor = theme.TitleBarColor;
            _gameLabel.BackColor = theme.FormBackground;
            _gameLabel.ForeColor = theme.TextColor;
            _progressLabel.BackColor = theme.FormBackground;
            _progressLabel.ForeColor = theme.TextColor;
            _stepsPanel.BackColor = theme.SectionBackground;
            _stepsPanel.ForeColor = theme.TextColor;
            _stepsBox.BackColor = theme.SectionBackground;
            _stepsBox.ForeColor = theme.TextColor;

            ThemeManager.ApplyButtonStyle(_minimizeButton);
            ThemeManager.ApplyButtonStyle(_closeButton);

            Invalidate(true);
        }

        public void UpdateProgress(int percentage, string message)
        {
            _progressBar.Value = Math.Min(percentage, 100);
            _progressLabel.Text = $"{percentage}% - {message}";

            _logger.LogInfo($"Status updated: {percentage}% - {message}");
            Application.DoEvents();
        }

        public void AddStep(string status, string message)
        {
            _stepsBox.AppendText($"\n[{status}] {message}");
            _stepsBox.SelectionStart = _stepsBox.TextLength;
            _stepsBox.ScrollToCaret();

            _logger.LogInfo($"Step added: [{status}] {message}");
            Application.DoEvents();
        }

        public void SetTitle(string title)
        {
            Text = $"ResolutionSwitcher Status - {title}";
            _gameLabel.Text = title;
        }
    }
}
