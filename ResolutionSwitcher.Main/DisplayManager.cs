using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ResolutionSwitcher.Main
{
    /// <summary>
    /// Manages display resolution changes using Windows API
    /// Provides safe, validated resolution switching
    /// </summary>
    public class DisplayManager
    {
        private static readonly Logger _logger = Logger.Instance;

        // Windows API P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int CDS_TEST = 0x00000004;
        private const int DISP_CHANGE_SUCCESSFUL = 0;
        private const int DISP_CHANGE_BADMODE = -2;
        private const int DISP_CHANGE_NOTUPDATED = -3;

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags]
        public enum DisplayDeviceStateFlags : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000,
        }

        public class MonitorInfo
        {
            public int Id { get; set; }
            public string DeviceName { get; set; } = "";
            public string FriendlyName { get; set; } = "";
            public bool IsPrimary { get; set; }
            public uint Width { get; set; }
            public uint Height { get; set; }
            public uint RefreshRate { get; set; }
        }

        /// <summary>
        /// Gets all connected monitors with their current resolution and refresh rate
        /// </summary>
        public static List<MonitorInfo> GetMonitors()
        {
            var monitors = new List<MonitorInfo>();
            uint displayIndex = 0;

            try
            {
                _logger.LogInfo("Enumerating connected monitors...");

                while (true)
                {
                    DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
                    displayDevice.cb = Marshal.SizeOf(displayDevice);

                    if (!EnumDisplayDevices(null, displayIndex, ref displayDevice, 0))
                        break;

                    if ((displayDevice.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0)
                    {
                        displayIndex++;
                        continue;
                    }

                    var deviceName = displayDevice.DeviceName.Trim('\0');
                    var friendlyName = displayDevice.DeviceString.Trim('\0');

                    DEVMODE devMode = new DEVMODE();
                    if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode) != 0)
                    {
                        var isPrimary = (displayDevice.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != 0;

                        var monitor = new MonitorInfo
                        {
                            Id = (int)displayIndex + 1,
                            DeviceName = deviceName,
                            FriendlyName = !string.IsNullOrEmpty(friendlyName) ? friendlyName : $"Monitor {displayIndex + 1}",
                            IsPrimary = isPrimary,
                            Width = devMode.dmPelsWidth,
                            Height = devMode.dmPelsHeight,
                            RefreshRate = devMode.dmDisplayFrequency
                        };

                        monitors.Add(monitor);
                        _logger.LogInfo($"Found monitor: {monitor.FriendlyName} ({monitor.Width}x{monitor.Height}@{monitor.RefreshRate}Hz)");
                    }

                    displayIndex++;
                }

                _logger.LogSuccess($"Enumerated {monitors.Count} monitor(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error enumerating monitors", ex);
                throw new Exception($"Error enumerating monitors: {ex.Message}");
            }

            return monitors;
        }

        /// <summary>
        /// Changes the resolution of a specific monitor
        /// </summary>
        public static bool ChangeResolution(string deviceName, uint width, uint height, uint refreshRate)
        {
            try
            {
                _logger.LogInfo($"Attempting to change resolution on {deviceName} to {width}x{height}@{refreshRate}Hz");

                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

                if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode) == 0)
                {
                    _logger.LogError($"Failed to enumerate current settings for {deviceName}");
                    return false;
                }

                devMode.dmPelsWidth = width;
                devMode.dmPelsHeight = height;
                devMode.dmDisplayFrequency = refreshRate;
                devMode.dmFields = 0x180000 | 0x00001000 | 0x00000400;

                int result = ChangeDisplaySettings(ref devMode, CDS_UPDATEREGISTRY);

                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    _logger.LogSuccess($"Resolution changed successfully to {width}x{height}@{refreshRate}Hz");
                    return true;
                }
                else
                {
                    string errorMsg = result switch
                    {
                        DISP_CHANGE_BADMODE => "Resolution not supported by monitor",
                        DISP_CHANGE_NOTUPDATED => "Display driver failed to update",
                        _ => $"Unknown error code: {result}"
                    };
                    _logger.LogError($"Failed to change resolution: {errorMsg}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during resolution change", ex);
                throw new Exception($"Error changing resolution: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests if a resolution is supported without applying it
        /// </summary>
        public static bool TestResolution(string deviceName, uint width, uint height, uint refreshRate)
        {
            try
            {
                _logger.LogInfo($"Testing resolution {width}x{height}@{refreshRate}Hz on {deviceName}");

                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

                if (EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode) == 0)
                    return false;

                devMode.dmPelsWidth = width;
                devMode.dmPelsHeight = height;
                devMode.dmDisplayFrequency = refreshRate;
                devMode.dmFields = 0x180000 | 0x00001000 | 0x00000400;

                int result = ChangeDisplaySettings(ref devMode, CDS_TEST);
                bool isSupported = result == DISP_CHANGE_SUCCESSFUL;

                _logger.LogInfo($"Resolution test result: {(isSupported ? "Supported" : "Not supported")}");
                return isSupported;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during resolution test", ex);
                return false;
            }
        }
    }
}
