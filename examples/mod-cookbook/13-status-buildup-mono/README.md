# 13 — Status Buildup Multiplier

**Category:** statuses / buildup  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example intercepts the real `CharacterStatuses.BuildupStatus` path and scales positive buildup only when the source character resolves to the current hero.

It leaves thresholds, decay, resistance and completion effects native.

## Try it

```text
[Status]
BuildupMultiplier = 2
```

## Build

```powershell
dotnet build .\StatusBuildupExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

The owner-side target/build exists, but actual Bleed/Burn/Poison buildup behavior remained untested in the inspected validation notes.
