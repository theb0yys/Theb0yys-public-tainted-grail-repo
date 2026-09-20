# Mono Runtime UI Overlay

Small BepInEx-owned IMGUI starter for diagnostics and narrow tools.

It does not claim native menu, cursor, pause, focus, or controller ownership. For those shared responsibilities, consume the appropriate public infrastructure contract.

Build:

```powershell
dotnet build .\RuntimeUiOverlay.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```

Default toggle key: `F6`.
