# Tainted Performance Public Template

Use this starter when you are building performance monitoring or optimization features across both runtimes. Keep reporting and feature policy shared, and collect runtime-specific counters or hooks in the Mono/IL2CPP hosts.

Source family: `tainted-performance`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
