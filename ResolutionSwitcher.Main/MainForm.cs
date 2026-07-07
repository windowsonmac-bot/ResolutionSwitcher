using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public class MainForm : Form
    {
        private const float LabelColumnWidth = 90f;
        private const string PresetSeparatorPrefix = "────";
        private const int DefaultPresetIndex = 3;
        private const string DefaultRefreshHz = "240";
        private ConfigManager? _configManager;
        private readonly List<DisplayManager.MonitorInfo> _detectedMonitors;
        private static readonly Logger _logger = Logger.Instance;
        private Panel _titlePanel = null!;
        private Panel _scrollPanel = null!;
        private Panel _statusPanel = null!;
        private Panel _profileCardPanel = null!;
        private Panel _statusSeparatorLine = null!;
        private Label _titleLabel = null!;
        private Label _statusHeaderLabel = null!;
        private Label _monitorDefaultLabel = null!;
        private Label _profileCardLine1 = null!;
        private Label _profileCardLine2 = null!;
        private RichTextBox _statusRichTextBox = null!;
        private Button _settingsButton = null!;
        private Button _aboutButton = null!;
        private Button _lightThemeButton = null!;
        private Button _darkThemeButton = null!;
        private Button _debugButton = null!;
        private Button _masterResetButton = null!;
        private Button _statusClearButton = null!;
        private ComboBox _monitorDropdown = null!;
        private ComboBox _profileDropdown = null!;
        private ComboBox _presetDropdown = null!;
        private ComboBox _hzDropdown = null!;
        private TextBox _customHzInput = null!;
        private Label _customHzLabel = null!;
        private TextBox _widthInput = null!;
        private TextBox _heightInput = null!;
        private bool _suppressPresetSync = false;
        private TextBox _gamePathInput = null!;
        private NotifyIcon _trayIcon = null!;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_RESET = 1;
        private const int HOTKEY_LAUNCH = 2;
        private const int HOTKEY_EMERGENCY = 3;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_R = 0x52;
        private const uint VK_L = 0x4C;
        private const uint VK_F12 = 0x7B;

        public MainForm()
        {
            _detectedMonitors = new List<DisplayManager.MonitorInfo>();
            SetupUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
            InitializeApplication();
        }

        private void SetupUI()
        {
            SuspendLayout();

            Text = "ResolutionSwitcher v1.0";
            ClientSize = new Size(820, 860);
            MinimumSize = new Size(630, 700);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(6F, 13F);
            Font = new Font("Tahoma", 8f);

            _titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(8, 0, 4, 0)
            };
            _titlePanel.SuspendLayout();

            _titleLabel = new Label
            {
                Text = "Resolution Manager",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 0, 0, 0)
            };

            var buttonStrip = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 7, 4, 0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            _lightThemeButton = new Button
            {
                Text = "☀ Light",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(3, 0, 0, 0)
            };
            _lightThemeButton.Click += LightThemeButton_Click;

            _darkThemeButton = new Button
            {
                Text = "🌙 Dark",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(3, 0, 0, 0)
            };
            _darkThemeButton.Click += DarkThemeButton_Click;

            _aboutButton = new Button
            {
                Text = "About",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(3, 0, 0, 0)
            };
            _aboutButton.Click += AboutBtn_Click;

            _settingsButton = new Button
            {
                Text = "Settings",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(3, 0, 0, 0)
            };
            _settingsButton.Click += SettingsBtn_Click;

            _debugButton = new Button
            {
                Text = "Debug",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(3, 0, 0, 0)
            };
            _debugButton.Click += DebugBtn_Click;

            _masterResetButton = new Button
            {
                Text = "Reset",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(3, 0, 0, 0)
            };
            _masterResetButton.Click += MasterResetBtn_Click;

            buttonStrip.Controls.Add(_lightThemeButton);
            buttonStrip.Controls.Add(_darkThemeButton);
            buttonStrip.Controls.Add(_aboutButton);
            buttonStrip.Controls.Add(_settingsButton);
            buttonStrip.Controls.Add(_debugButton);
            buttonStrip.Controls.Add(_masterResetButton);

            _titlePanel.Controls.Add(buttonStrip);
            _titlePanel.Controls.Add(_titleLabel);
            _titlePanel.ResumeLayout(false);

            _statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 176,
                MinimumSize = new Size(0, 176),
                BorderStyle = BorderStyle.Fixed3D,
                Padding = new Padding(8, 6, 8, 8)
            };

            var statusHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            _statusHeaderLabel = new Label
            {
                Text = "STATUS",
                Dock = DockStyle.Left,
                Width = 72,
                Font = new Font("Tahoma", 7.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            _statusClearButton = new Button
            {
                Text = "Clear",
                Dock = DockStyle.Right,
                Width = 48,
                Height = 20,
                Font = new Font("Tahoma", 7.5f)
            };
            _statusClearButton.Click += (s, e) => _statusRichTextBox.Clear();

            _statusSeparatorLine = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1
            };

            statusHeaderPanel.Controls.Add(_statusClearButton);
            statusHeaderPanel.Controls.Add(_statusHeaderLabel);
            statusHeaderPanel.Controls.Add(_statusSeparatorLine);

            _statusRichTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Font = new Font("Courier New", 8.5f),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false,
                WordWrap = false,
                TabStop = false,
                ShortcutsEnabled = true
            };

            _statusPanel.Controls.Add(_statusRichTextBox);
            _statusPanel.Controls.Add(statusHeaderPanel);

            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(6, 4, 6, 2)
            };
            _scrollPanel.SuspendLayout();

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
            for (int i = 0; i < 6; i++)
            {
                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            var profileGroup = MakeGroup("Profile");
            var profileLayout = MakeTwoColLayout(1);
            profileLayout.SuspendLayout();

            _profileDropdown = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Tahoma", 8f),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 180,
                Margin = new Padding(0)
            };
            _profileDropdown.Items.AddRange(new object[] { "Gaming", "Streaming", "Productivity" });
            _profileDropdown.SelectedIndex = 0;

            var newProfileBtn = new Button { Text = "+ New", Width = 58, Height = 24, Font = new Font("Tahoma", 7.5f), Margin = new Padding(4, 0, 0, 0) };
            var deleteProfileBtn = new Button { Text = "Delete", Width = 58, Height = 24, Font = new Font("Tahoma", 7.5f), Margin = new Padding(2, 0, 0, 0) };
            newProfileBtn.Click += NewProfileBtn_Click;
            deleteProfileBtn.Click += DeleteProfileBtn_Click;
            _profileDropdown.SelectedIndexChanged += ProfileDropdown_SelectedIndexChanged;

            var profileFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0, 3, 0, 3)
            };
            profileFlow.Controls.Add(_profileDropdown);
            profileFlow.Controls.Add(newProfileBtn);
            profileFlow.Controls.Add(deleteProfileBtn);

            profileLayout.Controls.Add(MakeLabel("Profile:"), 0, 0);
            profileLayout.Controls.Add(profileFlow, 1, 0);
            profileLayout.ResumeLayout(false);
            profileGroup.Controls.Add(profileLayout);

            _profileCardPanel = new Panel
            {
                Name = "_profileCardPanel",
                BorderStyle = BorderStyle.Fixed3D,
                Height = 30,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 2),
                Padding = new Padding(6, 2, 6, 2)
            };

            _profileCardLine1 = new Label
            {
                Name = "_profileCardLine1",
                Dock = DockStyle.Top,
                Height = 16,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Tahoma", 7.5f),
                Text = "Monitor: Not set   Resolution: Not set   Refresh: Not set Hz"
            };

            _profileCardLine2 = new Label
            {
                Name = "_profileCardLine2",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Font = new Font("Tahoma", 7.5f),
                Text = "Game: Not set   Launch Mode: Not set"
            };

            _profileCardPanel.Controls.Add(_profileCardLine2);
            _profileCardPanel.Controls.Add(_profileCardLine1);

            var monitorGroup = MakeGroup("Monitor");
            var monitorLayout = MakeTwoColLayout(2);
            monitorLayout.SuspendLayout();

            _monitorDropdown = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 3, 0, 3)
            };
            _monitorDefaultLabel = new Label
            {
                Text = "Detecting monitors...",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 7.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 0, 2)
            };

            monitorLayout.Controls.Add(MakeLabel("Monitor:"), 0, 0);
            monitorLayout.Controls.Add(_monitorDropdown, 1, 0);
            monitorLayout.Controls.Add(new Panel { Margin = Padding.Empty, Size = new Size(0, 0) }, 0, 1);
            monitorLayout.Controls.Add(_monitorDefaultLabel, 1, 1);
            monitorLayout.ResumeLayout(false);
            monitorGroup.Controls.Add(monitorLayout);

            var resGroup = MakeGroup("Resolution");
            var resLayout = MakeTwoColLayout(3);
            resLayout.SuspendLayout();
            resLayout.Padding = new Padding(0);

            _presetDropdown = new ComboBox
            {
                Name = "presetDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 3, 0, 3)
            };
            _presetDropdown.Items.AddRange(new object[]
            {
                $"{PresetSeparatorPrefix}──── 16:9 {PresetSeparatorPrefix}────",
                "16:9  3840x2160  (2160p / 4K)",
                "16:9  2560x1440  (1440p)",
                "16:9  1920x1080  (1080p)",
                "16:9  1600x900   (900p)",
                "16:9  1366x768   (768p laptop)",
                "16:9  1280x720   (720p)",
                "16:9  1024x576   (576p)",
                "16:9  800x450    (450p)",
                "16:9  640x360    (360p)",
                $"{PresetSeparatorPrefix}──── 16:10 {PresetSeparatorPrefix}───",
                "16:10  3840x2400  (2400p)",
                "16:10  2560x1600  (1600p)",
                "16:10  1920x1200  (1200p)",
                "16:10  1680x1050  (1050p)",
                "16:10  1440x900   (900p)",
                "16:10  1280x800   (800p)",
                "16:10  1024x640   (640p)",
                "16:10  800x500    (500p)",
                "16:10  640x400    (400p)",
                $"{PresetSeparatorPrefix}──── 4:3 {PresetSeparatorPrefix}─────",
                "4:3  2880x2160  (2160p)",
                "4:3  1920x1440  (1440p)",
                "4:3  1600x1200  (1200p)",
                "4:3  1440x1080  (1080p)",
                "4:3  1400x1050  (1050p)",
                "4:3  1280x960   (960p)",
                "4:3  1200x900   (900p)",
                "4:3  1024x768   (768p)",
                "4:3  960x720    (720p)",
                "4:3  800x600    (600p)",
                "4:3  640x480    (480p)",
                $"{PresetSeparatorPrefix}──── 5:4 {PresetSeparatorPrefix}─────",
                "5:4  2700x2160  (2160p)",
                "5:4  1800x1440  (1440p)",
                "5:4  1500x1200  (1200p)",
                "5:4  1350x1080  (1080p)",
                "5:4  1312x1050  (1050p)",
                "5:4  1280x1024  (1024p)",
                "5:4  1125x900   (900p)",
                "5:4  960x768    (768p)",
                "5:4  900x720    (720p)",
                "5:4  750x600    (600p)",
                "5:4  600x480    (480p)"
            });
            _hzDropdown = new ComboBox
            {
                Name = "hzDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 0, 4, 0)
            };
            _hzDropdown.Items.AddRange(new object[] { "24", "25", "29", "30", "48", "50", "60", "72", "75", "85", "90", "100", "120", "144", "165", "180", "200", "240", "280", "300", "360", "480", "500", "1000", "Custom..." });

            _customHzInput = new TextBox
            {
                Width = 60,
                Font = new Font("Tahoma", 8f),
                Text = "",
                Visible = false,
                Margin = new Padding(0, 0, 2, 0)
            };

            _customHzLabel = new Label
            {
                Text = "Hz",
                AutoSize = true,
                Font = new Font("Tahoma", 8f),
                Visible = false,
                Margin = new Padding(2, 5, 0, 0)
            };

            var hzPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0, 3, 0, 3)
            };
            hzPanel.Controls.Add(_hzDropdown);
            hzPanel.Controls.Add(_customHzInput);
            hzPanel.Controls.Add(_customHzLabel);

            var customFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0, 3, 0, 3)
            };
            _widthInput = new TextBox { Name = "widthInput", Text = "960", Width = 60, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f), Margin = new Padding(0) };
            _heightInput = new TextBox { Name = "heightInput", Text = "720", Width = 60, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f), Margin = new Padding(0) };

            customFlow.Controls.Add(new Label { Text = "W:", Width = 20, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f), Margin = new Padding(0) });
            customFlow.Controls.Add(_widthInput);
            customFlow.Controls.Add(new Label { Text = "H:", Width = 24, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f), Margin = new Padding(6, 0, 0, 0) });
            customFlow.Controls.Add(_heightInput);

            resLayout.Controls.Add(MakeLabel("Preset:"), 0, 0);
            resLayout.Controls.Add(_presetDropdown, 1, 0);
            resLayout.Controls.Add(MakeLabel("Refresh Hz:"), 0, 1);
            resLayout.Controls.Add(hzPanel, 1, 1);
            resLayout.Controls.Add(MakeLabel("Custom:"), 0, 2);
            resLayout.Controls.Add(customFlow, 1, 2);
            resLayout.ResumeLayout(false);
            resGroup.Controls.Add(resLayout);
            _hzDropdown.SelectedItem = DefaultRefreshHz;
            _presetDropdown.SelectedIndex = DefaultPresetIndex;
            _presetDropdown.SelectedIndexChanged += PresetDropdown_SelectedIndexChanged;
            _hzDropdown.SelectedIndexChanged += HzDropdown_SelectedIndexChanged;
            SyncCustomResolutionFromPreset(_presetDropdown.SelectedItem as string);

            var gameGroup = MakeGroup("Game");
            var gameLayout = MakeTwoColLayout(2);
            gameLayout.SuspendLayout();

            // Replace gameFlow FlowLayoutPanel with a TableLayoutPanel for proper stretching
            var gamePathLayout = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0),
                Margin = new Padding(0, 3, 0, 3)
            };
            gamePathLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));  // text box takes all space
            gamePathLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // browse button auto-sizes
            gamePathLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // steam button auto-sizes
            gamePathLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _gamePathInput = new TextBox
            {
                Name = "gamePathInput",
                Font = new Font("Tahoma", 8f),
                Dock = DockStyle.Fill,
                PlaceholderText = "Paste or browse to game .exe path...",
                Margin = new Padding(0, 2, 4, 2)
            };

            var browseGameBtn = new Button
            {
                Text = "Browse...",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(0, 2, 0, 2)
            };
            browseGameBtn.Click += BrowseGameBtn_Click;

            var scanSteamBtn = new Button
            {
                Text = "Steam...",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(4, 2, 0, 2)
            };
            scanSteamBtn.Click += ScanSteamBtn_Click;

            // Wire up path text box change to save to profile (on focus loss to avoid per-keystroke I/O)
            _gamePathInput.Leave += (s, ev) => SaveGamePathToProfile(GetSanitizedGamePath());

            gamePathLayout.Controls.Add(_gamePathInput, 0, 0);
            gamePathLayout.Controls.Add(browseGameBtn, 1, 0);
            gamePathLayout.Controls.Add(scanSteamBtn, 2, 0);

            var launchMethodDropdown = new ComboBox
            {
                Name = "launchMethodDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 3, 0, 3),
                Anchor = AnchorStyles.Left
            };
            launchMethodDropdown.Items.AddRange(new object[] { "Steam", "Steam (App ID)", "Direct EXE Path", "Custom Location" });
            launchMethodDropdown.SelectedIndex = 0;
            launchMethodDropdown.SelectedIndexChanged += (s, ev) =>
            {
                if (_configManager == null) return;
                var config = _configManager.GetConfig();
                var profile = GetActiveProfile(config);
                if (profile == null) return;
                profile.LaunchMethod = launchMethodDropdown.SelectedItem as string ?? "Steam";
                _configManager.Save();
            };

            gameLayout.Controls.Add(MakeLabel("Game:"), 0, 0);
            gameLayout.Controls.Add(gamePathLayout, 1, 0);
            gameLayout.Controls.Add(MakeLabel("Launcher:"), 0, 1);
            gameLayout.Controls.Add(launchMethodDropdown, 1, 1);
            gameLayout.ResumeLayout(false);
            gameGroup.Controls.Add(gameLayout);

            var launchGroup = MakeGroup("Launch Mode");
            var launchLayout = MakeTwoColLayout(1);
            launchLayout.SuspendLayout();

            launchLayout.ColumnStyles.Clear();
            launchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            launchLayout.ColumnCount = 1;
            launchLayout.RowCount = 1;
            launchLayout.RowStyles.Clear();
            launchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var radioPanel = new Panel
            {
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
                Location = new Point(0, 2)
            };
            var instantKillRadio = new RadioButton
            {
                Name = "instantKillRadio",
                Text = "Instant Kill Mode  -  app closes immediately, manual reset only",
                AutoSize = true,
                Font = new Font("Tahoma", 8f),
                Location = new Point(0, 24)
            };
            var learnMoreLink = new LinkLabel
            {
                Text = "Learn more about launch modes...",
                AutoSize = true,
                Font = new Font("Tahoma", 7.5f),
                Location = new Point(20, 46)
            };
            learnMoreLink.LinkClicked += LearnMoreBtn_LinkClicked;
            autoRestoreRadio.CheckedChanged += (s, ev) =>
            {
                if (!autoRestoreRadio.Checked) return;
                if (_configManager == null) return;
                var config = _configManager.GetConfig();
                var profile = GetActiveProfile(config);
                if (profile == null) return;
                profile.LaunchMode = "autoRestore";
                _configManager.Save();
            };
            instantKillRadio.CheckedChanged += (s, ev) =>
            {
                if (!instantKillRadio.Checked) return;
                if (_configManager == null) return;
                var config = _configManager.GetConfig();
                var profile = GetActiveProfile(config);
                if (profile == null) return;
                profile.LaunchMode = "instantKill";
                _configManager.Save();
            };

            radioPanel.Controls.Add(autoRestoreRadio);
            radioPanel.Controls.Add(instantKillRadio);
            radioPanel.Controls.Add(learnMoreLink);
            radioPanel.Height = 64;

            launchLayout.Controls.Add(radioPanel, 0, 0);
            launchLayout.ResumeLayout(false);
            launchGroup.Controls.Add(launchLayout);

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
                Height = 28,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            applyLaunchBtn.Click += LaunchGameBtn_Click;

            var applyOnlyBtn = new Button
            {
                Text = "Apply Only",
                Dock = DockStyle.Fill,
                Height = 28,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            applyOnlyBtn.Click += ApplyOnlyBtn_Click;

            var resetBtn = new Button
            {
                Text = "Reset Resolution",
                Dock = DockStyle.Fill,
                Height = 28,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            resetBtn.Click += ResetBtn_Click;

            actionBtnLayout.Controls.Add(applyLaunchBtn, 0, 0);
            actionBtnLayout.Controls.Add(applyOnlyBtn, 1, 0);
            actionBtnLayout.Controls.Add(resetBtn, 2, 0);
            actionGroup.Controls.Add(actionBtnLayout);

            mainLayout.Controls.Add(actionGroup, 0, 0);
            mainLayout.Controls.Add(profileGroup, 0, 1);
            mainLayout.Controls.Add(monitorGroup, 0, 2);
            mainLayout.Controls.Add(resGroup, 0, 3);
            mainLayout.Controls.Add(gameGroup, 0, 4);
            mainLayout.Controls.Add(launchGroup, 0, 5);

            mainLayout.ResumeLayout(false);
            _scrollPanel.Controls.Add(mainLayout);
            _scrollPanel.ResumeLayout(false);

            Controls.Add(_scrollPanel);
            Controls.Add(_statusPanel);
            Controls.Add(_titlePanel);

            _trayIcon = new NotifyIcon
            {
                Text = "ResolutionSwitcher",
                Icon = SystemIcons.Application,
                Visible = false
            };

            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, (s, ev) => RestoreFromTray());
            trayMenu.Items.Add("Reset Resolution", null, (s, ev) => ResetBtn_Click(null, EventArgs.Empty));
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (s, ev) => Application.Exit());
            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.DoubleClick += (s, ev) => RestoreFromTray();

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKey(Handle, HOTKEY_RESET);
            UnregisterHotKey(Handle, HOTKEY_LAUNCH);
            UnregisterHotKey(Handle, HOTKEY_EMERGENCY);
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            _trayIcon?.Dispose();
            base.OnFormClosed(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(Handle, HOTKEY_RESET, MOD_CONTROL | MOD_ALT, VK_R);
            RegisterHotKey(Handle, HOTKEY_LAUNCH, MOD_CONTROL | MOD_ALT, VK_L);
            RegisterHotKey(Handle, HOTKEY_EMERGENCY, MOD_CONTROL | MOD_ALT, VK_F12);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case HOTKEY_RESET:
                        ResetBtn_Click(null, EventArgs.Empty);
                        break;
                    case HOTKEY_LAUNCH:
                        LaunchGameBtn_Click(null, EventArgs.Empty);
                        break;
                    case HOTKEY_EMERGENCY:
                        ResetBtn_Click(null, EventArgs.Empty);
                        AppendStatus("⚡ Emergency Reset triggered via hotkey");
                        break;
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                _trayIcon.Visible = true;
                _trayIcon.ShowBalloonTip(1500, "ResolutionSwitcher", "Running in background. Double-click to restore.", ToolTipIcon.Info);
            }
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            _trayIcon.Visible = false;
        }

        private bool SpawnMonitorHelper(int gamePid, string deviceName, uint width, uint height, uint refreshRate)
        {
            try
            {
                var helperPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "ResolutionSwitcher.Monitor.exe");

                if (!System.IO.File.Exists(helperPath))
                {
                    _logger.LogError($"Monitor helper not found at: {helperPath}");
                    return false;
                }

                var args = $"{gamePid} \"{deviceName}\" {width} {height} {refreshRate}";

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = helperPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                var helperProcess = System.Diagnostics.Process.Start(startInfo);
                if (helperProcess == null)
                {
                    _logger.LogError("Failed to start monitor helper process");
                    return false;
                }

                _logger.LogInfo($"Monitor helper started (PID: {helperProcess.Id}) watching game PID {gamePid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error spawning monitor helper", ex);
                return false;
            }
        }

        private void ScanSteamBtn_Click(object? sender, EventArgs e)
        {
            AppendStatus("Scanning Steam library...");
            _logger.LogInfo("Steam scan initiated");

            List<SteamGame> games;
            try
            {
                games = SteamScanner.GetInstalledGames();
            }
            catch (Exception ex)
            {
                AppendStatus($"✗ Steam scan failed: {ex.Message}");
                return;
            }

            if (games.Count == 0)
            {
                AppendStatus("No Steam games found. Is Steam installed?");
                MessageBox.Show("No Steam games found. Make sure Steam is installed and you have games installed.", "Steam Scan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AppendStatus($"Found {games.Count} Steam games.");

            // Show picker dialog
            using var picker = new Form
            {
                Text = $"Select Steam Game ({games.Count} found)",
                Width = 520,
                Height = 420,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                Font = new Font("Tahoma", 8f),
                MinimumSize = new Size(400, 300)
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9f),
                IntegralHeight = false
            };
            foreach (var g in games)
                listBox.Items.Add(g.Name);
            if (listBox.Items.Count > 0) listBox.SelectedIndex = 0;

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 36,
                Padding = new Padding(4)
            };
            var selectBtn = new Button { Text = "Select", Width = 80, Height = 26, DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Cancel", Width = 80, Height = 26, DialogResult = DialogResult.Cancel };
            btnPanel.Controls.Add(selectBtn);
            btnPanel.Controls.Add(cancelBtn);
            picker.Controls.Add(listBox);
            picker.Controls.Add(btnPanel);
            picker.AcceptButton = selectBtn;
            picker.CancelButton = cancelBtn;
            listBox.DoubleClick += (s, ev) => { picker.DialogResult = DialogResult.OK; picker.Close(); };

            if (picker.ShowDialog(this) != DialogResult.OK) return;
            if (listBox.SelectedIndex < 0) return;

            var selected = games[listBox.SelectedIndex];
            var path = selected.ExePath;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                AppendStatus($"✗ Could not find executable for {selected.Name}. Browse manually.");
                MessageBox.Show($"Could not automatically find the executable for {selected.Name}.\n\nInstall directory:\n{selected.InstallDir}\n\nPlease use Browse to locate the .exe manually.", "EXE Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _gamePathInput.Text = path;
            SaveGamePathToProfile(path);
            AppendStatus($"✓ Game set from Steam: {selected.Name}");
            _logger.LogInfo($"Steam game selected: {selected.Name} -> {path}");
        }

        private GroupBox MakeGroup(string title)
        {
            return new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 2),
                Padding = new Padding(6, 12, 6, 2),
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
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumnWidth));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < rows; i++)
            {
                tl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

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
                Margin = new Padding(0, 2, 6, 2),
                AutoSize = false
            };
        }

        private void PresetDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_presetDropdown.SelectedItem is not string selectedItem)
            {
                return;
            }

            if (selectedItem.StartsWith(PresetSeparatorPrefix, StringComparison.Ordinal))
            {
                var nextIndex = _presetDropdown.SelectedIndex + 1;
                while (nextIndex < _presetDropdown.Items.Count
                    && _presetDropdown.Items[nextIndex] is string nextItem
                    && nextItem.StartsWith(PresetSeparatorPrefix, StringComparison.Ordinal))
                {
                    nextIndex++;
                }

                if (nextIndex < _presetDropdown.Items.Count)
                {
                    _presetDropdown.SelectedIndex = nextIndex;
                    return;
                }

                var previousIndex = _presetDropdown.SelectedIndex - 1;
                while (previousIndex >= 0
                    && _presetDropdown.Items[previousIndex] is string previousItem
                    && previousItem.StartsWith(PresetSeparatorPrefix, StringComparison.Ordinal))
                {
                    previousIndex--;
                }

                if (previousIndex >= 0)
                {
                    _presetDropdown.SelectedIndex = previousIndex;
                }

                return;
            }

            SyncCustomResolutionFromPreset(selectedItem);
        }

        private void SyncCustomResolutionFromPreset(string? selectedItem)
        {
            if (_suppressPresetSync) return;
            if (string.IsNullOrWhiteSpace(selectedItem)
                || selectedItem.StartsWith(PresetSeparatorPrefix, StringComparison.Ordinal))
            {
                return;
            }

            var resolutionToken = selectedItem
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(token => token.Contains('x'));

            if (string.IsNullOrWhiteSpace(resolutionToken))
            {
                return;
            }

            var parts = resolutionToken.Split('x');
            if (parts.Length != 2)
            {
                return;
            }

            _widthInput.Text = parts[0];
            _heightInput.Text = parts[1];
        }

        private void HzDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var isCustom = _hzDropdown.SelectedItem as string == "Custom...";
            _customHzInput.Visible = isCustom;
            _customHzLabel.Visible = isCustom;
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
            _scrollPanel.BackColor = theme.FormBackground;
            _titlePanel.BackColor = theme.TitleBarColor;
            _titleLabel.ForeColor = theme.TitleBarTextColor;
            _statusPanel.BackColor = theme.StatusBackground;
            _statusHeaderLabel.ForeColor = theme.StatusHeaderColor;
            _statusRichTextBox.BackColor = theme.StatusBackground;
            _statusRichTextBox.ForeColor = theme.TextColor;
            _monitorDefaultLabel.ForeColor = theme.GrayTextColor;
            _profileCardPanel.BackColor = theme.SectionBackground;
            _profileCardLine1.BackColor = theme.SectionBackground;
            _profileCardLine1.ForeColor = theme.TextColor;
            _profileCardLine2.BackColor = theme.SectionBackground;
            _profileCardLine2.ForeColor = theme.TextColor;
            _statusSeparatorLine.BackColor = BlendColors(theme.StatusHeaderColor, theme.StatusBackground);

            ThemeManager.ApplyButtonStyle(_lightThemeButton);
            ThemeManager.ApplyButtonStyle(_darkThemeButton);
            ThemeManager.ApplyButtonStyle(_aboutButton);
            ThemeManager.ApplyButtonStyle(_settingsButton);
            ThemeManager.ApplyButtonStyle(_debugButton);
            ThemeManager.ApplyButtonStyle(_masterResetButton);
            ThemeManager.ApplyButtonStyle(_statusClearButton);

            ApplyThemeToControls(_scrollPanel, false);
            Invalidate(true);
        }

        private void ApplyThemeToControls(Control parent, bool insideGroupBox)
        {
            var theme = ThemeManager.Palette;

            foreach (Control child in parent.Controls)
            {
                if (ReferenceEquals(child, _titlePanel) || ReferenceEquals(child, _statusPanel) || ReferenceEquals(child, _profileCardPanel))
                {
                    continue;
                }

                var childInsideGroupBox = insideGroupBox || child is GroupBox || child.Parent is GroupBox;

                if (child is GroupBox groupBox)
                {
                    groupBox.ForeColor = theme.TextColor;
                    if (groupBox.HasChildren)
                    {
                        ApplyThemeToControls(groupBox, true);
                    }
                    continue;
                }

                if (child is Panel || child is TableLayoutPanel || child is FlowLayoutPanel)
                {
                    child.BackColor = childInsideGroupBox ? theme.SectionBackground : theme.FormBackground;
                    child.ForeColor = theme.TextColor;
                }
                else if (child is Label label)
                {
                    label.ForeColor = theme.TextColor;
                    label.BackColor = childInsideGroupBox ? theme.SectionBackground : theme.FormBackground;
                }
                else if (child is LinkLabel linkLabel)
                {
                    linkLabel.ForeColor = theme.TextColor;
                    linkLabel.BackColor = childInsideGroupBox ? theme.SectionBackground : theme.FormBackground;
                    linkLabel.LinkColor = ThemeManager.CurrentTheme == AppTheme.Dark ? theme.StatusHeaderColor : theme.TitleBarColor;
                    linkLabel.ActiveLinkColor = ControlPaint.Light(linkLabel.LinkColor);
                    linkLabel.VisitedLinkColor = linkLabel.LinkColor;
                }
                else if (child is RadioButton radioButton)
                {
                    radioButton.ForeColor = theme.TextColor;
                    radioButton.BackColor = theme.SectionBackground;
                }
                else if (child is ComboBox comboBox)
                {
                    comboBox.BackColor = theme.InputBackground;
                    comboBox.ForeColor = theme.TextColor;
                }
                else if (child is TextBox textBox)
                {
                    textBox.BackColor = theme.InputBackground;
                    textBox.ForeColor = theme.TextColor;
                }
                else if (child is RichTextBox richTextBox)
                {
                    richTextBox.BackColor = childInsideGroupBox ? theme.SectionBackground : theme.FormBackground;
                    richTextBox.ForeColor = theme.TextColor;
                }
                else if (child is Button button)
                {
                    ThemeManager.ApplyButtonStyle(button);
                }

                if (child.HasChildren)
                {
                    ApplyThemeToControls(child, childInsideGroupBox);
                }
            }
        }

        private void AppendStatus(string message)
        {
            if (_statusRichTextBox.TextLength > 0)
            {
                _statusRichTextBox.AppendText(Environment.NewLine);
            }

            _statusRichTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}");
            _statusRichTextBox.SelectionStart = _statusRichTextBox.TextLength;
            _statusRichTextBox.ScrollToCaret();
        }

        private void InitializeApplication()
        {
            try
            {
                _logger.LogInfo("Initializing ResolutionSwitcher main application");

                _configManager = new ConfigManager();
                _detectedMonitors.Clear();
                _detectedMonitors.AddRange(DisplayManager.GetMonitors());

                if (_detectedMonitors.Count == 0)
                {
                    AppendStatus("No monitors detected.");
                    MessageBox.Show("No monitors detected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _logger.LogError("No monitors detected on startup");
                    return;
                }

                _configManager.InitializeFromDetectedMonitors(_detectedMonitors);

                _monitorDropdown.Items.Clear();
                foreach (var monitor in _detectedMonitors)
                {
                    _monitorDropdown.Items.Add($"{monitor.FriendlyName} ({monitor.Width}x{monitor.Height}@{monitor.RefreshRate}Hz)");
                }

                if (_monitorDropdown.Items.Count > 0)
                {
                    _monitorDropdown.SelectedIndex = 0;
                }

                var primary = _detectedMonitors.FirstOrDefault(m => m.IsPrimary) ?? _detectedMonitors[0];
                _monitorDefaultLabel.Text = $"Default: {primary.Width} x {primary.Height} @ {primary.RefreshRate} Hz";

                AppendStatus($"Detected {_detectedMonitors.Count} monitor(s).");
                LoadProfilesIntoDropdown();
                AppendStatus("Ready.");
                _logger.LogSuccess("Main application initialized successfully");
            }
            catch (Exception ex)
            {
                AppendStatus($"Initialization error: {ex.Message}");
                _logger.LogError("Error initializing application", ex);
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LightThemeButton_Click(object? sender, EventArgs e)
        {
            ThemeManager.SetTheme(AppTheme.Light);
            AppendStatus("Theme set to Light mode.");
        }

        private void DarkThemeButton_Click(object? sender, EventArgs e)
        {
            ThemeManager.SetTheme(AppTheme.Dark);
            AppendStatus("Theme set to Dark mode.");
        }

        private void LaunchGameBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                var monitor = GetSelectedMonitor();
                if (monitor == null)
                {
                    AppendStatus("No monitor selected.");
                    return;
                }

                var (width, height, hz) = GetSelectedResolution();

                if (_configManager == null)
                {
                    AppendStatus("Config not loaded.");
                    return;
                }

                // Read path directly from the text box first (handles pasted paths that haven't been saved yet)
                SaveGamePathToProfile(GetSanitizedGamePath());

                var config = _configManager.GetConfig();
                var profile = GetActiveProfile(config);

                if (profile == null || string.IsNullOrWhiteSpace(profile.LaunchPath))
                {
                    AppendStatus("No game configured. Paste or browse to a game .exe path.");
                    return;
                }

                AppendStatus($"Applying {width}x{height}@{hz}Hz...");
                bool resOk = DisplayManager.ChangeResolution(monitor.DeviceName, width, height, hz);
                if (!resOk)
                {
                    AppendStatus("✗ Failed to apply resolution. Launch cancelled.");
                    return;
                }
                AppendStatus($"✓ Resolution applied: {width}x{height}@{hz}Hz");

                var launchMethod = profile.LaunchMethod switch
                {
                    "Steam (App ID)" => GameLauncher.LaunchMethod.SteamAppId,
                    "Direct EXE Path" => GameLauncher.LaunchMethod.DirectEXE,
                    "Custom Location" => GameLauncher.LaunchMethod.Custom,
                    _ => GameLauncher.LaunchMethod.DirectEXE
                };

                AppendStatus($"Launching {profile.GameName}...");
                _logger.LogInfo($"Apply and Launch: {width}x{height}@{hz}Hz on {monitor.DeviceName}, game: {profile.LaunchPath}");
                int pid = GameLauncher.LaunchGame(launchMethod, profile.LaunchPath);

                if (pid > 0)
                {
                    AppendStatus($"✓ Game launched (PID: {pid})");
                    SaveCurrentProfileSettings();

                    var autoRestoreRadio = _scrollPanel.Controls.Find("autoRestoreRadio", true).FirstOrDefault() as RadioButton;
                    bool isAutoRestore = autoRestoreRadio?.Checked == true;

                    if (isAutoRestore)
                    {
                        // Auto-Restore Mode: spawn the helper exe then exit completely
                        var monitorConfig = _configManager?.GetConfig().Monitors
                            .FirstOrDefault(m => m.DeviceName == monitor.DeviceName);
                        if (monitorConfig != null)
                        {
                            var def = monitorConfig.DefaultResolution;
                            bool helperStarted = SpawnMonitorHelper(pid, monitor.DeviceName, def.Width, def.Height, def.RefreshRate);
                            if (helperStarted)
                            {
                                AppendStatus($"✓ Auto-Restore helper launched. Main app closing now.");
                                AppendStatus($"  Helper will revert to {def.Width}x{def.Height}@{def.RefreshRate}Hz when game closes.");
                                System.Threading.Thread.Sleep(800);
                                Application.Exit();
                            }
                            else
                            {
                                AppendStatus("⚠ Helper exe not found. Auto-restore will not work. Place ResolutionSwitcher.Monitor.exe in the same folder.");
                            }
                        }
                        else
                        {
                            AppendStatus("⚠ No default resolution saved for this monitor. Cannot start helper.");
                        }
                    }
                    else
                    {
                        // Instant Kill Mode: exit immediately, nothing runs
                        AppendStatus($"✓ Instant Kill Mode: closing now. Use Reset Resolution to revert when done gaming.");
                        System.Threading.Thread.Sleep(600);
                        Application.Exit();
                    }
                }
                else
                {
                    AppendStatus("✗ Game launch failed.");
                }
            }
            catch (InvalidOperationException ex)
            {
                AppendStatus($"✗ {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendStatus($"✗ Error: {ex.Message}");
                _logger.LogError("LaunchGame error", ex);
            }
        }

        private void ApplyOnlyBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                var monitor = GetSelectedMonitor();
                if (monitor == null)
                {
                    AppendStatus("No monitor selected.");
                    return;
                }

                var (width, height, hz) = GetSelectedResolution();
                AppendStatus($"Applying {width}x{height}@{hz}Hz on {monitor.FriendlyName}...");
                _logger.LogInfo($"Apply Only: {width}x{height}@{hz}Hz on {monitor.DeviceName}");

                bool success = DisplayManager.ChangeResolution(monitor.DeviceName, width, height, hz);
                if (success)
                {
                    AppendStatus($"✓ Resolution applied: {width}x{height}@{hz}Hz");
                    SaveCurrentProfileSettings();
                }
                else
                {
                    AppendStatus("✗ Failed to apply resolution. Check that the resolution is supported by your monitor.");
                }
            }
            catch (InvalidOperationException ex)
            {
                AppendStatus($"✗ {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendStatus($"✗ Error: {ex.Message}");
                _logger.LogError("ApplyOnly error", ex);
            }
        }

        private void ResetBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                var monitor = GetSelectedMonitor();
                if (monitor == null)
                {
                    AppendStatus("No monitor selected.");
                    return;
                }

                if (_configManager == null)
                {
                    AppendStatus("Config not loaded.");
                    return;
                }

                var config = _configManager.GetConfig();
                var monitorConfig = config.Monitors.FirstOrDefault(m => m.DeviceName == monitor.DeviceName);

                if (monitorConfig == null)
                {
                    AppendStatus("No saved default resolution for this monitor.");
                    return;
                }

                var def = monitorConfig.DefaultResolution;
                AppendStatus($"Resetting to {def.Width}x{def.Height}@{def.RefreshRate}Hz...");
                _logger.LogInfo($"Reset: {def.Width}x{def.Height}@{def.RefreshRate}Hz on {monitor.DeviceName}");

                bool success = DisplayManager.ChangeResolution(monitor.DeviceName, def.Width, def.Height, def.RefreshRate);
                if (success)
                    AppendStatus($"✓ Reset to default: {def.Width}x{def.Height}@{def.RefreshRate}Hz");
                else
                    AppendStatus("✗ Failed to reset resolution.");
            }
            catch (Exception ex)
            {
                AppendStatus($"✗ Error: {ex.Message}");
                _logger.LogError("Reset error", ex);
            }
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
                var path = openFileDialog.FileName;
                var gameName = Path.GetFileNameWithoutExtension(path);

                _gamePathInput.Text = path;

                if (_configManager != null)
                {
                    var config = _configManager.GetConfig();
                    var profile = GetActiveProfile(config);
                    if (profile != null)
                    {
                        profile.GameName = gameName;
                        profile.LaunchPath = path;

                        var launchMethodDropdown = _scrollPanel.Controls.Find("launchMethodDropdown", true).FirstOrDefault() as ComboBox;
                        profile.LaunchMethod = launchMethodDropdown?.SelectedItem as string ?? "Direct EXE Path";

                        _configManager.Save();
                    }
                }

                AppendStatus($"✓ Game set: {gameName}");
                _logger.LogInfo($"Game selected: {path}");
            }
        }

        private void SettingsBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Settings clicked");
            using var settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
        }

        private void DebugBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Debug clicked");
            using var debugForm = new DebugForm();
            debugForm.ShowDialog(this);
        }

        private void MasterResetBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Master Reset clicked");
            using var masterResetForm = new MasterResetForm();
            masterResetForm.ShowDialog(this);
        }

        private void AboutBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("About clicked");
            using var aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }

        private DisplayManager.MonitorInfo? GetSelectedMonitor()
        {
            var idx = _monitorDropdown.SelectedIndex;
            if (idx < 0 || idx >= _detectedMonitors.Count) return null;
            return _detectedMonitors[idx];
        }

        private string GetSanitizedGamePath()
            => _gamePathInput?.Text.Trim().Trim('"') ?? string.Empty;

        private void SaveGamePathToProfile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || _configManager == null) return;
            var config = _configManager.GetConfig();
            var profile = GetActiveProfile(config);
            if (profile != null)
            {
                profile.GameName = Path.GetFileNameWithoutExtension(path);
                profile.LaunchPath = path;
                _configManager.Save();
            }
        }

        private (uint width, uint height, uint hz) GetSelectedResolution()
        {
            uint width = 0, height = 0, hz = 60;

            var wText = _widthInput.Text.Trim();
            var hText = _heightInput.Text.Trim();

            if (string.IsNullOrEmpty(wText) || string.IsNullOrEmpty(hText))
            {
                // Fall back to selected preset
                if (_presetDropdown.SelectedItem is string presetStr &&
                    !presetStr.StartsWith(PresetSeparatorPrefix, StringComparison.Ordinal))
                {
                    var token = presetStr
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault(t => t.Contains('x'));
                    if (token != null)
                    {
                        var parts = token.Split('x');
                        if (parts.Length == 2)
                        {
                            uint.TryParse(parts[0], out width);
                            uint.TryParse(parts[1], out height);
                        }
                    }
                }
                if (width == 0 || height == 0)
                    throw new InvalidOperationException("No resolution selected. Choose a preset or enter W/H values.");
            }
            else
            {
                if (!uint.TryParse(wText, out width) || width == 0)
                    throw new InvalidOperationException("Invalid width value.");
                if (!uint.TryParse(hText, out height) || height == 0)
                    throw new InvalidOperationException("Invalid height value.");
            }

            var hzText = _hzDropdown.SelectedItem as string;
            if (hzText == "Custom...")
            {
                if (!uint.TryParse(_customHzInput.Text.Trim(), out hz) || hz == 0)
                    throw new InvalidOperationException("Invalid custom Hz value.");
            }
            else
            {
                if (!uint.TryParse(hzText, out hz) || hz == 0)
                    hz = 60;
            }

            return (width, height, hz);
        }

        private ConfigManager.GameProfile? GetActiveProfile(ConfigManager.Config config)
        {
            var name = _profileDropdown.SelectedItem as string;
            if (string.IsNullOrEmpty(name)) return null;
            return config.Profiles.FirstOrDefault(p => p.Name == name);
        }

        private void LoadProfilesIntoDropdown()
        {
            if (_configManager == null) return;
            var config = _configManager.GetConfig();

            _profileDropdown.SelectedIndexChanged -= ProfileDropdown_SelectedIndexChanged;
            _profileDropdown.Items.Clear();
            foreach (var profile in config.Profiles)
            {
                _profileDropdown.Items.Add(profile.Name);
            }

            if (_profileDropdown.Items.Count == 0)
            {
                _profileDropdown.Items.AddRange(new object[] { "Gaming", "Streaming", "Productivity" });
                foreach (string name in new[] { "Gaming", "Streaming", "Productivity" })
                {
                    if (!config.Profiles.Any(p => p.Name == name))
                        config.Profiles.Add(new ConfigManager.GameProfile { Name = name });
                }
                _configManager.Save();
            }

            _suppressPresetSync = true;
            try
            {
                if (_profileDropdown.Items.Count > 0)
                    _profileDropdown.SelectedIndex = 0;
            }
            finally
            {
                _suppressPresetSync = false;
            }

            _profileDropdown.SelectedIndexChanged += ProfileDropdown_SelectedIndexChanged;
        }

        private void SaveCurrentProfileSettings()
        {
            if (_configManager == null) return;
            var config = _configManager.GetConfig();
            var profile = GetActiveProfile(config);
            if (profile == null) return;

            var monitor = GetSelectedMonitor();
            if (monitor != null)
                profile.TargetMonitorId = monitor.Id;

            try
            {
                var (width, height, hz) = GetSelectedResolution();
                profile.TargetResolution = new ConfigManager.ResolutionConfig
                {
                    Width = width,
                    Height = height,
                    RefreshRate = hz
                };
            }
            catch (InvalidOperationException) { /* ignore parse errors during save */ }
            catch (Exception ex) { _logger.LogError("SaveCurrentProfileSettings error", ex); }

            _configManager.Save();
        }

        private void NewProfileBtn_Click(object? sender, EventArgs e)
        {
            using var inputForm = new Form
            {
                Text = "New Profile",
                Width = 420,
                Height = 210,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Font = new Font("Tahoma", 10f)
            };
            var lbl = new Label { Text = "Profile name:", Left = 16, Top = 24, Width = 116, Height = 28, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Tahoma", 10f) };
            var txt = new TextBox { Left = 136, Top = 22, Width = 248, Height = 28, Font = new Font("Tahoma", 10f), Text = "My Profile" };
            var okBtn = new Button { Text = "OK", Left = 162, Top = 90, Width = 100, Height = 38, DialogResult = DialogResult.OK, Font = new Font("Tahoma", 10f) };
            var cancelBtn = new Button { Text = "Cancel", Left = 272, Top = 90, Width = 100, Height = 38, DialogResult = DialogResult.Cancel, Font = new Font("Tahoma", 10f) };
            inputForm.Controls.AddRange(new Control[] { lbl, txt, okBtn, cancelBtn });
            inputForm.AcceptButton = okBtn;
            inputForm.CancelButton = cancelBtn;

            if (inputForm.ShowDialog(this) != DialogResult.OK) return;

            var name = txt.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            if (_configManager == null) return;
            var config = _configManager.GetConfig();

            if (config.Profiles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                AppendStatus($"Profile '{name}' already exists.");
                return;
            }

            config.Profiles.Add(new ConfigManager.GameProfile { Name = name });
            _configManager.Save();

            _profileDropdown.Items.Add(name);
            _profileDropdown.SelectedItem = name;
            AppendStatus($"✓ Profile '{name}' created.");
        }

        private void DeleteProfileBtn_Click(object? sender, EventArgs e)
        {
            var name = _profileDropdown.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(name)) return;

            var result = MessageBox.Show($"Delete profile '{name}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            if (_configManager == null) return;
            var config = _configManager.GetConfig();
            var profile = config.Profiles.FirstOrDefault(p => p.Name == name);
            if (profile != null) config.Profiles.Remove(profile);
            _configManager.Save();

            _profileDropdown.Items.Remove(name);
            if (_profileDropdown.Items.Count > 0)
                _profileDropdown.SelectedIndex = 0;

            AppendStatus($"Profile '{name}' deleted.");
        }

        private void ProfileDropdown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_configManager == null) return;
            var config = _configManager.GetConfig();
            var profile = GetActiveProfile(config);
            if (profile == null) return;

            _suppressPresetSync = true;
            try
            {
                if (profile.TargetResolution != null && profile.TargetResolution.Width > 0)
                {
                    _widthInput.Text = profile.TargetResolution.Width.ToString();
                    _heightInput.Text = profile.TargetResolution.Height.ToString();
                    var hzStr = profile.TargetResolution.RefreshRate.ToString();
                    if (_hzDropdown.Items.Contains(hzStr))
                        _hzDropdown.SelectedItem = hzStr;
                }
                else
                {
                    // No saved resolution — clear W/H so user starts fresh
                    _widthInput.Text = "";
                    _heightInput.Text = "";
                }

                if (!string.IsNullOrEmpty(profile.LaunchPath))
                {
                    _gamePathInput.Text = profile.LaunchPath;
                }
                else if (!string.IsNullOrEmpty(profile.GameName))
                {
                    _gamePathInput.Text = profile.GameName;
                }
                else
                {
                    _gamePathInput.Text = "";
                }

                if (!string.IsNullOrEmpty(profile.LaunchMethod))
                {
                    var launchMethodDropdown = _scrollPanel.Controls.Find("launchMethodDropdown", true).FirstOrDefault() as ComboBox;
                    if (launchMethodDropdown != null && launchMethodDropdown.Items.Contains(profile.LaunchMethod))
                        launchMethodDropdown.SelectedItem = profile.LaunchMethod;
                }

                var autoRestoreRadio = _scrollPanel.Controls.Find("autoRestoreRadio", true).FirstOrDefault() as RadioButton;
                var instantKillRadio = _scrollPanel.Controls.Find("instantKillRadio", true).FirstOrDefault() as RadioButton;
                if (autoRestoreRadio != null && instantKillRadio != null)
                {
                    if (profile.LaunchMode == "instantKill")
                        instantKillRadio.Checked = true;
                    else
                        autoRestoreRadio.Checked = true;
                }
            }
            finally
            {
                _suppressPresetSync = false;
            }
        }

        private void LearnMoreBtn_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            _logger.LogInfo("Learn More clicked");
            var modeForm = new Form
            {
                Text = "Launch Modes - Detailed Comparison",
                Width = 820,
                Height = 620,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                MinimizeBox = false,
                MinimumSize = new Size(600, 480),
                BackColor = ThemeManager.Palette.FormBackground,
                Font = new Font("Tahoma", 8f)
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                BackColor = ThemeManager.Palette.SectionBackground,
                ForeColor = ThemeManager.Palette.TextColor,
                WordWrap = true
            };

            var modeText = @"AUTO-RESTORE HELPER MODE
