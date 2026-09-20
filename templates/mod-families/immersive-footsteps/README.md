# Immersive Footsteps Public Template

Use this starter when you are building scoped footstep-audio replacement across Mono and IL2CPP. Keep replacement selection and configuration shared, while each host handles the exact FMOD/game caller and event identity.

Source family: `immersive-footsteps`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
