# Mono Runtime UI Starter

A small BepInEx-owned IMGUI surface intended for diagnostics, prototypes and narrow tools.

It deliberately does not claim ownership of FoA's native menu stack, cursor, focus, pause state or controller navigation.

## Start here

1. Rename the assembly, namespace, plug-in GUID and display name.
2. Build against your own local BepInEx/game references.
3. Launch with the plug-in installed and use the configured key (default `F6`) to toggle the panel.
4. Keep this pattern small. For production/native-screen integration, research the actual input/focus/modal owner instead of scaling this into a parallel UI framework.

Build:

```powershell
dotnet build .\RuntimeUiBasic.csproj -c Release -p:GameRoot="C:\Games\Tainted Grail FoA"
```

See [Runtime UI overlay example](../../../examples/mono/ui/runtime-overlay/README.md) for the mechanism demonstration.

**Evidence state:** template structure only. Runtime behaviour is not claimed until separately observed on the target build.