===================================================================
How It Works:
1. Applies your custom resolution
2. Launches the game
3. Spawns ResolutionSwitcher.Monitor.exe with ONLY the game's PID,
   device name, and target resolution as arguments
4. Main app exits COMPLETELY - no tray icon, no background service
5. Monitor helper uses OpenProcess(SYNCHRONIZE) to watch the game
6. WaitForSingleObject(INFINITE) — thread is kernel-parked at 0% CPU
7. Windows wakes the helper when the game process exits
8. Helper calls ChangeDisplaySettingsEx to revert your resolution
9. Helper exits — nothing running at all

Helper Resource Usage:
  Memory: ~4-6 MB (minimal .NET runtime + tiny code)
  CPU: 0.00% — thread is fully suspended by the kernel, not scheduled
  No polling, no timers, no interrupts until game exits

Anticheat Safety (SYNCHRONIZE access right):
  The helper calls OpenProcess(SYNCHRONIZE, false, pid)
  SYNCHRONIZE is the minimum possible access right
  It does NOT allow: memory read, memory write, module enumeration,
  thread injection, DLL manipulation, or any intrusive operations
  This is the SAME right used by Task Manager, Process Explorer,
  and Windows itself to watch process state
  Vanguard, VAC, FACEIT, and 5E Arena all explicitly allow SYNCHRONIZE
  It is fundamentally different from PROCESS_VM_READ (banned by anticheat)

