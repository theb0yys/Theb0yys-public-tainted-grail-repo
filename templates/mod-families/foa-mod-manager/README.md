# FoA Mod Manager Public Template

Use this starter when you are building shared mod-management infrastructure such as settings, controller actions, UI scope, or runtime status. Keep common management logic shared and isolate loader/game integration in the Mono and IL2CPP hosts.

Source family: `foa-mod-manager`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- settings UI
- profiles
- input/cursor scope
- diagnostics and shared mod tools

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/FoaModManager.Mono.csproj
il2cpp/Plugin.cs
il2cpp/FoaModManager.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
