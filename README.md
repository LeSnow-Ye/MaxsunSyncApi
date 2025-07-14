# MaxsunSyncApi

A .NET API for interacting with MaxsunSync service to control RGB lighting on Maxsun devices.

[![NuGet version](https://img.shields.io/nuget/v/MaxsunSyncApi.svg?style=flat-square)](https://www.nuget.org/packages/MaxsunSyncApi/)

## ⚠️Requirements⚠️

- [MAXSUN Sync](https://www.maxsun.com/blogs/maxsun-motherboard/revolutionize-your-gaming-experience-with-maxsun-syncs-advanced-rgb-control) must be installed. (Tested on Ver. 1.0.39)
- CppWindowsService must be running
- Administrator privileges required

## Installation

Install the package via NuGet Package Manager:

```
Install-Package MaxsunSyncApi
```

Or via .NET CLI:

```
dotnet add package MaxsunSyncApi
```

## Usage

### Basic Usage

```csharp
using MaxsunSyncApi;

// Check if the MaxsunSync service is running
MaxsunService.CheckService();

// Scan for connected devices
var devices = MaxsunService.ScanDevice();
foreach (var device in devices)
{
    Console.WriteLine($"Device: {device.deviceName}, Type: {device.deviceType}, Synced: {device.syncStatus}");
}

// Apply a simple blue effect
MaxsunService.ApplyEffect(
    speed: 1,
    mode: EffectMode.Single,
    hue: 240, // Blue
    sat: 1.0,
    val: 1.0
);
```

### Advanced Effects

```csharp
// Apply breathing effect
MaxsunService.ApplyEffect(
    speed: 2,
    mode: EffectMode.Breathing,
    hue: 120, // Green
    sat: 0.8,
    val: 1.0
);

// Apply rainbow effect
MaxsunService.ApplyEffect(
    speed: 3,
    mode: EffectMode.Rainbow
);

// Apply sleep effect (turns off lighting)
MaxsunService.ApplyEffectForSleep();
```

### Managing Sync Status

```csharp
// Get current sync status for all devices
var syncStatuses = MaxsunService.GetSyncStatusList();

// Enable sync for all devices
var newSyncStatuses = syncStatuses.Select(s => true).ToList();
MaxsunService.SetSyncStatus(newSyncStatuses);
```

## API Reference

### MaxsunService Class

#### Methods

- `CheckService()` - Ensures the MaxsunSync service is running
- `ScanDevice()` - Returns a list of connected RGB devices
- `ApplyEffect(...)` - Applies a lighting effect with specified parameters
- `ApplyEffectForSleep()` - Turns off all lighting
- `GetSyncStatusList()` - Gets sync status for all devices
- `SetSyncStatus(List<bool>)` - Sets sync status for devices

### Effect Modes

- `Single` - Solid color
- `Breathing` - Breathing effect
- `ColorCycle` - Cycles through colors
- `Rainbow` - Rainbow effect
- `CPU` - CPU-based effect
- `Music` - Music-reactive effect
- `Close` - Turn off lighting

### Device Types

- `MotherBoard`
- `Keyboard`
- `Mouse`
- `Dram`
- `Argb`
- `WaterCooler`
- `Vga`

## Notes

- This API requires the original MaxsunSync software to be installed and running
- Administrator privileges are required for proper operation
- Only Windows is supported
- Color values use HSV (Hue, Saturation, Value) format where:
  - Hue: 0-360 degrees
  - Saturation: 0.0-1.0
  - Value (Brightness): 0.0-1.0

## License

MIT License - see LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
