# Audio, FMOD, Event Identity, and Safe Replacement Boundaries

> **Reference page.** Use this when replacing or augmenting weapon, creature, world, or magic audio.

## What this system is

FoA uses FMOD Studio/runtime integrations for game audio.

The working research distinguishes:

- exact native FMOD event identity;
- native playback owner;
- optional mod-owned raw PCM/WAV sidecar playback;
- Studio event parameters/buses/occlusion;
- selector/authority mapping;
- audio file discovery/semantics.

A WAV filename is not the same thing as a native FMOD event identity.

## Who owns it in FoA

Important researched owners include:

- `VLocation.PlayAudioClip(EventReference, bool, GameObject, FMODParameter[])` for actor/location audio;
- `CharacterHandBase.PlayAudioClip(ItemAudioType, bool, FMODParameter[])` for equipped item audio;
- native FMOD manager/emitter routes;
- `EventReference.Guid` for exact native event identity;
- item/actor template GUIDs for scoping replacement authority.

## Important identities, types, and methods

### Actor audio selector

A proven/researched selector shape is:

~~~text
actorTemplateGuid
+ npcTemplateGuid
+ originalEventGuid
~~~

The original FMOD event GUID is part of the authority key.

### Item audio selector

A researched equipped-item selector shape is:

~~~text
itemTemplateGuid
+ consumerId
+ originalEventGuid
~~~

### Raw replacement playback

The active replacement proof uses FMOD Core:

~~~text
OPENMEMORY | CREATESAMPLE
~~~

with pinned PCM WAV bytes.

The native Studio call is normally allowed to continue unless a separately proven replacement policy says otherwise.

## Where it exists in the lifecycle

A safe scoped sidecar/replacement path is:

~~~text
native playback method called
→ identify exact actor/item + original event GUID
→ check binding/authority manifest
→ validate pinned source/hash
→ optional mod-owned FMOD Core one-shot
→ native Studio path continues
→ transient channel/resource cleanup
~~~

Parameterized/persistent native events are a different class of problem.

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

The tested active-injection route was rejected.

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

## Current proof boundary

The repository has strong bounded actor/item audio replacement evidence through exact FMOD-owned hooks and hash-pinned sidecar assets.

It does not imply every FMOD Studio event can be safely replaced by raw Core playback, and it does not establish a universal event semantic map from filenames alone.
