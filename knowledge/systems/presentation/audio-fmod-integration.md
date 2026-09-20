# Audio and FMOD Integration

Use this page when you want to **replace, augment, or selectively intercept weapon, creature, world, or magic audio**.

The first rule is:

> A WAV filename is not a native FMOD event identity.

FoA audio behavior is owned by FMOD playback routes and game-side callers, not by whatever file name looks semantically similar.

## Useful native playback owners

Researched examples include:

- `VLocation.PlayAudioClip(EventReference, bool, GameObject, FMODParameter[])` for actor/location audio;
- `CharacterHandBase.PlayAudioClip(ItemAudioType, bool, FMODParameter[])` for equipped-item audio;
- FMOD manager/emitter routes;
- `EventReference.Guid` for exact native event identity.

Template IDs are also useful for scoping:

- actor/location template GUID;
- NPC template GUID;
- item template GUID.

## Bind replacements to exact identity

A safe selector should include enough information to avoid replacing unrelated sounds.

Actor example:

~~~text
actorTemplateGuid
+ npcTemplateGuid
+ originalEventGuid
~~~

Item example:

~~~text
itemTemplateGuid
+ consumer/action
+ originalEventGuid
~~~

Do not replace "all sword sounds" or "all attack sounds" based on file/category names alone.

## Raw FMOD Core sidecar playback

One working replacement path uses FMOD Core with in-memory PCM/WAV data:

~~~text
OPENMEMORY | CREATESAMPLE
~~~

The flow is roughly:

~~~text
native playback method fires
→ identify exact actor/item/event
→ check replacement binding
→ validate replacement asset
→ optionally play mod-owned FMOD Core one-shot
→ preserve native Studio call unless policy explicitly suppresses it
→ clean up transient channel/resources
~~~

## Do not replace parameterized Studio events blindly

Raw Core playback cannot reproduce every FMOD Studio behavior:

- event parameters;
- bus routing;
- authored timeline logic;
- occlusion;
- persistent emitter state.

If the native event relies on those features, keep the Studio path native unless you have a separate proven replacement.

## Use FMOD when FMOD owns the behavior

Earlier experiments found Unity `AudioSource` unsuitable for the targeted active injection path in the tested FoA process.

That does not mean Unity audio APIs never exist; it means the targeted behavior was FMOD-owned.

Do not force a Unity audio route into a path the game itself owns through FMOD.

## Fail closed after updates

If your replacement depends on:

- exact method signature;
- exact assembly build;
- exact event GUID;
- exact asset hash;

disable the custom binding when those expectations no longer match.

Do not "best effort" your way past a stale identity.

## Check for competing Harmony patches

High-value broad audio methods may already be patched by another mod.

Before attaching a replacement patch, inspect existing Harmony ownership where practical.

A narrow caller/source filter is safer than globally replacing every invocation.

## Common mistakes

- filename/category = event identity;
- raw Core sound used to replace a parameterized Studio event;
- global playback hook without actor/item filtering;
- stale event/assembly authority used after a game update;
- Unity `AudioSource` forced into an FMOD-owned path;
- unlicensed audio committed to the public repo.

## How to verify a replacement

Check:

1. exact native playback method;
2. exact actor/item template identity;
3. exact original event GUID/path;
4. replacement asset identity/hash;
5. patch ownership/conflicts;
6. replacement sound loads;
7. native call is preserved/suppressed exactly as intended;
8. 3D position/follow owner is correct;
9. unsupported parameterized events stay native;
10. cleanup occurs;
11. update mismatch fails closed;
12. in-game listening confirms the intended result.

## Evidence limits

The repository has strong bounded evidence for actor/item audio replacement through exact FMOD-owned hooks and pinned replacement assets.

It does not prove that every FMOD Studio event is safely replaceable with raw Core playback, nor that file names provide a universal semantic event map.
