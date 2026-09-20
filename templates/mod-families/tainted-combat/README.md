# Tainted Combat Public Template

Use this starter for combat features that need one shared feature model with separate Mono and IL2CPP integration.

Keep configuration and combat policy shared, while each host owns the exact damage, stat, action, or lifecycle hooks used by that runtime.

## Reusable mechanisms

- block/parry tuning
- combat feel presets
- consumable pressure
- difficulty pressure

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedCombat.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedCombat.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
