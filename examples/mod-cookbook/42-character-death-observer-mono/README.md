# 42 — Character Death Observer

**Category:** combat / terminal character observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes FoA's common terminal health/death event seam:

~~~text
HealthElement.OnDeathEvents(...)
~~~

It requires `HealthElement.ParentModel` to implement `ICharacter`, classifies the target as the current hero or another character, and suppresses duplicate terminal rows for the same runtime target.

A death-event row does **not** by itself prove that:

- an NPC corpse has been created;
- loot is ready;
- the living NPC has been discarded;
- death animation or ragdoll presentation completed;
- XP/rewards were granted;
- persistence/save state completed.

Those belong to later native lifecycle owners.

## Build

~~~powershell
dotnet build .\CharacterDeathObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example does not kill characters, alter health, create corpses, change loot/rewards or write saves.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
