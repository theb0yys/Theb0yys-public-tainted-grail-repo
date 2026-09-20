---
document_type: system
scope: native FoA music vs ambience ownership relevant to additive music mods
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
last_verified: 2026-09-20
---

# Native Music and Ambience Ownership

FoA's audio stack separates several responsibilities that should not be treated as one “background audio” channel.

## Native music

`AudioCore` maintains separate exploration, alert and combat music managers/emitters.

The inspected implementation starts those lanes through separate internal paths and also registers a default world-exploration source during initialization.

## Native ambience

`AudioBiome` owns arrays for ambient sources separately from its music/alert/combat sources.

`ManualAudioZone` derives from `AudioBiome` and activates/deactivates as the hero enters/exits its zone.

Public `AudioCore.RegisterAudioSources` / `UnregisterAudioSources` surfaces can operate on a specific `AudioType`.

## Consequence

```text
native ambience
≠ exploration music
≠ alert music
≠ combat music
≠ dialogue/UI/SFX
```

A compatibility-minded music mod should suppress or coexist with the exact lane it intends to replace and leave unrelated audio owners untouched.
