# Move to a Real Game Patch

## What you're doing

You are moving from a self-owned test target to a real game method while keeping the first change small, reversible, and based on a target you verified for the game build you are actually testing.

This guide intentionally does **not** invent a Tainted Grail method for you to patch.

## What you need

Before writing the patch, identify:

- exact game build/version;
- whether the target installation is Mono or IL2CPP;
- exact type;
- exact method;
- overload/parameters if relevant;
- what behaviour you observed;
- what minimal change you want.

A real target must be something you have actually verified for the game build you are testing.

## What you'll learn

You will learn how to:

- choose a small first target;
- prefer a postfix when it can preserve original behaviour;
- avoid high-risk first patches;
- keep placeholder example code separate from game facts;
- make version-specific assumptions fail visibly.

## Steps

### 1. Choose a narrow target

Choose a method where a **postfix** can make the change if possible.

A **postfix** is code Harmony runs after the original game method finishes. It is often a good first patch because the game's normal logic still gets to run before your small adjustment.

Postfixes usually preserve more original behaviour than replacing the method.

### 2. Avoid high-risk first targets

Do not begin with:

- a giant update loop;
- save serialization;
- inventory persistence;
- player death;
- scene loading;
- a **transpiler** — a lower-level Harmony patch that rewrites the target method's instructions;
- suppressing an entire original method.

Choose a small reversible behaviour.

### 3. If you are on IL2CPP, add only the local game references you need

Your first IL2CPP runtime-change example used Unity APIs and did not need a Tainted Grail game assembly.

A game-specific Harmony patch is the next layer.

For a verified FoA target, your project will normally need:

- the BepInEx 6 IL2CPP references you already use;
- the local `0Harmony.dll`;
- the generated interop assembly containing the target, from `<GameRoot>\BepInEx\interop\`;
- any additional generated type assembly required by that target's signature.

For example, if the type you verified is in generated `TG.Main.dll`, reference your installation's:

~~~text
<GameRoot>\BepInEx\interop\TG.Main.dll
~~~

Do not copy that DLL into this repository.

Read [Runtime Guide — IL2CPP references after the smoke test](../runtime-modding/runtime-guide.md#il2cpp-references-after-the-smoke-test) for the layer model.

### 4. Start from the generic patch shape

~~~csharp
[HarmonyPatch(typeof(SomeType), nameof(SomeType.SomeMethod))]
internal static class SomePatch
{
    private static void Postfix()
    {
        // Minimal behaviour here.
    }
}
~~~

The placeholder names above are not FoA facts.

Replace them only with a target you have actually verified for your game build.

### 5. Fail visibly when an assumption is wrong

If your patch depends on a version-specific assumption, log when that assumption is not satisfied instead of silently continuing.

### 6. Re-check the target after a game update

Do not assume an old type/method identity remains valid.

Re-check the exact target before deciding the mod is broken somewhere else.

## What success looks like

**Progress: Start → Loader working → First plug-in → _First game change_ → First complete mod**

For a game-specific patch, you have reached **First game change** when the verified target runs on the build you tested and the observed behaviour matches the one small change you intended.

You have a real target that is explicitly tied to the game build you inspected, and your patch is narrow enough to reason about.

## Common problems

**A placeholder type or method was treated as a real FoA symbol:** placeholders in this guide are only patch-shape examples.

**The patch replaces too much original behaviour:** prefer a narrower postfix or another smaller target where practical.

**A game update breaks the patch:** re-check the exact type, method, and overload on your current game build before changing unrelated code.

**A version-specific assumption fails silently:** log the failure and disable the affected feature instead of guessing.

## Where to go next

When that one feature works reliably, use **[Finish Your First Complete Mod](first-complete-mod.md)** to close the beginner loop.

Use the **[Tainted Grail Mod Cookbook](../../../research/case-studies/README.md)** for game-target teaching examples that show how far each path has actually been checked.

For the underlying engineering model, continue to **[Understand How Mods Work](../foundations/README.md)**.
