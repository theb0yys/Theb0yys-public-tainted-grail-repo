# Avalon Companions Public Template

Source family: `avalon-companions`

This is a public, source-only starter distilled from the Avalon Companions mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
