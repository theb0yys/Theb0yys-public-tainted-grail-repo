# 23 — Character Damage Observer

**Category:** damage / diagnostics / VFX foundation  
**Source-path evidence:** RUNTIME_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example observes the real character damage route without changing damage.

Target:

~~~text
HealthElement.TakeDamage(Damage)
~~~

The postfix logs a bounded number of character-target damage events.

Before writing damage numbers, blood, hit sounds, hit markers, wounds, reactive UI or combat telemetry, prove that your event filter sees the targets you intend and excludes mining/resource nodes.

## Build

~~~powershell
dotnet build .\CharacterDamageObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This example intentionally does not mutate damage, spawn VFX or write files.

See Combat VFX sidecars under recipes/09-combat-vfx for the next stage.

The public rewrite is **NOT_RUN**.
