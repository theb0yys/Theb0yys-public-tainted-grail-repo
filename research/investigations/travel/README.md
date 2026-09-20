---
document_type: investigation
scope: fast travel / road travel / transition research
evidence:
  current_state: RESEARCH_ONLY
last_verified: 2026-09-20
---

# Travel Investigation

Travel touches too many authoritative systems to begin with a teleport patch.

The private Tainted Travel programme intentionally starts with **read-only discovery and dry-run plans**.

## Questions to answer before mutation

- How does native map fast travel resolve a destination?
- What marks a location fast-travel eligible?
- What marks it discovered?
- Which UI path starts travel?
- Which combat/story/scene states block it?
- Does travel advance time?
- How do weather/status/companions/summons interact?
- Are road signs native interactables, story steps or presentation only?
- Which scene-transition owner actually moves the player?
- Can a route preview be produced without executing travel?

## Blocked during research

Do not:

- move/teleport the player;
- call scene-transition execution;
- rewrite discovery or map markers;
- mutate time/weather;
- charge gold/items;
- bypass quest/story travel locks;
- replace native map UI;
- write saves.

Use diagnostics to turn unknowns into bounded owners before creating a travel mechanic.
