using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Host_Info_Wallpaper_Creator
{
    public class WallpaperConfig
    {
        public bool UseImage { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public Color BackgroundColor { get; set; } = Color.Black;
        public string FontFamilyName { get; set; } = "Arial";
        public int BigFontSize { get; set; } = 80;
        public int SmallFontSize { get; set; } = 25;
        public bool ShowHostname { get; set; } = true;
        public bool ShowIp { get; set; } = true;
        public bool ShowCores { get; set; } = true;
        public bool ShowRam { get; set; } = true;
        public bool ShowDrives { get; set; } = true;
    }

    public class WallpaperEngine
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        public static void ApplySysInfoWallpaper(WallpaperConfig config)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper_with_info.png");

            // Fetch System Info
            string hostname = Environment.MachineName;
            string ip = $"IP: {LocalIPAddress}";
            string cores = $"Cores: {CpuCores}";
            string ram = $"Ram: {RamGB}";

            Image img;
            int maxWidth = 0;
            int maxHeight = 0;

            foreach (var (Width, Height) in MonitorHelper.GetAllMonitors())
            {
                if (Width > maxWidth) maxWidth = Width;
                if (Height > maxHeight) maxHeight = Height;
            }

            // Load Image OR Create Solid Color Canvas spanning the full virtual desktop bounds
            if (config.UseImage)
            {
                if (!File.Exists(config.ImagePath)) throw new FileNotFoundException("Selected wallpaper image not found.");
                img = Image.FromFile(config.ImagePath);
                img = new Bitmap(img, new Size(maxWidth, maxHeight)); 
            }
            else
            { 
                img = new Bitmap(maxWidth, maxHeight);
                using Graphics bg = Graphics.FromImage(img);
                bg.Clear(config.BackgroundColor);
            }          

           

            using (img)
            using (Graphics g = Graphics.FromImage(img))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (Font fontBig = new(config.FontFamilyName, config.BigFontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                using (Font fontSmall = new(config.FontFamilyName, config.SmallFontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Brush textBrush = new SolidBrush(Color.White))
                using (Brush outlineBrush = new SolidBrush(Color.Black))
                {
                    // Measure strings
                    SizeF hostSize = config.ShowHostname ? g.MeasureString(hostname, fontBig) : SizeF.Empty;
                    SizeF ipSize = config.ShowIp ? g.MeasureString(ip, fontSmall) : SizeF.Empty;
                    SizeF coresSize = config.ShowCores ? g.MeasureString(cores, fontSmall) : SizeF.Empty;
                    SizeF ramSize = config.ShowRam ? g.MeasureString(ram, fontSmall) : SizeF.Empty;

                    // Calculate X position based on the widest active text element
                    float maxTextWidth = new float[] { ipSize.Width, coresSize.Width, ramSize.Width }.Max();
                    float maxHostNameTextWidth = new float[] { hostSize.Width }.Max();
                    float xBase = img.Width - maxTextWidth - 40;
                    float xBaseHost = img.Width - maxHostNameTextWidth - 40;
                    float currentY = 20;

                    var drivesInfo = config.ShowDrives ? DrivesInfo : [];

                    // Calculate X position based on the widest active text element
                    if (config.ShowHostname)
                    {
                        DrawOutlinedText(g, hostname, fontBig, outlineBrush, textBrush, xBaseHost, currentY, 5);
                        currentY += hostSize.Height + 5;
                    }
                    if (config.ShowIp)
                    {
                        DrawOutlinedText(g, ip, fontSmall, outlineBrush, textBrush, xBase, currentY, 2);
                        currentY += ipSize.Height + 5;
                    }
                    if (config.ShowCores)
                    {
                        DrawOutlinedText(g, cores, fontSmall, outlineBrush, textBrush, xBase, currentY, 2);
                        currentY += coresSize.Height + 5;
                    }
                    if (config.ShowRam)
                    {
                        DrawOutlinedText(g, ram, fontSmall, outlineBrush, textBrush, xBase, currentY, 2);
                        currentY += ramSize.Height + 5;
                    }
                    if (config.ShowDrives)
                    {
                        // Loop through each drive and draw it on its own line
                        foreach (string driveText in drivesInfo)
                        {
                            DrawOutlinedText(g, driveText, fontSmall, outlineBrush, textBrush, xBase, currentY, 2);
                            currentY += g.MeasureString(driveText, fontSmall).Height + 5;
                        }
                    }
                }

                img.Save(tempPath, ImageFormat.Png);
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (key != null)
                {
                    key.SetValue("WallpaperStyle", "2");
                    key.SetValue("TileWallpaper", "0");
                }
            }

            _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }


        private static List<string> DrivesInfo
        {
            get
            {
                var drivesList = new List<string>();
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    // Only check drives that are ready to avoid errors with empty CD-ROMs or unmounted drives
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        double totalGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                        drivesList.Add($"Drive {drive.Name} {totalGB:F1} GB");
                    }
                }
                return drivesList;
            }
        }

        private static string LocalIPAddress
        {
            get
            {
                var activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                          (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                                           ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet));

                if (activeInterface != null)
                {
                    var ipProps = activeInterface.GetIPProperties();
                    var ipv4 = ipProps.UnicastAddresses.FirstOrDefault(ip =>
                        ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                        !ip.Address.ToString().StartsWith("169.254"));

                    if (ipv4 != null) return ipv4.Address.ToString();
                }
                return "Unknown";
            }
        }

        private static string CpuCores
        {
            get
            {
                using ManagementObjectSearcher searcher = new("select NumberOfLogicalProcessors from Win32_Processor");
                foreach (ManagementObject item in searcher.Get().Cast<ManagementObject>())
                {
                    return item["NumberOfLogicalProcessors"].ToString() ?? "";
                }
                return "Unknown";
            }
        }

        private static string RamGB
        {
            get
            {
                long totalCapacity = 0;
                using (ManagementObjectSearcher searcher = new("select Capacity from Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject item in searcher.Get().Cast<ManagementObject>())
                    {
                        totalCapacity += Convert.ToInt64(item["Capacity"]);
                    }
                }
                return (totalCapacity / 1024 / 1024 / 1024).ToString() + " GB";
            }
        }



        private static void DrawOutlinedText(Graphics g, string text, Font font, Brush outline, Brush fill, float x, float y, int thickness)
        {
            for (int dx = -thickness; dx <= thickness; dx++)
            {
                for (int dy = -thickness; dy <= thickness; dy++)
                {
                    g.DrawString(text, font, outline, x + dx, y + dy);
                }
            }
            g.DrawString(text, font, fill, x, y);
        }
    }
}
