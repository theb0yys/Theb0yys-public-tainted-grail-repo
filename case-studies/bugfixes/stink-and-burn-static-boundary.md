---
document_type: case
scope: Stink and Burn recovery research
evidence:
  official_tooling: MERLIN_STATIC
  runtime: NOT_RUN
last_verified: 2026-09-20
---

# Stink and Burn: Static Identity Is Not a Repair

The research successfully resolved several identities from Merlin Workshop metadata/source:

- quest asset identity;
- Karla LocationSpec;
- Karla NPC template;
- actor identity;
- runtime tag;
- related story assets.

But it **did not** resolve the embedded Find Karla objective GUID/marker definition or a stable placed world/camp anchor.

## Critical correction

Karla's generic NPC marker was explicitly rejected as a substitute for the missing quest-objective marker.

## Lesson

Having the quest GUID and target NPC GUID is still insufficient to write a repair.

A safe recovery needs the exact broken state and exact owner of the missing transition/marker.
