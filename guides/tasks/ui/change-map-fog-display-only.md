# Change Map Fog Display Without Rewriting Discovery

**Evidence status: PARTIAL.** The safe display-only boundary is established; the case study does not record a full runtime validation matrix.

Working lineage: [Map Fog: Display Without Rewriting Discovery Memory](../../../research/case-studies/map/map-fog-display-only.md).

## Goal

Change what the map screen **shows** while leaving the game's discovery memory untouched.

The key owner is native `MapMemory.visitedPixels`.

## Process

```text
native discovery memory
→ map presentation owner reads it
→ mod adjusts display/reveal policy
→ rendered map changes
```

Do not rewrite `visitedPixels` merely to reveal the visual map.

## Why

Keeping native memory intact preserves:

- discovery progression;
- save ownership;
- quest/discovery state;
- fast-travel ownership.

## Verification

Check separately:

- map display changes;
- native visited/discovery memory remains unchanged;
- fast travel availability remains native;
- save/load does not record fake discovery;
- disabling the mod restores normal display.

## Current proof boundary

The ownership distinction is established. The exact public implementation still requires runtime display and rollback validation.
