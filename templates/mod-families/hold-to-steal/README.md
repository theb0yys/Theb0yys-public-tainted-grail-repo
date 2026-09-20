# Hold to Steal Public Template

Use this starter when you are building an illegal-interaction guard such as Hold to Steal. Keep the input/permission rule shared, and let each runtime host connect it to the exact native theft action while preserving normal item transfer and crime behavior.

Source family: `hold-to-steal`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- illegal pickup guard
- theft action gating
- native inventory preservation
- shared UI bridge

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/HoldToSteal.Mono.csproj
il2cpp/Plugin.cs
il2cpp/HoldToSteal.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
