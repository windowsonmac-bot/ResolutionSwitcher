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
                    LaunchMethod.SteamAppId => LaunchViaSteamAppId(launchPath),
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
        /// Launches game via Steam using app ID (e.g., steam://run/730)
        /// </summary>
        private static int LaunchViaSteamAppId(string appId)
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

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        System.Threading.Thread.Sleep(2000); // Give Steam time to launch game
                        var gameProcess = FindGameProcess(appId);
                        if (gameProcess != null)
                        {
                            return gameProcess.Id;
                        }
                    }
                }

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
