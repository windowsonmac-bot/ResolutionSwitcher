using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// DDU-style status window for displaying operation progress
    /// Static design, no animations
    /// </summary>
    public class StatusWindow : Form
    {
        private Label? _progressLabel;
        private ProgressBar? _progressBar;
        private Label? _stepsLabel;
        private static readonly Logger _logger = Logger.Instance;

        public StatusWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ResolutionSwitcher Status";
            this.Width = 550;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.TopMost = true;

            int padding = 20;
            int yPos = 15;

            // Title
            var titleLabel = new Label
            {
                Text = "Counter-Strike 2",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2),
                Height = 25,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            this.Controls.Add(titleLabel);
            yPos += 30;

            // Separator
            var sepLabel = new Label
            {
                Text = "─────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2),
                Height = 15,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            this.Controls.Add(sepLabel);
            yPos += 20;

            // Progress bar
            _progressBar = new ProgressBar
            {
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2),
                Height = 25,
                Value = 0,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.FromArgb(0, 0, 128)
            };
            this.Controls.Add(_progressBar);
            yPos += 30;

            // Progress text
            _progressLabel = new Label
            {
                Text = "0% - Initializing...",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2),
                Height = 20,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            this.Controls.Add(_progressLabel);
            yPos += 25;

            // Separator
            var sepLabel2 = new Label
            {
                Text = "─────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2),
                Height = 15,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            this.Controls.Add(sepLabel2);
            yPos += 20;

            // Steps
            _stepsLabel = new Label
            {
                Text = "STEPS:\n[✓] Monitor detected: ASUS VP28UQG (Primary)\n[✓] Default saved: 2560x1440 @ 165 Hz\n[→] Changing resolution...",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2),
                Height = 120,
                Font = new Font("Courier New", 9),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                AutoSize = false
            };
            this.Controls.Add(_stepsLabel);
            yPos += 130;

            // Buttons
            var minimizeBtn = new Button
            {
                Text = "Minimize",
                Left = this.Width - 230,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black
            };
            minimizeBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            this.Controls.Add(minimizeBtn);

            var closeBtn = new Button
            {
                Text = "Done",
                Left = this.Width - 120,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black
            };
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);
        }

        public void UpdateProgress(int percentage, string message)
        {
            if (_progressBar != null)
            {
                _progressBar.Value = Math.Min(percentage, 100);
            }

            if (_progressLabel != null)
            {
                _progressLabel.Text = $"{percentage}% - {message}";
            }

            _logger.LogInfo($"Status updated: {percentage}% - {message}");
            Application.DoEvents(); // Refresh UI
        }

        public void AddStep(string status, string message)
        {
            if (_stepsLabel != null)
            {
                _stepsLabel.Text += $"\n[{status}] {message}";
            }

            _logger.LogInfo($"Step added: [{status}] {message}");
            Application.DoEvents();
        }

        public void SetTitle(string title)
        {
            this.Text = $"ResolutionSwitcher Status - {title}";
        }
    }
}
