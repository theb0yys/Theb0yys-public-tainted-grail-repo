# Mono Death Observer

Starter for bounded terminal character-death observation.

It observes the native death lifecycle, deduplicates repeated callbacks, and does not claim corpse, loot, reward, respawn or persistence ownership.

Build:

```powershell
dotnet build .\DeathObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```
