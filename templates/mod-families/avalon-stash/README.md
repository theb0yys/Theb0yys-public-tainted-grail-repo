# Avalon Stash Public Template

Source family: `Avalon Stash`

This is a public, source-only starter distilled from the Avalon Stash mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
