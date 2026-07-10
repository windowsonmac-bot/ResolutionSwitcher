using System;
using System.IO;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Resolves the folder the running executable actually lives in on disk.
    /// For single-file self-contained publishes (PublishSingleFile +
    /// IncludeAllContentForSelfExtract), AppDomain.CurrentDomain.BaseDirectory /
    /// AppContext.BaseDirectory resolve to a temporary extraction folder under
    /// %TEMP%\.net\... instead of the folder containing the published exe, so
    /// config/log/theme files and the helper exe lookup must anchor off
    /// Environment.ProcessPath instead.
    /// </summary>
    public static class AppPaths
    {
        public static string ExecutableDirectory { get; } =
            Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
    }
}
