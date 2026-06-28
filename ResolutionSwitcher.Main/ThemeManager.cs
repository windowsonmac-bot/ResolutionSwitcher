using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public sealed class ThemePalette
    {
        public Color FormBackground { get; init; }
        public Color SectionBackground { get; init; }
        public Color TextColor { get; init; }
        public Color GrayTextColor { get; init; }
        public Color TitleBarColor { get; init; }
        public Color TitleBarTextColor { get; init; }
        public Color SubtitleTextColor { get; init; }
        public Color ButtonFaceColor { get; init; }
        public Color ButtonBorderColor { get; init; }
        public FlatStyle ButtonFlatStyle { get; init; }
        public Color StatusBackground { get; init; }
        public Color StatusHeaderColor { get; init; }
        public Color TabInactiveBackground { get; init; }
        public Color InputBackground { get; init; }
        public Color InputBorderColor { get; init; }
    }

    public static class ThemeManager
    {
        private static readonly ThemePalette LightPalette = new ThemePalette
        {
            FormBackground = ColorTranslator.FromHtml("#ECE9D8"),
            SectionBackground = ColorTranslator.FromHtml("#F0EFE7"),
            TextColor = ColorTranslator.FromHtml("#000000"),
            GrayTextColor = SystemColors.GrayText,
            TitleBarColor = ColorTranslator.FromHtml("#004E98"),
            TitleBarTextColor = Color.White,
            SubtitleTextColor = Color.FromArgb(180, 210, 240),
            ButtonFaceColor = SystemColors.Control,
            ButtonBorderColor = SystemColors.ControlDark,
            ButtonFlatStyle = FlatStyle.System,
            StatusBackground = ColorTranslator.FromHtml("#D4D0C8"),
            StatusHeaderColor = ColorTranslator.FromHtml("#004E98"),
            TabInactiveBackground = ColorTranslator.FromHtml("#D7D3C5"),
            InputBackground = Color.White,
            InputBorderColor = SystemColors.ControlDark
        };

        private static readonly ThemePalette DarkPalette = new ThemePalette
        {
            FormBackground = ColorTranslator.FromHtml("#1E1E2E"),
            SectionBackground = ColorTranslator.FromHtml("#2A2A3E"),
            TextColor = ColorTranslator.FromHtml("#E0E0E0"),
            GrayTextColor = Color.FromArgb(180, 180, 190),
            TitleBarColor = ColorTranslator.FromHtml("#0A2A5E"),
            TitleBarTextColor = Color.White,
            SubtitleTextColor = Color.FromArgb(162, 190, 232),
            ButtonFaceColor = ColorTranslator.FromHtml("#3A3A4E"),
            ButtonBorderColor = ColorTranslator.FromHtml("#5A5A7E"),
            ButtonFlatStyle = FlatStyle.Flat,
            StatusBackground = ColorTranslator.FromHtml("#1A1A2A"),
            StatusHeaderColor = ColorTranslator.FromHtml("#4A90D9"),
            TabInactiveBackground = ColorTranslator.FromHtml("#323248"),
            InputBackground = ColorTranslator.FromHtml("#232338"),
            InputBorderColor = ColorTranslator.FromHtml("#5A5A7E")
        };

        private static readonly string ThemeConfigFilePath = Path.Combine(AppContext.BaseDirectory, "theme.cfg");

        static ThemeManager()
        {
            CurrentTheme = LoadTheme();
        }

        public static event EventHandler? ThemeChanged;

        public static AppTheme CurrentTheme { get; private set; }

        public static ThemePalette Palette => CurrentTheme == AppTheme.Dark ? DarkPalette : LightPalette;

        public static void ToggleTheme()
        {
            SetTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        }

        public static void SetTheme(AppTheme theme)
        {
            if (CurrentTheme == theme)
            {
                return;
            }

            CurrentTheme = theme;
            SaveTheme(theme);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ApplyButtonStyle(Button button)
        {
            var palette = Palette;
            button.FlatStyle = palette.ButtonFlatStyle;
            button.ForeColor = palette.TextColor;

            if (palette.ButtonFlatStyle == FlatStyle.System)
            {
                button.UseVisualStyleBackColor = true;
                button.BackColor = SystemColors.Control;
                return;
            }

            button.UseVisualStyleBackColor = false;
            button.BackColor = palette.ButtonFaceColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = palette.ButtonBorderColor;
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(palette.ButtonFaceColor);
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(palette.ButtonFaceColor);
        }

        private static AppTheme LoadTheme()
        {
            try
            {
                if (File.Exists(ThemeConfigFilePath))
                {
                    var raw = File.ReadAllText(ThemeConfigFilePath).Trim();
                    if (raw.Equals("dark", StringComparison.OrdinalIgnoreCase))
                    {
                        return AppTheme.Dark;
                    }
                }
            }
            catch
            {
                // Ignore theme load errors and fall back to the default light theme.
            }

            return AppTheme.Light;
        }

        private static void SaveTheme(AppTheme theme)
        {
            try
            {
                File.WriteAllText(ThemeConfigFilePath, theme == AppTheme.Dark ? "dark" : "light");
            }
            catch
            {
                // Ignore theme save errors so UI interaction continues even if the app folder is not writable.
            }
        }
    }
}
