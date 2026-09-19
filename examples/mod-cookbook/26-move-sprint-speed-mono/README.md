# 26 — Move and Sprint Speed

**Category:** traversal / player stats  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example uses FoA's runtime stat system rather than changing transforms or writing save values.

Targets:

~~~text
HeroStats.MoveSpeed
HeroStats.SprintSpeed
~~~

The mod adds non-saved StatTweak elements after HeroStats initialization and reapplies them when config changes.

## Config

~~~text
[Movement]
MoveSpeedMultiplier = 1.0
SprintSpeedMultiplier = 1.0
~~~

Recommended first tests stay close to vanilla, for example 0.8 through 1.25.

## Build

~~~powershell
dotnet build .\MoveSprintSpeedExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example removes its own tweaks on unload.

The public rewrite is **NOT_RUN**.
