# Learn the Everyday Modding Loop

Use this path after your first mod loads successfully. It teaches the repeatable day-to-day loop: identify the exact game behaviour, make one controlled change, verify it in game, and only then expand the feature.

Once your first test works, this section teaches the repeatable loop you will use while actually making mods.

## Runtime plug-in track

Work through:

1. [Edit, rebuild, and redeploy](edit-rebuild-redeploy.md)
2. [Add your first config option](first-config-option.md)
3. **IL2CPP:** [Make your first runtime game change](first-il2cpp-game-change.md)
4. **Mono:** [Run the Harmony self-test](harmony-self-test.md)
5. **Both lanes:** [Move to a real game patch](first-real-patch-rules.md)
6. [Debug the basic loop](basic-debugging-flow.md)
7. [Finish your first complete mod](first-complete-mod.md)

## New-content track

Work through:

1. [Add your first new item](../../getting-started/first-content-authoring.md) on the currently proven Mono/BepInEx 5 item lane.
2. [Move beyond the first custom item](content-progression.md) without assuming the item APIs automatically apply to weapons, armour, creatures, spells or recipes.
3. Use the [technical handbook](../../../knowledge/reference/README.md) for identity, templates, hooks, ownership, assets, persistence and failure reasoning.
4. Use [the basic debugging flow](basic-debugging-flow.md) to separate registration, runtime ownership, presentation and persistence failures.

## Your progress

**Start → Loader working → First plug-in → First game change → First complete mod**

This section carries you from a working first plug-in to the final **First complete mod** checkpoint.

## What you should be able to do

By the end, you should be able to make a small change, rebuild or re-author it, deploy and test it, diagnose a failure, restore a known-good state, and finish one small mod without random file copying.

## Real FoA examples

After your first smoke test and the everyday modding loop make sense, browse [case-studies](../../../research/case-studies/README.md) as an example library rather than another required sequence. It contains small real-game examples for runtime behavior changes. Use the current handbook and cookbook paths linked from the repository.
