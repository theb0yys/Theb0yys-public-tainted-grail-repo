# Avalon AI FoA Host Public Template

Use this starter when you are building the FoA host that connects Avalon AI package decisions to the running game. Keep package/planning logic shared, and put the exact FoA observation and command-execution hooks in the runtime-specific hosts.

Source family: `avalon-ai-runtime`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
