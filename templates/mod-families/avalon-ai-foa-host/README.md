# Avalon AI FoA Host Public Template

Source family: `avalon-ai-runtime`

This is a public, source-only starter distilled from the Avalon AI FoA Host mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- FoA runtime host
- observation bridge
- command execution boundary
- blackboard/planning integration

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/AvalonAiFoaHost.Mono.csproj
il2cpp/Plugin.cs
il2cpp/AvalonAiFoaHost.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
