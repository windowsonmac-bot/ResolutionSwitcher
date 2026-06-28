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
            _configManager = null;
            _detectedMonitors = new List<DisplayManager.MonitorInfo>();
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
                    settingsBtn.Left = this.Width - 150;
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
            this.BackColor = Color.FromArgb(224, 224, 224);
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);

            int padding = 20;

            // Title bar with buttons
            _titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(224, 224, 224),
                BorderStyle = BorderStyle.FixedSingle
            };

            var settingsBtn = new Button
            {
                Text = "⚙️",
                Width = 60,
                Height = 50,
                Left = this.Width - 150,
                Top = 5,
                Font = new Font("Arial", 20),
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.Black
            };
            settingsBtn.Click += SettingsBtn_Click;

            var aboutBtn = new Button
            {
                Text = "?",
                Width = 60,
                Height = 50,
                Left = this.Width - 70,
                Top = 5,
                Font = new Font("Arial", 18),
                BackColor = Color.FromArgb(224, 224, 224),
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
                BackColor = Color.FromArgb(224, 224, 224)
            };

            int yPos = padding;

            // Profile section frame
            var profileFrame = new GroupBox
            {
                Text = "PROFILE",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 90,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            var profileLabel = new Label
            {
                Text = "Profile:",
                Left = 15,
                Top = 25,
                Width = 70,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(224, 224, 224)
            };
            profileFrame.Controls.Add(profileLabel);

            var profileDropdown = new ComboBox
            {
                Left = 100,
                Top = 25,
                Width = 200,
                Height = 25,
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
            profileFrame.Controls.Add(profileDropdown);

            var newProfileBtn = new Button
            {
                Text = "+ New",
                Left = 320,
                Top = 25,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 9)
            };
            profileFrame.Controls.Add(newProfileBtn);

            var deleteProfileBtn = new Button
            {
                Text = "Delete",
                Left = 405,
                Top = 25,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 9)
            };
            profileFrame.Controls.Add(deleteProfileBtn);

            scrollPanel.Controls.Add(profileFrame);
            yPos += 105;

            // Monitor section frame
            var monitorFrame = new GroupBox
            {
                Text = "MONITOR",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 100,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            var monitorSelectLabel = new Label
            {
                Text = "Monitor:",
                Left = 15,
                Top = 25,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            monitorFrame.Controls.Add(monitorSelectLabel);

            var monitorDropdown = new ComboBox
            {
                Left = 100,
                Top = 25,
                Width = this.Width - (padding * 2) - 150,
                Height = 25,
                Name = "monitorDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "monitorDropdown",
                Font = new Font("Segoe UI", 10)
            };
            monitorFrame.Controls.Add(monitorDropdown);

            var monitorDefaultLabel = new Label
            {
                Text = "Current Default: 2560 x 1440 @ 165 Hz (Auto-Detected)",
                Left = 15,
                Top = 55,
                Width = this.Width - (padding * 2) - 50,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.FromArgb(64, 64, 64),
                Font = new Font("Segoe UI", 9)
            };
            monitorFrame.Controls.Add(monitorDefaultLabel);

            scrollPanel.Controls.Add(monitorFrame);
            yPos += 115;

            // Resolution section frame
            var resFrame = new GroupBox
            {
                Text = "RESOLUTION",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 140,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            var presetLabel = new Label
            {
                Text = "Preset:",
                Left = 15,
                Top = 25,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            resFrame.Controls.Add(presetLabel);

            var presetDropdown = new ComboBox
            {
                Left = 100,
                Top = 25,
                Width = this.Width - (padding * 2) - 150,
                Height = 25,
                Name = "presetDropdown",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Tag = "presetDropdown",
                Font = new Font("Segoe UI", 9)
            };
            // 16:9 resolutions
            presetDropdown.Items.Add("🟦 16:9 — 3840 × 2160 (2160p)");
            presetDropdown.Items.Add("🟦 16:9 — 2560 × 1440 (1440p)");
            presetDropdown.Items.Add("🟦 16:9 — 1920 × 1080 (1080p)");
            presetDropdown.Items.Add("🟦 16:9 — 1600 × 900 (900p)");
            presetDropdown.Items.Add("🟦 16:9 — 1366 × 768 (768p laptop)");
            presetDropdown.Items.Add("🟦 16:9 — 1280 × 720 (720p)");
            presetDropdown.Items.Add("🟦 16:9 — 1024 × 576 (576p)");
            presetDropdown.Items.Add("🟦 16:9 — 800 × 450 (450p)");
            presetDropdown.Items.Add("🟦 16:9 — 640 × 360 (360p)");
            // 16:10 resolutions
            presetDropdown.Items.Add("🟨 16:10 — 3840 × 2400 (2400p)");
            presetDropdown.Items.Add("🟨 16:10 — 2560 × 1600 (1600p)");
            presetDropdown.Items.Add("🟨 16:10 — 1920 × 1200 (1200p)");
            presetDropdown.Items.Add("🟨 16:10 — 1680 × 1050 (1050p)");
            presetDropdown.Items.Add("🟨 16:10 — 1440 × 900 (900p)");
            presetDropdown.Items.Add("🟨 16:10 — 1280 × 800 (800p)");
            presetDropdown.Items.Add("🟨 16:10 — 1024 × 640 (640p)");
            presetDropdown.Items.Add("🟨 16:10 — 800 × 500 (500p)");
            presetDropdown.Items.Add("🟨 16:10 — 640 × 400 (400p)");
            // 4:3 resolutions
            presetDropdown.Items.Add("🟥 4:3 — 2880 × 2160 (2160p)");
            presetDropdown.Items.Add("🟥 4:3 — 1920 × 1440 (1440p)");
            presetDropdown.Items.Add("🟥 4:3 — 1600 × 1200 (1200p)");
            presetDropdown.Items.Add("🟥 4:3 — 1440 × 1080 (1080p)");
            presetDropdown.Items.Add("🟥 4:3 — 1400 × 1050 (1050p)");
            presetDropdown.Items.Add("🟥 4:3 — 1280 × 960 (960p)");
            presetDropdown.Items.Add("🟥 4:3 — 1200 × 900 (900p)");
            presetDropdown.Items.Add("🟥 4:3 — 1024 × 768 (768p)");
            presetDropdown.Items.Add("🟥 4:3 — 960 × 720 (720p)");
            presetDropdown.Items.Add("🟥 4:3 — 800 × 600 (600p)");
            presetDropdown.Items.Add("🟥 4:3 — 640 × 480 (480p)");
            // 5:4 resolutions
            presetDropdown.Items.Add("🟪 5:4 — 2700 × 2160 (2160p)");
            presetDropdown.Items.Add("🟪 5:4 — 1800 × 1440 (1440p)");
            presetDropdown.Items.Add("🟪 5:4 — 1500 × 1200 (1200p)");
            presetDropdown.Items.Add("🟪 5:4 — 1350 × 1080 (1080p)");
            presetDropdown.Items.Add("🟪 5:4 — 1312 × 1050 (1050p)");
            presetDropdown.Items.Add("🟪 5:4 — 1280 × 1024 (1024p)");
            presetDropdown.Items.Add("🟪 5:4 — 1125 × 900 (900p)");
            presetDropdown.Items.Add("🟪 5:4 — 960 × 768 (768p)");
            presetDropdown.Items.Add("🟪 5:4 — 900 × 720 (720p)");
            presetDropdown.Items.Add("🟪 5:4 — 750 × 600 (600p)");
            presetDropdown.Items.Add("🟪 5:4 — 600 × 480 (480p)");
            presetDropdown.SelectedIndex = 0;
            resFrame.Controls.Add(presetDropdown);

            var customResLabel = new Label
            {
                Text = "Custom:",
                Left = 15,
                Top = 60,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            resFrame.Controls.Add(customResLabel);

            var widthLabel = new Label
            {
                Text = "W:",
                Left = 100,
                Top = 60,
                Width = 25,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            resFrame.Controls.Add(widthLabel);

            var widthInput = new TextBox
            {
                Text = "960",
                Left = 130,
                Top = 60,
                Width = 60,
                Height = 25,
                Name = "widthInput",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            resFrame.Controls.Add(widthInput);

            var heightLabel = new Label
            {
                Text = "H:",
                Left = 200,
                Top = 60,
                Width = 25,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            resFrame.Controls.Add(heightLabel);

            var heightInput = new TextBox
            {
                Text = "720",
                Left = 230,
                Top = 60,
                Width = 60,
                Height = 25,
                Name = "heightInput",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            resFrame.Controls.Add(heightInput);

            var hzLabel = new Label
            {
                Text = "Hz:",
                Left = 300,
                Top = 60,
                Width = 35,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            resFrame.Controls.Add(hzLabel);

            var hzInput = new TextBox
            {
                Text = "165",
                Left = 340,
                Top = 60,
                Width = 60,
                Height = 25,
                Name = "hzInput",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            resFrame.Controls.Add(hzInput);

            scrollPanel.Controls.Add(resFrame);
            yPos += 155;

            // Game section frame
            var gameFrame = new GroupBox
            {
                Text = "GAME",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 125,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            var gameNameLabel = new Label
            {
                Text = "Game:",
                Left = 15,
                Top = 25,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            gameFrame.Controls.Add(gameNameLabel);

            var gameDropdown = new ComboBox
            {
                Left = 100,
                Top = 25,
                Width = this.Width - (padding * 2) - 200,
                Height = 25,
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
            gameFrame.Controls.Add(gameDropdown);

            var browseGameBtn = new Button
            {
                Text = "ADD",
                Left = this.Width - (padding * 2) - 100,
                Top = 25,
                Width = 85,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            browseGameBtn.Click += BrowseGameBtn_Click;
            gameFrame.Controls.Add(browseGameBtn);

            var launcherLabel = new Label
            {
                Text = "Launcher:",
                Left = 15,
                Top = 60,
                Width = 70,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 10)
            };
            gameFrame.Controls.Add(launcherLabel);

            var launchMethodDropdown = new ComboBox
            {
                Left = 100,
                Top = 60,
                Width = this.Width - (padding * 2) - 150,
                Height = 25,
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
            gameFrame.Controls.Add(launchMethodDropdown);

            scrollPanel.Controls.Add(gameFrame);
            yPos += 140;

            // Launch Mode section frame
            var launchModeFrame = new GroupBox
            {
                Text = "LAUNCH MODE",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 120,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            var autoRestoreRadio = new RadioButton
            {
                Text = "Auto-Restore Helper (auto-revert when game closes)",
                Left = 15,
                Top = 25,
                Width = this.Width - (padding * 2) - 60,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Checked = true,
                Name = "autoRestoreRadio",
                Tag = "autoRestoreRadio",
                Font = new Font("Segoe UI", 10),
                AutoSize = false
            };
            launchModeFrame.Controls.Add(autoRestoreRadio);

            var instantKillRadio = new RadioButton
            {
                Text = "Instant Kill Mode (manual reset only)",
                Left = 15,
                Top = 55,
                Width = this.Width - (padding * 2) - 150,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                Name = "instantKillRadio",
                Tag = "instantKillRadio",
                Font = new Font("Segoe UI", 10),
                AutoSize = false
            };
            launchModeFrame.Controls.Add(instantKillRadio);

            var learnMoreBtn = new LinkLabel
            {
                Text = "Learn More ▶",
                Left = this.Width - (padding * 2) - 120,
                Top = 55,
                Width = 110,
                Height = 25,
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.Blue,
                LinkColor = Color.Blue,
                ActiveLinkColor = Color.DarkBlue,
                VisitedLinkColor = Color.Blue,
                Font = new Font("Segoe UI", 10)
            };
            learnMoreBtn.LinkClicked += LearnMoreBtn_LinkClicked;
            launchModeFrame.Controls.Add(learnMoreBtn);

            scrollPanel.Controls.Add(launchModeFrame);
            yPos += 135;

            // Action buttons frame
            var actionFrame = new GroupBox
            {
                Text = "ACTIONS",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 85,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            int btnWidth = (this.Width - (padding * 2) - 60) / 3;

            var applyLaunchBtn = new Button
            {
                Text = "APPLY AND LAUNCH GAME",
                Left = 15,
                Top = 25,
                Width = btnWidth,
                Height = 45,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            applyLaunchBtn.Click += LaunchGameBtn_Click;
            actionFrame.Controls.Add(applyLaunchBtn);

            var applyOnlyBtn = new Button
            {
                Text = "APPLY ONLY",
                Left = 15 + btnWidth + 15,
                Top = 25,
                Width = btnWidth,
                Height = 45,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            applyOnlyBtn.Click += ApplyOnlyBtn_Click;
            actionFrame.Controls.Add(applyOnlyBtn);

            var resetBtn = new Button
            {
                Text = "RESET",
                Left = 15 + (btnWidth + 15) * 2,
                Top = 25,
                Width = btnWidth,
                Height = 45,
                BackColor = Color.FromArgb(224, 224, 224),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            resetBtn.Click += ResetBtn_Click;
            actionFrame.Controls.Add(resetBtn);

            scrollPanel.Controls.Add(actionFrame);
            yPos += 100;

            // Status bar
            var statusLabel = new Label
            {
                Text = "Status: Ready",
                Left = padding,
                Top = yPos,
                Width = this.Width - (padding * 2) - 20,
                Height = 30,
                BackColor = Color.FromArgb(192, 192, 192),
                ForeColor = Color.Black,
                Name = "statusLabel",
                Tag = "statusLabel",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.Fixed3D,
                Padding = new Padding(5)
            };
            scrollPanel.Controls.Add(statusLabel);

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

        private void LaunchGameBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("Apply and Launch Game clicked");
            var statusLabel = this.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name == "statusLabel") as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = "Status: Launching game...";
            }
        }

        private void ApplyOnlyBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("Apply Only clicked");
            var statusLabel = this.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name == "statusLabel") as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = "Status: Applying resolution...";
            }
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("Reset clicked");
            var statusLabel = this.Controls.Cast<Control>()
                .FirstOrDefault(c => c.Name == "statusLabel") as Label;
            if (statusLabel != null)
            {
                statusLabel.Text = "Status: Resetting to default...";
            }
        }

        private void BrowseGameBtn_Click(object sender, EventArgs e)
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

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("Settings clicked");
        }

        private void AboutBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("About clicked");
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }

        private void LearnMoreBtn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _logger.LogInfo("Learn More clicked");
            var modeForm = new Form
            {
                Text = "Launch Modes - Detailed Comparison",
                Width = 700,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(224, 224, 224),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.Fixed3D
            };

            string modeText = @"AUTO-RESTORE HELPER
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Description:
Automatically reverts your resolution back to your original settings when the game closes.

How It Works:
1. Applies your custom resolution and launches the game
2. Helper utility monitors the game process in background
3. When game exits, resolution automatically switches back

Resource Usage:
• Memory: ~5-10 MB (minimal background process)
• CPU: <1% (low priority thread)
• Disk: Negligible (helper runs in RAM)

Best For:
✓ Competitive gaming (less downtime between matches)
✓ Players who forget to reset resolution
✓ Frequent game launchers
✓ Minimal performance impact

Pros:
✓ Zero manual intervention required
✓ Fast workflow for multiple gaming sessions
✓ Reliable and automatic

Cons:
✗ Slight delay on game close while reverting


INSTANT KILL MODE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Description:
Launches your game with custom resolution. You manually reset when done.

How It Works:
1. Applies your custom resolution and launches the game
2. No background monitoring - pure launch
3. You click 'RESET' button when ready to restore original resolution

Resource Usage:
• Memory: 0 MB (no helper process)
• CPU: 0% (no monitoring)
• Disk: 0 MB

Best For:
✓ Single long gaming sessions
✓ Users who prefer full control
✓ Low-end systems (saves resources)
✓ Testing/development scenarios

Pros:
✓ Zero resource overhead
✓ Maximum user control
✓ Instant game launch (no monitoring setup)
✓ Best for low-end PCs

Cons:
✗ Requires manual reset
✗ Easy to forget resolution change";

            rtb.Text = modeText;
            modeForm.Controls.Add(rtb);
            modeForm.ShowDialog(this);
        }
    }
}
