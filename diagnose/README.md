<!-- Canonical documentation path. Migrated from docs/DEBUGGING.md. -->
# Debugging

> **Reference page.** Use this when you have a specific failure to diagnose. If your current tutorial has not failed, keep following the learning path instead of reading this front-to-back.

## Plug-in does not appear in logs

Check, in order:

1. correct runtime lane;
2. BepInEx itself starts;
3. DLL is under `BepInEx/plugins`;
4. target framework/API matches the installed BepInEx lane;
5. all referenced assemblies can resolve;
6. plug-in GUID is unique;
7. no Windows file blocking/quarantine issue.

## BepInEx starts but the plug-in fails

Look for:

- `FileNotFoundException` / `FileLoadException`;
- missing assembly/version messages;
- Harmony target-not-found errors;
- IL2CPP generated interop/type resolution errors;
- duplicate plug-in GUIDs.

Fix the earliest reliable error first.

## Harmony patch is not running

Verify:

- the patch class is discovered;
- the target type/method still exists;
- overload parameters match exactly;
- the patch is actually installed;
- another mod is not replacing the same behavior.

The `examples/mono-harmony-self-test` project is useful for proving the BepInEx + Harmony path without touching game code.

## Game update broke the mod

Re-establish:

- game build/version;
- runtime lane;
- BepInEx version;
- target method/type identity;
- generated interop state for IL2CPP.

Do not assume an older compatibility receipt still applies.

## What not to upload when asking for help

Do not upload raw saves, whole game folders, game DLL dumps, asset bundles, credentials, or unredacted logs containing private paths. Share the smallest redacted evidence that reproduces the problem.

## Symptom-first diagnostic routes

- [Weapon works but is invisible](presentation/weapon-works-but-is-invisible.md)
- [UI opens but the action does not fire](ui/opens-but-action-does-not-fire.md)
- [Expected behaviour is missing from the object you inspected](ownership/expected-behaviour-missing.md)
- [Works now but not after load](persistence/works-now-not-after-load.md)

Each route starts from what has definitely succeeded and finds the earliest unproven transition before recommending code changes.
