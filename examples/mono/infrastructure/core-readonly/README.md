# Avalon Core Read-Only Consumer Example

A minimal real BepInEx consumer for the public Core discovery baseline.

It demonstrates:

- a hard dependency on Avalon Core;
- read-only `Plugin.TrustReports`;
- exact lookup of the documented `adapter-registry` engine;
- registry report logging;
- fail-closed behaviour.

## Build

Point `AvalonCoreDir` at the released/installed Core assembly directory:

```powershell
dotnet build .\CoreReadOnly.csproj -c Release \
  -p:FoAGameRoot="C:\Games\Tainted Grail FoA" \
  -p:AvalonCoreDir="C:\path\to\AvalonCore"
```

The example references `AvalonCore.dll`, `AvalonCore.Abstractions.dll` and `AvalonCore.Trust.dll` from that directory.

It does not execute adapters or mutate gameplay.
