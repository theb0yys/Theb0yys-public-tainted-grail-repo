# Mono Death Observer

Use this starter when you need to react after a character reaches FoA's native death stage.

It observes the death lifecycle, avoids processing duplicate callbacks, and leaves corpse creation, loot, rewards, respawn, and persistence to the systems that own them.

Build:

```powershell
dotnet build .\DeathObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```
