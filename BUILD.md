# ResolutionSwitcher - Build Instructions

## Prerequisites

- Visual Studio 2022 (Community Edition is free)
- .NET 8 SDK

## Building from Source

### Option 1: Visual Studio (Easiest)

1. Clone or download this repository
2. Open `ResolutionSwitcher.sln` in Visual Studio
3. Right-click the solution → Select "Build Solution" (Ctrl+Shift+B)
4. Wait for the build to complete
5. Output files will be in:
   - `ResolutionSwitcher.Main\bin\Release\net8.0-windows\win-x64\`
   - `ResolutionSwitcher.Monitor\bin\Release\net8.0-windows\win-x64\`

### Option 2: Command Line

```bash
# Install .NET 8 SDK if not already installed
# Download from https://dotnet.microsoft.com/download

# Build the solution
dotnet build ResolutionSwitcher.sln --configuration Release

# Publish (self-contained with runtime)
dotnet publish ResolutionSwitcher.Main -c Release -r win-x64 --self-contained
dotnet publish ResolutionSwitcher.Monitor -c Release -r win-x64 --self-contained
```

## Project Structure

```
ResolutionSwitcher/
├── ResolutionSwitcher.sln              # Solution file
├── ResolutionSwitcher.Main/            # Main application
│   ├── ResolutionSwitcher.Main.csproj  # Project config
│   ├── Program.cs                      # Entry point
│   ├── MainForm.cs                     # Main UI window
│   ├── StatusWindow.cs                 # Status display
│   ├── AboutForm.cs                    # About window
│   ├── DisplayManager.cs               # Display API wrapper
│   ├── ConfigManager.cs                # Config file handling
│   ├── GameLauncher.cs                 # Game launching logic
│   └── Logger.cs                       # Debug logging
│
├── ResolutionSwitcher.Monitor/         # Background monitor
│   ├── ResolutionSwitcher.Monitor.csproj
│   └── Program.cs                      # Standalone watcher exe
│
├── README.md                           # User documentation
└── BUILD.md                            # This file
```

## Output Structure

After building, you'll have:

```
ResolutionSwitcher/
├── ResolutionSwitcher.exe              # Main application (~45 MB with runtime)
├── ResolutionSwitcher.Monitor.exe      # Auto-restore helper (~15 MB with runtime)
└── config.json                         # Auto-created on first run
```

## Self-Contained Runtime

The `.csproj` files are configured for self-contained deployment, which means:
- ✅ No .NET 8 SDK required to run the app
- ✅ Completely portable
- ✅ Works on any Windows 10/11 PC
- ✅ Larger file size (includes entire .NET runtime)

To change this, modify the `.csproj` files:

```xml
<!-- For self-contained (current) -->
<SelfContained>true</SelfContained>

<!-- For framework-dependent (requires .NET 8 installed) -->
<SelfContained>false</SelfContained>
```

## Configuration

### Debug Build

For development with debug symbols:

```bash
dotnet build ResolutionSwitcher.sln --configuration Debug
```

### Release Build

For production distribution:

```bash
dotnet build ResolutionSwitcher.sln --configuration Release
```

## Troubleshooting Build Issues

### "No .NET SDK found"
- Download .NET 8 from https://dotnet.microsoft.com/download
- Restart Visual Studio after installation

### "Newtonsoft.Json not found"
- NuGet should auto-restore packages
- If not, run: `dotnet restore ResolutionSwitcher.sln`

### "WinForms designer errors"
- This is normal in some VS versions
- The code will compile and run correctly
- You can ignore designer warnings

## Publishing for Distribution

To create a standalone folder for sharing:

```bash
# Publish main app
dotnet publish ResolutionSwitcher.Main/ResolutionSwitcher.Main.csproj `
  -c Release -r win-x64 --self-contained `
  -o ./publish/main

# Publish monitor app
dotnet publish ResolutionSwitcher.Monitor/ResolutionSwitcher.Monitor.csproj `
  -c Release -r win-x64 --self-contained `
  -o ./publish/monitor

# Copy both .exe files to distribution folder
```

## Testing

1. **Debug run:** Press F5 in Visual Studio
2. **Release run:** Build then manually run the .exe from the bin folder
3. **Test features:** Click buttons, check About window, test resolution changes

## Notes

- Build time: ~30-60 seconds on first build, ~5-10 seconds thereafter
- Output size: ~60 MB total (both .exe files with runtime)
- Runtime: .NET 8 (included, no external dependencies needed)

---

For support, see the README.md or the About section in the application.
