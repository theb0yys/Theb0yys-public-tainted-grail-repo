# Tainted Weapons 0.3.8 Lifecycle Validation Harness

## Scope

- Purpose: build the W7-W10/W14-W17 receipt harness required before claiming lifecycle stability.
- Research target: `TaintedWeaponsLifecycleAndTeardownReport`.
- Runtime behavior: observe and report framework state; do not equip, unequip, grant items, travel scenes, open UI, mutate saves, or create a visual fallback.

## Evidence Basis

- The weapons report requires lifecycle and teardown evidence to snapshot instrumented prototype objects, bundle owners, Addressables handles, mesh/material records, loading counters, and active consumers before and after lifecycle fixtures.
- The validation matrix defines W7-W10 as unequip loop, two instances, hide/show, and scene transition, and W14-W17 as mesh/material counter symmetry, bundle lifetime, and plugin teardown.
- Tainted Weapons research requires live game start/restart to remain manual.

## Implemented Harness

- Added default-off config section `LifecycleValidation`.
- `LifecycleValidation.RequestId` writes one `lifecycle-validation-*.tsv` receipt when incremented.
- Version `0.3.8` reloads the plugin config when the config file timestamp changes so external `RequestId` edits can be observed by the running plugin.
- `LifecycleValidation.WriteOnSceneChange` writes an observe-only receipt when the active scene changes.
- `LifecycleValidation.WriteOnShutdown` writes one idempotent before/after cleanup receipt before the native title-screen exit kills the process, with plugin destroy retained as a fallback.
- Receipt sections:
  - `[summary]`: framework record counts, runtime prototype counts, registered asset-key count, tracked handle counts, native source handle count, observed Drake manager count, and framework mesh/material counter totals.
  - `[records]`: per-weapon redirect counts, prototype-handle route counts, runtime handle counts, native source handle state, and Drake start/unload event counts.
  - `[drake_loading]`: observed Drake loading-manager rows with key, counter, handle validity, loaded state, and framework-key flag.
  - `[events]`: bounded lifecycle event log for redirects, prototype handles, Drake start/unload observations, and scene changes.
  - `[framework_keys]`: registered runtime mesh/material keys used to classify Drake rows.

## Hook Boundary

- Existing registered-key hooks on `DrakeRendererLoadingManager.StartLoadingMesh/StartLoadingMaterial` still serve only framework-owned keys.
- New observe-only hooks on `DrakeRendererLoadingManager.UnloadMesh/UnloadMaterial` record before/after counter state for registered keys and return native unload behavior unchanged.
- Generic `AssetReference.LoadAssetAsync<Mesh/Material>` and `Addressables.LoadAssetAsync<Mesh/Material>` hooks remain disabled.
- The shutdown repair uses a non-skipping Harmony prefix on the exact parameterless `Awaken.TG.Main.UI.TitleScreen.TitleScreenUI.Exit()` method. Decompiled and runtime-backed Avalon Exceptions evidence establishes that this path calls `WindowsKernelHelpers.KillCurrentProcess()` before `Application.Quit()`, so normal plugin teardown is not a reliable receipt trigger. If the target is absent, Tainted Weapons logs a warning and retains `Plugin.OnDestroy` as the fallback.

## Validation State

- Offline build: `0.3.8` passed with 0 warnings and 0 errors.
- Live deployment: `0.3.8` deployed and hash-verified after process guard found no FoA process.
- Live W7-W10/W14-W17 fixture execution: pending.
- Gate 6 native-exit repair build on 2026-08-09 passed with 0 warnings and 0 errors. Fresh manual native-exit receipt validation remains pending.

## Remaining Requirements

- User manually starts/restarts the game.
- Enable `LifecycleValidation`.
- Run manual fixture actions for repeated equip/unequip, two instances, hide/show, and scene transition.
- Increment `LifecycleValidation.RequestId` after each fixture stage or enable `WriteOnSceneChange`/`WriteOnShutdown` where applicable.
- Close FoA normally from its title-screen exit so the `native-title-screen-exit` teardown receipt can be inspected; Codex must not control the game process.
- Validate final receipt counters before claiming W7-W10 or W14-W17 pass.
