# Change Post-Roll Container Contents

Modify the generated runtime rows after FoA has rolled the container. Do not rewrite the source loot table just to apply a post-roll rule.

Working lineage: [Container Rules: Post-Roll and Save-Backed](../../../research/case-studies/economy/container-row-boundary.md).


## Runnable source

Start with the [Container post-roll rules example](../../../examples/mono/gameplay/container-post-roll-rules/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Runnable source

Start from the buildable public source: [Economy runtime rules example](../../../examples/mono/gameplay/economy-runtime-rules/README.md). Build it unchanged first, confirm the documented behavior, then change one mechanism at a time.

## Runtime owner

The working economy implementation operates on:

~~~text
Awaken.TG.Main.Locations.Actions.SearchAction
→ SearchAction.OnInitialize
→ private _itemsInsideContainer
→ ItemSpawningDataRuntime rows
~~~

The list is read from the current `SearchAction` instance after the container contents exist.

## What a row gives you

Each `ItemSpawningDataRuntime` can expose information such as:

- `ItemTemplate`;
- quantity;
- item level;
- weight level;
- New Game Plus level.

Use the resolved `ItemTemplate` and its flags/tags to classify the row. Do not infer everything from display text when a native flag exists.

## Bounded post-roll rules

Keep individual operations simple:

### Quantity scaling

~~~text
matching generated row
→ quantity × multiplier
→ clamp/remove zero row as intended
~~~

### Keep/drop rule

Use a deterministic per-row roll if you want stable results for the same row/context rather than calling a fresh random generator repeatedly during UI refreshes.

### Container-specific policy

Build a normalized container identity from the current `SearchAction`, its parent/location, and attached native context.

The working code also distinguishes useful context such as:

- a `LockAction` being present;
- lock state;
- an attached `NpcElement`;
- corpse/enemy-corpse context.

## Do not mix owners

This route is **after** content generation. It is not:

- loot-table authoring;
- template registration;
- item transfer;
- container UI replacement.

Let `ContainerUI` and native transfer continue with the modified runtime rows.

## Save-backed warning

`_itemsInsideContainer` can participate in saved container state.

That means the correct mental model is:

~~~text
generated runtime row mutation
→ may become the container's durable rolled state
~~~

So never reapply an irreversible rule on every reopen without checking whether the row has already been transformed.
