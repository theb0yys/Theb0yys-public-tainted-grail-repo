# Lockpicking Reforged Public Template

Use this starter when you are building lockpicking changes across Mono and IL2CPP. Keep the mod's rules/configuration shared, while each host handles the exact lock-entry, durability, minigame, crime, or unlock integration it needs.

Source family: `lockpicking-reforged`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
