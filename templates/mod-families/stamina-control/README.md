# StaminaControl Public Template

Use this starter for stamina-cost or stamina-action control across Mono and IL2CPP.

Keep user-facing rules and configuration shared; runtime hosts should apply them through the exact native stat or action owner available on each runtime.

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
