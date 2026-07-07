using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public class DebugForm : Form
    {
        private Panel _titlePanel = null!;
        private Label _titleLabel = null!;
        private GroupBox _logGroup = null!;
        private RichTextBox _logRichTextBox = null!;
        private CheckBox _enableLoggingCheckBox = null!;
        private readonly ConfigManager? _configManager;

        public DebugForm(ConfigManager? configManager = null)
        {
            _configManager = configManager;
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            ApplyTheme();
            LoadLogFromDisk();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            base.OnFormClosed(e);
        }

        private void InitializeUI()
        {
            SuspendLayout();

            Text = "Debug & Logging";
            Width = 1100;
            Height = 750;
            MinimumSize = new Size(800, 560);
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
                Text = "Debug && Logging",
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
                RowCount = 2,
                Padding = new Padding(10)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _logGroup = new GroupBox
            {
                Text = "Log Output",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8f, FontStyle.Bold),
                Padding = new Padding(8, 10, 8, 8)
            };

            _logRichTextBox = new RichTextBox
            {
                Name = "_logRichTextBox",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Courier New", 8.5f),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = false,
                BorderStyle = BorderStyle.None,
                Text = "[Debug logging is currently disabled. Enable below to start capturing.]"
            };
            _logGroup.Controls.Add(_logRichTextBox);

            var controlsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 4),
                Margin = new Padding(0, 8, 0, 0)
            };

            _enableLoggingCheckBox = new CheckBox
            {
                Name = "_enableLoggingCheckBox",
                Text = "Enable debug logging",
                AutoSize = true,
                Margin = new Padding(0, 4, 12, 4),
                Checked = Logger.Instance.IsLoggingEnabled
            };
            _enableLoggingCheckBox.CheckedChanged += EnableLoggingCheckBox_CheckedChanged;

            var openLogFileButton = new Button
            {
                Text = "Open Log File",
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(4, 2, 0, 2)
            };
            openLogFileButton.Click += OpenLogFileButton_Click;

            var clearLogButton = new Button
            {
                Text = "Clear Log",
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(4, 2, 0, 2)
            };
            clearLogButton.Click += ClearLogButton_Click;

            var refreshButton = new Button
            {
                Text = "Refresh",
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(4, 2, 0, 2)
            };
            refreshButton.Click += (_, _) => LoadLogFromDisk();

            var copyAllButton = new Button
            {
                Text = "Copy All",
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Font = new Font("Tahoma", 8f),
                Margin = new Padding(4, 2, 0, 2)
            };
            copyAllButton.Click += (_, _) =>
            {
                if (!string.IsNullOrEmpty(_logRichTextBox.Text))
                {
                    Clipboard.SetText(_logRichTextBox.Text);
                }
            };

            controlsPanel.Controls.Add(_enableLoggingCheckBox);
            controlsPanel.Controls.Add(openLogFileButton);
            controlsPanel.Controls.Add(clearLogButton);
            controlsPanel.Controls.Add(refreshButton);
            controlsPanel.Controls.Add(copyAllButton);

            content.Controls.Add(_logGroup, 0, 0);
            content.Controls.Add(controlsPanel, 0, 1);

            Controls.Add(content);
            Controls.Add(_titlePanel);

            ResumeLayout(false);
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void EnableLoggingCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            Logger.Instance.SetLoggingEnabled(_enableLoggingCheckBox.Checked);

            if (_configManager != null)
            {
                _configManager.GetConfig().Behavior.EnableDebugLogging = _enableLoggingCheckBox.Checked;
                _configManager.Save();
            }

            LoadLogFromDisk();
        }

        private void OpenLogFileButton_Click(object? sender, EventArgs e)
        {
            var logPath = Logger.Instance.LogPath;
            if (!System.IO.File.Exists(logPath))
            {
                MessageBox.Show($"No log file yet.\n\nIt will be created at:\n{logPath}\n\nonce debug logging is enabled and an event occurs.", "Log File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(logPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open log file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearLogButton_Click(object? sender, EventArgs e)
        {
            Logger.Instance.ClearLog();
            _logRichTextBox.Clear();
            _logRichTextBox.Text = "[Log cleared.]";
        }

        private void LoadLogFromDisk()
        {
            var contents = Logger.Instance.ReadLogContents();
            _logRichTextBox.Text = string.IsNullOrEmpty(contents)
                ? "[Debug logging is currently disabled or the log is empty. Enable logging above to start capturing.]"
                : contents;
            _logRichTextBox.SelectionStart = _logRichTextBox.Text.Length;
            _logRichTextBox.ScrollToCaret();
        }

        private void ApplyTheme()
        {
            var theme = ThemeManager.Palette;

            BackColor = theme.FormBackground;
            ForeColor = theme.TextColor;
            _titlePanel.BackColor = theme.TitleBarColor;
            _titleLabel.ForeColor = theme.TitleBarTextColor;
            _logGroup.ForeColor = theme.TextColor;
            _logRichTextBox.BackColor = theme.StatusBackground;
            _logRichTextBox.ForeColor = theme.TextColor;

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
                else if (child is Label || child is CheckBox)
                {
                    child.BackColor = inside ? theme.SectionBackground : theme.FormBackground;
                    child.ForeColor = theme.TextColor;
                }
                else if (child is RichTextBox)
                {
                    continue;
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
