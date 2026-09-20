# FoA Developer Tools

Small source-only PowerShell helpers for Tainted Grail: The Fall of Avalon mod authors.

These scripts automate repetitive setup work around the repository's existing templates. They do **not** ship game DLLs, BepInEx binaries, generated interop assemblies, or proprietary assets.

## Recommended flow

~~~text
Test-FoAEnvironment.ps1
→ Get-FoAFingerprint.ps1
→ New-FoAMod.ps1
→ Test-FoAModProject.ps1
→ Build-FoAMod.ps1
→ Compare-FoAModInstall.ps1
→ Install-FoAMod.ps1
→ Watch-FoALog.ps1

game/loader updated?
→ Compare-FoAFingerprint.ps1
→ research/tools/symbol-anchors/Test-FoASymbolAnchors.ps1
→ research/tools/harmony-runtime-audit
→ research/tools/runtime-tracer (when invocation is the remaining question)

mod-stack problem?
→ Get-FoAModInventory.ps1
→ Get-FoAHarmonyOwnership.ps1
→ research/tools/harmony-runtime-audit

support problem?
→ New-FoADiagnosticBundle.ps1

pre-release?
→ Test-FoAReleaseReady.ps1
→ New-FoARelease.ps1
~~~

## Scripts

### Test-FoAEnvironment.ps1

Detects the installed runtime lane and validates the expected local reference layout.

It distinguishes:

- Mono game markers;
- IL2CPP game markers;
- BepInEx 5 Mono;
- BepInEx 6 Mono;
- BepInEx 6 IL2CPP;
- generated IL2CPP interop readiness.

Example:

~~~powershell
.\platform\developer-tools\Test-FoAEnvironment.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
~~~

If -GameRoot is omitted, the script checks FOA_GAME_ROOT and common Steam locations.

### Get-FoAFingerprint.ps1

Hashes the important local binaries for the detected runtime lane so a mod author can record exact build/reference scope without manually hashing files.

Example:

~~~powershell
.\platform\developer-tools\Get-FoAFingerprint.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
~~~

A hash proves file identity only. It does not prove runtime compatibility.

### New-FoAMod.ps1

Creates a new project from the repository's maintained Harmony-basic template.

Supported lanes:

- Mono → BepInEx 5 Mono starter;
- IL2CPP → BepInEx 6 IL2CPP starter.

Example:

~~~powershell
.\platform\developer-tools\New-FoAMod.ps1 -Runtime Mono -Name "My First Mod" -PluginGuid "yourname.tgfoa.my-first-mod" -Destination ".\MyFirstMod"
~~~

The script updates the project filename, assembly name, root namespace, C# namespace, plug-in GUID, display name, version, and README build command.

The generated Harmony target is self-owned. Confirm its self-test before replacing it with a verified FoA target.

### Test-FoAModProject.ps1

Runs a non-destructive project doctor before build/release.

It checks:

- Mono vs IL2CPP host/reference consistency;
- plug-in GUID/name/version extraction;
- unrenamed starter identities;
- BepInEx host lifecycle shape;
- Harmony usage/reference mismatch;
- Private=true or missing Private=false on game/loader/runtime references;
- user-specific absolute paths;
- accidentally bundled game/loader/runtime binaries;
- selected local environment compatibility when -GameRoot is supplied.

Example:

~~~powershell
.\platform\developer-tools\Test-FoAModProject.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

Use -FailOnError when another script/CI lane should stop on blocking findings.

A project-doctor pass is a static/project-layout claim. It does not prove the plug-in loads.

### New-FoADiagnosticBundle.ps1

Creates a sanitized support folder without copying game assemblies, generated interop assemblies, saves, or proprietary assets.

The bundle can include:

- environment/runtime summary;
- local binary fingerprints;
- installed plug-in DLL names/versions/hashes;
- filtered BepInEx log lines;
- optional project-doctor report;
- bundle file manifest/hashes.

Paths and common secret/token patterns are redacted on a best-effort basis.

Example:

~~~powershell
.\platform\developer-tools\New-FoADiagnosticBundle.ps1 -GameRoot "C:\Games\Tainted Grail FoA" -Project ".\MyFirstMod\MyFirstMod.csproj"
~~~

Always review the generated text/JSON before sharing it publicly.

### New-FoARelease.ps1

Stages a reproducible source-traceable mod package.

It:

- requires the project doctor to pass;
- builds by default;
- copies only the built mod DLL, common documentation, and explicitly supplied additional files;
- rejects known game/BepInEx/Unity/interop binaries;
- writes release-manifest.json;
- writes SHA256SUMS.txt;
- records the source Git commit when available;
- can optionally create a local ZIP with -Zip.

Example:

~~~powershell
.\platform\developer-tools\New-FoARelease.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA" -Zip
~~~

Packaging does not perform runtime or feature validation. A package is not proof that the mod works in game.

### Build-FoAMod.ps1

Validates the local environment against the project lane, then runs dotnet build with the selected GameRoot.

~~~powershell
.\platform\developer-tools\Build-FoAMod.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

### Install-FoAMod.ps1

Builds by default, backs up an existing installed DLL, copies the new DLL into a dedicated BepInEx plug-in folder, and verifies the copied SHA-256.

~~~powershell
.\platform\developer-tools\Install-FoAMod.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

Use -SkipBuild only when you intentionally want to install an already-built artifact.

This helper verifies file copy identity. It does **not** launch the game or claim the plug-in loaded.

### Get-FoAModInventory.ps1

Inventories DLLs under BepInEx/plugins without loading them into PowerShell.

It records assembly/file identity, version, size and SHA-256, then reports potential:

