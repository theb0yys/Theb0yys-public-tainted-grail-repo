# Audio, Music, and FMOD Ownership

> **Reference page.** FoA audio can involve native FMOD events/banks, native audio owners, ambience zones, footsteps, and mod-owned embedded playback. Replacing one does not prove the others.

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

## How we interact with it

### Decide ownership first

Choose whether the mod:

- observes native audio;
- suppresses one native cue;
- replaces presentation through a proven owner;
- or plays a separate mod-owned cue.

Do not mix those claims.

### Cache and throttle

Audio context polling and resource creation should not happen unbounded every frame.

### Release owned sounds/channels

Mod-owned FMOD resources need explicit teardown.

### Keep voice/localisation separate

A Story line's text identity and its FMOD voice resource are not the same registration system.

## Why this route

Audio work produced several useful lessons:

- Unity audio is not always the correct native ownership surface;
- native FMOD Studio bank/event replacement is much riskier than playing bounded mod-owned audio;
- a damage hook can be useful as **context observation** without altering damage;
- broad ambience/music suppression needs exact owner/type scoping.

## What goes wrong

- replacing every native audio event globally;
- creating/releasing sounds repeatedly in hot paths;
- mutating shared FMOD bank/event ownership without lifecycle proof;
- suppressing footsteps and accidentally suppressing AI/noise gameplay semantics;
- using damage patch as music logic rather than a timestamp/context signal;
- assuming text localisation package contains voice content;
- forgetting to stop/release mod-owned voices.

## How to verify

For audio work, verify:

- exact owner/cue/event;
- trigger count/timing;
- 2D vs 3D position;
- volume/distance;
- native cue suppressed or left intact as intended;
- gameplay/noise semantics unaffected where required;
- scene/transition behavior;
- dialogue/combat ducking;
- disable/shutdown cleanup;
- performance/log spam.

## Current proof boundary

The repository contains multiple working/source-backed audio patterns: native music suppression by exact AudioCore calls, mod-owned FMOD playback, bounded footstep replacement/suppression, and embedded companion cues.

These do not form one universal native-audio replacement API.
