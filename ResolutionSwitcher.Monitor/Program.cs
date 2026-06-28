using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ResolutionSwitcher.Monitor
{
    /// <summary>
    /// Lightweight background process that monitors game and reverts resolution
    /// Uses kernel-level WaitForSingleObject for zero CPU usage
    /// Completely standalone, no dependencies on main app
    /// </summary>
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        private const uint INFINITE = 0xFFFFFFFF;
        private const uint WAIT_OBJECT_0 = 0;
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [StructLayout(LayoutKind.Sequential)]
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
            try
            {
                // Parse command line arguments
                // Expected: ResolutionSwitcher.Monitor.exe <gamePid> <deviceName> <width> <height> <refreshRate>

                if (args.Length < 5)
                {
                    Console.WriteLine("Invalid arguments");
                    return -1;
                }

                int gamePid = int.Parse(args[0]);
                string deviceName = args[1];
                uint width = uint.Parse(args[2]);
                uint height = uint.Parse(args[3]);
                uint refreshRate = uint.Parse(args[4]);

                // Wait for game process to exit
                bool success = WaitForGameClose(gamePid);

                if (success)
                {
                    // Revert resolution
                    RevertResolution(deviceName, width, height, refreshRate);
                }

                // Exit
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Waits for game process to exit using OS-level kernel wait
        /// Zero CPU usage, pure event-based waiting
        /// </summary>
        private static bool WaitForGameClose(int processId)
        {
            try
            {
                using (var process = Process.GetProcessById(processId))
                {
                    // This uses kernel-level waiting
                    // OS suspends the thread until the process exits
                    // Zero CPU spinning, zero polling
                    uint result = WaitForSingleObject(process.Handle, INFINITE);
                    return result == WAIT_OBJECT_0;
                }
            }
            catch
            {
                // Process might have already exited
                return true;
            }
        }

        /// <summary>
        /// Reverts display resolution to saved defaults
        /// </summary>
        private static bool RevertResolution(string deviceName, uint width, uint height, uint refreshRate)
        {
            try
            {
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

                if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode) == 0)
                    return false;

                devMode.dmPelsWidth = width;
                devMode.dmPelsHeight = height;
                devMode.dmDisplayFrequency = refreshRate;
                devMode.dmFields = 0x180000 | 0x00001000 | 0x00000400;

                int result = ChangeDisplaySettings(ref devMode, CDS_UPDATEREGISTRY);
                return result == DISP_CHANGE_SUCCESSFUL;
            }
            catch
            {
                return false;
            }
        }
    }
}
