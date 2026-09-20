# FoA Mod Manager Integration Example

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
