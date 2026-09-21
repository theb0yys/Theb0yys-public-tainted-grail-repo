# Avalon Broodmother Companion S3 Live Validation Gate

Date: 2026-08-06

Status: PASS for live-validation gate definition. BLOCKED for live execution until explicitly authorized.

## Scope

This gate defines the first live test for `Avalon Broodmother Companion` after the S2 scaffold and static validation passed.

This packet does not deploy the plugin, launch FoA, load a save, change config, spawn a Broodmother, copy assets, package a release, or touch custom icons.

## Evidence base

| Gate point | Evidence | S3 effect |
|---|---|---|
| S2 live boundary | `mods/avalon-awakened/docs/research/broodmother-first-s2-implementation-packet-2026-08-06.md:142` | Live validation must be separate, use a throwaway save, and check plugin load, disabled-default no-spawn, summon/swap, hostility, recall, dismiss, duplicate prevention, save exclusion, transition behavior, shutdown cleanup, command logs, and BetterSummon compatibility separately. |
| Static gate result | `mods/avalon-broodmother-companion/docs/test-notes.md:5-12` | Release build and structural checks passed; no runtime test has run. |
| Validation plan | `mods/avalon-broodmother-companion/docs/validation-plan.md:16-18` | Live validation is not authorized in the scaffold slice; later live gate must use a throwaway save. |
| Runtime safety defaults | `mods/avalon-broodmother-companion/docs/research.md:14-19`; `mods/avalon-broodmother-companion/docs/design.md:6-14` | Runtime summon is disabled by default, all hotkeys default to `KeyCode.None`, and behavior is limited to resolver-only explicit commands, not-saved lifecycle, and native ally marker setup. |

## Required explicit authorization for S3 execution

Before executing this gate, the user must explicitly authorize all of these in the current conversation:

- deploy `AvalonBroodmotherCompanion.dll` to the FoA BepInEx plugins folder;
- change only the Broodmother Companion BepInEx config needed for the test;
- launch FoA;
- use a named throwaway save or explicitly confirm the current loaded save is disposable;
- press or request the configured summon, recall, dismiss, and shutdown checks;
- inspect BepInEx logs and plugin command logs after the run.

Without those exact approvals, S3 stays documentation-only.

## Planned deployment inputs

Required build artifact:

```text
mods/avalon-broodmother-companion/src/bin/Release/netstandard2.1/AvalonBroodmotherCompanion.dll
```

Required installed plugin directory:

```text
A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonBroodmotherCompanion\
```

Required config file after first plugin load:

```text
A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\config\kane.tgfoa.avalon-broodmother-companion.cfg
```

Expected log markers:

- `Avalon Broodmother Companion 0.1.1 loaded.`
- `BROODMOTHER_CALL_MERCHANT_STOCK added`
- `summoned Broodmother`
- `recalled Broodmother`
- `dismissed Broodmother`

Expected command-log path:

```text
A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\config\kane.tgfoa.avalon-broodmother-companion\broodmother-command-log.csv
```

## S3 execution sequence

1. Build Release again and record the DLL SHA-256.
2. Copy only `AvalonBroodmotherCompanion.dll` plus optional README/changelog into the installed plugin folder.
3. Launch FoA once with defaults only.
4. Confirm plugin loaded and `Safety.AllowRuntimeSummon=false`.
5. Confirm disabled-default behavior: no Broodmother spawn occurs from plugin load alone.
6. Close FoA.
7. Edit only the Broodmother Companion config:
   - keep `General.Enabled=true`;
   - set `Safety.AllowRuntimeSummon=true`;
   - set one temporary summon/swap key;
   - set one temporary recall key;
   - set one temporary dismiss key;
   - leave `Defend.EnableNativeDefendAssist=false` for the first live test;
   - leave all unrelated settings unchanged.
8. Launch FoA and load only the authorized throwaway save.
9. Trigger `Summon / Swap` once.
10. Observe and log:
    - exactly one Broodmother appears near the hero;
    - no immediate hostility to the hero;
    - plugin log reports exact template/NPC identity;
    - command log records `summon-or-swap,summoned`;
    - actor is not saved according to available readback/log evidence.
11. Trigger `Recall` once.
12. Observe and log:
    - same active Broodmother moves near the hero;
    - no duplicate actor is created;
    - command log records `recall,recalled`.
13. Trigger `Summon / Swap` again.
14. Observe and log:
    - previous active Broodmother is dismissed before replacement;
    - there is no more than one active managed Broodmother.
15. Trigger `Dismiss`.
16. Observe and log:
    - active Broodmother is discarded;
    - command log records `dismiss,dismissed`.
17. Close FoA and inspect BepInEx log plus command log.

## Pass criteria

S3 can pass only if all of these are true:

- plugin loads without errors;
- default config produces no spawn;
- enabled throwaway-save test summons exactly one Broodmother;
- spawned actor uses `Spec_Broodmother_CI4` and NpcTemplate `9a1585dc7e420de4da65a839d062a647`;
- actor is not immediately hostile;
- recall works without creating a duplicate;
- summon/swap leaves no more than one active managed Broodmother;
- dismiss removes the active Broodmother;
- no random spawn, population, route, item, recipe, native spell, or custom summon-template behavior appears in logs;
- command log records the expected commands;
- no save-owned persistence claim is made;
- no crash or exception is observed.

## Fail-closed conditions

Stop the test and do not continue if any of these occur:

- Avalon Awakened resolver is missing or returns a template identity mismatch;
- plugin load errors occur;
- any Broodmother appears before `Safety.AllowRuntimeSummon=true`;
- the first summon creates a hostile actor;
- the actor lacks `NpcElement` or native ally marker setup;
- more than one managed Broodmother remains after swap;
- recall creates a duplicate;
- dismiss fails to remove the active actor;
- BepInEx logs show exceptions from Avalon Broodmother Companion or Avalon Awakened resolver calls;
- save file, story, population, item, recipe, native spell, or template-registration behavior is observed.

## Actions still blocked

- running S3 without exact live-test authorization;
- using a non-disposable save;
- enabling native defend assist in the first live test;
- testing BetterSummon compatibility in the first live test;
- changing Avalon Awakened source, templates, or assets;
- wiring custom icons;
- packaging or publishing the mod;
- claiming save/load, transition, rest, quit/reload, or compatibility support before separate gates.

## Next safe executable step

If the user authorizes S3 execution, run only this live gate. The first executable action is a fresh Release build and hash capture, followed by guarded plugin-folder deployment. FoA launch and save use remain blocked until the user explicitly authorizes them as part of S3.
