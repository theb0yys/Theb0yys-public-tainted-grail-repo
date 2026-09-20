# FoA Mod Manager Integration Example

Use this example when your mod needs the ordinary FoA Mod Manager integration: expose BepInEx settings, register a controller action, or publish read-only runtime status.

Shows three author-ready surfaces:

- ordinary `Config.Bind<T>` settings;
- controller-action registration;
- read-only status-provider registration.

## Build

```powershell
dotnet build .\ModManagerIntegration.csproj -c Release \
  -p:FoAGameRoot="C:\Games\Tainted Grail FoA"
```

The project expects:

`BepInEx/plugins/FoAModManager/FoAModManager.dll`

Override `FoAModManagerDir` if your package is installed elsewhere.

The example unregisters controller/status entries on teardown and does not own a custom UI screen.
