using System;
using System.Drawing;
using System.Windows.Forms;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Loads the application's embedded icon (Resources/AppIcon.ico, wired up via
    /// the ApplicationIcon MSBuild property) once and hands it out to every form
    /// and the tray icon, so the whole app - title bars, taskbar, Alt-Tab, and the
    /// system tray - consistently shows the same retro-monitor icon instead of the
    /// generic WinForms default.
    /// </summary>
    public static class IconProvider
    {
        private static Icon? _appIcon;

        public static Icon AppIcon => _appIcon ??= LoadIcon();

        private static Icon LoadIcon()
        {
            try
            {
                var exePath = Application.ExecutablePath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    var extracted = Icon.ExtractAssociatedIcon(exePath);
                    if (extracted != null)
                    {
                        return extracted;
                    }
                }
            }
            catch
            {
                // Fall back below if the executable's icon can't be extracted for any reason.
            }

            return SystemIcons.Application;
        }
    }
}
