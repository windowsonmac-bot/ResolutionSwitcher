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
        if (hProcess == IntPtr.Zero)
        {
            RevertResolution(deviceName, width, height, refreshRate);
            return 0;
        }

        try
        {
            WaitForSingleObject(hProcess, INFINITE);
        }
        finally
        {
            CloseHandle(hProcess);
        }

        RevertResolution(deviceName, width, height, refreshRate);
        return 0;
    }

    private static void RevertResolution(string deviceName, uint width, uint height, uint refreshRate)
    {
        try
        {
            var devMode = new DEVMODE();
            devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

            EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode);

            devMode.dmPelsWidth = width;
            devMode.dmPelsHeight = height;
            devMode.dmDisplayFrequency = refreshRate;
            devMode.dmFields = 0x00080000 | 0x00100000 | 0x00400000;

            ChangeDisplaySettingsEx(deviceName, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
        }
        catch { }
    }
}
