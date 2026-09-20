# Tainted Music Public Template

Use this starter when you are building music or ambience features across Mono and IL2CPP. Keep policy, track selection, and configuration shared while each host owns the exact FoA/FMOD integration.

Source family: `tainted-music`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- music arbitration
- native music suppression
- custom music menu
- owned resource loading

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedMusic.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedMusic.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
