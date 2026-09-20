# No Fall Damage Public Template

Source family: `no-fall-damage`

This is a public, source-only starter distilled from the No Fall Damage mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
