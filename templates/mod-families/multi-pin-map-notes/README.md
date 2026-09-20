# Multi-Pin Map Notes Public Template

Use this starter when you are building map-note or multi-pin features across Mono and IL2CPP. Keep note/pin rules shared and put exact map UI, marker, discovery, or persistence integration in the runtime-specific hosts.

Source family: `multi-pin-map-notes`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
