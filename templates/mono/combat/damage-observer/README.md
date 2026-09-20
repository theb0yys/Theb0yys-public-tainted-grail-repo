# Mono Damage Observer

Use this starter when you need to observe character damage from a Mono/BepInEx 5 mod without changing the damage calculation.

The template logs a limited number of damage events so you can verify the hook and inspect context safely. The observed events can then drive your own diagnostics, UI, audio, or VFX.

Build:

```powershell
dotnet build .\DamageObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

Keep FoA's native damage system in control unless your feature separately proves that it needs to change the calculation itself.
