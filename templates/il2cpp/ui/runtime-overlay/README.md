# IL2CPP Runtime UI Overlay

Copy-ready BepInEx 6 IL2CPP IMGUI starter using the thin-host + registered-behaviour pattern.

Build:

```powershell
dotnet build .\Il2CppRuntimeOverlay.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```

The template owns only its small panel. Native menu/cursor/pause/controller ownership remains outside this starter.
