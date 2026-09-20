# Dungeon Exit Marker Public Template

Use this starter for a dungeon-exit marker or navigation helper that needs cross-runtime support.

Keep marker and feature rules shared; runtime hosts should resolve the exact scene, map, UI, and location access needed by Mono or IL2CPP.

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
