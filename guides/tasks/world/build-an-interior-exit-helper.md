# Build an Interior Exit Helper

**Evidence status: PARTIAL.** The narrow architecture is established, but a complete runtime validation matrix is not recorded in the case study.

Working lineage: [Small Exit Helper Instead of a Dungeon Map](../../../research/case-studies/travel/interior-exit-helper.md).

## Goal

Answer only:

- where did I enter?
- which direction leads back out?

Do **not** build a minimap or reverse-engineer dungeon geometry unless that is genuinely required.

## Process

1. Observe scene transition/metadata.
2. Capture the hero position associated with the entry point.
3. Store only session-local helper state.
4. Read current hero coordinates.
5. Render a passive direction/distance overlay.
6. Clear/rebind state on scene transition.

```text
interior entered
→ capture entry anchor
→ current hero position
→ direction + distance
→ passive overlay
```

## Ownership

The helper owns:

- captured entry anchor;
- overlay presentation.

FoA still owns:

- scene;
- navigation;
- map;
- player movement;
- save state.

## Verification

Test:

- enter one interior;
- move away from entrance;
- direction/distance update correctly;
- return toward entrance;
- leave scene;
- helper state clears/rebinds;
- no native map/discovery state changes.

## Current proof boundary

The reduced owner surface and sidecar design are established. Full runtime/UI/scene-transition proof remains to be completed for the public implementation.
