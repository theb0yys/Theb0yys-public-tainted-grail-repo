# Immersive Backgrounds Public Template

Source family: `immersive-backgrounds`

This is a public, source-only starter distilled from the Immersive Backgrounds mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- choice background replacement
- choice preview ownership
- item-set visual context
- title-screen integration

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/ImmersiveBackgrounds.Mono.csproj
il2cpp/Plugin.cs
il2cpp/ImmersiveBackgrounds.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
