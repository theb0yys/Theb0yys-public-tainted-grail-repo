# Tainted Weapons Mono `24270691` Source Cleanup, Binary Reconciliation, and Startup-Trace Handoff

## Document control

- Status: Implementation complete; Windows execution not run
- Date: 2026-08-10
- Runtime lane: Windows Steam Mono Build `24270691`
- Repository cleanup commit: `954283179b1e59c34aa9e457fe30305896b11f18`
- Harness baseline at creation: `ea2be387858094fe36e87310d06e95609a118d31`
- Save or game-state mutation performed: None
- Native template mutation performed: None
- Copied-save testing authorised: **No — blocked until both gates below pass**

## Current decision

The previous source-conflict blocker has been cleared for the Tainted Weapons source tree.

The current source lane is:

```text
source cleanup                              COMPLETE in GitHub
exact Windows binary reconciliation         IMPLEMENTED, NOT RUN
non-mutating Windows startup trace           IMPLEMENTED, NOT RUN
copied-save registration-order experiment    BLOCKED
```

The next valid sequence is strictly:

```text
clean source
    → exact source/build/live DLL reconciliation
    → same-install assembly closure
    → non-mutating startup trace
    → only then copied-save registration-order research
```

## Source cleanup result

Commit `954283179b1e59c34aa9e457fe30305896b11f18` restored conflict-bearing repository files to clean pre-contamination blobs.

The Tainted Weapons source files at that cleanup baseline are:

| Path | Git blob SHA |
|---|---|
| `mods/tainted-weapons/src/Plugin.cs` | `5fae0e0098996e3e9abb326e3070ba67a959cd83` |
| `mods/tainted-weapons/src/TaintedWeaponFramework.cs` | `6d872ae6f38d22701f327b66d567f14bd3d3fa74` |
| `mods/tainted-weapons/src/TaintedWeaponNativeItemRegistrar.cs` | `8050cf59a6b6641202ad0f3a73ab6c231761cab9` |
| `mods/tainted-weapons/src/TaintedWeaponProviderLifecycleHarness.cs` | `3aecd12ce22dc96abf60508541a8d1f30e8ba357` |
| `mods/tainted-weapons/src/TaintedWeaponProviderOwnerLedger.cs` | `a6b81a7cb6e0e2aa23d39bed2280078202a646e7` |
| `mods/tainted-weapons/src/TaintedWeapons.csproj` | `a7828f20a928ddd05199518c57523c773190cbeb` |

`Plugin.cs` and `TaintedWeapons.csproj` both declare version `0.3.20` / `0.3.20.0`.

The current GitHub source contains no merge markers in `mods/tainted-weapons/src`. The Windows reconciliation script repeats that check against the exact local checkout and fails before build if any marker or working-tree change is present.

The cleaned historical Gate 7 receipt records a previous successful `0.3.20` source/live deployment with SHA-256:

```text
D60E15081EA7E3A5F537D62D40E1103538FCD7063D175E5F2021C9B372BB1816
```

That historical hash is **not** reused as the current result. The new binary-reconciliation gate must rebuild from the current clean commit and freshly fingerprint both built and installed DLLs.

## Gate A — exact binary reconciliation

### Tool

```text
mods/tainted-weapons/tools/Invoke-TaintedWeaponsMonoReconciliation.ps1
```

### Purpose

The script is Windows-only and performs these operations in one evidence lane:

1. requires a clean Git checkout;
2. scans `mods/tainted-weapons/src` for merge markers;
3. reads the same-install Steam appmanifest;
4. verifies branch `mono` and BuildID `24270691`;
5. captures `ScriptingAssemblies.json` and its SHA-256;
6. inventories every listed managed assembly;
7. records size, SHA-256, assembly version, and MVID;
8. records managed DLLs not listed by the manifest;
9. verifies the expected `TG.Main.dll` contract:
   - SHA-256 `749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982`;
   - MVID `68528841-991C-481E-BD94-7F1776FC3579`;
10. records every Tainted Weapons source-file Git blob and SHA-256;
11. builds `TaintedWeapons.csproj` in Release against that exact game root;
12. optionally backs up and deploys the built DLL;
13. records built/live size, SHA-256, MVID, and assembly version;
14. fails unless built and installed DLL identities match when live equality is required.

### First execution command

From a clean Windows checkout at the intended source commit:

```powershell
pwsh -NoProfile -File .\mods\tainted-weapons\tools\Invoke-TaintedWeaponsMonoReconciliation.ps1 `
  -FoAGameRoot '<local-path>`
  -ExpectedBuildId '24270691' `
  -ExpectedBranch 'mono' `
  -Deploy `
  -RequireLiveMatch
```

`-Deploy` is intentional for the first reconciliation run. The script refuses deployment while a process under the FoA root is running, backs up an existing live DLL, deploys once, then verifies the built/live identity.

### Outputs

Default evidence root:

```text
documents/runtime-evidence/weapon-framework/mono-24270691/binary-reconciliation/<timestamp>/
```

Expected files:

```text
binary-reconciliation.json
scripting-assemblies.csv
extra-managed-assemblies.csv
core-artifacts.csv
source-files.csv
build.log
```

### Gate A pass criteria

