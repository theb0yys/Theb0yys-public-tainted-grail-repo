---
document_type: system
scope: native FoA music vs ambience ownership relevant to additive music mods
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
last_verified: 2026-09-20
---

# Music and Ambience Ownership

Use this page when you are changing **background music, combat music, Wyrd ambience, or environmental audio** and need to know which native lane you are actually replacing.

FoA does not have one generic "background audio" owner.

## Native music

`AudioCore` maintains separate music owners for:

- exploration;
- alert;
- combat.

The inspected implementation starts these through separate internal paths and registers a default world-exploration source during initialization.

## Native ambience

`AudioBiome` keeps ambient sources separate from its music/alert/combat sources.

`ManualAudioZone` derives from `AudioBiome` and activates/deactivates as the Hero enters or leaves its zone.

Public `AudioCore.RegisterAudioSources` / `UnregisterAudioSources` calls can operate on a specific `AudioType`.

## Keep the lanes separate

~~~text
ambience
≠ exploration music
≠ alert music
≠ combat music
≠ dialogue
≠ UI
≠ ordinary SFX
~~~

If a mod wants to replace combat music, it should not suppress ambience by accident.

If it wants to replace Wyrd ambience, it should not treat the exploration-music manager as the same owner.

## Practical rule

Identify the exact audio category and native owner you intend to replace, then suppress or coexist with only that path.

Leave unrelated audio categories native.
