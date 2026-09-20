# StaminaControl Public Template

Use this starter when you are building stamina-action or stamina-cost control across Mono and IL2CPP. Keep the player-facing rules shared and apply them through the exact native stat/action owner in each runtime host.

Source family: `stamina-action-control`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- stamina action tuning
- character-stat initialization
- configurable sprint/combat split

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/StaminaControl.Mono.csproj
il2cpp/Plugin.cs
il2cpp/StaminaControl.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
