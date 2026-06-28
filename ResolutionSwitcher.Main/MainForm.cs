using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace ResolutionSwitcher.Main
{
    public class MainForm : Form
    {
        private ConfigManager _configManager;
        private List<DisplayManager.MonitorInfo> _detectedMonitors;
        private static readonly Logger _logger = Logger.Instance;

        public MainForm()
        {
            SetupUI();
            InitializeApplication();
        }

        private void SetupUI()
        {
            this.Text = "ResolutionSwitcher";
            this.Width = 700;
            this.Height = 900;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int yPos = 15;
            int padding = 20;
            int controlWidth = this.Width - (padding * 2) - 20;

            // Title bar with buttons
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var settingsBtn = new Button
            {
                Text = "⚙️",
                Width = 40,
                Height = 30,
                Left = this.Width - 110,
                Top = 10,
                Font = new Font("Arial", 16),
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black
            };
            settingsBtn.Click += SettingsBtn_Click;

            var aboutBtn = new Button
            {
                Text = "?",
                Width = 40,
                Height = 30,
                Left = this.Width - 60,
                Top = 10,
                Font = new Font("Arial", 14),
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black
            };
            aboutBtn.Click += AboutBtn_Click;

            titlePanel.Controls.Add(settingsBtn);
            titlePanel.Controls.Add(aboutBtn);
            this.Controls.Add(titlePanel);

            // Scrollable content panel
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Profile section
            var profileLabel = new Label
            {
                Text = "PROFILE:",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 25,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(profileLabel);

            var profileDropdown = new ComboBox
            {
                Left = padding + 110,
                Top = yPos - 3,
                Width = 200,
                Height = 30,
                Name = "profileDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "profileDropdown"
            };
            profileDropdown.Items.Add("Gaming");
            profileDropdown.Items.Add("Streaming");
            profileDropdown.Items.Add("Productivity");
            profileDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(profileDropdown);

            var newProfileBtn = new Button
            {
                Text = "+ New",
                Left = padding + 320,
                Top = yPos - 3,
                Width = 60,
                Height = 30,
                BackColor = Color.FromArgb(192, 192, 192)
            };
            scrollPanel.Controls.Add(newProfileBtn);

            var deleteProfileBtn = new Button
            {
                Text = "Delete",
                Left = padding + 390,
                Top = yPos - 3,
                Width = 60,
                Height = 30,
                BackColor = Color.FromArgb(192, 192, 192)
            };
            scrollPanel.Controls.Add(deleteProfileBtn);

            yPos += 50;

            // Separator
            var sep1 = new Label
            {
                Text = "─────────────────────────────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 20,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            scrollPanel.Controls.Add(sep1);
            yPos += 25;

            // Monitor section
            var monitorLabel = new Label
            {
                Text = "MONITOR",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 25,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(monitorLabel);
            yPos += 30;

            var monitorSelectLabel = new Label
            {
                Text = "Monitor:",
                Left = padding,
                Top = yPos,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(monitorSelectLabel);

            var monitorDropdown = new ComboBox
            {
                Left = padding + 80,
                Top = yPos - 3,
                Width = controlWidth - 90,
                Height = 30,
                Name = "monitorDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "monitorDropdown"
            };
            scrollPanel.Controls.Add(monitorDropdown);
            yPos += 35;

            var monitorDefaultLabel = new Label
            {
                Text = "Current Default: 2560 x 1440 @ 165 Hz (Auto-Detected)",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };
            scrollPanel.Controls.Add(monitorDefaultLabel);
            yPos += 30;

            // Resolution section
            var resLabel = new Label
            {
                Text = "RESOLUTION",
                Left = padding,
                Top = yPos,
                Width = 150,
                Height = 25,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(resLabel);
            yPos += 30;

            var presetLabel = new Label
            {
                Text = "Preset:",
                Left = padding,
                Top = yPos,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(presetLabel);

            var presetDropdown = new ComboBox
            {
                Left = padding + 80,
                Top = yPos - 3,
                Width = controlWidth - 90,
                Height = 30,
                Name = "presetDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "presetDropdown"
            };
            presetDropdown.Items.Add("960x720 (4:3)");
            presetDropdown.Items.Add("1080x810 (4:3)");
            presetDropdown.Items.Add("1280x960 (4:3)");
            presetDropdown.Items.Add("1440x1080 (4:3)");
            presetDropdown.Items.Add("1600x1200 (4:3)");
            presetDropdown.Items.Add("1152x690 (5:3)");
            presetDropdown.Items.Add("1440x864 (5:3)");
            presetDropdown.Items.Add("1728x1036 (5:3)");
            presetDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(presetDropdown);
            yPos += 35;

            var customResLabel = new Label
            {
                Text = "Custom:",
                Left = padding,
                Top = yPos,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(customResLabel);

            var widthInput = new TextBox
            {
                Text = "960",
                Left = padding + 80,
                Top = yPos - 3,
                Width = 60,
                Height = 25,
                Name = "widthInput",
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            scrollPanel.Controls.Add(widthInput);

            var heightInput = new TextBox
            {
                Text = "720",
                Left = padding + 160,
                Top = yPos - 3,
                Width = 60,
                Height = 25,
                Name = "heightInput",
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            scrollPanel.Controls.Add(heightInput);

            var hzLabel = new Label
            {
                Text = "Hz:",
                Left = padding + 235,
                Top = yPos,
                Width = 30,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(hzLabel);

            var hzInput = new TextBox
            {
                Text = "165",
                Left = padding + 265,
                Top = yPos - 3,
                Width = 60,
                Height = 25,
                Name = "hzInput",
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            scrollPanel.Controls.Add(hzInput);
            yPos += 35;

            // Separator
            var sep2 = new Label
            {
                Text = "─────────────────────────────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 20,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            scrollPanel.Controls.Add(sep2);
            yPos += 25;

            // Game section
            var gameLabel = new Label
            {
                Text = "GAME",
                Left = padding,
                Top = yPos,
                Width = 150,
                Height = 25,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(gameLabel);
            yPos += 30;

            var gameNameLabel = new Label
            {
                Text = "Game:",
                Left = padding,
                Top = yPos,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(gameNameLabel);

            var gameDropdown = new ComboBox
            {
                Left = padding + 80,
                Top = yPos - 3,
                Width = controlWidth - 170,
                Height = 30,
                Name = "gameDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "gameDropdown"
            };
            gameDropdown.Items.Add("Counter-Strike 2");
            gameDropdown.Items.Add("Valorant");
            gameDropdown.Items.Add("Other");
            gameDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(gameDropdown);

            var browseGameBtn = new Button
            {
                Text = "Browse",
                Left = controlWidth - 70,
                Top = yPos - 3,
                Width = 70,
                Height = 30,
                BackColor = Color.FromArgb(192, 192, 192)
            };
            browseGameBtn.Click += BrowseGameBtn_Click;
            scrollPanel.Controls.Add(browseGameBtn);
            yPos += 35;

            var launchViaLabel = new Label
            {
                Text = "Launch via:",
                Left = padding,
                Top = yPos,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(launchViaLabel);

            var launchMethodDropdown = new ComboBox
            {
                Left = padding + 80,
                Top = yPos - 3,
                Width = controlWidth - 90,
                Height = 30,
                Name = "launchMethodDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "launchMethodDropdown"
            };
            launchMethodDropdown.Items.Add("Steam");
            launchMethodDropdown.Items.Add("Steam (App ID)");
            launchMethodDropdown.Items.Add("Direct EXE Path");
            launchMethodDropdown.Items.Add("Custom Location");
            launchMethodDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(launchMethodDropdown);
            yPos += 35;

            // Launch Mode section
            var launchModeLabel = new Label
            {
                Text = "LAUNCH MODE",
                Left = padding,
                Top = yPos,
                Width = 150,
                Height = 25,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(launchModeLabel);
            yPos += 30;

            var autoRestoreRadio = new RadioButton
            {
                Text = "Auto-Restore Helper (auto-revert when game closes)",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                Checked = true,
                Name = "autoRestoreRadio",
                Tag = "autoRestoreRadio"
            };
            scrollPanel.Controls.Add(autoRestoreRadio);
            yPos += 30;

            var instantKillRadio = new RadioButton
            {
                Text = "Instant Kill Mode (manual reset only)",
                Left = padding,
                Top = yPos,
                Width = controlWidth - 100,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                Name = "instantKillRadio",
                Tag = "instantKillRadio"
            };
            scrollPanel.Controls.Add(instantKillRadio);

            var learnMoreBtn = new LinkLabel
            {
                Text = "Learn More ▶",
                Left = controlWidth - 90,
                Top = yPos,
                Width = 100,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                LinkColor = Color.Blue
            };
            learnMoreBtn.LinkClicked += LearnMoreBtn_LinkClicked;
            scrollPanel.Controls.Add(learnMoreBtn);
            yPos += 35;

            // Action buttons
            var sep3 = new Label
            {
                Text = "─────────────────────────────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 20,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black
            };
            scrollPanel.Controls.Add(sep3);
            yPos += 25;

            var launchGameBtn = new Button
            {
                Text = "LAUNCH GAME & APPLY",
                Left = padding,
                Top = yPos,
                Width = (controlWidth - 10) / 3,
                Height = 40,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            launchGameBtn.Click += LaunchGameBtn_Click;
            scrollPanel.Controls.Add(launchGameBtn);

            var applyOnlyBtn = new Button
            {
                Text = "APPLY ONLY",
                Left = padding + (controlWidth / 3) + 5,
                Top = yPos,
                Width = (controlWidth - 10) / 3,
                Height = 40,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            applyOnlyBtn.Click += ApplyOnlyBtn_Click;
            scrollPanel.Controls.Add(applyOnlyBtn);

            var resetBtn = new Button
            {
                Text = "RESET",
                Left = padding + ((controlWidth / 3) * 2) + 10,
                Top = yPos,
                Width = (controlWidth - 10) / 3,
                Height = 40,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            resetBtn.Click += ResetBtn_Click;
            scrollPanel.Controls.Add(resetBtn);
            yPos += 50;

            // Status bar
            var statusLabel = new Label
            {
                Text = "Status: Ready",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Name = "statusLabel",
                Tag = "statusLabel"
            };
            scrollPanel.Controls.Add(statusLabel);
            yPos += 30;

            this.Controls.Add(scrollPanel);
        }

        private void InitializeApplication()
        {
            try
            {
                _logger.LogInfo("Initializing ResolutionSwitcher main application");

                // Initialize config manager
                _configManager = new ConfigManager();

                // Detect monitors
                _detectedMonitors = DisplayManager.GetMonitors();

                if (_detectedMonitors.Count == 0)
                {
                    MessageBox.Show("No monitors detected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _logger.LogError("No monitors detected on startup");
                    return;
                }

                // Initialize config if first run
                _configManager.InitializeFromDetectedMonitors(_detectedMonitors);

                // Populate monitor dropdown
                var monitorDropdown = this.Controls.Cast<Control>()
                    .FirstOrDefault(c => c.Name == "monitorDropdown") as ComboBox;
                if (monitorDropdown != null)
                {
                    monitorDropdown.Items.Clear();
                    foreach (var monitor in _detectedMonitors)
                    {
                        monitorDropdown.Items.Add($"{monitor.FriendlyName} ({monitor.Width}x{monitor.Height}@{monitor.RefreshRate}Hz)");
                    }
                    if (monitorDropdown.Items.Count > 0)
                    {
                        monitorDropdown.SelectedIndex = 0;
                    }
                }

                _logger.LogSuccess("Main application initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error initializing application", ex);
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchGameBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Launch Game & Apply clicked");
            var statusLabel = this.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name == "statusLabel") as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = "Status: Launching game...";
            }
            // TODO: Implement launch logic
        }

        private void ApplyOnlyBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Apply Only clicked");
            var statusLabel = this.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name == "statusLabel") as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = "Status: Applying resolution...";
            }
            // TODO: Implement apply-only logic
        }

        private void ResetBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Reset clicked");
            var statusLabel = this.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name == "statusLabel") as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = "Status: Resetting to default...";
            }
            // TODO: Implement reset logic
        }

        private void BrowseGameBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Browse Game clicked");
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Game EXE"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _logger.LogInfo($"Game selected: {openFileDialog.FileName}");
            }
        }

        private void SettingsBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Settings clicked");
            // TODO: Open settings window
        }

        private void AboutBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("About clicked");
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }

        private void LearnMoreBtn_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            _logger.LogInfo("Learn More clicked");
            MessageBox.Show("See 'Launch Modes' tab in About for detailed information.", "Launch Modes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
