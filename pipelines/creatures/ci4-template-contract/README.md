# Creature CI4 — NpcTemplate and LocationTemplate Contract

## Objective

Author pack-owned creature definitions that preserve the selected native actor contract.

## Required definitions

- stable pack-owned NpcTemplate GUID/name;
- stable pack-owned LocationTemplate GUID/name;
- native-compatible attachment/component order;
- behavior/fighting-style references;
- animation mapping;
- scale policy;
- visual root references;
- controller/collider/grounding data;
- faction/stat/profile decisions;
- provider/resolver identity.

## Procedure

1. Create the NpcTemplate from the reviewed baseline contract.
2. Create the LocationTemplate required to construct the actor/location.
3. Preserve required attachment order and component expectations.
4. Bind the exact visual and animation identities from CI3/CI4A.
5. Validate all stable GUIDs and provider ownership.
6. Load/resolve definitions in isolation.
7. Reject raw fallback templates that fork provider identity or bypass the pack owner.

## Check

Confirm that the exact pack-owned definitions resolve and satisfy the recorded native structural contract.

## Before continuing

A valid template is not a live actor and does not cover movement, AI, combat, death, cleanup, population, or persistence.

## Next

Proceed to [CI5 runtime actor lifecycle](../ci5-runtime-lifecycle/README.md).
