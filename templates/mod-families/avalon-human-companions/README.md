# Avalon Human Companions Public Template

Use this starter when you are building human-companion features across Mono and IL2CPP. Keep companion rules and commands shared, while each host handles the exact FoA actor, faction, interaction, and lifecycle access for that runtime.

Source family: `avalon-human-companions`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
