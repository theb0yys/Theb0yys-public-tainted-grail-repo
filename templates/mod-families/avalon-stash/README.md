# Avalon Stash Public Template

Use this starter when you are building stash or storage features across both runtimes. Keep shared storage rules and UI-independent logic common, while the runtime hosts connect to the exact Hero/storage owners.

Source family: `Avalon Stash`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
