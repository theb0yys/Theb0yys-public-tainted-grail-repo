# Tainted Music Public Template

Source family: `tainted-music`

This is a public, source-only starter distilled from the Tainted Music mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
