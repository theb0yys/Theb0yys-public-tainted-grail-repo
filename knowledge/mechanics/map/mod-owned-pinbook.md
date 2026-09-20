---
document_type: mechanic
scope: persistent personal map notes without FoA save mutation
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  persistence: MOD_OWNED_FILE
last_verified: 2026-09-20
---

# Mod-Owned World Pinbook

Multi-Pin Map Notes demonstrates a simple alternative to native map/save injection.

## Position source

Primary:

`Hero.Current.Coords`

Fallback on explicit user actions:

`GameObject.FindWithTag("Player").transform.position`

## Storage

Pins are stored in a mod-owned file under BepInEx config:

`kane.tgfoa.multi-pin-map-notes.pins.tsv`

The durable payload is intentionally small:

- pin name;
- Unity world coordinates.

## Why this boundary

The feature does not require:

- native map-marker creation;
- FoA save mutation;
- arbitrary file paths;
- native quest/location ownership.

Deleting the BepInEx config data deletes the pinbook; that is part of the contract.

## Direction labels

Compact direction labels may use the player transform's forward/right basis. They are **relative-to-player labels**, not proven compass north/east/west.
