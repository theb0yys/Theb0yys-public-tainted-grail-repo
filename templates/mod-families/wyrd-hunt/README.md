# Wyrd Hunt Public Template

Source family: `wyrd-hunt`

This is a public, source-only starter distilled from the Wyrd Hunt mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- wyrdness pressure
- AI package
- encounter lifecycle
- progression and UI

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/WyrdHunt.Mono.csproj
il2cpp/Plugin.cs
il2cpp/WyrdHunt.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
