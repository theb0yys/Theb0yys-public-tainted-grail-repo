# No Fall Damage Public Template

Use this starter when you are building a fall-damage change across both runtimes. Keep the feature toggle/rule shared and connect it to the narrow native fall-damage path in each runtime host rather than patching all damage globally.

Source family: `no-fall-damage`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- fall-damage guard
- result override
- narrow movement safety patch

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/NoFallDamage.Mono.csproj
il2cpp/Plugin.cs
il2cpp/NoFallDamage.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
