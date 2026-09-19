# Run the Harmony Self-Test

## What you're doing

You are proving **Harmony**—the patching library used to attach your code to existing methods—on code the example owns before trying to patch a real Tainted Grail method.

The repository includes:

~~~text
examples\mono-harmony-self-test
~~~

This specific example is for the supported **Mono/BepInEx 5** lane.

## What you need

- a working Mono/BepInEx 5 Tainted Grail setup;
- the repository;
- your real `GameRoot`;
- the ability to build and deploy a Mono plug-in.

## What you'll learn

You will learn how to recognize:

- a `HarmonyPatch` target — the method Harmony should attach to;
- a **postfix** — code Harmony runs after the original target method;
- a returned value modified through `ref __result`;
- `PatchAll()`;
- `UnpatchSelf()`;
- the difference between "Harmony works" and "I chose the correct game method."

## Steps

### 1. Understand why the self-test comes first

It separates two questions:

1. Does Harmony patching work in my loader/project?
2. Did I correctly identify the game's target method?

If you start directly on a game method, those failures are mixed together.

### 2. Build the self-test

From the repository root:

~~~powershell
$GameRoot = "C:\Path\To\Tainted Grail FoA"

dotnet build `
  .\examples\mono-harmony-self-test\HarmonySelfTest.csproj `
  -c Release `
  -p:GameRoot="$GameRoot"
~~~

### 3. Deploy it using your proven Mono plug-in workflow

Deploy the built plug-in DLL into your Mono BepInEx plug-in directory, then launch the game.

Do not deploy this Mono example into an IL2CPP setup.

### 4. Check the result

A successful run logs:

~~~text
Harmony self-test result: patched
~~~

### 5. Read the example until the mechanism is clear

Find these pieces in the example:

- `HarmonyPatch` identifies the method being patched;
- `Postfix` runs after that original method finishes;
- `ref string __result` changes the returned string;
- `PatchAll()` installs the patch;
- `UnpatchSelf()` removes this Harmony owner's patches.

## What success looks like

The self-test builds, loads through the Mono/BepInEx 5 lane, and logs:

~~~text
Harmony self-test result: patched
~~~

You can also explain what each patching piece above is doing.

## Common problems

**You are running IL2CPP:** this exact self-test is not the IL2CPP example. Do not use a Mono example to diagnose an IL2CPP setup.

**The plug-in loads but the result stays unpatched:** inspect patch installation and the first relevant Harmony error.

**You jump straight to a game target:** first establish whether the patch mechanism itself works on the self-owned target.

## Where to go next

Continue to **[Move to a Real Game Patch](04_FIRST_REAL_PATCH_RULES.md)**.
