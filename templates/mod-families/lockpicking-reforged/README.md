# Lockpicking Reforged Public Template

Source family: `lockpicking-reforged`

This is a public, source-only starter distilled from the Lockpicking Reforged mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
