using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Creates a Windows desktop .lnk shortcut that points at the running
    /// ResolutionSwitcher executable and uses its embedded custom icon.
    /// Uses late-bound "WScript.Shell" COM automation (Type.InvokeMember)
    /// so no COM interop wrapper, tlbimp reference, or NuGet package is required.
    /// </summary>
    public static class ShortcutManager
    {
        private const string ShortcutFileName = "ResolutionSwitcher.lnk";
        private const string FirstRunRegistryPath = @"Software\ResolutionSwitcher";
        private const string FirstRunValueName = "DesktopShortcutOffered";
        private const BindingFlags InvokeMethod = BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags SetProperty = BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance;

        public static string DesktopShortcutPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), ShortcutFileName);

        public static bool DesktopShortcutExists() => File.Exists(DesktopShortcutPath);

        /// <summary>
        /// True the very first time this Windows user account runs the app.
        /// Backed by HKCU (not config.json) so that handing the published app
        /// folder to someone else - or copying it to another PC/account - still
        /// results in a genuine first run for them, instead of inheriting the
        /// original user's "already offered" state.
        /// </summary>
        public static bool IsFirstRunForThisUser()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(FirstRunRegistryPath, false);
                return key?.GetValue(FirstRunValueName) == null;
            }
            catch
            {
                return false;
            }
        }

        public static void MarkFirstRunHandled()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(FirstRunRegistryPath, true);
                key?.SetValue(FirstRunValueName, 1);
            }
            catch
            {
                // Non-fatal: worst case the shortcut prompt/creation is offered again next launch.
            }
        }

        /// <summary>
        /// Creates (or overwrites) a desktop shortcut targeting the running executable.
        /// The shortcut's icon is set to the exe's own embedded icon (the custom
        /// retro-monitor .ico wired up via ApplicationIcon), so the desktop icon
        /// visually matches the app's title bar, taskbar, and tray icon.
        /// </summary>
        public static bool CreateDesktopShortcut(out string? errorMessage)
        {
            errorMessage = null;
            object? shell = null;
            object? shortcut = null;

            try
            {
                var exePath = Application.ExecutablePath;
                var workingDir = Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory;

                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    errorMessage = "WScript.Shell COM component is not available on this system.";
                    return false;
                }

                shell = Activator.CreateInstance(shellType);
                if (shell == null)
                {
                    errorMessage = "Could not create the WScript.Shell COM object.";
                    return false;
                }

                shortcut = shellType.InvokeMember("CreateShortcut", InvokeMethod, null, shell, new object[] { DesktopShortcutPath });
                if (shortcut == null)
                {
                    errorMessage = "Could not create the shortcut COM object.";
                    return false;
                }

                var shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", SetProperty, null, shortcut, new object[] { exePath });
                shortcutType.InvokeMember("WorkingDirectory", SetProperty, null, shortcut, new object[] { workingDir });
                shortcutType.InvokeMember("Description", SetProperty, null, shortcut, new object[] { "Launch ResolutionSwitcher" });
                shortcutType.InvokeMember("IconLocation", SetProperty, null, shortcut, new object[] { $"{exePath},0" });
                shortcutType.InvokeMember("Save", InvokeMethod, null, shortcut, null);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (shortcut != null && Marshal.IsComObject(shortcut))
                {
                    Marshal.ReleaseComObject(shortcut);
                }

                if (shell != null && Marshal.IsComObject(shell))
                {
                    Marshal.ReleaseComObject(shell);
                }
            }
        }
    }
}
