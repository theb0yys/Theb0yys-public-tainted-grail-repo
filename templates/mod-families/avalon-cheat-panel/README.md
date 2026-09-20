# Avalon Cheat Panel Public Template

Use this starter for a cross-runtime cheat or developer panel.

It keeps panel/feature logic shared while the Mono and IL2CPP hosts own loader setup and exact game access, so debug features do not need two unrelated implementations.

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
