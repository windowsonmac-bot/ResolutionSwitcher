using System;
using System.Diagnostics;
using System.IO;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Handles game launching via Steam or direct path
    /// Ensures zero game file modifications
    /// </summary>
    public class GameLauncher
    {
        private static readonly Logger _logger = Logger.Instance;

        public enum LaunchMethod
        {
            Steam,
            SteamAppId,
            DirectEXE,
            Custom
        }

        /// <summary>
        /// Launches a game using the specified method
        /// Returns the process ID if successful, -1 otherwise
        /// </summary>
        public static int LaunchGame(LaunchMethod method, string launchPath, string? additionalArgs = null)
        {
            try
            {
                _logger.LogInfo($"Launching game via {method} with path: {launchPath}");

                int processId = method switch
                {
                    LaunchMethod.Steam => LaunchViaSteam(launchPath),
                    LaunchMethod.SteamAppId => LaunchViaSteamAppId(launchPath, additionalArgs),
                    LaunchMethod.DirectEXE => LaunchDirectEXE(launchPath),
                    LaunchMethod.Custom => LaunchCustom(launchPath, additionalArgs),
                    _ => throw new Exception($"Unknown launch method: {method}")
                };

                if (processId > 0)
                {
                    _logger.LogSuccess($"Game launched successfully with PID: {processId}");
                    return processId;
                }
                else
                {
                    _logger.LogError("Failed to launch game - invalid process ID");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception while launching game", ex);
                throw new Exception($"Error launching game: {ex.Message}");
            }
        }

        /// <summary>
        /// Launches game via Steam using app ID (e.g., steam://run/730).
        /// Steam relaunches the game under its own process tree, so the app ID itself is never
        /// a valid process name. The actual game PID is found via a short, bounded check
        /// (launch-time only, not continuous polling while gaming) using the profile's known
        /// executable name as a hint.
        /// </summary>
        private static int LaunchViaSteamAppId(string appId, string? exeHintPath)
        {
            try
            {
                _logger.LogInfo($"Launching via Steam app ID: {appId}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = $"steam://run/{appId}",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                Process.Start(startInfo);

                var processName = !string.IsNullOrEmpty(exeHintPath)
                    ? Path.GetFileNameWithoutExtension(exeHintPath)
                    : null;

                if (string.IsNullOrEmpty(processName))
                {
                    _logger.LogError("Cannot detect the launched game process: no executable name known for this profile. Re-run Steam scan to link this profile to its game executable.");
                    return -1;
                }

                // Bounded, one-time check at launch (NOT continuous scanning while gaming) -
                // Steam can take a few seconds to actually spawn the game process.
                const int pollIntervalMs = 2000;
                const int maxAttempts = 5;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    System.Threading.Thread.Sleep(pollIntervalMs);

                    var gameProcess = FindGameProcess(processName);
                    if (gameProcess != null)
                    {
                        return gameProcess.Id;
                    }
                }

                _logger.LogError($"Timed out waiting for '{processName}' to start via Steam (AppID: {appId})");
                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error launching via Steam app ID", ex);
                throw;
            }
        }

        /// <summary>
        /// Launches game via default Steam client (for already-configured games)
        /// </summary>
        private static int LaunchViaSteam(string gameName)
        {
            try
            {
                _logger.LogInfo($"Launching via Steam: {gameName}");

                // Open Steam if not running
                try
                {
                    var steamPath = FindSteamPath();
                    if (!string.IsNullOrEmpty(steamPath) && !IsProcessRunning("Steam"))
                    {
                        Process.Start(steamPath);
                        System.Threading.Thread.Sleep(3000);
                    }
                }
                catch { /* Steam might already be running */ }

                return -1; // Caller should monitor for game process
            }
            catch (Exception ex)
            {
                _logger.LogError("Error launching via Steam", ex);
                throw;
            }
        }

        /// <summary>
        /// Launches game directly from EXE path
        /// </summary>
        private static int LaunchDirectEXE(string exePath)
        {
            try
            {
                _logger.LogInfo($"Launching direct EXE: {exePath}");

                if (!File.Exists(exePath))
                {
                    _logger.LogError($"EXE file not found: {exePath}");
                    throw new Exception($"Game executable not found: {exePath}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        return process.Id;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error launching direct EXE", ex);
                throw;
            }
        }

        /// <summary>
        /// Launches game with custom path and arguments
        /// </summary>
        private static int LaunchCustom(string exePath, string? args)
        {
            try
            {
                _logger.LogInfo($"Launching custom: {exePath} with args: {args}");

                if (!File.Exists(exePath))
                {
                    _logger.LogError($"EXE file not found: {exePath}");
                    throw new Exception($"Game executable not found: {exePath}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args ?? "",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        return process.Id;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error launching custom", ex);
                throw;
            }
        }

        /// <summary>
        /// Finds a running process by name
        /// </summary>
        private static Process? FindGameProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0 ? processes[0] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error finding game process", ex);
                return null;
            }
        }

        /// <summary>
        /// Checks if a process is running
        /// </summary>
        private static bool IsProcessRunning(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds Steam installation path
        /// </summary>
        private static string? FindSteamPath()
        {
            try
            {
                var steamPath = @"C:\Program Files (x86)\Steam\Steam.exe";
                if (File.Exists(steamPath)) return steamPath;

                steamPath = @"C:\Program Files\Steam\Steam.exe";
                if (File.Exists(steamPath)) return steamPath;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
