# Hold to Steal Public Template

Source family: `hold-to-steal`

This is a public, source-only starter distilled from the Hold to Steal mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- illegal pickup guard
- theft action gating
- native inventory preservation
- shared UI bridge

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/HoldToSteal.Mono.csproj
il2cpp/Plugin.cs
il2cpp/HoldToSteal.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
