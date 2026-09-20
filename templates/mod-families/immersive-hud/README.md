# Immersive HUD Public Template

Source family: `always-show-hud`

This is a public, source-only starter distilled from the Immersive HUD mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- HUD visibility context
- damage-number presentation
- hero bar visibility
- shared UI integration

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/ImmersiveHud.Mono.csproj
il2cpp/Plugin.cs
il2cpp/ImmersiveHud.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
