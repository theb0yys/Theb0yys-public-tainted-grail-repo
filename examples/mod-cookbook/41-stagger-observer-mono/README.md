# 41 — Stagger Observer

**Category:** combat / stagger outcome observation  
**Source-path evidence:** SOURCE_CONFIRMED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes two native stagger seams:

~~~text
EnemyBaseClass.EnterStagger(float? duration)
StaggerBehaviour.UpdateStaggerDuration(float? duration)
~~~

The first row records a stagger-entry request. The second records the duration value reaching native stagger behaviour.

A null duration is reported as `native-default`; the observer does not substitute its own duration.

Stagger is not treated as poise break. Native evidence shows stamina-driven stagger/rest handling and poise-break handling are separate systems.

## Build

~~~powershell
dotnet build .\StaggerObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This example does not change stamina, thresholds, duration, behaviour selection, animation state or saves.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
