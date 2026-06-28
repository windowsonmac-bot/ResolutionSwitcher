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

        private ComboBox _monitorDropdown;
        private Label _monitorDefaultLabel;
        private ComboBox _profileDropdown;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;

        public MainForm()
        {
            _configManager = null;
            _detectedMonitors = new List<DisplayManager.MonitorInfo>();
            SetupUI();
            InitializeApplication();
        }

        private void SetupUI()
        {
            this.SuspendLayout();

            // ── Form properties ──────────────────────────────────────────
            this.Text = "ResolutionSwitcher v1.0";
            this.ClientSize = new Size(780, 560);
            this.MinimumSize = new Size(620, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.Font = new Font("Tahoma", 8f);

            // ── Title banner (CPU-Z style teal/navy header) ───────────────
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.FromArgb(0, 78, 152),
                Padding = new Padding(8, 0, 4, 0)
            };
            titlePanel.SuspendLayout();

            var titleLabel = new Label
            {
                Text = "ResolutionSwitcher",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Tahoma", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            var subtitleLabel = new Label
            {
                Text = "Display Resolution Manager",
                Dock = DockStyle.Right,
                ForeColor = Color.FromArgb(180, 210, 240),
                Font = new Font("Tahoma", 7.5f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Width = 200,
                BackColor = Color.Transparent
            };
            var settingsBtn = new Button
            {
                Text = "Settings",
                Dock = DockStyle.Right,
                Width = 64,
                FlatStyle = FlatStyle.System,
                Font = new Font("Tahoma", 7.5f)
            };
            settingsBtn.Click += SettingsBtn_Click;
            var aboutBtn = new Button
            {
                Text = "About",
                Dock = DockStyle.Right,
                Width = 54,
                FlatStyle = FlatStyle.System,
                Font = new Font("Tahoma", 7.5f)
            };
            aboutBtn.Click += AboutBtn_Click;

            // Add right-docked controls first (they stack right-to-left)
            titlePanel.Controls.Add(subtitleLabel);
            titlePanel.Controls.Add(settingsBtn);
            titlePanel.Controls.Add(aboutBtn);
            titlePanel.Controls.Add(titleLabel);  // Fill goes last
            titlePanel.ResumeLayout(false);
            _titlePanel = titlePanel;

            // ── Status strip (authentic XP utility bottom bar) ────────────
            _statusStrip = new StatusStrip
            {
                SizingGrip = true,
                Font = new Font("Tahoma", 7.5f)
            };
            _statusLabel = new ToolStripStatusLabel
            {
                Text = "Ready.",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var versionLabel = new ToolStripStatusLabel
            {
                Text = "v1.0.0",
                TextAlign = ContentAlignment.MiddleRight
            };
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(new ToolStripSeparator());
            _statusStrip.Items.Add(versionLabel);

            // ── Scroll container ──────────────────────────────────────────
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8, 6, 8, 4)
            };
            scrollPanel.SuspendLayout();

            // ── Main vertical layout ──────────────────────────────────────
            var mainLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            mainLayout.SuspendLayout();
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // Add 6 rows
            for (int i = 0; i < 6; i++)
                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ═════════════════════════════════════════════════════════════
            // PROFILE section
            // ═════════════════════════════════════════════════════════════
            var profileGroup = MakeGroup("Profile");
            var profileLayout = MakeTwoColLayout(1);
            profileLayout.SuspendLayout();

            _profileDropdown = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 8f),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 180
            };
            _profileDropdown.Items.AddRange(new object[] { "Gaming", "Streaming", "Productivity" });
            _profileDropdown.SelectedIndex = 0;

            var newProfileBtn = new Button { Text = "+ New", Width = 58, Height = 23, FlatStyle = FlatStyle.System, Font = new Font("Tahoma", 7.5f), Margin = new Padding(4, 0, 0, 0) };
            var deleteProfileBtn = new Button { Text = "Delete", Width = 58, Height = 23, FlatStyle = FlatStyle.System, Font = new Font("Tahoma", 7.5f), Margin = new Padding(2, 0, 0, 0) };

            var profileFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2)
            };
            profileFlow.Controls.Add(_profileDropdown);
            profileFlow.Controls.Add(newProfileBtn);
            profileFlow.Controls.Add(deleteProfileBtn);

            profileLayout.Controls.Add(MakeLabel("Profile:"), 0, 0);
            profileLayout.Controls.Add(profileFlow, 1, 0);
            profileLayout.ResumeLayout(false);
            profileGroup.Controls.Add(profileLayout);

            // ═════════════════════════════════════════════════════════════
            // MONITOR section
            // ═════════════════════════════════════════════════════════════
            var monitorGroup = MakeGroup("Monitor");
            var monitorLayout = MakeTwoColLayout(2);
            monitorLayout.SuspendLayout();

            _monitorDropdown = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 2, 0, 2)
            };
            _monitorDefaultLabel = new Label
            {
                Text = "Detecting monitors...",
                Dock = DockStyle.Fill,
                ForeColor = SystemColors.GrayText,
                Font = new Font("Tahoma", 7.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 1, 0, 3)
            };

            monitorLayout.Controls.Add(MakeLabel("Monitor:"), 0, 0);
            monitorLayout.Controls.Add(_monitorDropdown, 1, 0);
            monitorLayout.Controls.Add(MakeLabel(""), 0, 1);
            monitorLayout.Controls.Add(_monitorDefaultLabel, 1, 1);
            monitorLayout.ResumeLayout(false);
            monitorGroup.Controls.Add(monitorLayout);

            // ═════════════════════════════════════════════════════════════
            // RESOLUTION section
            // ═════════════════════════════════════════════════════════════
            var resGroup = MakeGroup("Resolution");
            var resLayout = MakeTwoColLayout(2);
            resLayout.SuspendLayout();

            var presetDropdown = new ComboBox
            {
                Name = "presetDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 2, 0, 2)
            };
            // 16:9
            presetDropdown.Items.Add("16:9  3840x2160  (2160p / 4K)");
            presetDropdown.Items.Add("16:9  2560x1440  (1440p)");
            presetDropdown.Items.Add("16:9  1920x1080  (1080p)");
            presetDropdown.Items.Add("16:9  1600x900   (900p)");
            presetDropdown.Items.Add("16:9  1366x768   (768p laptop)");
            presetDropdown.Items.Add("16:9  1280x720   (720p)");
            presetDropdown.Items.Add("16:9  1024x576   (576p)");
            presetDropdown.Items.Add("16:9  800x450    (450p)");
            presetDropdown.Items.Add("16:9  640x360    (360p)");
            // 16:10
            presetDropdown.Items.Add("16:10  3840x2400  (2400p)");
            presetDropdown.Items.Add("16:10  2560x1600  (1600p)");
            presetDropdown.Items.Add("16:10  1920x1200  (1200p)");
            presetDropdown.Items.Add("16:10  1680x1050  (1050p)");
            presetDropdown.Items.Add("16:10  1440x900   (900p)");
            presetDropdown.Items.Add("16:10  1280x800   (800p)");
            presetDropdown.Items.Add("16:10  1024x640   (640p)");
            presetDropdown.Items.Add("16:10  800x500    (500p)");
            presetDropdown.Items.Add("16:10  640x400    (400p)");
            // 4:3
            presetDropdown.Items.Add("4:3  2880x2160  (2160p)");
            presetDropdown.Items.Add("4:3  1920x1440  (1440p)");
            presetDropdown.Items.Add("4:3  1600x1200  (1200p)");
            presetDropdown.Items.Add("4:3  1440x1080  (1080p)");
            presetDropdown.Items.Add("4:3  1400x1050  (1050p)");
            presetDropdown.Items.Add("4:3  1280x960   (960p)");
            presetDropdown.Items.Add("4:3  1200x900   (900p)");
            presetDropdown.Items.Add("4:3  1024x768   (768p)");
            presetDropdown.Items.Add("4:3  960x720    (720p)");
            presetDropdown.Items.Add("4:3  800x600    (600p)");
            presetDropdown.Items.Add("4:3  640x480    (480p)");
            // 5:4
            presetDropdown.Items.Add("5:4  2700x2160  (2160p)");
            presetDropdown.Items.Add("5:4  1800x1440  (1440p)");
            presetDropdown.Items.Add("5:4  1500x1200  (1200p)");
            presetDropdown.Items.Add("5:4  1350x1080  (1080p)");
            presetDropdown.Items.Add("5:4  1312x1050  (1050p)");
            presetDropdown.Items.Add("5:4  1280x1024  (1024p)");
            presetDropdown.Items.Add("5:4  1125x900   (900p)");
            presetDropdown.Items.Add("5:4  960x768    (768p)");
            presetDropdown.Items.Add("5:4  900x720    (720p)");
            presetDropdown.Items.Add("5:4  750x600    (600p)");
            presetDropdown.Items.Add("5:4  600x480    (480p)");
            presetDropdown.SelectedIndex = 0;

            // Custom resolution row
            var customFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2)
            };
            var widthInput = new TextBox { Name = "widthInput", Text = "960", Width = 52, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f) };
            var heightInput = new TextBox { Name = "heightInput", Text = "720", Width = 52, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f) };
            var hzInput = new TextBox { Name = "hzInput", Text = "240", Width = 52, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f) };

            customFlow.Controls.Add(new Label { Text = "W:", Width = 20, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f) });
            customFlow.Controls.Add(widthInput);
            customFlow.Controls.Add(new Label { Text = "H:", Width = 24, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f), Margin = new Padding(6, 0, 0, 0) });
            customFlow.Controls.Add(heightInput);
            customFlow.Controls.Add(new Label { Text = "Hz:", Width = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f), Margin = new Padding(6, 0, 0, 0) });
            customFlow.Controls.Add(hzInput);

            resLayout.Controls.Add(MakeLabel("Preset:"), 0, 0);
            resLayout.Controls.Add(presetDropdown, 1, 0);
            resLayout.Controls.Add(MakeLabel("Custom:"), 0, 1);
            resLayout.Controls.Add(customFlow, 1, 1);
            resLayout.ResumeLayout(false);
            resGroup.Controls.Add(resLayout);

            // ═════════════════════════════════════════════════════════════
            // GAME section
            // ═════════════════════════════════════════════════════════════
            var gameGroup = MakeGroup("Game");
            var gameLayout = MakeTwoColLayout(2);
            gameLayout.SuspendLayout();

            var gameDropdown = new ComboBox
            {
                Name = "gameDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 8f),
                Width = 240,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            gameDropdown.Items.AddRange(new object[] { "Counter-Strike 2", "Valorant", "Other" });
            gameDropdown.SelectedIndex = 0;

            var addGameBtn = new Button { Text = "Add...", Width = 58, Height = 23, FlatStyle = FlatStyle.System, Font = new Font("Tahoma", 7.5f), Margin = new Padding(4, 0, 0, 0) };
            addGameBtn.Click += BrowseGameBtn_Click;

            var gameFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2)
            };
            gameFlow.Controls.Add(gameDropdown);
            gameFlow.Controls.Add(addGameBtn);

            var launchMethodDropdown = new ComboBox
            {
                Name = "launchMethodDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 2, 0, 2)
            };
            launchMethodDropdown.Items.AddRange(new object[] { "Steam", "Steam (App ID)", "Direct EXE Path", "Custom Location" });
            launchMethodDropdown.SelectedIndex = 0;

            gameLayout.Controls.Add(MakeLabel("Game:"), 0, 0);
            gameLayout.Controls.Add(gameFlow, 1, 0);
            gameLayout.Controls.Add(MakeLabel("Launcher:"), 0, 1);
            gameLayout.Controls.Add(launchMethodDropdown, 1, 1);
            gameLayout.ResumeLayout(false);
            gameGroup.Controls.Add(gameLayout);

            // ═════════════════════════════════════════════════════════════
            // LAUNCH MODE section
            // ═════════════════════════════════════════════════════════════
            var launchGroup = MakeGroup("Launch Mode");
            var launchLayout = MakeTwoColLayout(1);
            launchLayout.SuspendLayout();

            // Reconfigure as single-column for the radio rows
            launchLayout.ColumnStyles.Clear();
            launchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            launchLayout.ColumnCount = 1;
            launchLayout.RowCount = 1;
            launchLayout.RowStyles.Clear();
            launchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var radioPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            var autoRestoreRadio = new RadioButton
            {
                Name = "autoRestoreRadio",
                Text = "Auto-Restore Helper  -  reverts resolution automatically when game closes",
                Checked = true,
                AutoSize = true,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 2, 0, 1)
            };

            var instantKillRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0, 1, 0, 2)
            };
            var instantKillRadio = new RadioButton
            {
                Name = "instantKillRadio",
                Text = "Instant Kill Mode  -  manual reset only",
                AutoSize = true,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 1, 0, 0)
            };
            var learnMoreLink = new LinkLabel
            {
                Text = "Learn more...",
                AutoSize = true,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(12, 3, 0, 0)
            };
            learnMoreLink.LinkClicked += LearnMoreBtn_LinkClicked;
            instantKillRow.Controls.Add(instantKillRadio);
            instantKillRow.Controls.Add(learnMoreLink);

            radioPanel.Controls.Add(autoRestoreRadio);
            radioPanel.Controls.Add(instantKillRow);

            launchLayout.Controls.Add(radioPanel, 0, 0);
            launchLayout.ResumeLayout(false);
            launchGroup.Controls.Add(launchLayout);

            // ═════════════════════════════════════════════════════════════
            // ACTIONS section
            // ═════════════════════════════════════════════════════════════
            var actionGroup = MakeGroup("Actions");
            var actionBtnLayout = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Fill,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0, 4, 0, 4),
                Margin = new Padding(0)
            };
            actionBtnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            actionBtnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            actionBtnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            actionBtnLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var applyLaunchBtn = new Button
            {
                Text = "Apply and Launch Game",
                Dock = DockStyle.Fill,
                Height = 36,
                FlatStyle = FlatStyle.System,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            applyLaunchBtn.Click += LaunchGameBtn_Click;

            var applyOnlyBtn = new Button
            {
                Text = "Apply Only",
                Dock = DockStyle.Fill,
                Height = 36,
                FlatStyle = FlatStyle.System,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            applyOnlyBtn.Click += ApplyOnlyBtn_Click;

            var resetBtn = new Button
            {
                Text = "Reset Resolution",
                Dock = DockStyle.Fill,
                Height = 36,
                FlatStyle = FlatStyle.System,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            resetBtn.Click += ResetBtn_Click;

            actionBtnLayout.Controls.Add(applyLaunchBtn, 0, 0);
            actionBtnLayout.Controls.Add(applyOnlyBtn, 1, 0);
            actionBtnLayout.Controls.Add(resetBtn, 2, 0);
            actionGroup.Controls.Add(actionBtnLayout);

            // ═════════════════════════════════════════════════════════════
            // Assemble mainLayout rows
            // ═════════════════════════════════════════════════════════════
            mainLayout.Controls.Add(profileGroup, 0, 0);
            mainLayout.Controls.Add(monitorGroup, 0, 1);
            mainLayout.Controls.Add(resGroup, 0, 2);
            mainLayout.Controls.Add(gameGroup, 0, 3);
            mainLayout.Controls.Add(launchGroup, 0, 4);
            mainLayout.Controls.Add(actionGroup, 0, 5);

            mainLayout.ResumeLayout(false);
            scrollPanel.Controls.Add(mainLayout);
            scrollPanel.ResumeLayout(false);

            // ── Wire form together (order matters for DockStyle) ──────────
            this.Controls.Add(scrollPanel);      // Fill
            this.Controls.Add(_statusStrip);     // Bottom
            this.Controls.Add(_titlePanel);      // Top

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private GroupBox MakeGroup(string title)
        {
            return new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 6),
                Padding = new Padding(8, 10, 8, 6),
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
        }

        private TableLayoutPanel MakeTwoColLayout(int rows)
        {
            var tl = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = rows,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72f));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < rows; i++)
                tl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            return tl;
        }

        private Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                ForeColor = SystemColors.ControlText,
                Margin = new Padding(0, 3, 6, 3),
                AutoSize = false
            };
        }

        private void InitializeApplication()
        {
            try
            {
                _logger.LogInfo("Initializing ResolutionSwitcher main application");

                _configManager = new ConfigManager();
                _detectedMonitors = DisplayManager.GetMonitors();

                if (_detectedMonitors.Count == 0)
                {
                    MessageBox.Show("No monitors detected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _logger.LogError("No monitors detected on startup");
                    return;
                }

                _configManager.InitializeFromDetectedMonitors(_detectedMonitors);

                _monitorDropdown.Items.Clear();
                foreach (var monitor in _detectedMonitors)
                    _monitorDropdown.Items.Add($"{monitor.FriendlyName} ({monitor.Width}x{monitor.Height}@{monitor.RefreshRate}Hz)");
                if (_monitorDropdown.Items.Count > 0)
                    _monitorDropdown.SelectedIndex = 0;

                var primary = _detectedMonitors.FirstOrDefault(m => m.IsPrimary) ?? _detectedMonitors[0];
                _monitorDefaultLabel.Text = $"Default: {primary.Width} x {primary.Height} @ {primary.RefreshRate} Hz";

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
            _statusLabel.Text = "Launching game...";
        }

        private void ApplyOnlyBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("Apply Only clicked");
            _statusLabel.Text = "Applying resolution...";
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            _logger.LogInfo("Reset clicked");
            _statusLabel.Text = "Resetting to default resolution...";
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
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9f),
                ReadOnly = true,
                BorderStyle = BorderStyle.Fixed3D
            };

            string modeText = @"AUTO-RESTORE HELPER
