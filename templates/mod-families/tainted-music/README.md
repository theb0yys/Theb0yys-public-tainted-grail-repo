# Tainted Music Public Template

Use this starter for music or ambience features that need shared selection and configuration across Mono and IL2CPP.

Keep music policy and state shared; each host should own the exact AudioCore/FMOD hooks and lifecycle needed for that runtime.

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
