# Multi-Pin Map Notes Public Template

Use this starter for map-note or multi-pin features that should work across both runtimes.

Keep note/pin rules and user configuration shared, while each host connects them to the exact map UI, marker, scene, or persistence access available on that runtime.

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
