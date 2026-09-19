# 40 — Poise-Break Observer

**Category:** combat / poise outcome observation  
**Source-path evidence:** SOURCE_CONFIRMED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes the native poise-break entry seam:

~~~text
EnemyBaseClass.EnterPoise(NpcStateType poiseBreakDirection, bool isInCombat)
~~~

It reports the requested directional state, combat context, and read-only stagger capability/state.

Poise break is deliberately kept separate from stagger. This example does not edit poise damage, meters, thresholds, behaviours, animations or saves.

## Build

~~~powershell
dotnet build .\PoiseBreakObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
