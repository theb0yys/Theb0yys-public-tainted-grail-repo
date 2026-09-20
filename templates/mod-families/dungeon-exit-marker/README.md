# Dungeon Exit Marker Public Template

Source family: `dungeon-exit-helper`

This is a public, source-only starter distilled from the Dungeon Exit Marker mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- dungeon exit discovery
- marker presentation
- shared UI bridge
- scene/context gating

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/DungeonExitMarker.Mono.csproj
il2cpp/Plugin.cs
il2cpp/DungeonExitMarker.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
