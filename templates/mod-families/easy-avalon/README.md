# Easy Avalon Public Template

Source family: `easy-avalon`

This is a public, source-only starter distilled from the Easy Avalon mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- damage scaling
- difficulty tuning
- narrow Harmony mutation
- configuration-first defaults

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/EasyAvalon.Mono.csproj
il2cpp/Plugin.cs
il2cpp/EasyAvalon.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
