# 30 — Character-State Observer

**Category:** character state / diagnostics  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED for selected state reads  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example reads a small set of current-hero state surfaces and logs only when the combined state changes.

Observed fields:

- alive;
- crouching, including story crouch;
- swimming;
- mounted;
- portaling;
- native hero combat state;
- performing an action;
- current movement-system type.

The observer uses a bounded polling interval rather than emitting one row per frame. It does not change movement, combat, input, actions, mounts, portals or saves.

## Build

~~~powershell
dotnet build .\CharacterStateObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This is a useful foundation for context routing, safety gates and diagnostics. Treat every new state surface you add as a separate research claim rather than assuming similarly named properties have equivalent semantics.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
