# Wyrd Hunt Public Template

Use this starter for Wyrd Hunt-style encounter or hunt logic across Mono and IL2CPP.

Keep hunt rules, state, and configuration shared; runtime hosts should own the exact actor, death, reward, scene, and other FoA integration required by each build.

## Reusable mechanisms

- wyrdness pressure
- AI package
- encounter lifecycle
- progression and UI

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/WyrdHunt.Mono.csproj
il2cpp/Plugin.cs
il2cpp/WyrdHunt.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
