# Validation Plan

## Required level

- Level: build for the scaffold; in-game load/config validation before any release claim.
- Reason: this mod currently only loads, binds config, and logs blocked diagnostic state.

## Build Validation

- Command: `dotnet build .\mods\avalon-wolf-companion\src\AvalonWolfCompanion.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed on 2026-06-14 with `-p:DeployOnBuild=true`; 0 warnings, 0 errors. Re-run after review-gate logging passed with 0 warnings and 0 errors.

## Load Validation

- Expected: BepInEx logs `Avalon Wolf Companion 0.1.0 loaded`.
- Expected: when `Diagnostics.LogTargetOnLoad=true`, BepInEx logs `Review=AVALON-PET-WOLF-001`, `Status=BlockedUntilManualProof`, `FoaDiagnosticSpawnerRefs=26`, `AvalonSceneSpawnerRefs=0`, `safeSpawnCandidate=false`, and `rosterApproved=false`.
- Actual: passed on 2026-06-14. BepInEx log showed `Avalon Wolf Companion 0.1.0 loaded` and the review-gate line with `Review=AVALON-PET-WOLF-001`, `Status=BlockedUntilManualProof`, `FoaDiagnosticSpawnerRefs=26`, `AvalonSceneSpawnerRefs=0`, `safeSpawnCandidate=false`, `rosterApproved=false`, and `behavior=blocked`.

## Not Run

- Disabled summon-test hotkey validation.
- Any spawn/follow/defend/save-load behavior validation.
