# Easy Avalon Public Template

Use this starter when you are building a collection of difficulty or quality-of-life tweaks that should share configuration across runtimes. Each actual game change should still be connected to its exact FoA stat, method, or service in the runtime host.

Source family: `easy-avalon`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
