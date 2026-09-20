# Jump Higher Public Template

Use this starter when you are building a simple movement/stat tweak such as increased jump height across both runtimes. Keep the setting and feature rule shared, then connect it to the exact native movement/stat owner in each host.

Source family: `jump-higher`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
