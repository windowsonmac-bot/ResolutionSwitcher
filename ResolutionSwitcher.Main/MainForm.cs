using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public class MainForm : Form
    {
        private const float LabelColumnWidth = 90f;
        private ConfigManager? _configManager;
        private readonly List<DisplayManager.MonitorInfo> _detectedMonitors;
        private static readonly Logger _logger = Logger.Instance;
        private Panel _titlePanel = null!;
        private Panel _scrollPanel = null!;
        private Panel _statusPanel = null!;
        private Panel _profileCardPanel = null!;
        private Panel _statusSeparatorLine = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
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
        private TextBox _widthInput = null!;
        private TextBox _heightInput = null!;
        private AspectRatioPreviewPanel _aspectRatioPreviewPanel = null!;

        public MainForm()
        {
            _detectedMonitors = new List<DisplayManager.MonitorInfo>();
            SetupUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
            InitializeApplication();
            UpdateAspectRatioPreview();
        }

        private void SetupUI()
        {
            SuspendLayout();

            Text = "ResolutionSwitcher v1.0";
            ClientSize = new Size(780, 680);
            MinimumSize = new Size(540, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
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
                Text = "ResolutionSwitcher",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            _subtitleLabel = new Label
            {
                Text = "Display Resolution Manager",
                Dock = DockStyle.Right,
                AutoSize = false,
                Width = 185,
                Font = new Font("Tahoma", 7.5f, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };

            var buttonStrip = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 8, 4, 0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            _lightThemeButton = new Button
            {
                Text = "☀ Light",
                Width = 58,
                Height = 24,
                Font = new Font("Tahoma", 7.5f)
            };
            _lightThemeButton.Click += LightThemeButton_Click;

            _darkThemeButton = new Button
            {
                Text = "🌙 Dark",
                Width = 54,
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(4, 0, 0, 0)
            };
            _darkThemeButton.Click += DarkThemeButton_Click;

            _aboutButton = new Button
            {
                Text = "About",
                Width = 54,
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(4, 0, 0, 0)
            };
            _aboutButton.Click += AboutBtn_Click;

            _settingsButton = new Button
            {
                Text = "Settings",
                Width = 64,
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(4, 0, 0, 0)
            };
            _settingsButton.Click += SettingsBtn_Click;

            _debugButton = new Button
            {
                Text = "Debug",
                Width = 54,
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(4, 0, 0, 0)
            };
            _debugButton.Click += DebugBtn_Click;

            _masterResetButton = new Button
            {
                Text = "Reset",
                Width = 54,
                Height = 24,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(4, 0, 0, 0)
            };
            _masterResetButton.Click += MasterResetBtn_Click;

            buttonStrip.Controls.Add(_lightThemeButton);
            buttonStrip.Controls.Add(_darkThemeButton);
            buttonStrip.Controls.Add(_aboutButton);
            buttonStrip.Controls.Add(_settingsButton);
            buttonStrip.Controls.Add(_debugButton);
            buttonStrip.Controls.Add(_masterResetButton);

            _titlePanel.Controls.Add(_subtitleLabel);
            _titlePanel.Controls.Add(buttonStrip);
            _titlePanel.Controls.Add(_titleLabel);
            _titlePanel.ResumeLayout(false);

            _statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                MinimumSize = new Size(0, 96),
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
                Padding = new Padding(8, 6, 8, 4)
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
            for (int i = 0; i < 7; i++)
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
                Width = 180
            };
            _profileDropdown.Items.AddRange(new object[] { "Gaming", "Streaming", "Productivity" });
            _profileDropdown.SelectedIndex = 0;

            var newProfileBtn = new Button { Text = "+ New", Width = 58, Height = 24, Font = new Font("Tahoma", 7.5f), Margin = new Padding(4, 0, 0, 0) };
            var deleteProfileBtn = new Button { Text = "Delete", Width = 58, Height = 24, Font = new Font("Tahoma", 7.5f), Margin = new Padding(2, 0, 0, 0) };

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

            _profileCardPanel = new Panel
            {
                Name = "_profileCardPanel",
                BorderStyle = BorderStyle.Fixed3D,
                Height = 56,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(8, 6, 8, 4)
            };

            _profileCardLine1 = new Label
            {
                Name = "_profileCardLine1",
                Dock = DockStyle.Top,
                Height = 20,
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
                Margin = new Padding(0, 2, 0, 2)
            };
            _monitorDefaultLabel = new Label
            {
                Text = "Detecting monitors...",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 7.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 1, 0, 3)
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

            _presetDropdown = new ComboBox
            {
                Name = "presetDropdown",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 2, 0, 2)
            };
            _presetDropdown.Items.Add("16:9  3840x2160  (2160p / 4K)");
            _presetDropdown.Items.Add("16:9  2560x1440  (1440p)");
            _presetDropdown.Items.Add("16:9  1920x1080  (1080p)");
            _presetDropdown.Items.Add("16:9  1600x900   (900p)");
            _presetDropdown.Items.Add("16:9  1366x768   (768p laptop)");
            _presetDropdown.Items.Add("16:9  1280x720   (720p)");
            _presetDropdown.Items.Add("16:9  1024x576   (576p)");
            _presetDropdown.Items.Add("16:9  800x450    (450p)");
            _presetDropdown.Items.Add("16:9  640x360    (360p)");
            _presetDropdown.Items.Add("16:10  3840x2400  (2400p)");
            _presetDropdown.Items.Add("16:10  2560x1600  (1600p)");
            _presetDropdown.Items.Add("16:10  1920x1200  (1200p)");
            _presetDropdown.Items.Add("16:10  1680x1050  (1050p)");
            _presetDropdown.Items.Add("16:10  1440x900   (900p)");
            _presetDropdown.Items.Add("16:10  1280x800   (800p)");
            _presetDropdown.Items.Add("16:10  1024x640   (640p)");
            _presetDropdown.Items.Add("16:10  800x500    (500p)");
            _presetDropdown.Items.Add("16:10  640x400    (400p)");
            _presetDropdown.Items.Add("4:3  2880x2160  (2160p)");
            _presetDropdown.Items.Add("4:3  1920x1440  (1440p)");
            _presetDropdown.Items.Add("4:3  1600x1200  (1200p)");
            _presetDropdown.Items.Add("4:3  1440x1080  (1080p)");
            _presetDropdown.Items.Add("4:3  1400x1050  (1050p)");
            _presetDropdown.Items.Add("4:3  1280x960   (960p)");
            _presetDropdown.Items.Add("4:3  1200x900   (900p)");
            _presetDropdown.Items.Add("4:3  1024x768   (768p)");
            _presetDropdown.Items.Add("4:3  960x720    (720p)");
            _presetDropdown.Items.Add("4:3  800x600    (600p)");
            _presetDropdown.Items.Add("4:3  640x480    (480p)");
            _presetDropdown.Items.Add("5:4  2700x2160  (2160p)");
            _presetDropdown.Items.Add("5:4  1800x1440  (1440p)");
            _presetDropdown.Items.Add("5:4  1500x1200  (1200p)");
            _presetDropdown.Items.Add("5:4  1350x1080  (1080p)");
            _presetDropdown.Items.Add("5:4  1312x1050  (1050p)");
            _presetDropdown.Items.Add("5:4  1280x1024  (1024p)");
            _presetDropdown.Items.Add("5:4  1125x900   (900p)");
            _presetDropdown.Items.Add("5:4  960x768    (768p)");
            _presetDropdown.Items.Add("5:4  900x720    (720p)");
            _presetDropdown.Items.Add("5:4  750x600    (600p)");
            _presetDropdown.Items.Add("5:4  600x480    (480p)");
            _presetDropdown.SelectedIndex = 0;
            _presetDropdown.SelectedIndexChanged += (_, _) => UpdateAspectRatioPreview();

            var customFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2)
            };
            _widthInput = new TextBox { Name = "widthInput", Text = "960", Width = 52, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f) };
            _heightInput = new TextBox { Name = "heightInput", Text = "720", Width = 52, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f) };
            var hzInput = new TextBox { Name = "hzInput", Text = "240", Width = 52, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Tahoma", 8f) };

            _widthInput.TextChanged += (_, _) => UpdateAspectRatioPreview();
            _heightInput.TextChanged += (_, _) => UpdateAspectRatioPreview();

            customFlow.Controls.Add(new Label { Text = "W:", Width = 20, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f) });
            customFlow.Controls.Add(_widthInput);
            customFlow.Controls.Add(new Label { Text = "H:", Width = 24, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f), Margin = new Padding(6, 0, 0, 0) });
            customFlow.Controls.Add(_heightInput);
            customFlow.Controls.Add(new Label { Text = "Hz:", Width = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Tahoma", 8f), Margin = new Padding(6, 0, 0, 0) });
            customFlow.Controls.Add(hzInput);

            _aspectRatioPreviewPanel = new AspectRatioPreviewPanel
            {
                Width = 80,
                Height = 54,
                Margin = new Padding(0, 0, 0, 2)
            };

            var previewHost = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 2)
            };
            previewHost.Controls.Add(_aspectRatioPreviewPanel);

            resLayout.Controls.Add(MakeLabel("Preset:"), 0, 0);
            resLayout.Controls.Add(_presetDropdown, 1, 0);
            resLayout.Controls.Add(MakeLabel("Custom:"), 0, 1);
            resLayout.Controls.Add(customFlow, 1, 1);
            resLayout.Controls.Add(MakeLabel("Preview:"), 0, 2);
            resLayout.Controls.Add(previewHost, 1, 2);
            resLayout.ResumeLayout(false);
            resGroup.Controls.Add(resLayout);

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

            var addGameBtn = new Button { Text = "Add...", Width = 58, Height = 24, Font = new Font("Tahoma", 7.5f), Margin = new Padding(4, 0, 0, 0) };
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

            var launchGroup = MakeGroup("Launch Mode");
            var launchLayout = MakeTwoColLayout(1);
            launchLayout.SuspendLayout();

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
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            applyLaunchBtn.Click += LaunchGameBtn_Click;

            var applyOnlyBtn = new Button
            {
                Text = "Apply Only",
                Dock = DockStyle.Fill,
                Height = 36,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            applyOnlyBtn.Click += ApplyOnlyBtn_Click;

            var resetBtn = new Button
            {
                Text = "Reset Resolution",
                Dock = DockStyle.Fill,
                Height = 36,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(2, 0, 2, 0)
            };
            resetBtn.Click += ResetBtn_Click;

            actionBtnLayout.Controls.Add(applyLaunchBtn, 0, 0);
            actionBtnLayout.Controls.Add(applyOnlyBtn, 1, 0);
            actionBtnLayout.Controls.Add(resetBtn, 2, 0);
            actionGroup.Controls.Add(actionBtnLayout);

            mainLayout.Controls.Add(profileGroup, 0, 0);
            mainLayout.Controls.Add(_profileCardPanel, 0, 1);
            mainLayout.Controls.Add(monitorGroup, 0, 2);
            mainLayout.Controls.Add(resGroup, 0, 3);
            mainLayout.Controls.Add(gameGroup, 0, 4);
            mainLayout.Controls.Add(launchGroup, 0, 5);
            mainLayout.Controls.Add(actionGroup, 0, 6);

            mainLayout.ResumeLayout(false);
            _scrollPanel.Controls.Add(mainLayout);
            _scrollPanel.ResumeLayout(false);

            Controls.Add(_scrollPanel);
            Controls.Add(_statusPanel);
            Controls.Add(_titlePanel);

            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            base.OnFormClosed(e);
        }

        private GroupBox MakeGroup(string title)
        {
            return new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8),
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
                Margin = new Padding(0, 3, 6, 3),
                AutoSize = false
            };
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
            _subtitleLabel.ForeColor = theme.SubtitleTextColor;
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

            _aspectRatioPreviewPanel.BackColor = theme.SectionBackground;
            _aspectRatioPreviewPanel.ForeColor = theme.TextColor;
            _aspectRatioPreviewPanel.Invalidate();

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
                if (ReferenceEquals(child, _titlePanel) || ReferenceEquals(child, _statusPanel) || ReferenceEquals(child, _profileCardPanel) || ReferenceEquals(child, _aspectRatioPreviewPanel))
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
            _logger.LogInfo("Apply and Launch Game clicked");
            AppendStatus("Launching game...");
        }

        private void ApplyOnlyBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Apply Only clicked");
            AppendStatus("Applying resolution...");
        }

        private void ResetBtn_Click(object? sender, EventArgs e)
        {
            _logger.LogInfo("Reset clicked");
            AppendStatus("Resetting to default resolution...");
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
                AppendStatus($"Game selected: {openFileDialog.FileName}");
                _logger.LogInfo($"Game selected: {openFileDialog.FileName}");
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

        private void LearnMoreBtn_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            _logger.LogInfo("Learn More clicked");
            var modeForm = new Form
            {
                Text = "Launch Modes - Detailed Comparison",
                Width = 720,
                Height = 620,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                MinimizeBox = false,
                MinimumSize = new Size(560, 480),
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

            var modeText = @"AUTO-RESTORE HELPER
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

        private void UpdateAspectRatioPreview()
        {
            var ratio = ParseRatioFromPreset(_presetDropdown.SelectedItem?.ToString() ?? string.Empty);

            if (int.TryParse(_widthInput.Text, out var width) && int.TryParse(_heightInput.Text, out var height) && width > 0 && height > 0)
            {
                var gcd = GreatestCommonDivisor(width, height);
                if (gcd > 0)
                {
                    ratio = (width / gcd, height / gcd);
                }
            }

            _aspectRatioPreviewPanel.SetRatio(ratio.Item1, ratio.Item2);
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }

        private (int, int) ParseRatioFromPreset(string item)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                var parts = item.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    var ratioParts = parts[0].Split(':');
                    if (ratioParts.Length == 2 && int.TryParse(ratioParts[0], out var left) && int.TryParse(ratioParts[1], out var right) && left > 0 && right > 0)
                    {
                        return (left, right);
                    }
                }
            }

            return (16, 9);
        }

        private static Color BlendColors(Color top, Color bottom)
        {
            return Color.FromArgb(
                (top.R + bottom.R) / 2,
                (top.G + bottom.G) / 2,
                (top.B + bottom.B) / 2);
        }

        private sealed class AspectRatioPreviewPanel : Panel
        {
            private static readonly Color RatioFourThreeColor = ColorTranslator.FromHtml("#4CAF50");
            private static readonly Color RatioSixteenNineColor = ColorTranslator.FromHtml("#2196F3");
            private static readonly Color RatioFiveFourColor = ColorTranslator.FromHtml("#FF9800");
            private static readonly Color RatioOtherColor = ColorTranslator.FromHtml("#9E9E9E");
            private int _ratioWidth = 16;
            private int _ratioHeight = 9;

            public AspectRatioPreviewPanel()
            {
                DoubleBuffered = true;
                BorderStyle = BorderStyle.FixedSingle;
                Size = new Size(80, 54);
                Margin = new Padding(0);
            }

            public void SetRatio(int width, int height)
            {
                _ratioWidth = width > 0 ? width : 16;
                _ratioHeight = height > 0 ? height : 9;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.Clear(BackColor);

                var color = GetRatioColor(_ratioWidth, _ratioHeight);
                var available = new Rectangle(4, 3, Width - 8, Math.Max(16, Height - 22));

                var scale = Math.Min((float)available.Width / _ratioWidth, (float)available.Height / _ratioHeight);
                var drawWidth = Math.Max(8, (int)(_ratioWidth * scale));
                var drawHeight = Math.Max(8, (int)(_ratioHeight * scale));
                var drawRect = new Rectangle(
                    available.X + (available.Width - drawWidth) / 2,
                    available.Y + (available.Height - drawHeight) / 2,
                    drawWidth,
                    drawHeight);

                using (var fill = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(fill, drawRect);
                }

                using (var border = new Pen(ControlPaint.Dark(color)))
                {
                    e.Graphics.DrawRectangle(border, drawRect);
                }

                var ratioTextRect = new Rectangle(0, Height - 16, Width, 14);
                TextRenderer.DrawText(
                    e.Graphics,
                    $"{_ratioWidth}:{_ratioHeight}",
                    new Font("Tahoma", 7.5f),
                    ratioTextRect,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            private static Color GetRatioColor(int width, int height)
            {
                if (width == 4 && height == 3)
                {
                    return RatioFourThreeColor;
                }

                if (width == 16 && height == 9)
                {
                    return RatioSixteenNineColor;
                }

                if (width == 5 && height == 4)
                {
                    return RatioFiveFourColor;
                }

                return RatioOtherColor;
            }
        }
    }
}
