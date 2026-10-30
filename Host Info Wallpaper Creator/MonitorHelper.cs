using System.Runtime.InteropServices;

namespace Host_Info_Wallpaper_Creator
{
    public static class MonitorHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        private delegate bool MonitorEnumProc(
            IntPtr hMonitor,
            IntPtr hdcMonitor,
            ref RECT lprcMonitor,
            IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(
            IntPtr hdc,
            IntPtr lprcClip,
            MonitorEnumProc lpfnEnum,
            IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(
            IntPtr hMonitor,
            ref MONITORINFO lpmi);

        public static List<(int Width, int Height)> GetAllMonitors()
        {
            var monitors = new List<(int Width, int Height)>();

            EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor,
                 ref RECT rect, IntPtr data) =>
                {
                    var info = new MONITORINFO();
                    info.cbSize = Marshal.SizeOf<MONITORINFO>();

                    if (GetMonitorInfo(hMonitor, ref info))
                    {
                        monitors.Add((
                            info.rcMonitor.Right - info.rcMonitor.Left,
                            info.rcMonitor.Bottom - info.rcMonitor.Top
                        ));
                    }

                    return true;
                },
                IntPtr.Zero);

            return monitors;
        }
    }
}