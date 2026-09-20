# FoA Mod Manager Public Template

Use this starter when building shared mod-management features such as settings, commands, status, or UI support across both runtimes.

Keep common management logic shared while Mono and IL2CPP hosts own loader integration and any exact game/UI access they require.

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
