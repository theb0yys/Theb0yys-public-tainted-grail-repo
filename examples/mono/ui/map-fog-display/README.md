# Map Fog Display

A display-only map example.

It changes MapUI/FogOfWar presentation through:

- MapUI.ToggleFogOfWar
- FogOfWar.CreateMaskTexture
- FogOfWar.IsPositionRevealed

It does not write MapMemory.visitedPixels, quest discovery, location discovery, or fast-travel state.

## Build

~~~powershell
dotnet build .\MapFogDisplay.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Change map fog display only](../../../../guides/tasks/ui/change-map-fog-display-only.md)
