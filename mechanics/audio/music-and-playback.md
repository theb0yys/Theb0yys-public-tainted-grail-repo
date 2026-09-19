<!-- Canonical Wave 5 mechanic page split from docs/reference/AUDIO_MUSIC.md. -->
# Audio, Music, and FMOD Ownership — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/audio/README.md) first for ownership, identities, lifecycle, and the current technical proof boundary.

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

## Evidence boundary

This Wave 5 split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/audio/README.md).
