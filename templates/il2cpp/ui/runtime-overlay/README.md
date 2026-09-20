# IL2CPP Runtime UI Overlay

Use this starter when you need a small BepInEx 6 IL2CPP IMGUI window for diagnostics or a focused mod tool.

It uses a registered IL2CPP behaviour and owns only its own panel. FoA's native menu, cursor, pause, focus, and controller state remain outside this starter unless you deliberately integrate the shared UI tooling.

Build:

```powershell
dotnet build .\Il2CppRuntimeOverlay.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```
