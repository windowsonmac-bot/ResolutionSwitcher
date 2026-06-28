using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public class SettingsForm : Form
    {
        private const string StartupMessage = "STARTUP ON BOOT\n\nThis will add a single registry entry to:\nHKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run\n\nWhat this does:\n• Windows will launch ResolutionSwitcher when you log in\n• The app opens normally - no background process starts automatically\n• No services, no tray icon, no monitoring\n\nPerformance impact: ZERO\nThe app only uses resources when you are actively using it.\n\nHow to undo:\n• Uncheck this box at any time\n• OR delete the app folder (startup entry auto-removes on next launch)\n\nEnable startup on boot?";
        private const string SafeModeMessage = "SAFE MODE FALLBACK\n\nIf a resolution change fails or your display goes black for more than 10 seconds,\nthe app will automatically revert to your saved default resolution.\n\nThis uses the same rollback mechanism built into Windows display settings.\n\nPerformance impact: None (triggered only on failure)\n\nRecommended: ON for most users";
        private Panel _titlePanel = null!;
        private Label _titleLabel = null!;
        private TabControl _tabControl = null!;
        private CheckBox _startupCheckBox = null!;
        private CheckBox _safeModeCheckBox = null!;
        private CheckBox _crashRecoveryCheckBox = null!;
        private RadioButton _lightModeRadio = null!;
        private RadioButton _darkModeRadio = null!;
        private Label _startupHintLabel = null!;
        private Label _safeModeHintLabel = null!;
        private Label _crashRecoveryHintLabel = null!;
        private readonly Dictionary<TextBox, string> _hotkeyDefaults = new Dictionary<TextBox, string>();

        public SettingsForm()
        {
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            base.OnFormClosed(e);
        }

        private void InitializeUI()
        {
            SuspendLayout();

            Text = "Settings";
            Width = 900;
            Height = 780;
            MinimumSize = new Size(700, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Tahoma", 8f);

            _titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(8, 0, 8, 0)
            };

            _titleLabel = new Label
            {
                Text = "Settings",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            _titlePanel.Controls.Add(_titleLabel);

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(12, 6)
            };

            _tabControl.TabPages.Add(CreateGeneralTab());
            _tabControl.TabPages.Add(CreateHotkeysTab());
            _tabControl.TabPages.Add(CreateAdvancedTab());

            Controls.Add(_tabControl);
            Controls.Add(_titlePanel);

            ResumeLayout(false);
        }

        private TabPage CreateGeneralTab()
        {
            var tab = new TabPage("General");
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(10),
                RowCount = 4
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var startupGroup = new GroupBox
            {
                Text = "Startup",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(8, 10, 8, 8)
            };

            var startupLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true
            };
            startupLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            _startupCheckBox = new CheckBox
            {
                Name = "_startupCheckBox",
                Text = "Launch ResolutionSwitcher when Windows starts",
                AutoSize = true,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 0, 0, 4)
            };
            _startupCheckBox.CheckedChanged += StartupCheckBox_CheckedChanged;

            _startupHintLabel = new Label
            {
                Text = "Adds one registry key. Removed automatically if app folder is deleted.",
                AutoSize = true,
                Font = new Font("Tahoma", 7f, FontStyle.Italic),
                Margin = new Padding(16, 0, 0, 0)
            };

            startupLayout.Controls.Add(_startupCheckBox, 0, 0);
            startupLayout.Controls.Add(_startupHintLabel, 0, 1);
            startupGroup.Controls.Add(startupLayout);

            var launchModeGroup = new GroupBox
            {
                Text = "Default Launch Mode",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(8, 10, 8, 8)
            };

            var launchModeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true
            };
            launchModeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            launchModeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var launchModeLabel = new Label
            {
                Text = "Default mode:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 3, 6, 3)
            };
            var launchModeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f)
            };
            launchModeCombo.Items.AddRange(new object[] { "Auto-Restore Helper", "Instant Kill Mode" });
            launchModeCombo.SelectedIndex = 0;
            launchModeLayout.Controls.Add(launchModeLabel, 0, 0);
            launchModeLayout.Controls.Add(launchModeCombo, 1, 0);
            launchModeGroup.Controls.Add(launchModeLayout);

            var themeGroup = new GroupBox
            {
                Text = "Theme",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(8, 10, 8, 8)
            };

            var themeFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };

            _lightModeRadio = new RadioButton
            {
                Text = "Light mode",
                AutoSize = true,
                Font = new Font("Tahoma", 8f)
            };
            _lightModeRadio.CheckedChanged += (_, _) =>
            {
                if (_lightModeRadio.Checked)
                {
                    ThemeManager.SetTheme(AppTheme.Light);
                }
            };

            _darkModeRadio = new RadioButton
            {
                Text = "Dark mode",
                AutoSize = true,
                Font = new Font("Tahoma", 8f)
            };
            _darkModeRadio.CheckedChanged += (_, _) =>
            {
                if (_darkModeRadio.Checked)
                {
                    ThemeManager.SetTheme(AppTheme.Dark);
                }
            };

            themeFlow.Controls.Add(_lightModeRadio);
            themeFlow.Controls.Add(_darkModeRadio);
            themeGroup.Controls.Add(themeFlow);

            var steamGroup = new GroupBox
            {
                Text = "Steam Library",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Padding = new Padding(8, 10, 8, 8)
            };

            var steamFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6)
            };

            var scanSteamButton = new Button
            {
                Text = "Scan Steam Library",
                Width = 140,
                Height = 26,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 2, 0, 2)
            };
            scanSteamButton.Click += (_, _) => MessageBox.Show(
                "Steam library scanning will be available in the next update.",
                "Coming Soon",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            var steamHintLabel = new Label
            {
                Text = "Click to scan and import your Steam games",
                AutoSize = true,
                Font = new Font("Tahoma", 7.5f, FontStyle.Italic),
                Margin = new Padding(10, 5, 0, 0)
            };

            steamFlow.Controls.Add(scanSteamButton);
            steamFlow.Controls.Add(steamHintLabel);
            steamGroup.Controls.Add(steamFlow);

            layout.Controls.Add(startupGroup, 0, 0);
            layout.Controls.Add(launchModeGroup, 0, 1);
            layout.Controls.Add(themeGroup, 0, 2);
            layout.Controls.Add(steamGroup, 0, 3);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CreateHotkeysTab()
        {
            var tab = new TabPage("Hotkeys");
            var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true };

            var group = new GroupBox
            {
                Text = "Global Hotkeys",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Padding = new Padding(8, 10, 8, 8)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 3,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var noteLabel = new Label
            {
                Text = "These hotkeys work even when the app is minimized.\nRequires the app to be running (use Startup option above).\nClick any field and press your desired key combination to change it.",
                AutoSize = true,
                Font = new Font("Tahoma", 7.5f, FontStyle.Italic),
                Margin = new Padding(0, 0, 0, 8)
            };

            var table = new TableLayoutPanel
            {
                ColumnCount = 3,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58f));

            AddHotkeyRow(table, 0, "Reset Resolution", "Ctrl + Alt + R", "_resetHotkeyBox");
            AddHotkeyRow(table, 1, "Apply & Launch Last Profile", "Ctrl + Alt + L", "_launchHotkeyBox");
            AddHotkeyRow(table, 2, "Switch to Light Mode", "Ctrl + Alt + 1", "_lightHotkeyBox");
            AddHotkeyRow(table, 3, "Switch to Dark Mode", "Ctrl + Alt + 2", "_darkHotkeyBox");
            AddHotkeyRow(table, 4, "Emergency Reset (force revert)", "Ctrl + Alt + F12", "_emergencyHotkeyBox");

            var footerLabel = new Label
            {
                Text = "Note: 60% keyboard friendly defaults — no function keys required except Emergency Reset.",
                AutoSize = true,
                Font = new Font("Tahoma", 7.5f, FontStyle.Italic)
            };

            layout.Controls.Add(noteLabel, 0, 0);
            layout.Controls.Add(table, 0, 1);
            layout.Controls.Add(footerLabel, 0, 2);

            group.Controls.Add(layout);
            root.Controls.Add(group);
            tab.Controls.Add(root);
            return tab;
        }

        private void AddHotkeyRow(TableLayoutPanel table, int rowIndex, string action, string defaultValue, string textBoxName)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var actionLabel = new Label
            {
                Text = action,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 3, 8, 3),
                Padding = new Padding(0, 3, 0, 0)
            };

            var textBox = new TextBox
            {
                Name = textBoxName,
                Text = defaultValue,
                Width = 140,
                Font = new Font("Tahoma", 9f),
                ReadOnly = false,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 3, 0, 3)
            };
            _hotkeyDefaults[textBox] = defaultValue;

            var resetButton = new Button
            {
                Text = "Reset",
                Width = 52,
                Height = 23,
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(0, 3, 0, 3)
            };
            resetButton.Click += (_, _) => textBox.Text = _hotkeyDefaults[textBox];

            table.Controls.Add(actionLabel, 0, rowIndex);
            table.Controls.Add(textBox, 1, rowIndex);
            table.Controls.Add(resetButton, 2, rowIndex);
        }

        private TabPage CreateAdvancedTab()
        {
            var tab = new TabPage("Advanced");
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var safeModeGroup = new GroupBox
            {
                Text = "Safe Mode Fallback",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(8, 10, 8, 8)
            };

            var safeModeLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true
            };

            _safeModeCheckBox = new CheckBox
            {
                Name = "_safeModeCheckBox",
                Text = "Enable safe mode fallback",
                Checked = true,
                AutoSize = true
            };
            _safeModeCheckBox.CheckedChanged += SafeModeCheckBox_CheckedChanged;

            _safeModeHintLabel = new Label
            {
                Text = "Auto-reverts if resolution change causes display issues",
                AutoSize = true,
                Font = new Font("Tahoma", 7f, FontStyle.Italic),
                Margin = new Padding(16, 0, 0, 0)
            };

            safeModeLayout.Controls.Add(_safeModeCheckBox, 0, 0);
            safeModeLayout.Controls.Add(_safeModeHintLabel, 0, 1);
            safeModeGroup.Controls.Add(safeModeLayout);

            var crashGroup = new GroupBox
            {
                Text = "Crash Recovery",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Padding = new Padding(8, 10, 8, 8)
            };

            var crashLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true
            };

            _crashRecoveryCheckBox = new CheckBox
            {
                Name = "_crashRecoveryCheckBox",
                Text = "Enable crash recovery",
                Checked = true,
                AutoSize = true
            };

            _crashRecoveryHintLabel = new Label
            {
                Text = "If the app closes unexpectedly while a custom resolution is active, it will auto-revert on next launch",
                AutoSize = true,
                Font = new Font("Tahoma", 7f, FontStyle.Italic),
                Margin = new Padding(16, 0, 0, 0)
            };

            crashLayout.Controls.Add(_crashRecoveryCheckBox, 0, 0);
            crashLayout.Controls.Add(_crashRecoveryHintLabel, 0, 1);
            crashGroup.Controls.Add(crashLayout);

            layout.Controls.Add(safeModeGroup, 0, 0);
            layout.Controls.Add(crashGroup, 0, 1);
            tab.Controls.Add(layout);
            return tab;
        }

        private void StartupCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (!_startupCheckBox.Checked)
            {
                return;
            }

            var result = MessageBox.Show(
                StartupMessage,
                "Startup on Boot",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Cancel)
            {
                _startupCheckBox.Checked = false;
            }
        }

        private void SafeModeCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (!_safeModeCheckBox.Checked)
            {
                return;
            }

            MessageBox.Show(
                SafeModeMessage,
                "Safe Mode Fallback",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            _titleLabel.ForeColor = theme.TitleBarTextColor;
            _tabControl.BackColor = theme.FormBackground;
            _tabControl.ForeColor = theme.TextColor;
            _startupHintLabel.ForeColor = theme.GrayTextColor;
            _safeModeHintLabel.ForeColor = theme.GrayTextColor;
            _crashRecoveryHintLabel.ForeColor = theme.GrayTextColor;

            _lightModeRadio.Checked = ThemeManager.CurrentTheme == AppTheme.Light;
            _darkModeRadio.Checked = ThemeManager.CurrentTheme == AppTheme.Dark;

            ApplyThemeRecursive(this, false);
            Invalidate(true);
        }

        private void ApplyThemeRecursive(Control parent, bool insideGroup)
        {
            var theme = ThemeManager.Palette;

            foreach (Control child in parent.Controls)
            {
                var inside = insideGroup || child is GroupBox || child.Parent is GroupBox;

                if (child is GroupBox)
                {
                    child.ForeColor = theme.TextColor;
                }
                else if (child is Panel || child is TableLayoutPanel || child is FlowLayoutPanel || child is TabPage)
                {
                    child.BackColor = inside ? theme.SectionBackground : theme.FormBackground;
                    child.ForeColor = theme.TextColor;
                }
                else if (child is Label)
                {
                    child.BackColor = inside ? theme.SectionBackground : theme.FormBackground;
                    child.ForeColor = theme.TextColor;
                }
                else if (child is CheckBox || child is RadioButton)
                {
                    child.BackColor = theme.SectionBackground;
                    child.ForeColor = theme.TextColor;
                }
                else if (child is ComboBox combo)
                {
                    combo.BackColor = theme.InputBackground;
                    combo.ForeColor = theme.TextColor;
                }
                else if (child is TextBox box)
                {
                    box.BackColor = theme.InputBackground;
                    box.ForeColor = theme.TextColor;
                }
                else if (child is Button button)
                {
                    ThemeManager.ApplyButtonStyle(button);
                }

                if (child.HasChildren)
                {
                    ApplyThemeRecursive(child, inside);
                }
            }
        }
    }
}
