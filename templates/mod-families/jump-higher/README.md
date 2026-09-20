# Jump Higher Public Template

Source family: `jump-higher`

This is a public, source-only starter distilled from the Jump Higher mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- movement jump scaling
- bounded movement patch
- configurable multiplier

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/JumpHigher.Mono.csproj
il2cpp/Plugin.cs
il2cpp/JumpHigher.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
