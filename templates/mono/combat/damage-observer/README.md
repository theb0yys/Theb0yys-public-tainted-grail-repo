# Mono Damage Observer

Read-only starter for observing the native character-damage lifecycle.

The template logs a bounded number of character damage rows and deliberately does not alter damage calculation.

Build:

```powershell
dotnet build .\DamageObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

Use observed data for mod-owned VFX/UI/audio/diagnostics sidecars. Keep native damage ownership native unless the mod explicitly owns a separately researched mutation.
