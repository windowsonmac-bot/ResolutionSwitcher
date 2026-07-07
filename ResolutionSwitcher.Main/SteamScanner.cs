using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ResolutionSwitcher.Main
{
    public class SteamGame
    {
        public string AppId { get; set; } = "";
        public string Name { get; set; } = "";
        public string InstallDir { get; set; } = "";
        public string ExePath { get; set; } = "";
    }

    public static class SteamScanner
    {
        private static readonly Logger _logger = Logger.Instance;

        public static string? FindSteamInstallPath()
        {
            // Check HKLM first (typical system-wide install), then HKCU
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"Software\Valve\Steam")
                             ?? Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Valve\Steam");
                var path = key?.GetValue("InstallPath") as string;
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    return path;
            }
            catch { }

            // Fallback: HKCU (per-user install)
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var path = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    return path;
            }
            catch { }

            // Fallback to common paths
            var commonPaths = new[]
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam")
            };

            foreach (var p in commonPaths)
            {
                if (Directory.Exists(p) && File.Exists(Path.Combine(p, "steam.exe")))
                    return p;
            }

            return null;
        }

        public static List<string> GetSteamLibraryPaths()
        {
            var libraries = new List<string>();
            var steamPath = FindSteamInstallPath();
            if (steamPath == null) return libraries;

            var defaultLib = Path.Combine(steamPath, "steamapps");
            if (Directory.Exists(defaultLib))
                libraries.Add(defaultLib);

            // Parse libraryfolders.vdf
            var vdfPath = Path.Combine(defaultLib, "libraryfolders.vdf");
            if (!File.Exists(vdfPath)) return libraries;

            try
            {
                var content = File.ReadAllText(vdfPath);
                // Match "path" entries
                var matches = Regex.Matches(content, @"""path""\s+""([^""]+)""");
                foreach (Match m in matches)
                {
                    var libPath = m.Groups[1].Value.Replace(@"\\", @"\");
                    var steamappsPath = Path.Combine(libPath, "steamapps");
                    if (Directory.Exists(steamappsPath) && !libraries.Contains(steamappsPath))
                        libraries.Add(steamappsPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error parsing libraryfolders.vdf", ex);
            }

            return libraries;
        }

        public static List<SteamGame> GetInstalledGames()
        {
            var games = new List<SteamGame>();

            try
            {
                var libraries = GetSteamLibraryPaths();

                foreach (var library in libraries)
                {
                    try
                    {
                        var manifests = Directory.GetFiles(library, "appmanifest_*.acf");
                        foreach (var manifest in manifests)
                        {
                            try
                            {
                                var game = ParseAppManifest(manifest, library);
                                if (game != null)
                                    games.Add(game);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                games.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                _logger.LogInfo($"Steam scan found {games.Count} installed games");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error scanning Steam library", ex);
            }

            return games;
        }

        private static SteamGame? ParseAppManifest(string manifestPath, string libraryPath)
        {
            var content = File.ReadAllText(manifestPath);

            var appIdMatch = Regex.Match(content, @"""appid""\s+""?(\d+)""?");
            var nameMatch = Regex.Match(content, @"""name""\s+""([^""]+)""");
            var installDirMatch = Regex.Match(content, @"""installdir""\s+""([^""]+)""");

            if (!appIdMatch.Success || !nameMatch.Success || !installDirMatch.Success)
                return null;

            var appId = appIdMatch.Groups[1].Value;
            var name = nameMatch.Groups[1].Value;
            var installDir = installDirMatch.Groups[1].Value;
            var fullInstallPath = Path.Combine(libraryPath, "common", installDir);

            if (!Directory.Exists(fullInstallPath))
                return null;

            // Try to find the main exe
            var exePath = FindMainExe(fullInstallPath, name);

            return new SteamGame
            {
                AppId = appId,
                Name = name,
                InstallDir = fullInstallPath,
                ExePath = exePath ?? ""
            };
        }

        /// <summary>
        /// Filename substrings of bundled tool/debug/redistributable executables that are
        /// commonly shipped alongside the real game binary but are never the correct launch
        /// target (e.g. Source 2's VConsole2, anti-cheat installers, crash handlers).
        /// </summary>
        private static readonly string[] NonGameExeMarkers =
        {
            "vconsole", "crashpad", "crashhandler", "crashreporter", "crashreport",
            "easyanticheat", "eac_launcher", "eac_setup", "battleye", "beservice",
            "unitycrashhandler", "unins000", "uninstall", "vc_redist", "vcredist",
            "dotnetfx", "directx", "dxsetup", "dxwebsetup", "redist", "dedicated_server",
            "srcds", "steamservice", "steamerrorreporter"
        };

        private static bool IsLikelyNonGameExe(string exePath)
        {
            var name = Path.GetFileNameWithoutExtension(exePath).ToLowerInvariant();
            foreach (var marker in NonGameExeMarkers)
            {
                if (name.Contains(marker)) return true;
            }
            return false;
        }

        private static string? FindMainExe(string installDir, string gameName)
        {
            try
            {
                // Look for exe with similar name to game first
                var exes = Directory.GetFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly);
                if (exes.Length == 0)
                    exes = Directory.GetFiles(installDir, "*.exe", SearchOption.AllDirectories);

                if (exes.Length == 0) return null;
                if (exes.Length == 1) return exes[0];

                // Exclude known bundled tool/debug/redist binaries before any matching so they
                // never win the name-similarity or size fallback below (e.g. VConsole2.exe next
                // to cs2.exe in Source 2 games).
                var candidates = Array.FindAll(exes, e => !IsLikelyNonGameExe(e));
                if (candidates.Length == 0) candidates = exes;
                if (candidates.Length == 1) return candidates[0];

                // Try to match game name
                var cleanName = Regex.Replace(gameName, @"[^a-zA-Z0-9]", "").ToLower();
                foreach (var exe in candidates)
                {
                    var exeName = Regex.Replace(Path.GetFileNameWithoutExtension(exe), @"[^a-zA-Z0-9]", "").ToLower();
                    if (exeName.Contains(cleanName) || cleanName.Contains(exeName))
                        return exe;
                }

                // Return the largest exe as likely the main one
                string? best = null;
                long bestSize = 0;
                foreach (var exe in candidates)
                {
                    try
                    {
                        var size = new FileInfo(exe).Length;
                        if (size > bestSize) { bestSize = size; best = exe; }
                    }
                    catch { }
                }
                return best;
            }
            catch { return null; }
        }
    }
}
