# Tainted Weapons Gate 6 Shutdown Repair Deployment Receipt

## Scope

- Date: 2026-08-09
- Receipt type: `TaintedWeaponsDeploymentReceipt`
- Purpose: deploy the framework-owned native title-screen shutdown receipt repair before another manual Gate 6 run.
- Status: built, deployed, and hash-verified; fresh runtime teardown receipt pending.
- Gate boundary: Gate 7 remains unstarted.

## Evidence Basis

- `mods/avalon-exceptions/docs/test-notes.md` records decompilation of the parameterless `Awaken.TG.Main.UI.TitleScreen.TitleScreenUI.Exit()` path and its call to `WindowsKernelHelpers.KillCurrentProcess()` before `Application.Quit()`.
- `mods/avalon-exceptions/docs/design.md` records the proven narrow Harmony-prefix repair used to capture shutdown state before that process kill.
- `mods/tainted-weapons/src/Plugin.cs` previously emitted the configured teardown receipt only from `Plugin.OnDestroy`, which the native exit path can bypass.
- The user authorized the exact live destination and retained manual ownership of FoA start and normal close.

## Repository And Build

- Source commit identity: `68344a7d82e0a0bf7ac575c64e8dbdeb65d92300` with task-scoped working-tree changes.
- Built project: `mods/tainted-weapons/src/TaintedWeapons.csproj`
- Build command: `dotnet build "mods/tainted-weapons/src/TaintedWeapons.csproj" -c Release -p:FoAGameRoot="<local-path>" -p:UseSharedCompilation=false -p:NuGetAudit=false -nr:false`
- Build result: passed, 0 warnings, 0 errors.

## Deployment

- Read-only process guard: no FoA process was found before copy.
- Source DLL: `<local-path>`
- Live DLL: `<local-path>`
- Previous-live backup: `<local-path>`
- Source/live file version: `0.3.16.0`
- Source/live product version: `0.3.16+68344a7d82e0a0bf7ac575c64e8dbdeb65d92300`
- Source SHA-256: `1BD36F49F4248C6479C6ADE0C468736534B66DC5025071EFC6CD5953EA93CD80`
- Live SHA-256: `1BD36F49F4248C6479C6ADE0C468736534B66DC5025071EFC6CD5953EA93CD80`
- Previous-live backup SHA-256: `B5B6D1F9E0B48E36F01E884A9DFE7A5A333AC0B08EFD82080AC5351D465FFC72`
- Containment check: live DLL resolved directly inside the approved `TaintedWeapons` plugin directory.
- Hash verdict: pass.
- Command note: the deployment command emitted a non-terminating PowerShell `Split-Path` parameter-set warning before continuing. A corrected read-only containment check then passed, and source/live/backup hashes were independently re-read.

## Expected Fresh Runtime Evidence

- Startup marker: `Tainted Weapons patched native game quit lifecycle handler. target=Awaken.TG.Main.UI.TitleScreen.TitleScreenUI.Exit, originalPreserved=true, pluginDestroyFallback=true`.
- Normal user-controlled title-screen close writes one `lifecycle-validation-*.tsv` receipt with reason `native-title-screen-exit`.
- The receipt contains before and after-dispose snapshots and reports `disposeAfterSnapshot=True`.
- Gate 6 remains blocked until the fresh receipt is inspected and its teardown rows pass; Gate 7 remains unstarted.

## Runtime Restrictions

- The user manually starts and normally closes FoA.
- Codex may inspect resulting logs and TSVs.
- Codex must not control the game process, mutate saves, or invoke native registration.
