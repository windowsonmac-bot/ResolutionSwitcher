# ResolutionSwitcher

**Lightweight resolution switching for competitive gaming**

A completely offline, self-contained Windows utility that instantly switches your monitor to competitive stretched resolutions (4:3, 5:3) and auto-reverts when your game closes.

## Features

✅ **Instant Resolution Switching** - Change to stretched ratios in milliseconds
✅ **Auto-Revert or Manual Reset** - Choose your preferred mode
✅ **Multi-Monitor Support** - Works with any monitor configuration
✅ **Game Profiles** - Save, load, and switch between per-game settings (including per-profile theme)
✅ **Global Hotkeys** - Reset, launch, theme switching, and emergency reset, all rebindable
✅ **Zero System Impact** - No registry modifications, completely portable
✅ **Simple Distribution** - Just two self-contained .exe files, no installer, no clutter
✅ **Anticheat Safe** - Works with Valorant, CS2, FACEIT, 5E Arena
✅ **Optional Debug Logging** - Troubleshooting when needed

## Getting Started

### First Run

1. Download and unzip - you'll only find two files: `ResolutionSwitcher.exe` and `ResolutionSwitcher.Monitor.exe`
2. Run `ResolutionSwitcher.exe`
3. A desktop shortcut is created automatically the first time you run it (or create one anytime via the **Desktop Shortcut** button in the main window)
4. App auto-detects your monitors and default resolution
5. Select or create a profile, and choose your target resolution
6. Click "LAUNCH GAME & APPLY" or "APPLY ONLY"

### Profiles

- **New** - create a profile from your current settings
- **Load** - switch to a saved profile and apply its resolution, refresh rate, and theme
- **Save** - update the active profile with your current settings
- **Delete** - remove a profile
- The last profile you used is automatically restored the next time you open the app

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

### Global Hotkeys

Default combinations (rebindable in Settings):

| Action | Default Hotkey |
|---|---|
| Reset resolution | `Ctrl+Alt+R` |
| Launch game | `Ctrl+Alt+L` |
| Switch to Light theme | `Ctrl+Alt+1` |
| Switch to Dark theme | `Ctrl+Alt+2` |
| Emergency reset | `Ctrl+Alt+F12` |

## Configuration

All settings are stored in `config.json` in the same folder as the .exe.

### Uninstall

Just delete the folder. No registry entries, no leftover files.

### Master / Factory Reset

Use the **Master Reset** button in the main window to restore the default profiles and hotkeys immediately (no restart needed), or delete `config.json` for a full factory reset.

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
- **Disk footprint:** Just 2 files (~90 MB combined, self-contained .NET 8 runtime included)

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

**Version 1.0.3** | Completely Portable | Self-Contained | Two Files, Zero Clutter
