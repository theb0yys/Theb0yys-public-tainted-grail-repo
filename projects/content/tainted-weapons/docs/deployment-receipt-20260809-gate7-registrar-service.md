# Tainted Weapons Gate 7 Registrar Service Deployment Receipt

## Scope

- Date: 2026-08-09
- Receipt type: `TaintedWeaponsDeploymentReceipt`
- Human promotion owner: `theb0yys`
- Purpose: deploy the first-class, explicitly invoked native item registrar service inside the Tainted Weapons framework.
- Status: source implemented, built, deployed, and hash-verified; runtime registration and Gate 7 promotion are not claimed.
- Gate boundary: Gate 6 remains unpassed because the configured fresh teardown receipt has not passed. Gate 7 T0-T14 remain unrun.

## Evidence Basis

- `docs/research/frameworks/weapon-importer-gated-completion-process-2026-08-08.md` defines Gate 7's shared registrar, source-cloned registration, receipt, collision, idempotency, and T0-T14 boundaries.
- `docs/research/frameworks/weapon-importer-gate6-shared-registrar-native-contract-probe-packet-2026-08-08.md` and `docs/research/frameworks/weapon-importer-gate6-read-only-native-contract-probe-run-2026-08-08.md` identify the inspected native provider and loader contract.
- `docs/research/frameworks/weapon-importer-g0-g2-governance-decisions-2026-08-08.md` limits Evil Greatsword to a separately authorized first-consumer migration.
- The current user instruction explicitly directed work to move on to the integrated Gate 7 framework service while retaining manual control of FoA and forbidding Codex from invoking native registration or mutating saves.

## Repository And Build

- Source commit identity: `8ba57fd49a3325737a1d9b6cc8e5e8c58762a05a` with task-scoped working-tree changes.
- Built project: `mods/tainted-weapons/src/TaintedWeapons.csproj`
- Build command: `dotnet build "mods/tainted-weapons/src/TaintedWeapons.csproj" -c Release -p:FoAGameRoot="<local-path>"`
- Build result: passed, 0 warnings, 0 errors.
- Static public-contract inspection: compiled assembly exposes `TaintedWeaponsApi`, `TaintedWeaponNativeItemRegistrationRequest`, `TaintedWeaponNativeItemRegistrationReceipt`, and `TaintedWeaponNativeItemRegistrarStatus`.
- Static invocation-boundary inspection: the sole Tainted Weapons `TemplatesLoader.AddToMap` invocation is contained in the registrar's guarded execution path; no automatic request construction or consumer migration was found.

## Deployment

- Read-only process guard: no FoA process was found before copy.
- Source DLL: `<local-path>`
- Live DLL: `<local-path>`
- Previous-live backup: `<local-path>`
- Source/live file version: `0.3.17.0`
- Source/live product version: `0.3.17+8ba57fd49a3325737a1d9b6cc8e5e8c58762a05a`
- Source/live length: `235008` bytes.
- Source/live last-write UTC: `2026-08-09T18:33:39.5495122Z`
- Source SHA-256: `21C1C51C4B879FD5BD0D0B62472B85E15DAD9A78DF4936FE268D8D8FC181C818`
- Live SHA-256: `21C1C51C4B879FD5BD0D0B62472B85E15DAD9A78DF4936FE268D8D8FC181C818`
- Previous-live backup SHA-256: `1BD36F49F4248C6479C6ADE0C468736534B66DC5025071EFC6CD5953EA93CD80`
- Hash verdict: pass.

## Explicit Non-Actions

- Codex did not start, stop, or otherwise control FoA.
- Codex did not invoke `RegisterNativeWeaponTemplate` or `TemplatesLoader.AddToMap`.
- Codex did not create or grant an item and did not mutate a save.
- Codex did not migrate Evil Greatsword or modify its local registrar.
- The service does not auto-register at startup or when a presentation definition is accepted.

## Runtime And Promotion Boundary

- Expected inert startup marker: `NativeItemRegistrationAutoInvoke=false` together with the registrar readiness status.
- Gate 6 remains unpassed until the user manually starts and normally closes FoA and the configured teardown receipt passes inspection.
- Gate 7 remains unpassed until a separately authorized consumer migration and native request execute the complete T0-T14 matrix, including collision, idempotency, MVC item creation, copied-save timing, missing-registrar behavior, cleanup, and overclaim checks.
- Only the named human promotion owner, `theb0yys`, may promote the resulting material claim after the required machine-valid review hierarchy passes.
