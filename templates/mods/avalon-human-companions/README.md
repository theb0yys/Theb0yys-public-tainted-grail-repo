# Avalon Human Companions Public Template

Source family: `avalon-human-companions`

This is a public, source-only starter distilled from the Avalon Human Companions mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
