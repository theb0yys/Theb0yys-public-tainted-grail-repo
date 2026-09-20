# Origins of Avalon Public Template

Source family: `origins-of-avalon`

This is a public, source-only starter distilled from the Origins of Avalon mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- origin selection
- growth/progression provider
- journey state
- shared UI integration

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/OriginsOfAvalon.Mono.csproj
il2cpp/Plugin.cs
il2cpp/OriginsOfAvalon.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
