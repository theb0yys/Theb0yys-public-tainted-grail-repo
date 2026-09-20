# Avalon Cheat Panel Public Template

Use this starter when you are building a cheat or developer panel with shared feature logic and separate Mono/IL2CPP hosts. Keep the panel model and commands shared; keep exact game access and loader-specific UI wiring in the runtime host.

Source family: `avalon-cheat-panel`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
