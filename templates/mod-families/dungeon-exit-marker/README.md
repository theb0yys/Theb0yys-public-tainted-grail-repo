# Dungeon Exit Marker Public Template

Use this starter when you are building a dungeon-exit marker or navigation helper across both runtimes. Keep marker rules shared; resolve scene, map, marker, and lifecycle access in the runtime-specific host.

Source family: `dungeon-exit-helper`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
