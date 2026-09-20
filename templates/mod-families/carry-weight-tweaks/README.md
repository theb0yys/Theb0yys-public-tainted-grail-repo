# CarryWeightTweaks Public Template

Use this starter when you are building carry-weight tuning that should share configuration across Mono and IL2CPP. The runtime hosts are where you connect the shared rule to the exact FoA encumbrance/stat owner.

Source family: `carry-weight-tweaks`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
