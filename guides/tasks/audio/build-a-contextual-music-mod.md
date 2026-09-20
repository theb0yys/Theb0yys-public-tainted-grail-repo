# Build a Contextual Music Mod

Use a mod-owned FMOD Core music player and read FoA context to select lanes. Keep native dialogue, UI, SFX and ambience as separate owners.

Working lineage: [Tainted Music: Own Your Lane, Not All Audio](../../../research/case-studies/audio/tainted-music-lane-ownership.md).

## Runnable source

Start with the [Contextual music example](../../../examples/mono/audio/contextual-music/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Player structure

The maintained Tainted Music implementation owns:

- FMOD.System;
- a mod-owned FMOD.ChannelGroup;
- a list of MusicVoice objects;
- lane → track definitions;
- lane → next-track index.

Example lanes include:

~~~text
Wyrdness
DayOpenWorld
Interior
Settlement
ScaryPlace
None
~~~

## Create a mod-owned sound

For embedded WAV bytes, the working code uses FMOD Core:

~~~csharp
FMOD.CREATESOUNDEXINFO info = new()
{
    cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)),
    length = checked((uint)wavBytes.Length),
    suggestedsoundtype = FMOD.SOUND_TYPE.WAV
};

FMOD.MODE mode =
    FMOD.MODE.OPENMEMORY |
    FMOD.MODE.CREATESAMPLE |
    FMOD.MODE.LOOP_OFF |
    FMOD.MODE._2D;

_coreSystem.createSound(
    wavBytes,
    mode,
    ref info,
    out FMOD.Sound sound);
~~~

Validate the FMOD result and require a valid sound handle.

## Start a voice paused at zero volume

The working start route is:

~~~csharp
_coreSystem.playSound(
    sound,
    _masterChannelGroup,
    true,
    out FMOD.Channel channel);

channel.setVolume(0f);
channel.setPaused(false);
~~~

Then store a MusicVoice with:

- lane;
- channel;
- sound;
- display name;
- current volume;
- target volume;
- fade seconds;
- stop-when-silent flag.

## Select a lane from read-only game context

Poll context at a bounded interval rather than reflecting every frame.

Useful inputs already used by the implementation include:

- Hero.Current/playable state;
- scene/open-world state;
- game time/day-night;
- Wyrdness state;
- dialogue context;
- recent hero-involved combat;
- configured settlement/scary-place scene policy.

Lane selection returns a MusicLane. It should not start/stop audio itself.

## Crossfade

When a lane changes:

1. mark old voices to fade toward zero;
2. start the new voice at zero;
3. set its target to the lane volume;
4. move current volume toward target every update;
5. call channel.setVolume(current);
6. when a stop-when-silent voice reaches zero, stop the channel and release its FMOD.Sound.

The maintained implementation clamps fade time and uses separate lane/context fade settings.

## Dialogue/combat ducking

Do not lower native dialogue/combat volume.

Calculate the target volume of the **plugin-owned** voice:

~~~text
global volume
× lane volume
× dialogue multiplier when dialogue active
× combat multiplier while combat hold timer active
~~~

When target volume changes, use the shorter context-fade duration.

## Pause state

Read the mod-owned master channel group's pause state and mirror it to owned voices.

Do not let fade time advance while the master group is paused.

## Native music suppression

If the mod is intended to replace native music, add suppression as a separate feature.

Suppress only the known native exploration/alert/combat start paths while plugin music is actually active. If a private target cannot be resolved, leave native music running.

Do not mute AudioCore globally.

## Ambience remains separate

ManualAudioZone ambience is a different owner.

Only suppress a specific ambient-zone source when the feature explicitly needs it, and re-register/restore that zone when suppression ends.

## Shutdown

On disable/unload:

~~~text
stop all mod-owned FMOD channels
→ release all mod-owned FMOD.Sound handles
→ clear lane/track state
→ remove Harmony suppression patches
→ restore any specifically suppressed native ambience
~~~

Never leave the game silent because your plug-in unloaded.
