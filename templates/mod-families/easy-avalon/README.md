# Easy Avalon Public Template

Use this starter for difficulty or quality-of-life tuning that should behave consistently across Mono and IL2CPP.

Put the player-facing options and tuning rules in shared code, then connect them to the exact FoA stats, hooks, or services in each runtime host.

## Reusable mechanisms

- damage scaling
- difficulty tuning
- narrow Harmony mutation
- configuration-first defaults

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/EasyAvalon.Mono.csproj
il2cpp/Plugin.cs
il2cpp/EasyAvalon.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
