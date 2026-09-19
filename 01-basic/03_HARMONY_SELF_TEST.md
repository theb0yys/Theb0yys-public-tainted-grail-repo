# 03 - Harmony Self-Test

Before patching the game, prove you understand the patch mechanism on code you own.

The repository includes:

~~~text
examples\mono-harmony-self-test
~~~

That example patches a method inside its own assembly.

A successful run logs:

~~~text
Harmony self-test result: patched
~~~

## Why self-test first

It separates two questions:

1. Does Harmony patching work in my loader/project?
2. Did I correctly identify the game's target method?

If you start directly on a game method, those failures are mixed together.

## Mono self-test build

From the repository root:

~~~powershell
$GameRoot = "C:\Path\To\Tainted Grail FoA"

dotnet build `
  .\examples\mono-harmony-self-test\HarmonySelfTest.csproj `
  -c Release `
  -p:GameRoot="$GameRoot"
~~~

This example is **Mono/BepInEx 5 only**.

Do not deploy it into an IL2CPP setup merely to see what happens.

## What you should understand before moving on

Find these pieces in the example:

- HarmonyPatch identifies the target;
- Postfix runs after the target method;
- ref string __result changes the returned string;
- PatchAll() installs the patch;
- UnpatchSelf() removes this Harmony owner's patches.

Once that makes sense, read the next guide before targeting game code.
