# Finish Your First Complete Mod

## What you're doing

You are turning a successful experiment into a small mod you can rebuild, test, remove, and explain.

A **complete first mod** does not need to be large. It needs one clear purpose and a repeatable working loop.

## Your progress

**Start → Loader working → First plug-in → First game change → _First complete mod_**

You are at the final beginner milestone.

## What you need

For a runtime code mod:

- a plug-in that loads;
- one small behaviour you have actually changed and observed;
- a repeatable build/deploy loop;
- the game/runtime version you tested.

For authored content:

- one mod-owned definition or asset path you created;
- a clean editor workflow;
- the exact editor/game checks you actually performed;
- a way to remove or revert your test content.

## What you'll learn

You will learn what separates a working experiment from a small finished mod:

- stable identity;
- one clear feature;
- repeatable build or authoring steps;
- useful logging/config where appropriate;
- a known success condition;
- a known rollback/uninstall path;
- an honest statement of what was tested.

## Steps

### 1. Give the mod one sentence of purpose

You should be able to describe it without listing implementation details.

Examples:

- "Caps the running game to a chosen frame-rate target."
- "Changes one verified gameplay behaviour."
- "Adds one new mod-owned item."

If the description needs several unrelated "and" clauses, split the work.

### 2. Make the working loop repeatable

For a runtime plug-in, prove you can repeat:

~~~text
edit -> build -> deploy -> launch -> observe
~~~

For authored content, prove you can repeat:

~~~text
edit -> save/build -> load/test -> observe
~~~

Do not rely on remembering which random file you copied last time.

### 3. Keep a stable identity

For a plug-in:

- keep a unique, stable plug-in GUID;
- keep a clear name;
- give the build a version.

For authored content:

- use mod-owned names/identifiers;
- avoid depending on copied proprietary assets merely to make the example work.

### 4. Add only the configuration the feature needs

If the feature benefits from an on/off switch or one small value, use configuration.

Do not create a settings system just to make the mod look more complete.

### 5. Define one success test

Write down what proves the feature worked.

Examples:

- a specific BepInEx log line plus an observable runtime change;
- a verified Harmony target producing the intended behaviour;
- an authored item resolving correctly in the editor and, when tested, in the game.

"Build succeeded" alone is not a complete feature test.

### 6. Define rollback or uninstall

For a plug-in, know which mod-owned DLL/folder to remove.

For authored content, know which mod-owned files or definitions to remove/revert.

A first mod should not leave you guessing how to get back to the known-good state.

### 7. Record what you actually tested

At minimum, record:

- game version/build;
- Mono or IL2CPP;
- BepInEx/toolkit version when relevant;
- mod version;
- what exact action you tested;
- what you observed;
- anything you did **not** test.

## What success looks like

Your first mod is complete when you can answer **yes** to these:

- Does it have one clear purpose?
- Can I rebuild or re-author it without guessing?
- Can I deploy/test it repeatedly?
- Did I observe the intended result?
- Can I identify the exact build/runtime/toolkit context I tested?
- Can I remove or roll it back cleanly?
- Can another person read the project and understand where to start?

That is enough for a first complete mod.

## Common problems

**Adding a second feature before finishing the first:** complete one small loop first.

**Treating a successful build as feature proof:** run the feature and observe the intended result.

**Changing identity every build:** keep the plug-in GUID or content identity stable.

**No rollback plan:** define uninstall/revert before adding more complexity.

**Trying to polish everything:** correctness and repeatability come before a large settings UI, framework, or release system.

## Where to go next

You now have the complete beginner progression:

**Start → Loader working → First plug-in → First game change → First complete mod**

From here:

- [Understand How Mods Work](../foundations/README.md) to understand the engineering underneath the working mod.
- [Tainted Grail Mod Cookbook](../../case-studies/README.md) for more kinds of changes.
- [Build Robust Game Changes](../advanced/README.md) when your mod needs stronger compatibility, diagnostics, packaging, or conflict handling.
