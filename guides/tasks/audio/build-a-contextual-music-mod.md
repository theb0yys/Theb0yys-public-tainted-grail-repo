# Build a Contextual Music Mod

Use this guide to build a Mono/BepInEx music mod that plays **mod-owned background music** based on read-only FoA game context.

This guide intentionally does **not** replace all game audio. It keeps these owners separate:

```text
plugin-owned background music
native exploration music
native alert music
native combat music
native ambience zones
dialogue / UI / SFX
```

The architecture comes from [Tainted Music: Own Your Lane, Not All Audio](../../../research/case-studies/audio/tainted-music-lane-ownership.md).

## Evidence status

This guide is based on a **PARTIAL** case study.

Established:

- native music and ambience are separate owners;
- plugin-owned FMOD Core playback is a valid lane;
- read-only context can select a music lane;
- dialogue/combat can duck plugin-owned channels;
- native exploration/alert/combat suppression can be scoped narrowly;
- native ambience can be handled separately;
- private suppression failures should fail open to native audio.

Still not promoted as a complete public guarantee:

- the full audible overlap matrix;
- every lane transition;
- every unique/boss/dramatic exception;
- every ambience restore case.

Build the proven architecture first, then run the validation matrix at the end.

## What you will build

```text
read FoA context
→ choose one plugin music lane
→ load/play mod-owned FMOD music
→ crossfade plugin-owned channels
→ duck plugin music during dialogue/recent combat
→ optionally suppress only overlapping native music
→ leave ambience/dialogue/UI/SFX alone
→ restore native ownership when plugin music stops
→ release plugin-owned FMOD resources
```

Canonical system pages:

- [Audio, Music, and FMOD Ownership](../../../knowledge/systems/presentation/audio-music.md)
- [Native Music and Ambience Ownership](../../../knowledge/systems/presentation/audio-ownership.md)
- [Plugin-Owned Contextual Music Lanes](../../../knowledge/mechanics/audio/plugin-owned-music-lanes.md)
- [Scoped Native Music Suppression](../../../knowledge/mechanics/audio/native-music-suppression.md)
- [Scoped Ambient-Zone Suppression](../../../knowledge/mechanics/audio/scoped-ambient-zone-suppression.md)

## Prerequisites

1. complete the [first Mono plug-in](../../getting-started/first-mono-plugin.md);
2. prove your plug-in can load and update periodically without log spam;
3. prepare music you have the right to redistribute/use;
4. start with one or two tracks, not a full soundtrack replacement.

Do not begin by patching every FMOD event.

## Step 1 — choose your ownership model

For the first version, your mod owns only its own FMOD Core sounds/channels.

It does not own:

- dialogue;
- UI;
- SFX;
- native ambience;
- native game state;
- native music managers except for narrowly scoped optional suppression.

That means teardown is simple:

```text
your mod started the channel
→ your mod fades/stops it
→ your mod releases its sound resources
```

## Step 2 — define a small lane set

A source-inspected Tainted Music policy used context such as:

- playable `Hero.Current`;
- strict Wyrdness exposure;
- `SceneService.IsOpenWorld`;
- native time / `WeatherTime.IsNight`;
- dialogue involvement;
- recent hero-involved damage;
- configurable scene/display-name keywords.

A deliberately bounded first policy is:

```text
no playable hero → silence
Wyrdness        → wyrdness lane
scary place     → scary lane
settlement      → settlement lane
interior        → interior lane
open-world day  → day lane
otherwise       → silence
```

"Settlement" and "scary place" are **mod policy**, not invented native FoA taxonomies. Keep the keyword/configuration distinction visible.

## Step 3 — build a read-only context snapshot

Do not let the music selector mutate game state.

At a throttled interval, collect only the values required by your policy:

```text
hero available?
open world?
day/night?
strict Wyrdness?
dialogue active?
recent combat timestamp?
scene/display classification?
```

Return a small immutable/context value to the music selector.

Do not poll expensive reflection/discovery every frame.

## Step 4 — select exactly one target lane

Keep lane selection deterministic.

Conceptually:

```csharp
MusicLane SelectLane(Context c)
{
    if (!c.HasPlayableHero) return MusicLane.Silence;
    if (c.InWyrdness)      return MusicLane.Wyrdness;
    if (c.IsScaryPlace)    return MusicLane.Scary;
    if (c.IsSettlement)    return MusicLane.Settlement;
    if (!c.IsOpenWorld)    return MusicLane.Interior;
    if (c.IsDay)           return MusicLane.Day;
    return MusicLane.Silence;
}
```

Do not mix playback into this function. Selection and playback are separate responsibilities.

## Step 5 — load plugin-owned music

For each configured track:

1. validate the file;
2. create/load it through the mod's FMOD Core lane;
3. retain the sound resource while needed;
4. keep clear ownership so shutdown can release it.