Finding the helper in Task Manager:
  If you ever need to close it manually:
  Open Task Manager → Details tab → find ResolutionSwitcher.Monitor.exe
  Right-click → End Task
  Your resolution will NOT auto-revert but you can use Reset Resolution
  in the main app to restore it manually

===================================================================

INSTANT KILL MODE
===================================================================
How It Works:
1. Applies your custom resolution
2. Launches the game
3. Confirms game started (PID > 0)
4. Main app calls Application.Exit() — completely gone
   No tray icon. No background process. Nothing running at all.
5. When you finish gaming, reopen the main app
6. Click Reset Resolution to restore your original resolution

Resource Usage (while gaming):
  Memory: 0 MB
  CPU: 0%
  Nothing is running

Manual Reset:
  Open ResolutionSwitcher.exe → click Reset Resolution
  Or use the hotkey Ctrl+Alt+R if the app is open

===================================================================

STARTUP REGISTRY (Settings > Startup)
===================================================================
What it does:
  Adds ONE registry value to:
  HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
  Value name: ResolutionSwitcher
  Value: path to ResolutionSwitcher.exe

What it does NOT do:
  Does not install a service
  Does not add kernel drivers
  Does not create scheduled tasks
  Does not modify any system files

How to remove it:
  Uncheck the ""Launch on startup"" box in Settings
  The key is deleted IMMEDIATELY and confirmed in a dialog
  OR use Master Reset → it also removes the registry entry
  OR manually delete the value in regedit

