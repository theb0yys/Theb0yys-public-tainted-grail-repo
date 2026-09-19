<!-- Canonical Wave 5 mechanic page split from docs/reference/AUDIO_FMOD_INTEGRATION.md. -->
# Audio, FMOD, Event Identity, and Safe Replacement Boundaries — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/audio/fmod-event-identity.md) first for ownership, identities, lifecycle, and the current technical proof boundary.

## How we interact with it

### Scope replacement to exact native identities

Do not replace "all sword sounds" based only on filename/category guesses.

Bind:

- exact item/actor identity;
- exact native event GUID;
- exact consumer/action;
- exact replacement asset/hash.

### Preserve native playback when parameters matter

Raw FMOD Core playback cannot reproduce every Studio event feature:

- parameters;
- authored buses;
- event timelines;
- occlusion;
- persistent emitter state.

The working audio proof therefore leaves parameterized/persistent calls native-only.

### Fail closed on build/hash mismatch

The audio research includes a useful compatibility event: after a game update changed `TG.Main.dll`, stale pinned authority failed closed with zero active bindings rather than bypassing the guard.

That is the correct behavior.

### Guard against foreign Harmony ownership

For exact high-value audio overloads, inspect Harmony patch ownership before attaching a conflicting replacement patch.

## Why this route

Earlier experiments established that Unity `AudioSource` was not a reliable active-injection path in the tested FoA process.

Diagnostics showed unusable/zeroed Unity audio state for the intended path, so the active proof moved to FMOD Core rather than forcing Unity audio.

This is a key failure lesson:

> The engine contains Unity audio APIs, but the native game owner for the target behavior is FMOD. Use the actual owner.

## What goes wrong

### Filename/category semantics treated as event authority

A file named "attack" does not prove which native event should trigger it.

### Raw Core sound replaces parameterized Studio event

Loses authored parameter semantics, mixer routing, occlusion or event state.

### Global audio hook

Can replace unrelated actors/items and collide with other mods.

### Stale exact assembly/event authority used after update

The correct behavior is to disable the custom binding until revalidated.

### Unity AudioSource forced into an FMOD-owned lane

Use the replacement/ownership route described below rather than active injection.

### Raw audio committed without rights/provenance

Public packaging must respect asset rights; local research assets should stay local unless redistribution is allowed.

## How to verify

For an audio replacement:

1. exact native playback owner;
2. exact actor/item template identity;
3. exact original FMOD event GUID/path;
4. exact replacement asset hash;
5. binding authority manifest;
6. patch ownership;
7. custom sound loads;
8. native call behavior (preserved/suppressed) matches design;
9. 3D position/follow owner is correct;
10. parameterized events remain native when unsupported;
11. no warnings/errors;
12. game update causes fail-closed until revalidated;
13. subjective in-game audition passes.

## Evidence boundary

This Wave 5 split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/audio/fmod-event-identity.md).
