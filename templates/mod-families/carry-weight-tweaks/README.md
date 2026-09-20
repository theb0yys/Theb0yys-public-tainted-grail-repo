# CarryWeightTweaks Public Template

Use this starter for carry-weight tuning that should share settings and feature rules across Mono and IL2CPP.

Each runtime host is responsible for connecting those rules to the exact FoA stat owner or hook used on that runtime.

## Reusable mechanisms

- carry-weight limit tuning
- configurable stat override
- native stat initialization boundary

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/CarryWeightTweaks.Mono.csproj
il2cpp/Plugin.cs
il2cpp/CarryWeightTweaks.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
