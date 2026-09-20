# Immersive Backgrounds Public Template

Use this starter for background or choice-presentation features that need to work on both runtimes.

Shared code can own the feature rules and configuration; runtime hosts should own the exact FoA UI or presentation access required to show or update the feature.

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
