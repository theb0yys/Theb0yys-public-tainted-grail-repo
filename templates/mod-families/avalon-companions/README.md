# Avalon Companions Public Template

Use this starter when you are building companion features that need a shared command/state model but runtime-specific FoA actor integration. Native spawning, movement, combat, death, and cleanup still need exact runtime owners.

Source family: `avalon-companions`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- companion lifecycle
- AI owner/package
- follow/recall
- HUD/control panel

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/AvalonCompanions.Mono.csproj
il2cpp/Plugin.cs
il2cpp/AvalonCompanions.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
