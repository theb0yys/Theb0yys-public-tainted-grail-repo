# Wave 5 Packet — Audio and FMOD

## Reader job

Separate FMOD/music/native audio ownership from bounded replacement or mod-owned playback procedures.

## Legacy source pages

- `docs/reference/AUDIO_FMOD_INTEGRATION.md`
- `docs/reference/AUDIO_MUSIC.md`

## Why a packet is required

These pages use the old universal handbook body and combine at least two current archetypes:

1. native-system explanation/ownership/lifecycle;
2. actionable modding intervention;
3. failure/correction reasoning;
4. verification/proof-boundary guidance.

The split is by **information responsibility**, not page size.

## Canonical mapping

| Legacy source | Native-system destination | Mechanic destination |
|---|---|---|
| `AUDIO_FMOD_INTEGRATION.md` | `systems/audio/fmod-event-identity.md` | `mechanics/audio/fmod-replacement.md` |
| `AUDIO_MUSIC.md` | `systems/audio/README.md` | `mechanics/audio/music-and-playback.md` |

## System-page responsibility

The system destination owns:

- what the subsystem is;
- native/game owners;
- identities/types/methods required to understand ownership;
- lifecycle/data flow;
- current technical proof boundary.

## Mechanic-page responsibility

The mechanic destination owns:

- how a mod may interact;
- why the intervention seam is chosen;
- failed/rejected approaches;
- verification of the intervention;
- a link back to the canonical system owner.

## Material that must survive the split

- exact native event identity;
- native playback owner;
- music/ambience/item/actor lane separation;
- raw playback boundaries;
- rights/provenance and release ownership;

## Evidence / claim boundary

This packet authorizes **documentation-role separation only**.

- Existing public technical claims are not silently strengthened.
- The migration does not newly prove runtime, save, compatibility, performance, or release behaviour.
- Any claim that cannot be cleanly preserved from the current public body remains bounded/unknown rather than being invented.
- Private production source and proprietary assets remain out of scope.

## Legacy path

After both canonical pages exist, the legacy page becomes a compatibility redirect to the **system page**, which links the mechanic.

## Completion checks

- no duplicate canonical explanation;
- system ownership/lifecycle is readable without the procedure;
- mechanic is actionable without restating the complete system model;
- failures remain attached to the intervention they constrain;
- evidence limits remain explicit;
- internal links resolve after relocation.
