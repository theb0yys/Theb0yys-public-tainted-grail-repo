# Tainted Core's Public Template

Use this starter when you are building shared Core-style discovery or infrastructure rather than a feature mod. Keep public contracts and portable logic shared, and isolate runtime-specific host access behind the Mono and IL2CPP entry points.

Source family: `avalon-core`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- adapter registry
- trust reports
- capability contracts
- read-only consumer discovery

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedCore.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedCore.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
