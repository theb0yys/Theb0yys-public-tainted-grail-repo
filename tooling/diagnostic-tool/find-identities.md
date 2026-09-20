# Find Real Identities Before Coding

## Item

Use `item_templates.csv`.

Record:

- template GUID;
- internal template name;
- readable name;
- classification/tag evidence.

## Recipe

Use `recipe_visibility.csv`.

Record recipe GUID/name, station identity, outcome and ingredient evidence.

Do not infer “hero knows recipe” from station membership.

## Creature / actor

Use:

- `creature_templates.csv`;
- `spawner_refs.csv`;
- `actor_authority_refs.csv`.

A loaded spawner reference does not prove a template is safe for arbitrary runtime spawn.

## World / route

Use:

- `scene_entity_context.csv`;
- `scene_object_context.csv`;
- `patrol_path_context.csv`.

Capture evidence near the actual location rather than searching by display name alone.

## Next step

Promote the exact identity into the appropriate [reference](../../reference/README.md) or use it as evidence for an [investigation](../../investigate/README.md).