Do not rewrite native Studio banks merely to play your own background track.

## Step 6 — crossfade only your own channels

When the selected lane changes:

```text
old plugin lane playing
→ start/prepare new plugin lane
→ fade old plugin target volume down
→ fade new plugin target volume up
→ stop/release old channel when finished
```

Crossfade state should belong to your player, not to FoA's native music objects.

## Step 7 — duck plugin music for dialogue/combat

Dialogue and recent combat can reduce the **plugin-owned target volume**.

For example:

```text
base lane volume
× dialogue duck factor
× combat duck factor
= plugin target volume
```

Do not mute dialogue or alter combat to achieve ducking.

The damage observation used by the working design is context only: it records recent hero-involved combat timing and does not alter damage.

## Step 8 — decide coexistence vs replacement

Start in **coexistence mode** if possible:

```text
plugin lane plays
+ native music remains untouched
```

This proves your context/player/crossfade system before adding suppression.

If your intended mod replaces native background music, add suppression as a separate stage.

## Step 9 — suppress only native music lanes you replace

FoA separates native exploration, alert and combat music.

The researched suppression boundary is:

```text
plugin music active
→ suppress native exploration/alert/combat start paths
→ optionally stop only those three native music emitters as a backstop
```

Do **not** broadly suppress:

- ambience;
- snapshots;
- dialogue;
- UI;
- SFX.

The suppression targets are private/internal and therefore patch-sensitive.

If an expected target cannot be found:

> fail open to native music.

Do not respond to a missing private method by muting broader `AudioCore` behaviour.

## Step 10 — add explicit exception policy

Some scenes/events may require authored native music.

Represent those as explicit configuration/policy, for example:

```text
unique/boss/dramatic exception active
→ plugin lane yields or changes policy
→ native authored music is allowed
```

Do not claim that a keyword list perfectly identifies every authored special event.

## Step 11 — keep ambience separate

A repetitive ambient zone is not the same owner as native music.

If you have a specific proven reason to suppress an active `ManualAudioZone` ambience:

1. enumerate the relevant active zone;
2. unregister only its ambient sources;
3. leave music/dialogue/UI/SFX alone;
4. re-register still-active native ambience when suppression ends.

Do not include ambient-zone suppression in your first music build unless you actually need it.

## Step 12 — shutdown and disable cleanly

On disable/shutdown:

```text
stop/fade plugin channels
→ release plugin-owned sounds
→ clear plugin lane state
→ remove scoped suppression patches
→ ensure native music/ambience ownership is no longer suppressed
```

Never leave the game silent because your mod unloaded.

## Validation matrix

### A. Plugin-owned player

Verify:

- one lane starts;
- no duplicate channels accumulate;
- changing lane crossfades;
- silence state really stops/releases owned playback;
- shutdown releases resources.

### B. Context transitions

Exercise separately:

- no hero → playable hero;
- interior → open world;
- day → night;
- normal → Wyrdness;
- normal → settlement/scary policy bucket;
- scene transition.

Record which transitions were actually tested.

### C. Ducking

Verify independently:

- dialogue begins → plugin music ducks;
- dialogue ends → plugin volume restores;
- combat observation → plugin music ducks;
- combat timeout → plugin volume restores;
- dialogue/combat audio themselves remain untouched.

### D. Native coexistence/suppression

With suppression disabled:

- native music still behaves normally.

With suppression enabled:

- only exploration/alert/combat overlap is targeted;
- ambience remains;
- dialogue remains;
- UI/SFX remain;
- missing/private target failure allows native music.

### E. Exceptions

Test each configured unique/boss/dramatic exception separately.

Do not mark the feature fully validated until those audible cases are actually heard in game.

## Common mistakes

### Treating all background sound as "music"

FoA ambience has separate owners. Suppressing the wrong owner can remove environmental sound unintentionally.

### Making context selection mutate gameplay

Music should consume read-only context wherever possible.

### Ducking native audio

Duck your own channels. Dialogue/combat are context, not volume targets.

### Muting broad AudioCore behaviour

Suppress only the exact native music lanes you intend to replace.

### Claiming a full matrix from partial evidence

A clean build and registered patches do not prove audible overlap, transitions or authored exceptions.

## Current proof boundary

**Established architecture:** plugin-owned FMOD Core music lane, read-only context selection, plugin-only ducking, scoped native music suppression design, separate ambience ownership, fail-open behaviour.

**Partial runtime evidence:** some day/night/Wyrdness lane transitions and suppression/load behaviour.

**Still requires explicit audible validation:** complete overlap matrix, all lane transitions, authored exceptions, and scoped ambient restore for the current target build.

## Next steps

After the baseline player passes:

1. add multiple tracks per lane;
2. add shuffle/repeat policy;
3. add user configuration;
4. test every transition;
5. only then consider ambience exceptions or deeper native integration.

The central rule remains: **own your music lane, not all audio.**
