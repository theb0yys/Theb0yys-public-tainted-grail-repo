# Avalon Stash Public Template

Use this starter for stash and storage features that need to support both Mono and IL2CPP.

Keep shared storage behavior and UI-independent logic common, while each host connects to the exact Hero/storage lifecycle and runtime APIs available on that build.

## Reusable mechanisms

- campfire storage entry
- stash-aware ingredient counts
- storage summary UI
- native stash ownership

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/AvalonStash.Mono.csproj
il2cpp/Plugin.cs
il2cpp/AvalonStash.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
