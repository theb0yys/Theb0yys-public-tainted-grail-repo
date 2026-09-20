# Lockpicking Reforged Public Template

Use this starter for lockpicking changes that need shared behavior with runtime-specific hooks.

Keep the lockpicking rules and configuration common; Mono and IL2CPP hosts should connect them to the exact lock entry, durability, crime, or unlock owner used by that runtime.

## Reusable mechanisms

- lock entry
- tolerance tuning
- pick durability
- auto-unlock guard

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/LockpickingReforged.Mono.csproj
il2cpp/Plugin.cs
il2cpp/LockpickingReforged.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
