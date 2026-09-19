# 04 - Moving to a Real Game Patch

This guide intentionally does **not** invent a game method for you to patch.

A real target must come from current evidence for the game build you are testing.

## Before writing the patch

Write down:

- exact game build/version;
- runtime lane;
- exact type;
- exact method;
- overload/parameters if relevant;
- what behaviour you observed;
- what minimal change you want.

## Beginner target rule

Choose a method where a **postfix** can make the change if possible.

Postfixes usually preserve more original behaviour than replacing the method.

## Avoid this first

Do not begin with:

- a giant update loop;
- save serialization;
- inventory persistence;
- player death;
- scene loading;
- a transpiler;
- suppressing an entire original method.

Choose a small reversible behaviour.

## Generic patch shape

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

The placeholder names above are not FoA facts. Replace them only with a target you have actually verified for your game build.

## Fail visibly

If your patch depends on a version-specific assumption, log when that assumption is not satisfied instead of silently continuing.

## After a game update

Re-check the exact target before deciding the mod is broken somewhere else.
