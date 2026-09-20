# Mono Runtime UI Overlay

Use this starter when you need a small Mono/BepInEx 5 IMGUI window for diagnostics or a focused mod tool.

The template owns only its own panel. It does not take over FoA's native menus, cursor, pause state, focus, or controller input; use the shared UI tooling when your screen needs those responsibilities.

Build:

```powershell
dotnet build .\RuntimeUiOverlay.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```

Default toggle key: `F6`.
