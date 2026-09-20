# Replace a Sound Without Silencing the Game on Failure

Use this for a bounded audio-replacement hook.

Canonical ownership: [Audio/FMOD integration](../../systems/presentation/audio-fmod-integration.md).

## Replacement rule

For a replacement hook:

```text
identify exact native playback owner
→ verify target identity
→ attempt replacement
→ replacement actually starts
→ only then suppress original
```

If replacement fails, let the original native path continue.

This is the **replacement-first / suppress-second** invariant.

## Procedure

1. Identify the exact native playback method/event owner.
2. Identify the actor/item/world target with native identity, not a filename guess.
3. Record the original FMOD event identity when relevant.
4. Load/prepare the replacement under mod ownership.
5. In the hook, attempt playback inside a bounded failure path.
6. Return/suppress the original only after replacement reports success.
7. On any error or unsupported case, fail open to native playback.
8. Dispose owned clips/sources/banks/resources on teardown.

## Preserve unsupported native semantics

Do not replace parameterized/complex FMOD routes with a raw `AudioSource` merely because a WAV can be played.

If the replacement route cannot reproduce required parameters, spatial ownership, lifecycle or follow behavior, preserve the native event.

## Asset boundary

Do not commit commercial/ripped game audio.

For external sounds, record licence/provenance and redistribution permission before publication.

See [Modder Resources](../../reference/modder-resources/README.md).

## Verification

Check:

- exact native owner/target;
- replacement asset hash/source;
- playback success marker;
- original suppression only on success;
- failure path preserves original;
- 3D position/follow behavior;
- repeated-call behavior;
- teardown/disposal;
- Mono/IL2CPP independently if both are claimed;
- subjective in-game audition.

Working lineage: [Audio case studies](../../case-studies/audio/README.md).
