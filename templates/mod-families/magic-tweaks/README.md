# Magic Tweaks Public Template

Use this starter when you are building magic tuning that should share configuration across runtimes. Put selection/tuning rules in common code and keep exact projectile, mana-cost, cast, status, or other FoA hooks in the runtime hosts.

Source family: `magic-tweaks`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
