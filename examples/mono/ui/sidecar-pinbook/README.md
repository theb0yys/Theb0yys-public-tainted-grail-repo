# Sidecar Pinbook

A buildable mod-owned location-note example.

It uses Hero.Current.Coords, persists versioned TSV rows under BepInEx/config, writes through a temporary file, and displays nearest pins without touching native map discovery.

## Controls

- F6 — save current position
- F7 — toggle compact HUD
- F9 — delete nearest saved pin

## Build

~~~powershell
dotnet build .\SidecarPinbook.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Build a sidecar map pinbook](../../../../guides/tasks/ui/build-a-sidecar-map-pinbook.md)
