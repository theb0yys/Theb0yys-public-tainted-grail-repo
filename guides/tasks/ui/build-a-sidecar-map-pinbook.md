# Build a Sidecar Map Pinbook

**Evidence status: PARTIAL.** The ownership architecture is established, but a complete public runtime/UI/restore matrix is not recorded.

Working lineage: [Sidecar Pinbook Instead of Native Map Injection](../../../research/case-studies/map/mod-owned-pinbook.md).

## Goal

Create durable personal position notes without claiming native map-marker registration.

## Ownership model

Use:

- native hero coordinates as position truth;
- plugin-owned marker records;
- plugin-owned UI;
- a small sidecar file under BepInEx config.

Do not mutate native map/compass markers or FoA save data.

## Process

```text
user creates pin
→ capture current native coordinates
→ assign mod-owned ID/name
→ write sidecar record
→ project records in plugin UI
→ edit/delete only mod-owned records
```

A simple TSV/JSON-like sidecar is enough if its format/versioning is explicit.

## Required fields

At minimum:

- mod-owned pin ID;
- label;
- world/scene identity;
- coordinates;
- optional notes/category;
- format version.

## Failure handling

If the sidecar is missing:

- start empty.

If one row is invalid:

- reject/report that row rather than corrupting native game state.

## Verification

Prove:

- create pin;
- reload plugin/game;
- sidecar loads;
- scene identity is interpreted correctly;
- edit/delete works;
- malformed row fails safely;
- native map/discovery state is unchanged.

## Current proof boundary

The sidecar design is the established lesson. Complete runtime persistence/UI validation remains to be demonstrated for the public implementation.
