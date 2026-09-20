# Avalon AI FoA Host Public Template

Use this starter when you are building the FoA host for Avalon AI and want the shared feature logic separated from Mono- and IL2CPP-specific host code.

It gives you the project split for runtime hosting, observation, command execution, and planning integration; you still need to connect each host to the exact current FoA APIs it uses.

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