===================================================================
Description:
Automatically reverts your resolution back to your original settings when the game closes.

How It Works:
1. Applies your custom resolution and launches the game
2. Helper utility monitors the game process in background
3. When game exits, resolution automatically switches back

Resource Usage:
  Memory: ~5-10 MB (minimal background process)
  CPU: <1% (kernel wait - zero CPU spinning)
  Disk: Negligible (helper runs in RAM)

Best For:
  Competitive gaming (less downtime between matches)
  Players who forget to reset resolution
  Frequent game launchers
  Minimal performance impact

Pros:
  Zero manual intervention required
  Fast workflow for multiple gaming sessions
  Reliable and automatic

Cons:
  Slight delay on game close while reverting


INSTANT KILL MODE
===================================================================
Description:
Launches your game with custom resolution. You manually reset when done.

How It Works:
1. Applies your custom resolution and launches the game
2. No background monitoring - pure launch
3. You click 'Reset Resolution' button when ready to restore original resolution

Resource Usage:
  Memory: 0 MB (no helper process)
  CPU: 0% (no monitoring)
  Disk: 0 MB

Best For:
  Single long gaming sessions
  Users who prefer full control
  Low-end systems (saves resources)
  Testing/development scenarios

Pros:
  Zero resource overhead
  Maximum user control
  Instant game launch (no monitoring setup)
  Best for low-end PCs

Cons:
  Requires manual reset
  Easy to forget resolution change";

            rtb.Text = modeText;
            modeForm.Controls.Add(rtb);
            modeForm.ShowDialog(this);
        }
    }
}
