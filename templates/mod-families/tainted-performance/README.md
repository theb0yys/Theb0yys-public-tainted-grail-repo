# Tainted Performance Public Template

Use this starter for performance monitoring or optimization features that need to work on both runtimes.

Keep report formats, thresholds, and shared feature logic portable, while each host owns the exact counters, hooks, or interop used to collect runtime data.

## Reusable mechanisms

- frame sampling
- incident reports
- plugin inventory
- performance overview UI

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedPerformance.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedPerformance.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
