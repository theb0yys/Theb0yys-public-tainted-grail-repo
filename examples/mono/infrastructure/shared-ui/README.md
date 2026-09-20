# Shared UI Integration Example

Use this example when your custom screen needs shared input/cursor handling and common UI resources.

It demonstrates the intended split: FoA Mod Manager handles shared input/cursor/freeze responsibilities, Tainted Interface provides reusable visual resources, and your mod continues to own the actual window, commands, and feature state.

## Build

```powershell
dotnet build .\SharedUiIntegration.csproj -c Release \
  -p:FoAGameRoot="C:\Games\Tainted Grail FoA"
```

Expected defaults:

- `BepInEx/plugins/FoAModManager/FoAModManager.dll`
- `BepInEx/plugins/TaintedInterface/TaintedInterface.dll`

Override the corresponding MSBuild directory properties if needed.

Press **F8** to open/close the example. The example releases the shared scope on close and teardown.
