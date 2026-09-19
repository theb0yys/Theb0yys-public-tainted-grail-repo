# Move to a Real Game Patch

## What you're doing

You are moving from a self-owned test target to a real game method while keeping the first change small, reversible, and based on a target you verified for the game build you are actually testing.

This guide intentionally does **not** invent a Tainted Grail method for you to patch.

## What you need

Before writing the patch, identify:

- exact game build/version;
- runtime lane;
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

Postfixes usually preserve more original behaviour than replacing the method.

### 2. Avoid high-risk first targets

Do not begin with:

- a giant update loop;
- save serialization;
- inventory persistence;
- player death;
- scene loading;
- a transpiler;
- suppressing an entire original method.

Choose a small reversible behaviour.

### 3. Start from the generic patch shape

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

### 4. Fail visibly when an assumption is wrong

If your patch depends on a version-specific assumption, log when that assumption is not satisfied instead of silently continuing.

### 5. Re-check the target after a game update

Do not assume an old type/method identity remains valid.

Re-check the exact target before deciding the mod is broken somewhere else.

## What success looks like

You have a real target that is explicitly tied to the game build you inspected, your patch is narrow enough to reason about, and the observed behaviour matches the one small change you intended.

## Common problems

**A placeholder type or method was treated as a real FoA symbol:** placeholders in this guide are only patch-shape examples.

**The patch replaces too much original behaviour:** prefer a narrower postfix or another smaller target where practical.

**A game update breaks the patch:** re-check the exact type, method, and overload on your current game build before changing unrelated code.

**A version-specific assumption fails silently:** log the failure and disable the affected feature instead of guessing.

## Where to go next

Use the **[Tainted Grail Mod Cookbook](../examples/mod-cookbook/README.md)** for game-target teaching examples that show how far each path has actually been checked.

For the underlying engineering model, continue to **[Understand How Mods Work](../02-foundational/README.md)**.
