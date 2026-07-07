using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

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
        private Button _createShortcutButton = null!;
        private Label _shortcutHintLabel = null!;
        private readonly Dictionary<TextBox, string> _hotkeyDefaults = new Dictionary<TextBox, string>();
        private TextBox _resetHotkeyBox = null!;
        private TextBox _launchHotkeyBox = null!;
        private TextBox _lightHotkeyBox = null!;
        private TextBox _darkHotkeyBox = null!;
        private TextBox _emergencyHotkeyBox = null!;
        private readonly ConfigManager? _configManager;

        public SettingsForm(ConfigManager? configManager = null)
        {
            _configManager = configManager;
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
            Icon = IconProvider.AppIcon;
            Width = 900;
            Height = 780;
            MinimumSize = new Size(700, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(6F, 13F);
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
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(10),
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var startupGroup = new GroupBox
            {
                Text = "Startup",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(8, 12, 8, 10)
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
            _startupCheckBox.Checked = GetStartupEnabled();
            _startupCheckBox.CheckedChanged += StartupCheckBox_CheckedChanged;

            _startupHintLabel = new Label
            {
                Text = "Check = adds registry key. Uncheck = immediately removes it. No hidden state.",
                AutoSize = true,
                Font = new Font("Tahoma", 7f, FontStyle.Italic),
                Margin = new Padding(16, 0, 0, 0)
            };

            startupLayout.Controls.Add(_startupCheckBox, 0, 0);
            startupLayout.Controls.Add(_startupHintLabel, 0, 1);
            startupGroup.Controls.Add(startupLayout);

            var shortcutGroup = new GroupBox
            {
                Text = "Desktop Shortcut",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(8, 12, 8, 10)
            };

            var shortcutLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true
            };
            shortcutLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            _createShortcutButton = new Button
            {
                Text = "Create Desktop Shortcut",
                AutoSize = true,
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(0, 0, 0, 4),
                Padding = new Padding(10, 4, 10, 4)
            };
            _createShortcutButton.Click += CreateShortcutButton_Click;

            _shortcutHintLabel = new Label
            {
                Text = "Adds a ResolutionSwitcher icon to your desktop, matching the app icon.",
                AutoSize = true,
                Font = new Font("Tahoma", 7f, FontStyle.Italic),
                Margin = new Padding(0, 0, 0, 0)
            };

            shortcutLayout.Controls.Add(_createShortcutButton, 0, 0);
            shortcutLayout.Controls.Add(_shortcutHintLabel, 0, 1);
            shortcutGroup.Controls.Add(shortcutLayout);

            var themeGroup = new GroupBox
            {
                Text = "Theme",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(8, 12, 8, 10)
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

            layout.Controls.Add(startupGroup, 0, 0);
            layout.Controls.Add(shortcutGroup, 0, 1);
            layout.Controls.Add(themeGroup, 0, 2);
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
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 8)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            AddHotkeyRow(table, 0, "Reset Resolution", "Ctrl + Alt + R", out _resetHotkeyBox);
            AddHotkeyRow(table, 1, "Apply & Launch Last Profile", "Ctrl + Alt + L", out _launchHotkeyBox);
            AddHotkeyRow(table, 2, "Switch to Light Mode", "Ctrl + Alt + 1", out _lightHotkeyBox);
            AddHotkeyRow(table, 3, "Switch to Dark Mode", "Ctrl + Alt + 2", out _darkHotkeyBox);
            AddHotkeyRow(table, 4, "Emergency Reset (force revert)", "Ctrl + Alt + F12", out _emergencyHotkeyBox);

            LoadHotkeysFromConfig();

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

        private void AddHotkeyRow(TableLayoutPanel table, int rowIndex, string action, string defaultValue, out TextBox createdTextBox)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var actionLabel = new Label
            {
                Text = action,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Size = new Size(220, 28),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 8, 0)
            };

            var textBox = new TextBox
            {
                Text = defaultValue,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9f),
                ReadOnly = false,
                Margin = new Padding(0, 3, 0, 3)
            };
            _hotkeyDefaults[textBox] = defaultValue;
            textBox.Leave += HotkeyTextBox_Leave;
            textBox.KeyDown += HotkeyTextBox_KeyDown;

            var resetButton = new Button
            {
                Text = "Reset",
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 7.5f),
                Margin = new Padding(0, 3, 0, 3)
            };
            resetButton.Click += (_, _) =>
            {
                textBox.Text = _hotkeyDefaults[textBox];
                SaveHotkeysToConfig();
            };

            table.Controls.Add(actionLabel, 0, rowIndex);
            table.Controls.Add(textBox, 1, rowIndex);
            table.Controls.Add(resetButton, 2, rowIndex);

            createdTextBox = textBox;
        }

        /// <summary>
        /// Captures the key combination pressed while a hotkey textbox has focus,
        /// so users type the actual keys rather than typing text manually.
        /// </summary>
        private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            if (e.KeyCode is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin)
            {
                return;
            }

            var parts = new List<string>();
            if (e.Control) parts.Add("Ctrl");
            if (e.Alt) parts.Add("Alt");
            if (e.Shift) parts.Add("Shift");

            if (parts.Count == 0)
            {
                // Require at least one modifier for a global hotkey
                return;
            }

            parts.Add(e.KeyCode.ToString());
            textBox.Text = string.Join(" + ", parts);
        }

        private void HotkeyTextBox_Leave(object? sender, EventArgs e)
        {
            SaveHotkeysToConfig();
        }

        private void LoadHotkeysFromConfig()
        {
            if (_configManager == null) return;
            var hotkeys = _configManager.GetConfig().Hotkeys;

            _resetHotkeyBox.Text = hotkeys.ResetHotkey;
            _launchHotkeyBox.Text = hotkeys.LaunchHotkey;
            _lightHotkeyBox.Text = hotkeys.LightThemeHotkey;
            _darkHotkeyBox.Text = hotkeys.DarkThemeHotkey;
            _emergencyHotkeyBox.Text = hotkeys.EmergencyResetHotkey;
        }

        private void SaveHotkeysToConfig()
        {
            if (_configManager == null) return;

            var hotkeys = _configManager.GetConfig().Hotkeys;
            hotkeys.ResetHotkey = _resetHotkeyBox.Text.Trim();
            hotkeys.LaunchHotkey = _launchHotkeyBox.Text.Trim();
            hotkeys.LightThemeHotkey = _lightHotkeyBox.Text.Trim();
            hotkeys.DarkThemeHotkey = _darkHotkeyBox.Text.Trim();
            hotkeys.EmergencyResetHotkey = _emergencyHotkeyBox.Text.Trim();
            _configManager.Save();

            HotkeysChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised whenever hotkey text is committed, so MainForm can re-register
        /// global hotkeys immediately without requiring an app restart.
        /// </summary>
        public event EventHandler? HotkeysChanged;

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
                Checked = _configManager?.GetConfig().Behavior.SafeModeEnabled ?? true,
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
                Checked = _configManager?.GetConfig().Behavior.CrashRecoveryEnabled ?? true,
                AutoSize = true
            };
            _crashRecoveryCheckBox.CheckedChanged += CrashRecoveryCheckBox_CheckedChanged;

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

        private static bool GetStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("ResolutionSwitcher") != null;
            }
            catch { return false; }
        }

        private static void SetStartupEnabled(bool enabled)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;
                if (enabled)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                    key.SetValue("ResolutionSwitcher", $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue("ResolutionSwitcher", false);
                }
            }
            catch { }
        }

        private void CreateShortcutButton_Click(object? sender, EventArgs e)
        {
            if (ShortcutManager.CreateDesktopShortcut(out var error))
            {
                MessageBox.Show(
                    $"✓ Desktop shortcut created.\n\nLocation:\n{ShortcutManager.DesktopShortcutPath}\n\nIt uses the same icon as ResolutionSwitcher itself.",
                    "Shortcut Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"Could not create the desktop shortcut.\n\n{error}",
                    "Shortcut Not Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void StartupCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            const string registryPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "ResolutionSwitcher";

            if (_startupCheckBox.Checked)
            {
                SetStartupEnabled(true);
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "(unknown)";
                MessageBox.Show(
                    $"✓ Startup entry ADDED.\n\nRegistry key written:\n{registryPath}\nValue name: {valueName}\nValue: \"{exePath}\"\n\nTo undo: uncheck this box at any time.\nThe key will be deleted immediately.",
                    "Startup Enabled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                SetStartupEnabled(false);
                MessageBox.Show(
                    $"✓ Startup entry REMOVED.\n\nRegistry key deleted:\n{registryPath}\\{valueName}\n\nResolutionSwitcher will no longer launch on Windows startup.\nNo registry entries remain.",
                    "Startup Disabled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void SafeModeCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_configManager != null)
            {
                _configManager.GetConfig().Behavior.SafeModeEnabled = _safeModeCheckBox.Checked;
                _configManager.Save();
            }

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

        private void CrashRecoveryCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_configManager == null) return;
            _configManager.GetConfig().Behavior.CrashRecoveryEnabled = _crashRecoveryCheckBox.Checked;
            _configManager.Save();
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
            _shortcutHintLabel.ForeColor = theme.GrayTextColor;
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
