# StaminaControl Public Template

Source family: `stamina-action-control`

This is a public, source-only starter distilled from the StaminaControl mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
