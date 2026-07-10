using System.Runtime.InteropServices;
namespace ResolutionSwitcher.Monitor;

static class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

    private const uint SYNCHRONIZE = 0x00100000;
    private const uint INFINITE = 0xFFFFFFFF;
    private const int CDS_UPDATEREGISTRY = 0x00000001;
    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int DISP_CHANGE_SUCCESSFUL = 0;

    // DEVMODE.dmFields bits needed to request a width/height/refresh-rate change.
    private const uint DM_PELSWIDTH = 0x00080000;
    private const uint DM_PELSHEIGHT = 0x00100000;
    private const uint DM_DISPLAYFREQUENCY = 0x00400000;

    // The game may still hold the display in exclusive-fullscreen mode for a brief
    // window after its process handle is signaled, so the first revert attempt can
    // fail. Wait briefly, then retry a few times before giving up.
    private const int SettleDelayMs = 500;
    private const int RetryDelayMs = 400;
    private const int MaxRevertAttempts = 5;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public ushort dmSpecVersion;
        public ushort dmDriverVersion;
        public ushort dmSize;
        public ushort dmDriverExtra;
        public uint dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public uint dmDisplayOrientation;
        public uint dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public ushort dmLogPixels;
        public uint dmBitsPerPel;
        public uint dmPelsWidth;
        public uint dmPelsHeight;
        public uint dmDisplayFlags;
        public uint dmDisplayFrequency;
        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;
        public uint dmPanningWidth;
        public uint dmPanningHeight;
    }

    [STAThread]
    static int Main(string[] args)
    {
        try { System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle; } catch { }

        if (args.Length < 5) return -1;

        if (!int.TryParse(args[0], out int gamePid)) return -1;
        string deviceName = args[1];
        if (!uint.TryParse(args[2], out uint width)) return -1;
        if (!uint.TryParse(args[3], out uint height)) return -1;
        if (!uint.TryParse(args[4], out uint refreshRate)) return -1;

        IntPtr hProcess = OpenProcess(SYNCHRONIZE, false, gamePid);
        if (hProcess != IntPtr.Zero)
        {
            try
            {
                WaitForSingleObject(hProcess, INFINITE);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        RevertResolution(deviceName, width, height, refreshRate);
        return 0;
    }

    /// <summary>
    /// Restores the display mode after the game exits. Some fullscreen-exclusive
    /// games still hold the display for a brief moment after their process handle
    /// is signaled, so the very first attempt can fail - wait briefly and retry a
    /// few times, checking the real return code, before giving up.
    /// </summary>
    private static void RevertResolution(string deviceName, uint width, uint height, uint refreshRate)
    {
        System.Threading.Thread.Sleep(SettleDelayMs);

        for (int attempt = 1; attempt <= MaxRevertAttempts; attempt++)
        {
            try
            {
                var devMode = new DEVMODE();
                devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

                EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode);

                devMode.dmPelsWidth = width;
                devMode.dmPelsHeight = height;
                devMode.dmDisplayFrequency = refreshRate;
                devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;

                int result = ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    return;
                }

                if (attempt == MaxRevertAttempts)
                {
                    LogFailure($"ChangeDisplaySettingsEx failed with code {result} after {attempt} attempt(s) for {deviceName} -> {width}x{height}@{refreshRate}Hz.");
                }
            }
            catch (Exception ex)
            {
                if (attempt == MaxRevertAttempts)
                {
                    LogFailure($"Exception while reverting {deviceName} -> {width}x{height}@{refreshRate}Hz: {ex.Message}");
                }
            }

            if (attempt < MaxRevertAttempts)
            {
                System.Threading.Thread.Sleep(RetryDelayMs);
            }
        }
    }

    /// <summary>
    /// Appends a single diagnostic line to monitor-helper.log next to the exe.
    /// Only ever called after every revert attempt has failed, so the helper stays
    /// silent (no disk writes at all) on the normal, successful path.
    /// </summary>
    private static void LogFailure(string message)
    {
        try
        {
            var exeDir = System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
            var logPath = Path.Combine(exeDir, "monitor-helper.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch { }
    }
}
