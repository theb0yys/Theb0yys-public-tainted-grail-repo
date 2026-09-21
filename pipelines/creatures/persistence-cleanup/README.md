# Creature Stage — Persistence and Cleanup

## Objective

Define and prove the actor/package lifetime instead of allowing proof actors or populations to become accidentally persistent.

## Session-only route

For controlled proof actors:

1. mark not saved immediately;
2. retain exact spawned Location identity;
3. clean up on dismiss/failure/shutdown according to the native lifecycle;
4. preserve native death/corpse ownership before discarding the Location;
5. confirm restart does not create a permanent duplicate.

## Durable route

If persistence is intended:

1. identify what actor/location/template identity is serialized;
2. establish provider/template availability ordering during cold load;
3. test save and full exit;
4. cold-start and restore;
5. validate actor state, placement, renderer/animation, AI/combat ownership;
6. test missing/disabled package separately if claimed;
7. test migration when identities or schemas change.

## Validation gate

Runtime spawn is not persistence proof. Cold save/load is required for a durable claim.

## Current boundary

The public creature process deliberately uses save-excluded session actors for several controlled proofs. Do not promote those receipts into general persistent-population guarantees.
