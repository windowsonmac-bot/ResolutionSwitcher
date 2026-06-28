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
        private Panel _titlePanel;

        public MainForm()
        {
            SetupUI();
            InitializeApplication();
            this.Resize += MainForm_Resize;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Reposition buttons when window resizes
            if (_titlePanel != null && _titlePanel.Controls.Count >= 2)
            {
                var settingsBtn = _titlePanel.Controls[0] as Button;
                var aboutBtn = _titlePanel.Controls[1] as Button;
                
                if (settingsBtn != null)
                    settingsBtn.Left = this.Width - 130;
                if (aboutBtn != null)
                    aboutBtn.Left = this.Width - 70;
            }
        }

        private void SetupUI()
        {
            this.Text = "ResolutionSwitcher";
            this.Width = 1000;
            this.Height = 1200;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);

            int yPos = 15;
            int padding = 30;
            int controlWidth = this.Width - (padding * 2) - 20;

            // Title bar with buttons
            _titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var settingsBtn = new Button
            {
                Text = "⚙️",
                Width = 50,
                Height = 40,
                Left = this.Width - 130,
                Top = 10,
                Font = new Font("Arial", 18),
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black
            };
            settingsBtn.Click += SettingsBtn_Click;

            var aboutBtn = new Button
            {
                Text = "?",
                Width = 50,
                Height = 40,
                Left = this.Width - 70,
                Top = 10,
                Font = new Font("Arial", 16),
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black
            };
            aboutBtn.Click += AboutBtn_Click;

            _titlePanel.Controls.Add(settingsBtn);
            _titlePanel.Controls.Add(aboutBtn);
            this.Controls.Add(_titlePanel);

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
                Width = 120,
                Height = 30,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(profileLabel);

            var profileDropdown = new ComboBox
            {
                Left = padding + 140,
                Top = yPos,
                Width = 250,
                Height = 35,
                Name = "profileDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "profileDropdown",
                Font = new Font("Segoe UI", 10)
            };
            profileDropdown.Items.Add("Gaming");
            profileDropdown.Items.Add("Streaming");
            profileDropdown.Items.Add("Productivity");
            profileDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(profileDropdown);

            var newProfileBtn = new Button
            {
                Text = "+ New",
                Left = padding + 410,
                Top = yPos,
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(newProfileBtn);

            var deleteProfileBtn = new Button
            {
                Text = "Delete",
                Left = padding + 510,
                Top = yPos,
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(deleteProfileBtn);

            yPos += 55;

            // Separator
            var sep1 = new Label
            {
                Text = "─────────────────────────────────────────────────────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            scrollPanel.Controls.Add(sep1);
            yPos += 30;

            // Monitor section
            var monitorLabel = new Label
            {
                Text = "MONITOR",
                Left = padding,
                Top = yPos,
                Width = 150,
                Height = 30,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(monitorLabel);
            yPos += 40;

            var monitorSelectLabel = new Label
            {
                Text = "Monitor:",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(monitorSelectLabel);

            var monitorDropdown = new ComboBox
            {
                Left = padding + 120,
                Top = yPos,
                Width = controlWidth - 140,
                Height = 35,
                Name = "monitorDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "monitorDropdown",
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(monitorDropdown);
            yPos += 45;

            var monitorDefaultLabel = new Label
            {
                Text = "Current Default: 2560 x 1440 @ 165 Hz (Auto-Detected)",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(monitorDefaultLabel);
            yPos += 40;

            // Resolution section
            var resLabel = new Label
            {
                Text = "RESOLUTION",
                Left = padding,
                Top = yPos,
                Width = 200,
                Height = 30,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(resLabel);
            yPos += 40;

            var presetLabel = new Label
            {
                Text = "Preset:",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(presetLabel);

            var presetDropdown = new ComboBox
            {
                Left = padding + 120,
                Top = yPos,
                Width = controlWidth - 140,
                Height = 35,
                Name = "presetDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "presetDropdown",
                Font = new Font("Segoe UI", 10)
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
            yPos += 45;

            var customResLabel = new Label
            {
                Text = "Custom:",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(customResLabel);

            var widthInput = new TextBox
            {
                Text = "960",
                Left = padding + 120,
                Top = yPos,
                Width = 80,
                Height = 30,
                Name = "widthInput",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(widthInput);

            var heightInput = new TextBox
            {
                Text = "720",
                Left = padding + 220,
                Top = yPos,
                Width = 80,
                Height = 30,
                Name = "heightInput",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(heightInput);

            var hzLabel = new Label
            {
                Text = "Hz:",
                Left = padding + 320,
                Top = yPos,
                Width = 40,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(hzLabel);

            var hzInput = new TextBox
            {
                Text = "165",
                Left = padding + 360,
                Top = yPos,
                Width = 80,
                Height = 30,
                Name = "hzInput",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(hzInput);
            yPos += 45;

            // Separator
            var sep2 = new Label
            {
                Text = "─────────────────────────────────────────────────────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            scrollPanel.Controls.Add(sep2);
            yPos += 30;

            // Game section
            var gameLabel = new Label
            {
                Text = "GAME",
                Left = padding,
                Top = yPos,
                Width = 200,
                Height = 30,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(gameLabel);
            yPos += 40;

            var gameNameLabel = new Label
            {
                Text = "Game:",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(gameNameLabel);

            var gameDropdown = new ComboBox
            {
                Left = padding + 120,
                Top = yPos,
                Width = controlWidth - 220,
                Height = 35,
                Name = "gameDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "gameDropdown",
                Font = new Font("Segoe UI", 10)
            };
            gameDropdown.Items.Add("Counter-Strike 2");
            gameDropdown.Items.Add("Valorant");
            gameDropdown.Items.Add("Other");
            gameDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(gameDropdown);

            var browseGameBtn = new Button
            {
                Text = "ADD",
                Left = controlWidth - 80,
                Top = yPos,
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 10)
            };
            browseGameBtn.Click += BrowseGameBtn_Click;
            scrollPanel.Controls.Add(browseGameBtn);
            yPos += 45;

            var launchViaLabel = new Label
            {
                Text = "Launch via:",
                Left = padding,
                Top = yPos,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(launchViaLabel);

            var launchMethodDropdown = new ComboBox
            {
                Left = padding + 120,
                Top = yPos,
                Width = controlWidth - 140,
                Height = 35,
                Name = "launchMethodDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "launchMethodDropdown",
                Font = new Font("Segoe UI", 10)
            };
            launchMethodDropdown.Items.Add("Steam");
            launchMethodDropdown.Items.Add("Steam (App ID)");
            launchMethodDropdown.Items.Add("Direct EXE Path");
            launchMethodDropdown.Items.Add("Custom Location");
            launchMethodDropdown.SelectedIndex = 0;
            scrollPanel.Controls.Add(launchMethodDropdown);
            yPos += 45;

            // Launch Mode section
            var launchModeLabel = new Label
            {
                Text = "LAUNCH MODE",
                Left = padding,
                Top = yPos,
                Width = 200,
                Height = 30,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            scrollPanel.Controls.Add(launchModeLabel);
            yPos += 40;

            var autoRestoreRadio = new RadioButton
            {
                Text = "Auto-Restore Helper (auto-revert when game closes)",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240),
                Checked = true,
                Name = "autoRestoreRadio",
                Tag = "autoRestoreRadio",
                Font = new Font("Segoe UI", 10),
                AutoSize = false
            };
            scrollPanel.Controls.Add(autoRestoreRadio);
            yPos += 50;

            var instantKillRadio = new RadioButton
            {
                Text = "Instant Kill Mode (manual reset only)",
                Left = padding,
                Top = yPos,
                Width = controlWidth - 140,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240),
                Name = "instantKillRadio",
                Tag = "instantKillRadio",
                Font = new Font("Segoe UI", 10),
                AutoSize = false
            };
            scrollPanel.Controls.Add(instantKillRadio);

            var learnMoreBtn = new LinkLabel
            {
                Text = "Learn More ▶",
                Left = controlWidth - 100,
                Top = yPos + 5,
                Width = 120,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Blue,
                LinkColor = Color.Blue,
                ActiveLinkColor = Color.Blue,
                VisitedLinkColor = Color.Blue,
                Font = new Font("Segoe UI", 10)
            };
            learnMoreBtn.LinkClicked += LearnMoreBtn_LinkClicked;
            scrollPanel.Controls.Add(learnMoreBtn);
            yPos += 55;

            // Action buttons
            var sep3 = new Label
            {
                Text = "─────────────────────────────────────────────────────────────────────────────────────────────────────",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 25,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9)
            };
            scrollPanel.Controls.Add(sep3);
            yPos += 30;

            var launchGameBtn = new Button
            {
                Text = "LAUNCH GAME & APPLY",
                Left = padding,
                Top = yPos,
                Width = (controlWidth - 20) / 3,
                Height = 50,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            launchGameBtn.Click += LaunchGameBtn_Click;
            scrollPanel.Controls.Add(launchGameBtn);

            var applyOnlyBtn = new Button
            {
                Text = "APPLY ONLY",
                Left = padding + (controlWidth / 3) + 10,
                Top = yPos,
                Width = (controlWidth - 20) / 3,
                Height = 50,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            applyOnlyBtn.Click += ApplyOnlyBtn_Click;
            scrollPanel.Controls.Add(applyOnlyBtn);

            var resetBtn = new Button
            {
                Text = "RESET",
                Left = padding + ((controlWidth / 3) * 2) + 20,
                Top = yPos,
                Width = (controlWidth - 20) / 3,
                Height = 50,
                BackColor = Color.FromArgb(192, 192, 192),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            resetBtn.Click += ResetBtn_Click;
            scrollPanel.Controls.Add(resetBtn);
            yPos += 60;

            // Status bar
            var statusLabel = new Label
            {
                Text = "Status: Ready",
                Left = padding,
                Top = yPos,
                Width = controlWidth,
                Height = 30,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                Name = "statusLabel",
                Tag = "statusLabel",
                Font = new Font("Segoe UI", 10)
            };
            scrollPanel.Controls.Add(statusLabel);
            yPos += 40;

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