- exact duplicate assemblies;
- duplicate assembly names with different hashes;
- multiple assembly versions;
- duplicate file names with different hashes.

Example:

~~~powershell
.\platform\developer-tools\Get-FoAModInventory.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
~~~

Use -OutputPath to save JSON and -FailOnConflict when a validation lane should stop on duplicate findings.

This is a file/assembly identity inventory. It does not extract BepInPlugin GUIDs and does not prove two mods are behaviourally incompatible.

### Compare-FoAModInstall.ps1

Compares the current build artifact with the installed DLL by SHA-256.

States include:

~~~text
MATCH
DIFFERENT
NOT_INSTALLED
BUILD_NOT_FOUND
~~~

Example:

~~~powershell
.\platform\developer-tools\Compare-FoAModInstall.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

This is useful when source changes appear to have no effect because the game is still loading an older DLL.

The script can also restore the latest backup created by Install-FoAMod.ps1:

~~~powershell
.\platform\developer-tools\Compare-FoAModInstall.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA" -RestoreLatestBackup
~~~

Rollback is explicit and exact. Before restoring, the currently installed DLL is preserved as a pre-restore backup.

### Compare-FoAFingerprint.ps1

Compares the current game/loader fingerprint with a previously saved baseline.

Create a baseline:

~~~powershell
.\platform\developer-tools\Get-FoAFingerprint.ps1 -GameRoot "C:\Games\Tainted Grail FoA" -OutputPath ".\foa-fingerprint.json"
~~~

Compare after an update:

~~~powershell
.\platform\developer-tools\Compare-FoAFingerprint.ps1 -Baseline ".\foa-fingerprint.json" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

A change means **revalidation required**. It does not automatically mean the mod is broken.

### Get-FoAHarmonyOwnership.ps1

Uses the canonical symbol-anchor extractor to inventory supported literal Harmony target declarations across source projects and report cross-owner target overlaps.

It recognizes common patterns such as:

- HarmonyPatch(typeof(Type), nameof(Type.Method));
- HarmonyPatch(typeof(Type), "Method");
- HarmonyPatch("Namespace.Type", "Method");
- AccessTools.Method/PropertyGetter/PropertySetter with literal targets.

Example:

~~~powershell
.\platform\developer-tools\Get-FoAHarmonyOwnership.ps1 -Root ".\mods"
~~~

This is deliberately a **source-level ownership report**. Dynamic targets and unsupported source shapes remain explicit in the unresolved inventory. Use the research Harmony runtime audit when live in-process ownership is needed.

### Test-FoAReleaseReady.ps1

Runs the broad pre-release/static readiness sequence and reports explicit evidence states.

It can cover:

- environment;
- project doctor;
- optional compatibility fingerprint;
- build;
- source-level Harmony ownership;
- installed mod duplicate/conflict inventory;
- built-vs-installed comparison;
- optional release package staging;
- runtime/feature/persistence evidence state.

Example:

~~~powershell
.\platform\developer-tools\Test-FoAReleaseReady.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA" -BaselineFingerprint ".\foa-fingerprint.json" -StagePackage
~~~

The command reports each check separately rather than collapsing static, build, install, runtime, feature and persistence evidence into one implied result.

The command also accepts `-SymbolAnchorManifest`. When supplied, it verifies the selected runtime lane against local Mono managed assemblies or IL2CPP interop assemblies using the canonical symbol verifier. Use `-IlSpyCmd` to select a specific ILSpy CLI executable.

The umbrella command never upgrades runtime or feature validation just because static/build/package checks pass.

### Watch-FoALog.ps1

Tails BepInEx\LogOutput.log, optionally filtering lines.

~~~powershell
.\platform\developer-tools\Watch-FoALog.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
.\platform\developer-tools\Watch-FoALog.ps1 -GameRoot "C:\Games\Tainted Grail FoA" -Pattern "My First Mod|Harmony"
~~~

## Safety model

The tools deliberately separate:

~~~text
environment/reference validation
≠ build
≠ install
≠ plug-in load
≠ hook activation
≠ gameplay behaviour
≠ persistence
~~~

File-writing tools have explicit ownership boundaries:

- New-FoAMod.ps1 writes only to the requested scaffold destination.
- Install-FoAMod.ps1 writes to the selected local game's dedicated BepInEx plug-in folder and backs up an existing DLL.
- New-FoADiagnosticBundle.ps1 writes only to its requested diagnostic output directory.
- New-FoARelease.ps1 writes only to its requested/local release staging directory and replaces an existing staged package only with -Force.
- Compare-FoAModInstall.ps1 is read-only by default; -RestoreLatestBackup explicitly mutates only the exact installed mod DLL and its owned _backup folder.
- Test-FoAReleaseReady.ps1 writes build output when build runs and writes release staging only when -StagePackage is requested.

Test-FoAEnvironment.ps1, Get-FoAFingerprint.ps1 (unless -OutputPath is used), Test-FoAModProject.ps1, Get-FoAModInventory.ps1 (unless -OutputPath is used), Compare-FoAFingerprint.ps1 (unless -OutputPath is used), Get-FoAHarmonyOwnership.ps1 (unless -OutputPath is used), Compare-FoAModInstall.ps1 without rollback, and Watch-FoALog.ps1 are read-only with respect to the game/project.

## Source lineage

The Mono conventions are derived from the repository's public Mono templates and the maintained private Mono starter pattern.

The IL2CPP conventions are derived from public IL2CPP templates plus inspected working BepInEx 6 / generated-interop project layouts.

The scripts are convenience tooling, not evidence that any generated mod is runtime-safe.
