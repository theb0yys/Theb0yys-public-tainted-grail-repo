# Avalon Companions Public Template

Use this starter for a companion feature that needs shared behavior across Mono and IL2CPP.

Keep companion rules and commands in common code, and put exact actor, movement, combat, UI, and lifecycle integration in the runtime-specific hosts.

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
