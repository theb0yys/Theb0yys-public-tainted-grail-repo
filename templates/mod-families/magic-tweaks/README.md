# Magic Tweaks Public Template

Use this starter for spell and magic tuning across Mono and IL2CPP.

Keep configuration and tuning policy shared, then use the runtime hosts for the exact projectile, cost, status, cast, or other FoA magic hooks required by each build.

## Reusable mechanisms

- cast speed
- projectile speed
- area/cooldown/cost tuning
- player-owned filtering

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/MagicTweaks.Mono.csproj
il2cpp/Plugin.cs
il2cpp/MagicTweaks.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
