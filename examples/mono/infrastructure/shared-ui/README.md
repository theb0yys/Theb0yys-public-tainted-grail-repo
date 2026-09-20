# Shared UI Integration Example

Shows the intended ownership split:

- **FoA Mod Manager** — shared cursor/input/controller/world-freeze scope.
- **Tainted Interface** — common visual styles/resources.
- **this mod** — the actual window, commands and feature state.

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
