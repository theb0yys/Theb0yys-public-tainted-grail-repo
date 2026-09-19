# 37 — Combat State Observer

**Category:** combat / state observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes the current hero's native combat-state read:

~~~text
Hero.Current.HeroCombat?.IsHeroInFight
~~~

It polls at a bounded interval and logs only when the state changes.

Each transition row also includes read-only equipment context:

- current main-hand item;
- current off-hand item;
- `Hero.WeaponsVisible`.

The observer does not build its own nearby-hostile detector and does not infer combat from input, animation or damage frequency.

## Build

~~~powershell
dotnet build .\CombatStateObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example does not force combat, change aggro, alter factions, draw weapons or write saves.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
