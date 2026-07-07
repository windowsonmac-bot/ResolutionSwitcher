using System;
using System.Drawing;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Safe Mode Fallback confirmation dialog. Shown after a manual resolution change
    /// (Apply Only) when Safe Mode is enabled. Auto-reverts if the user does not
    /// confirm within the countdown, protecting against unreadable/black screens.
    /// </summary>
    public class SafeModeConfirmForm : Form
    {
        private const int CountdownSeconds = 10;
        private Panel _titlePanel = null!;
        private Label _titleLabel = null!;
        private Label _messageLabel = null!;
        private Label _countdownLabel = null!;
        private Button _keepButton = null!;
        private Button _revertButton = null!;
        private Timer _timer = null!;
        private int _secondsRemaining = CountdownSeconds;

        public bool RevertRequested { get; private set; }

        public SafeModeConfirmForm(string resolutionDescription)
        {
            InitializeUI(resolutionDescription);
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
            StartCountdown();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer?.Stop();
            _timer?.Dispose();
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            base.OnFormClosed(e);
        }

        private void InitializeUI(string resolutionDescription)
        {
            SuspendLayout();

            Text = "Safe Mode Fallback";
            Width = 460;
            Height = 260;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Tahoma", 8f);

            _titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(8, 0, 8, 0)
            };

            _titleLabel = new Label
            {
                Text = "Safe Mode Fallback",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            _titlePanel.Controls.Add(_titleLabel);

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _messageLabel = new Label
            {
                Text = $"Resolution changed to {resolutionDescription}.\n\nIf this doesn't look right, click Revert Now.\nOtherwise it will revert automatically unless you confirm.",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9f),
                Margin = new Padding(0, 0, 0, 8)
            };

            _countdownLabel = new Label
            {
                Text = $"Reverting automatically in {CountdownSeconds}s...",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 12)
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true
            };

            _keepButton = new Button
            {
                Text = "Keep This Resolution",
                Width = 160,
                Height = 30,
                Margin = new Padding(6, 0, 0, 0),
                Font = new Font("Tahoma", 8f)
            };
            _keepButton.Click += (s, e) => { RevertRequested = false; Close(); };

            _revertButton = new Button
            {
                Text = "Revert Now",
                Width = 110,
                Height = 30,
                Margin = new Padding(6, 0, 0, 0),
                Font = new Font("Tahoma", 8f)
            };
            _revertButton.Click += (s, e) => { RevertRequested = true; Close(); };

            buttonPanel.Controls.Add(_keepButton);
            buttonPanel.Controls.Add(_revertButton);

            layout.Controls.Add(_messageLabel, 0, 0);
            layout.Controls.Add(_countdownLabel, 0, 1);
            layout.Controls.Add(buttonPanel, 0, 2);

            contentPanel.Controls.Add(layout);
            Controls.Add(contentPanel);
            Controls.Add(_titlePanel);

            ResumeLayout(false);
        }

        private void StartCountdown()
        {
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += (s, e) =>
            {
                _secondsRemaining--;
                if (_secondsRemaining <= 0)
                {
                    _timer.Stop();
                    RevertRequested = true;
                    Close();
                    return;
                }
                _countdownLabel.Text = $"Reverting automatically in {_secondsRemaining}s...";
            };
            _timer.Start();
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
            _messageLabel.ForeColor = theme.TextColor;
            _countdownLabel.ForeColor = theme.StatusHeaderColor;

            ThemeManager.ApplyButtonStyle(_keepButton);
            ThemeManager.ApplyButtonStyle(_revertButton);

            Invalidate(true);
        }
    }
}
