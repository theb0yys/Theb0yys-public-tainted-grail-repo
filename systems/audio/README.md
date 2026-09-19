<!-- Canonical Wave 5 native-system page split from docs/reference/AUDIO_MUSIC.md. -->
# Audio, Music, and FMOD Ownership

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/audio/music-and-playback.md).

## What this system is

Important audio lanes include:

~~~text
native music ownership
native ambience/zone ownership
native item/creature/interaction audio
hero footsteps/noise
dialogue/voice
mod-owned FMOD Core playback
embedded audio resources
~~~

Text localisation and voice/audio are separate resource domains.

## Who owns it in FoA

Research-backed owners/surfaces include:

- `AudioCore`
- native music emitters
- `ManualAudioZone`
- `AudioBiome`
- FMOD Runtime/Core
- `VHeroFootsteps`
- movement/noise owners such as `HumanoidMovementBase.MakeMovementSound`
- `ThieveryNoise`
- Story sound-bank dependencies
- mod-owned embedded audio runtime when intentionally separate from native banks

## Important identities, types, and methods

Examples:

- `AudioCore.PlayExplorationMusic`
- `AudioCore.PlayAlertMusic`
- `AudioCore.PlayCombatMusic`
- scoped native audio-source unregister/suppression surfaces
- `VHeroFootsteps.FootStep(...)`
- `HealthElement.TakeDamage` observation used only to mark recent combat context for music ducking
- FMOD Core 2D/3D sound/channel creation for mod-owned cues

## Where it exists in the lifecycle

### Plugin-owned music example

~~~text
read/throttle scene/time/Wyrdness/dialogue/combat context
→ choose one music lane
→ crossfade plugin-owned FMOD channels
→ optionally suppress native music start calls while plugin lane owns music
→ restore/release on disable/shutdown
~~~

### Footstep example

~~~text
native footstep event
→ exact scoped condition
→ suppress/replace only that cue
→ leave movement/gameplay state unchanged
~~~

### Embedded creature cues

A mod can embed validated PCM assets and play its own bounded 3D sounds without mutating native FMOD banks/events.

That proves a mod-owned audio lane, not native creature-audio integration.

## Current proof boundary

The repository contains multiple working/source-backed audio patterns: native music suppression by exact AudioCore calls, mod-owned FMOD playback, bounded footstep replacement/suppression, and embedded companion cues.

These do not form one universal native-audio replacement API.
