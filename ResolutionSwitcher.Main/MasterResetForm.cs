using System;
using System.Drawing;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public class MasterResetForm : Form
    {
        private Panel _titlePanel = null!;
        private Label _titleLabel = null!;
        private Label _warningLabel = null!;
        private Button _performResetButton = null!;

        public MasterResetForm()
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

            Text = "Master Reset";
            Width = 520;
            Height = 400;
            MinimumSize = new Size(460, 340);
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

            optionsFlow.Controls.Add(new CheckBox { Text = "Delete all profiles", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
            optionsFlow.Controls.Add(new CheckBox { Text = "Clear saved monitor defaults", Checked = true, AutoSize = true, Font = new Font("Tahoma", 8f) });
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
                "Are you sure? This will delete all your profiles and settings. This cannot be undone.",
                "Confirm Master Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show(
                    "Master reset complete. Please restart the app.",
                    "Master Reset",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Close();
            }
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
