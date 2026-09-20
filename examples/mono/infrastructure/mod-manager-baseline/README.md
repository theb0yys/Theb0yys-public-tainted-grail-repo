# FoA Mod Manager Integration Example

Use this example when your mod needs the basic FoA Mod Manager integrations most authors are likely to use.

It demonstrates ordinary BepInEx settings, a controller-triggered action, and a read-only runtime status provider without handing gameplay ownership to the manager.

## Build

```powershell
dotnet build .\ModManagerIntegration.csproj -c Release \
  -p:FoAGameRoot="C:\Games\Tainted Grail FoA"
```

The project expects:

`BepInEx/plugins/FoAModManager/FoAModManager.dll`

Override `FoAModManagerDir` if your package is installed elsewhere.

The example unregisters controller/status entries on teardown and does not own a custom UI screen.
