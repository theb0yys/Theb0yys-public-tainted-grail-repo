<!-- Canonical Wave 5 native-system page split from docs/reference/AUDIO_FMOD_INTEGRATION.md. -->
# Audio, FMOD, Event Identity, and Safe Replacement Boundaries

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/audio/fmod-replacement.md).

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

## Current proof boundary

The repository has strong bounded actor/item audio replacement evidence through exact FMOD-owned hooks and hash-pinned sidecar assets.

It does not imply every FMOD Studio event can be safely replaced by raw Core playback, and it does not establish a universal event semantic map from filenames alone.
