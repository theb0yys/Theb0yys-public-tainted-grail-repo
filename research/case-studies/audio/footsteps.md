# Native Hero Footstep Replacement

Immersive Footsteps provides a proven, narrow FMOD replacement seam.

## Working implementation lineage

The runtime path loaded custom audio, installed the exact patch and recorded a walking trace with 57 custom footstep plays while native suppression was enabled.

## Native owner

FoA's hero footstep owner is `VHeroFootsteps`.

Its playback path eventually calls this FMOD overload:

~~~text
FMODManager.PlayOneShot(
    EventReference,
    Vector3,
    UnityEngine.Object,
    FMODParameter[])
~~~

The working mod filters that call so it only handles requests whose debug/source object is `VHeroFootsteps`.

## Surface information

FoA already supplies footstep parameters such as:

- `FTS_Grass`
- `FTS_Gravel`
- `FTS_Ground`
- `FTS_Mud`
- `FTS_Puddle`
- `FTS_Snow`
- `FTS_Stone`
- `FTS_Metal`

Use those parameters instead of raycasting/classifying the world again.

## Replacement rule

~~~text
native footstep request
→ confirm source is VHeroFootsteps
→ read FoA surface parameters
→ find one matching custom sound
→ start custom playback
→ suppress native event only if custom playback succeeded
~~~

If no replacement exists, let the native sound continue.

## Runtime hygiene

- cache loaded sound data;
- cap diagnostic logging;
- expose one master volume;
- do not intercept unrelated `PlayOneShot` calls.
