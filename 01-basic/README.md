# Learn the Everyday Modding Loop

Once your first test works, this section teaches the repeatable loop you will use while actually making mods.

## Runtime plug-in track

Work through:

1. [Edit, rebuild, and redeploy](01_EDIT_REBUILD_REDEPLOY.md)
2. [Add your first config option](02_FIRST_CONFIG_OPTION.md)
3. **IL2CPP:** [Make your first runtime game change](03A_FIRST_IL2CPP_GAME_CHANGE.md)
4. **Mono:** [Run the Harmony self-test](03_HARMONY_SELF_TEST.md)
5. **Both lanes:** [Move to a real game patch](04_FIRST_REAL_PATCH_RULES.md)
6. [Debug the basic loop](06_BASIC_DEBUGGING_FLOW.md)
7. [Finish your first complete mod](07_FIRST_COMPLETE_MOD.md)

## New-content track

Work through:

1. [Add your first new item](../00-never-made-a-mod-start-here/05_FIRST_CONTENT_AUTHORING.md) on the currently proven Mono/BepInEx 5 item lane.
2. [Move beyond the first custom item](05_CONTENT_PROGRESSION.md) without assuming the item APIs automatically apply to weapons, armour, creatures, spells or recipes.
3. Use the [technical handbook](../docs/REFERENCE_MAP.md) for identity, templates, hooks, ownership, assets, persistence and failure reasoning.
4. Use [the basic debugging flow](06_BASIC_DEBUGGING_FLOW.md) to separate registration, runtime ownership, presentation and persistence failures.

## Your progress

**Start → Loader working → First plug-in → First game change → First complete mod**

This section carries you from a working first plug-in to the final **First complete mod** checkpoint.

## What you should be able to do

By the end, you should be able to make a small change, rebuild or re-author it, deploy and test it, diagnose a failure, restore a known-good state, and finish one small mod without random file copying.

## Real FoA examples

After your first smoke test and the everyday modding loop make sense, browse [examples/mod-cookbook](../examples/mod-cookbook/README.md) as an example library rather than another required sequence. It contains small real-game examples for runtime behavior changes. Use the current handbook and cookbook paths linked from the repository.
