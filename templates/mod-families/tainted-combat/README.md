# Tainted Combat Public Template

Use this starter when you are building combat features that share rules across runtimes but require runtime-specific damage, stat, action, or death hooks. Keep shared combat policy separate from the exact native integration.

Source family: `Tainted Combat`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
