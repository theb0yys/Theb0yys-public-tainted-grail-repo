# HLOD Proxy vs Source Ownership

Use this page when a distant object does not match the nearby/source representation.

Canonical overview: [HLOD](README.md).

## Two representations, one world subject

A world object/group may have:

```text
near/source representation
↔ distance transition
↔ HLOD proxy representation
```

The proxy is a performance representation. It is not the gameplay source of truth.

## Modding consequence

Changing only the near/source renderer may leave the distant HLOD proxy unchanged.

Changing only the proxy may make the distant view look right while the nearby object remains native.

Therefore decide which claim you are making:

- near rendering change;
- distant proxy change;
- both representations kept visually coherent.

## Runtime ownership

HLOD owns:

- hierarchy/tree state;
- camera/distance transitions;
- proxy load/release;
- low/high representation switching.

Do not replace that with manual GameObject enable/disable logic unless you have proven the exact owner path is insufficient.

## Verification

Test at multiple distances:

1. approach from far away;
2. cross the proxy/source transition;
3. remain near;
4. move back out;
5. repeat after scene reload;
6. inspect for duplicate/overlapping representations;
7. verify resources release correctly.

A screenshot at one distance is not HLOD integration proof.
