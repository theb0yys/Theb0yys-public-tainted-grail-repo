---
document_type: mechanic
scope: recover or clean up an already-managed one-session companion
runtime: mono
evidence:
  source: SOURCE_INSPECTED
  runtime: PROJECT_SPECIFIC
last_verified: 2026-09-20
---

# Recover a Managed One-Session Companion

Recovery should operate only on actors the mod already owns.

## Safe recovery outcomes

For each tracked managed actor:

- discarded Location → remove stale tracking;
- missing required live components → discard if lifecycle guard owns cleanup;
- dead character → remove command surfaces and discard; **do not resurrect**;
- live but far/stuck → recall through the already-verified placement route;
- healthy/in-range → leave native behaviour alone.

## Deliberately excluded

Recovery is not:

- healing;
- resurrection;
- respawn;
- reload restoration;
- actor re-adoption;
- custom pathfinding;
- arbitrary scene search.

If a one-session actor is dead, cleanup it and let a later **explicit new summon** create a new actor.
