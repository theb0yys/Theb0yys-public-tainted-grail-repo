---
document_type: mechanic
scope: pickable/regrowable runtime harvest quantity
runtime: mono
evidence:
  static: SOURCE_AND_TARGET_RESEARCH
  runtime: CONFIG_GATED_PRIVATE_LANE
  persistence: INDIRECT_NATIVE_ITEM_EFFECT
last_verified: 2026-09-20
---

# Harvest Yield Scaling

The researched live lane adjusts runtime item-row quantity immediately before vanilla creates/adds the harvested item.

Native routes include:

- `PickItemAction.OnStart`
- `Pickable.StartInteraction`
- `Regrowable.StartInteraction`

Fishing is a different lane and is not covered.

## Safety boundary

Preserve protected rows such as:

- quest/important items;
- hidden/not-droppable items;
- relic/artifact/unique/legendary identities;
- specifically protected utility items.

Do not mutate item templates, recipes, generic containers or merchant stock through this mechanic.

The quantity change becomes part of the item state vanilla creates; that is different from a purely presentational modifier.
