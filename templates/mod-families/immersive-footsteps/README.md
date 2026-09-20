# Immersive Footsteps Public Template

Use this starter for footstep-audio replacement across Mono and IL2CPP.

Keep replacement selection and configuration shared, while each runtime host handles the exact FMOD/game hook needed to identify and replace only the intended native footstep event.

## Reusable mechanisms

- FMOD footstep interception
- surface/context replacement
- replacement-first suppression
- runtime audio decode

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/ImmersiveFootsteps.Mono.csproj
il2cpp/Plugin.cs
il2cpp/ImmersiveFootsteps.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
