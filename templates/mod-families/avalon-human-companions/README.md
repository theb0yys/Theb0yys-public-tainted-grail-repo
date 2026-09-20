# Avalon Human Companions Public Template

Use this starter for human-companion features that need one shared feature model with separate Mono and IL2CPP integration.

The shared project is for companion behavior and configuration; runtime hosts are where exact FoA actor, AI, dialogue, combat, or UI access belongs.

## Reusable mechanisms

- human companion roster
- AI package
- companion commands
- shared UI/control panel

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/AvalonHumanCompanions.Mono.csproj
il2cpp/Plugin.cs
il2cpp/AvalonHumanCompanions.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
