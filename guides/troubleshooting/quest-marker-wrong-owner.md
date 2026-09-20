---
document_type: troubleshooting
scope: quest marker repair research points at generic NPC marker instead of objective marker
last_verified: 2026-09-20
---

# The NPC Has a Marker, but It Is Not the Quest Marker

Do not repair a quest marker by editing/revealing a generic NPC marker merely because the quest target is an NPC.

Check:

1. exact quest template GUID;
2. exact active objective GUID;
3. objective target location/scene;
4. Story-flag marker condition;
5. additional objective markers;
6. runtime membership in active marker-objective collections.

A generic `MarkerData_NPC_Important`-style asset describes normal NPC presentation. It is not evidence of the quest objective's marker owner.
