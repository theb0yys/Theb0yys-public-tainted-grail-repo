# Immersive Progression Public Template

Use this starter for progression features that share rules across runtimes but require different native integration.

Keep progression rules, ranks, and configuration common; put exact XP, talent, stat, or UI hooks in the runtime-specific host.

## Reusable mechanisms

- progression planning
- campfire skill UI
- effect-intent apply/query
- provider bridge

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/ImmersiveProgression.Mono.csproj
il2cpp/Plugin.cs
il2cpp/ImmersiveProgression.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