===================================================================

ANTICHEAT COMPATIBILITY
===================================================================
Safe for: Valorant (Vanguard), CS2 (VAC), FACEIT, 5E Arena, ESEA

What this app uses:
  ✓ ChangeDisplaySettingsEx — standard Windows display API
  ✓ OpenProcess(SYNCHRONIZE) — same as Task Manager
  ✓ WaitForSingleObject — standard kernel wait

What this app does NOT do:
  ✗ No memory reading (PROCESS_VM_READ)
  ✗ No memory writing (PROCESS_VM_WRITE)
  ✗ No code injection
  ✗ No DLL hooking or injection
  ✗ No kernel drivers
  ✗ No debug access (PROCESS_ALL_ACCESS)
  ✗ No game file modification
  ✗ No network communication
  ✗ No process scanning or enumeration

Note: Anticheat software evolves. While SYNCHRONIZE is universally
safe as of 2024-2025, always check the latest anticheat policies.";

            rtb.Text = modeText;
            modeForm.Controls.Add(rtb);
            modeForm.ShowDialog(this);
        }

        private static Color BlendColors(Color top, Color bottom)
        {
            return Color.FromArgb(
                (top.R + bottom.R) / 2,
                (top.G + bottom.G) / 2,
                (top.B + bottom.B) / 2);
        }
    }
}
