# Graphics Cookbook

Graphics covers camera presentation, post-processing, visibility, fog, sky and weather rendering.

## Camera and comfort

Current public examples:

- [17 Movement FOV kick](../17-fov-kick-mono/README.md) — LOAD_EVIDENCED
- [20 Camera-shake strength](../20-camera-shake-strength-mono/README.md) — LOAD_EVIDENCED
- [21 FOV transition duration](../21-fov-transition-duration-mono/README.md) — SOURCE_BUILD_EVIDENCED
- [24 Head-bob strength](../24-head-bob-strength-mono/README.md) — LOAD_EVIDENCED
- [25 Motion-blur toggle](../25-motion-blur-toggle-mono/README.md) — LOAD_EVIDENCED

These evidence labels apply to the stated underlying paths. They do not mean every public rewrite was feature-tested.

## Fog and visibility

The maintainer corpus contains a stronger proven mechanism than the current cookbook:

**existing HDRP/FoA fog capture → apply → restore**

A Views of Avalon feature-tested version used reflection against existing active HDRP/FoA volume-profile fog components and received live screenshot confirmation of the visibility change.

That mechanism is recorded in [Proven mechanics](../systems/PROVEN_MECHANICS.md). A clean-room public fog pattern should be published from that proven lineage before adding speculative HLOD/culling examples.

## Culling and HLOD

Native HLOD/culler investigation exists, but broad HLOD mutation is not promoted here. Keep probe-only or source-only material in [Research](../research/README.md) until an exact runtime owner and behaviour are proved.

## Sky and weather

- Generic skybox ownership/apply/restore shapes live under [proven-path mechanism templates](../../proven-paths/README.md).
- Weather rendering has controlled runtime proof in the maintainer corpus, including a bounded Weather Maker profile/prefab execution lane. It is an advanced mechanism because lifecycle, camera/audio/fog interaction and performance must be controlled.

See [Proven mechanics](../systems/PROVEN_MECHANICS.md).
