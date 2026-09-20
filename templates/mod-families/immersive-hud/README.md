# Immersive HUD Public Template

Use this starter for HUD visibility or presentation changes that should behave consistently across both runtimes.

Keep visibility rules and configuration shared, and let the Mono/IL2CPP hosts connect them to the correct native HUD lifecycle and refresh methods.

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
