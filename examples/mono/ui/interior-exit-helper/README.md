# Interior Exit Helper

A complete small utility mod:

~~~text
scene changes
→ wait 1.25 seconds
→ capture Hero.Current.Coords
→ project that world position through Camera.main
→ show an EXIT + distance marker
→ discard the old position automatically on the next scene
~~~

SceneService.IsOpenWorld is used when available. The fallback is only for identifying likely interior scene names.

## Build

~~~powershell
dotnet build .\InteriorExitHelper.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Build an interior exit helper](../../../../guides/tasks/world/build-an-interior-exit-helper.md)
