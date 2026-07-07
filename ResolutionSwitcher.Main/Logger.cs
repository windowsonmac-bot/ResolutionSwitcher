using System;
using System.IO;
using System.Text;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Thread-safe logging system with optional file output
    /// Designed for minimal performance impact
    /// </summary>
    public class Logger
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private readonly string _logPath;
        private bool _enableLogging;

        private Logger()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");
            _enableLogging = false; // Disabled by default
        }

        /// <summary>
        /// Full path to the debug log file on disk.
        /// </summary>
        public string LogPath => _logPath;

        /// <summary>
        /// Whether logging is currently enabled.
        /// </summary>
        public bool IsLoggingEnabled => _enableLogging;

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new Logger();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Enable or disable logging
        /// </summary>
        public void SetLoggingEnabled(bool enabled)
        {
            _enableLogging = enabled;
            if (enabled)
            {
                Log("=== Logging Started ===");
            }
        }

        /// <summary>
        /// Log an information message
        /// </summary>
        public void LogInfo(string message)
        {
            if (_enableLogging)
            {
                WriteLog($"[INFO] {message}");
            }
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            if (_enableLogging)
            {
                WriteLog($"[WARN] {message}");
            }
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public void LogError(string message, Exception? ex = null)
        {
            if (_enableLogging)
            {
                var errorMsg = ex != null ? $"{message} - {ex.Message}" : message;
                WriteLog($"[ERROR] {errorMsg}");
            }
        }

        /// <summary>
        /// Log a success message
        /// </summary>
        public void LogSuccess(string message)
        {
            if (_enableLogging)
            {
                WriteLog($"[SUCCESS] {message}");
            }
        }

        /// <summary>
        /// Log a general message
        /// </summary>
        public void Log(string message)
        {
            if (_enableLogging)
            {
                WriteLog($"[LOG] {message}");
            }
        }

        private void WriteLog(string message)
        {
            try
            {
                lock (_lock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"{timestamp} - {message}";
                    File.AppendAllText(_logPath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Silently fail - don't let logging errors break the app
            }
        }

        /// <summary>
        /// Clear the debug log file
        /// </summary>
        public void ClearLog()
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    File.Delete(_logPath);
                }
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Reads the current contents of the debug log file, if it exists.
        /// Returns an empty string if the file is missing or cannot be read.
        /// </summary>
        public string ReadLogContents()
        {
            try
            {
                lock (_lock)
                {
                    if (!File.Exists(_logPath)) return string.Empty;
                    using var stream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
