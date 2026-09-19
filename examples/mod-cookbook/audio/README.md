# Audio Cookbook

Audio covers footsteps, music arbitration/routing, weapon and magic SFX, and world audio.

## Footsteps

- [07 Hero footstep replacement](../07-footstep-beep-replacement-mono/README.md)

The underlying footstep interception/replacement mechanism has runtime precedent: a working owner loaded custom WAV banks, applied the exact FMOD interception path, enabled native suppression and captured dozens of custom footstep plays during a walking trace.

The public beep rewrite remains **NOT_RUN**.

## Music

- [19 Context-lane observer](../19-context-lane-observer-mono/README.md)
- [22 Native music mute](../22-native-music-mute-mono/README.md)
- [Contextual music routing recipe](../recipes/03-contextual-music/README.md)

The maintainer corpus has live load/indexing and context-routing evidence, but each public claim must stay scoped to the exact observed lane. Do not treat track indexing or patch registration alone as proof that every scene/category transition sounds correct.

## Weapon and magic SFX

No generic public pattern should be promoted merely because a weapon or spell action hook exists. SFX needs a proved action/event owner, playback owner, replacement/additive policy and cleanup/voice limit.

## World audio

World ambience and actor/item/footstep/music audio are separate owners. Do not collapse them into a single FMOD replacement recipe without exact event ownership evidence.
