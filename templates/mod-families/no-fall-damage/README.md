# No Fall Damage Public Template

Use this starter for a fall-damage feature that should support both Mono and IL2CPP.

Keep the setting and high-level rule shared; each runtime host should connect it to the narrow native fall-damage path instead of patching all damage globally.

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
