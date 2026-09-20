# Jump Higher Public Template

Use this starter for a simple movement or stat tweak that should work on both Mono and IL2CPP.

Keep the setting and feature rule shared, while each runtime host applies it through the exact FoA movement/stat owner available on that build.

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