```text
status = PASS
Steam branch = mono
Steam BuildID = 24270691
repository clean = true
weapon source marker scan = pass
TG.Main SHA/MVID = expected values
ScriptingAssemblies listed files = all present
Release build exit code = 0
built DLL SHA = installed DLL SHA
built DLL MVID = installed DLL MVID
built DLL version = installed DLL version
```

Any mismatch blocks the startup trace and copied-save research.

## Gate B — non-mutating startup trace

### Tools

```text
mods/tainted-weapons/tools/Invoke-TaintedWeaponsStartupTrace.ps1
mods/tainted-weapons/tools/TaintedWeapons.StartupTrace/
```

The diagnostic plugin is a separate soft-dependent BepInEx assembly. It does not call the native registrar, grant items, write saves, alter inventory, register Babel terms, or mutate Drake entities.

### Observed methods

The plugin dynamically observes available methods on:

```text
ApplicationScene
    InitAll
    InitLocalization
    InitializeServices

BabelManager
    Initialize
    SwitchLanguage
    Dispose

TemplatesLoader
    CreateAndLoad
    LoadAssetsInBuild
    set_FinishedLoading

TitleScreenUI
    Awake
    Start
    OnMount
```

Each trace row records:

```text
UTC timestamp
frame
realtime since startup
managed thread ID
method identity and phase
selected locale
template-provider presence
TemplatesProvider.AllLoaded
GUID-map count
type-map count
TaintedWeapons loaded version/SHA/MVID
custom registered-template count
mutation-violation flag
```

### Execution command

Run only after Gate A has passed and the live DLL already matches the current clean source build:

```powershell
pwsh -NoProfile -File .\mods\tainted-weapons\tools\Invoke-TaintedWeaponsStartupTrace.ps1 `
  -FoAGameRoot '<local-path>`
  -ExpectedBuildId '24270691' `
  -ExpectedBranch 'mono' `
  -LaunchGame
```

The harness launches FoA when `-LaunchGame` is supplied, but does not terminate it. Reach the title screen and close FoA normally. The trace plugin is removed after capture unless `-KeepTracePlugin` is explicitly supplied.

### Outputs

Default evidence root:

```text
documents/runtime-evidence/weapon-framework/mono-24270691/startup-trace/<timestamp>/
```

Expected files:

```text
binary-reconciliation/binary-reconciliation.json
binary-reconciliation/*.csv
startup-trace-build.log
startup-trace.jsonl
bepinex-log-delta.log
startup-trace-receipt.json
```

### Gate B pass criteria

The receipt must report `PASS` and prove:

```text
Tainted Weapons 0.3.20 loaded from the reconciled DLL
ApplicationScene.InitAll observed
ApplicationScene.InitLocalization observed
BabelManager.Initialize observed
TemplatesLoader.CreateAndLoad observed
TemplatesLoader.set_FinishedLoading observed
TemplatesProvider transitions to AllLoaded = true
custom registered-template count never exceeds 0
no template_registered receipt occurs
no item grant occurs
no inventory write occurs
no save write occurs
no mutation violation occurs
FoA exits normally
trace plugin is removed/restored cleanly
```

Missing optional methods are recorded, but the required startup methods above must be present for the gate to pass.

## Failure rules

### Source or binary failure

```text
STOP
Do not launch FoA for startup tracing
Do not register a template
Do not grant an item
Do not open a copied-save gate
```

### Startup-trace mutation detection

If any custom template registration or mutation indicator is observed:

```text
status = FAIL
preserve evidence
remove diagnostic plugin
restart before any later experiment
investigate the unexpected caller
```

### Post-`AddToMap` registrar failure

The current native registrar has no proven rollback path. Any future failure after `TemplatesLoader.AddToMap` must be treated as a poisoned process:

```text
no retry
no save continuation
no second registration
restart required
```

The copied-save test must not begin until registrar fail-stop handling has been reviewed and accepted.

## Copied-save gate dependency

The copied-save registration-order research remains blocked until both receipts are retained and reviewed:

```text
Gate A: binary-reconciliation.json      PASS
Gate B: startup-trace-receipt.json      PASS
```

Only after those pass may the next trace exercise:

```text
FinishedLoading
    → custom template registration
    → provider lookup success
    → campaign SaveReader start
    → ReadTemplate(customGuid)
    → Item restore
    → ItemEquip restore
    → one equipped View / Drake presentation
```

No real user campaign may be used. The next gate must use a disposable copied campaign and preserve the original save unchanged.

## Current implementation status

| Item | Status |
|---|---|
| Tainted Weapons source conflict cleanup | **Complete in GitHub** |
| Clean current source blob set | **Recorded** |
| Historical conflicted deployment receipt | **Replaced by clean receipt** |
| Exact Windows binary reconciliation tool | **Implemented** |
| Full `ScriptingAssemblies.json` closure capture | **Implemented, not run** |
| Built/live DLL SHA/MVID equality check | **Implemented, not run** |
| Separate non-mutating startup trace plugin | **Implemented** |
| Startup trace orchestration and pass/fail receipt | **Implemented, not run** |
| Runtime acceptance | **Not claimed** |
| Copied-save registration-order research | **Blocked pending Gate A and Gate B PASS** |
