# Avalon Cheat Panel Public Template

Source family: `avalon-cheat-panel`

This is a public, source-only starter distilled from the Avalon Cheat Panel mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- bounded cheat commands
- shared custom UI
- action receipts
- movement/no-clip prototype boundary

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/AvalonCheatPanel.Mono.csproj
il2cpp/Plugin.cs
il2cpp/AvalonCheatPanel.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
