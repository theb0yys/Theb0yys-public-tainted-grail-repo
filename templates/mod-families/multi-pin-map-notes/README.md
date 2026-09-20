# Multi-Pin Map Notes Public Template

Source family: `multi-pin-map-notes`

This is a public, source-only starter distilled from the Multi-Pin Map Notes mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- custom map pins
- note state
- map input integration
- shared UI bridge

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/MultiPinMapNotes.Mono.csproj
il2cpp/Plugin.cs
il2cpp/MultiPinMapNotes.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
