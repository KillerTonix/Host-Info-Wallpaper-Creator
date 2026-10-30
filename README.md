# Host Info Wallpaper Creator

A small Windows desktop app that stamps key information about the current machine (hostname, IP address, CPU cores, RAM and drive sizes) onto the desktop wallpaper. It is aimed at servers, VMs and lab machines, where you want to see at a glance which host you are connected to.

<img width="1919" height="1079" alt="Host Info Wallpaper Creator screenshot" src="https://github.com/user-attachments/assets/8176f5a9-13bd-449d-a0b8-bcd09ae1168e" />

## Features

- **Choose your background**: use any image (`.jpg`, `.jpeg`, `.png`, `.bmp`) or a solid color picked with a color picker.
- **Pick what to show**: toggle each item on or off:
  - Hostname
  - IP address
  - CPU cores
  - RAM
  - Local drives
- **Adjust the typography**: set the font family, the hostname size and the info text size (in pixels).
- **Readable on any background**: text is drawn in white with a black outline in the top-right corner of the screen.
- **Multi-monitor aware**: the wallpaper is rendered at the largest monitor width and height found on the system.
- **One click**: press **Apply Wallpaper** and the desktop wallpaper is updated immediately.

## What gets displayed

| Item | How it is detected |
|------|--------------------|
| Hostname | `Environment.MachineName` |
| IP address | First active Ethernet or Wi-Fi adapter with an IPv4 address (link-local `169.254.x.x` addresses are ignored) |
| CPU cores | Logical processor count reported by WMI (`Win32_Processor`) |
| RAM | Sum of installed physical memory modules, in GB (`Win32_PhysicalMemory`) |
| Local drives | Total size of each ready, fixed drive, in GB |

## Requirements

- Windows 10 or Windows 11 (the app uses WPF, WMI and Win32 APIs, so it does not run on Linux or macOS)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build, or the .NET 10 Desktop Runtime to run a published build
- Visual Studio with the **.NET desktop development** workload (optional)

## Getting started

### Build and run from the command line

```powershell
git clone https://github.com/KillerTonix/Host-Info-Wallpaper-Creator.git
cd Host-Info-Wallpaper-Creator
dotnet run --project "Host Info Wallpaper Creator"
```

### Build with Visual Studio

1. Open `Host Info Wallpaper Creator.slnx`.
2. Set the build configuration to **Release**.
3. Press **F5** to run, or use **Build > Build Solution**.

### Publish a standalone build

```powershell
dotnet publish "Host Info Wallpaper Creator" -c Release -r win-x64 --self-contained false
```

The output ends up in `Host Info Wallpaper Creator\bin\Release\net10.0-windows\win-x64\publish\`.

## Usage

1. Start the app.
2. Under **Background**, select **Image** and click **Browse** (the default is `C:\Windows\Web\Screen\img105.jpg`), or select **Solid color** and pick a color.
3. Under **Typography**, set the font family, **Host Name Size** and **Info Size**.
4. Under **Information Display**, tick the items you want on the wallpaper.
5. Click **Apply Wallpaper**.

### Default settings

| Setting | Default |
|---------|---------|
| Font family | Arial |
| Host name size | 80 px |
| Info size | 25 px |
| Information shown | Hostname, IP, CPU cores, RAM, local drives |

## How it works

1. `MonitorHelper` enumerates the connected monitors through `EnumDisplayMonitors` and `GetMonitorInfo`.
2. `WallpaperEngine` loads the chosen image, or creates a solid-color canvas, and sizes it to the largest monitor resolution.
3. System information is collected and drawn with GDI+ (`System.Drawing`) using an outlined-text routine.
4. The result is saved to `%TEMP%\wallpaper_with_info.png`.
5. The wallpaper style is set to stretch in `HKCU\Control Panel\Desktop`, and the image is applied through `SystemParametersInfo`.

> **Note:** The wallpaper is a snapshot taken when you click **Apply Wallpaper**. It does not update automatically, so run the app again after the hostname, IP address or hardware changes.

## Project structure

```
Host-Info-Wallpaper-Creator/
├── Host Info Wallpaper Creator.slnx        # Solution file
└── Host Info Wallpaper Creator/
    ├── App.xaml / App.xaml.cs              # WPF application entry point
    ├── MainWindow.xaml / .xaml.cs          # UI (dark theme) and button handlers
    ├── WallpaperEngine.cs                  # System info gathering, rendering, wallpaper setting
    ├── MonitorHelper.cs                    # Monitor enumeration via Win32 API
    ├── AssemblyInfo.cs
    ├── gallery.ico                         # Application icon
    └── Host Info Wallpaper Creator.csproj
```

## Built with

- C# and [WPF](https://learn.microsoft.com/dotnet/desktop/wpf/) on .NET 10 (`net10.0-windows`)
- [Extended.Wpf.Toolkit](https://github.com/xceedsoftware/wpftoolkit) 5.1.2 for the color picker
- [System.Management](https://www.nuget.org/packages/System.Management) 10.0.14 for WMI queries
- `System.Drawing` (GDI+) for image rendering

## Troubleshooting

- **"Selected wallpaper image not found"**: the path in the image field does not exist. Click **Browse** and pick a valid image, or switch to **Solid color**.
- **Wrong or missing IP address**: the app uses the first active Ethernet or Wi-Fi adapter. If the machine only has other adapter types (VPN, virtual), the IP shows as `Unknown`.
- **Font not applied**: make sure the font family name matches an installed font exactly.
- **An error appears when applying**: the size fields must contain whole numbers.

## Author

Created by Artur Tonoyan ([@KillerTonix](https://github.com/KillerTonix)).
