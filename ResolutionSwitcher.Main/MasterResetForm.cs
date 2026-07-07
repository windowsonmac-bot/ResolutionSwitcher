using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ResolutionSwitcher.Main
{
    public class MasterResetForm : Form
    {
        private readonly ConfigManager? _configManager;
        private Panel _titlePanel = null!;
        private Label _titleLabel = null!;
        private Label _warningLabel = null!;
        private Button _performResetButton = null!;

        /// <summary>
        /// Raised after a reset has been performed so the owning MainForm can
        /// refresh its live UI (profiles, hotkeys, monitor defaults) without
        /// requiring an app restart.
        /// </summary>
        public event EventHandler? ResetCompleted;

        public MasterResetForm(ConfigManager? configManager = null)
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

            Text = "Master Reset";
            Icon = IconProvider.AppIcon;
            Width = 640;
            Height = 500;
            MinimumSize = new Size(560, 420);
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
                Text = "Master Reset",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            _titlePanel.Controls.Add(_titleLabel);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _warningLabel = new Label
            {
                Text = "⚠ WARNING: These actions cannot be undone.",
                AutoSize = true,
                Font = new Font("Tahoma", 8.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8)
            };

            var resetOptionsGroup = new GroupBox
            {
                Text = "Reset Options",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Padding = new Padding(8, 10, 8, 8)
            };

            var optionsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true
            };

            optionsFlow.Controls.Add(new CheckBox { Text = "Delete all profiles (restores the 3 default profiles)", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
            optionsFlow.Controls.Add(new CheckBox { Text = "Clear saved monitor defaults", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
            optionsFlow.Controls.Add(new CheckBox { Text = "Reset hotkeys to default", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
            optionsFlow.Controls.Add(new CheckBox { Text = "Remove startup registry entry (if set)", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
            optionsFlow.Controls.Add(new CheckBox { Text = "Delete debug log file", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
            optionsFlow.Controls.Add(new CheckBox { Text = "Reset theme to Light mode", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });

            resetOptionsGroup.Controls.Add(optionsFlow);

            var buttonsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _performResetButton = new Button
            {
                Text = "PERFORM MASTER RESET",
                Dock = DockStyle.Fill,
                Height = 36,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#C0392B"),
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0)
            };
            _performResetButton.Click += PerformButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                Width = 90,
                Height = 36,
                Font = new Font("Tahoma", 8f)
            };
            cancelButton.Click += (_, _) => Close();

            buttonsPanel.Controls.Add(_performResetButton, 0, 0);
            buttonsPanel.Controls.Add(cancelButton, 1, 0);

            content.Controls.Add(_warningLabel, 0, 0);
            content.Controls.Add(resetOptionsGroup, 0, 1);
            content.Controls.Add(buttonsPanel, 0, 2);

            Controls.Add(content);
            Controls.Add(_titlePanel);

            ResumeLayout(false);
        }

        private void PerformButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure? This will reset all selected settings and cannot be undone.",
                "Confirm Master Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            var actions = new System.Text.StringBuilder();
            actions.AppendLine("Master Reset Results:");
            actions.AppendLine();

            // Find the optionsFlow panel and get its checkboxes in order
            var optionsFlow = FindControlByType<FlowLayoutPanel>(this);
            var checkboxes = new List<CheckBox>();
            if (optionsFlow != null)
            {
                foreach (Control c in optionsFlow.Controls)
                {
                    if (c is CheckBox cb) checkboxes.Add(cb);
                }
            }

            bool resetProfiles = checkboxes.Count > 0 && checkboxes[0].Checked;
            bool clearMonitors = checkboxes.Count > 1 && checkboxes[1].Checked;
            bool resetHotkeys  = checkboxes.Count > 2 && checkboxes[2].Checked;
            bool removeStartup = checkboxes.Count > 3 && checkboxes[3].Checked;
            bool deleteLog     = checkboxes.Count > 4 && checkboxes[4].Checked;
            bool resetTheme    = checkboxes.Count > 5 && checkboxes[5].Checked;

            // Operate on the live ConfigManager when available so MainForm's in-memory
            // config, dropdowns, and hotkey registrations can be refreshed immediately
            // afterward instead of requiring an app restart. Fall back to a fresh
            // instance (which loads/creates config.json on disk) if opened standalone.
            var configManager = _configManager ?? new ConfigManager();
            bool configChanged = false;

            // 1. Reset profiles back to the 3 built-in defaults
            if (resetProfiles)
            {
                try
                {
                    configManager.ResetProfilesToDefaults();
                    actions.AppendLine("✓ Profiles reset to defaults (Gaming, Streaming, Productivity)");
                    configChanged = true;
                }
                catch (Exception ex)
                {
                    actions.AppendLine($"✗ Failed to reset profiles: {ex.Message}");
                }
            }

            // 2. Clear saved monitor defaults
            if (clearMonitors)
            {
                try
                {
                    configManager.ClearMonitorDefaults();
                    actions.AppendLine("✓ Monitor defaults cleared");
                    configChanged = true;
                }
                catch (Exception ex)
                {
                    actions.AppendLine($"✗ Failed to clear monitor defaults: {ex.Message}");
                }
            }

            // 3. Reset hotkeys to default
            if (resetHotkeys)
            {
                try
                {
                    configManager.ResetHotkeysToDefaults();
                    actions.AppendLine("✓ Hotkeys reset to defaults");
                    configChanged = true;
                }
                catch (Exception ex)
                {
                    actions.AppendLine($"✗ Failed to reset hotkeys: {ex.Message}");
                }
            }

            // 4. Remove startup registry entry
            if (removeStartup)
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(
                        @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    key?.DeleteValue("ResolutionSwitcher", false);
                    actions.AppendLine("✓ Startup registry entry removed");
                }
                catch (Exception ex)
                {
                    actions.AppendLine($"✗ Failed to remove startup entry: {ex.Message}");
                }
            }

            // 5. Delete log file
            if (deleteLog)
            {
                try
                {
                    var logPath = Logger.Instance.LogPath;
                    bool existed = System.IO.File.Exists(logPath);
                    Logger.Instance.ClearLog();
                    actions.AppendLine(existed ? "✓ Log file deleted" : "- Log file not found");
                }
                catch (Exception ex)
                {
                    actions.AppendLine($"✗ Failed to delete log: {ex.Message}");
                }
            }

            // 6. Reset theme to light
            if (resetTheme)
            {
                ThemeManager.SetTheme(AppTheme.Light);
                actions.AppendLine("✓ Theme reset to Light mode");
            }

            if (configChanged)
            {
                ResetCompleted?.Invoke(this, EventArgs.Empty);
            }

            MessageBox.Show(
                actions.ToString(),
                "Master Reset Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Close();
        }

        private static T? FindControlByType<T>(Control parent) where T : Control
        {
            foreach (Control child in parent.Controls)
            {
                if (child is T match) return match;
                var found = FindControlByType<T>(child);
                if (found != null) return found;
            }
            return null;
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
            _warningLabel.ForeColor = ThemeManager.CurrentTheme == AppTheme.Dark
                ? ColorTranslator.FromHtml("#F39C12")
                : ColorTranslator.FromHtml("#C0392B");

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
                else if (child is Panel || child is TableLayoutPanel || child is FlowLayoutPanel)
                {
                    child.BackColor = inside ? theme.SectionBackground : theme.FormBackground;
                    child.ForeColor = theme.TextColor;
                }
                else if (child is Label label)
                {
                    if (!ReferenceEquals(label, _warningLabel))
                    {
                        label.BackColor = inside ? theme.SectionBackground : theme.FormBackground;
                        label.ForeColor = theme.TextColor;
                    }
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.BackColor = theme.SectionBackground;
                    checkBox.ForeColor = theme.TextColor;
                }
                else if (child is Button button)
                {
                    if (ReferenceEquals(button, _performResetButton))
                    {
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderColor = ControlPaint.Dark(button.BackColor);
                    }
                    else
                    {
                        ThemeManager.ApplyButtonStyle(button);
                    }
                }

                if (child.HasChildren)
                {
                    ApplyThemeRecursive(child, inside);
                }
            }
        }
    }
}
