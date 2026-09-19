# Recipe — Loot and Corpse Loot

**Category:** items / NPC content  
**Evidence:** STATIC_CONFIRMED authoring surface  
**Public recipe:** NOT_RUN

The inspected Merlin/NPC authoring contract exposes inventory, loot and corpse-loot surfaces on NPC gameplay data.

That is the correct beginner route for authored creature loot. It is safer than patching a global item-drop method before you understand ownership.

## Suggested workflow

1. Start from a mod-owned/duplicated NPC template.
2. Keep the existing creature behavior chain working first.
3. Configure one inventory or loot-table change.
4. Spawn the creature in a disposable test.
5. Verify:
   - inventory before death when relevant;
   - death completes normally;
   - corpse transition completes;
   - loot appears once;
   - scene/save reload does not duplicate it.
6. Add rarity/quantity variation only after the deterministic case works.

## Do not infer

A field existing in an NPC template does not prove:

- every creature uses the same corpse route;
- loot tables are evaluated exactly once;
- a runtime mutation will persist safely;
- copied vanilla loot assets are redistributable.

For generic item stat authoring, start with [content/01-item-stats](../../content/01-item-stats/README.md).
