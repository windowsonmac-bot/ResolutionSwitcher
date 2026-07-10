# ResolutionSwitcher

**Lightweight resolution switching for competitive gaming**

A completely offline, self-contained Windows utility that instantly switches your monitor to competitive stretching resolutions (4:3, 5:3) and auto-reverts when your game closes.

## Features

✅ **Instant Resolution Switching** - Change to stretched ratios in milliseconds
✅ **Auto-Revert or Manual Reset** - Choose your preferred mode
✅ **Multi-Monitor Support** - Works with any monitor configuration
✅ **Game Profiles** - Save different settings per game
✅ **Zero System Impact** - No registry modifications, completely portable
✅ **Simple Distribution** - Just two self-contained .exe files, no installer, no clutter
✅ **Anticheat Safe** - Works with Valorant, CS2, FACEIT, 5E Arena
✅ **Optional Debug Logging** - Troubleshooting when needed

## Getting Started

### First Run

1. Download and unzip - you'll only find two files: `ResolutionSwitcher.exe` and `ResolutionSwitcher.Monitor.exe`
2. Run `ResolutionSwitcher.exe`
3. A desktop shortcut is created automatically the first time you run it
4. App auto-detects your monitors and default resolution
5. Select your game and target resolution
6. Click "LAUNCH GAME & APPLY" or "APPLY ONLY"

### Two Launch Modes

**Auto-Restore Helper** (Default)
- Game launches with resolution applied
- Background helper monitors game
- When game closes, resolution auto-reverts
- 8-12 MB RAM usage, 0.01% CPU

**Instant Kill Mode**
- Game launches with resolution applied
- ResolutionSwitcher closes immediately
- Zero background processes
- Manual click "RESET" to revert when done

## Configuration

All settings are stored in `config.json` in the same folder as the .exe.

### Uninstall

Just delete the folder. No registry entries, no leftover files.

### Factory Reset

Click Settings (⚙️) → Delete the `config.json` file or use the Factory Reset button in settings.

## System Requirements

- Windows 10 or Windows 11
- Multi-monitor setups fully supported
- Works with NVIDIA, AMD, Intel GPUs

## About

Click the **?** button in the app for detailed information about:
- What it does and doesn't do
- All features explained
- Launch modes comparison
- Safety & anticheat information
- System resource impact

## Performance

- **Main App:** 30-50 MB RAM (includes .NET 8 runtime)
- **Auto-Restore Helper:** 8-12 MB RAM, 0.01-0.05% CPU
- **Instant Kill Mode:** 0% CPU, 0% RAM while gaming
- **FPS Impact:** Zero (kernel-level waiting, no polling)
- **Network:** 0 bytes (completely offline)
- **Disk footprint:** Just 2 files (~60-70 MB combined, self-contained .NET 8 runtime included)

## Anticheat Safety

✅ Uses standard Windows APIs (not kernel drivers)
✅ No memory injection or hooking
✅ No game file modifications
✅ Safe for all major anticheat systems

## Debug Logging

Enable debug logging in Settings → Behavior to troubleshoot issues.
Logs are saved to `debug.log` in the app folder.

## Support & Issues

For issues or feature requests, check the About section for detailed information about functionality and limitations.

---

**Version 1.0.2** | Completely Portable | Self-Contained | Two Files, Zero Clutter
