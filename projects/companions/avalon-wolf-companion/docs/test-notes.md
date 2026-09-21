# Test Notes

## 0.1.0

- Build validation: passed on 2026-06-14 with `dotnet build .\mods\avalon-wolf-companion\src\AvalonWolfCompanion.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`; 0 warnings, 0 errors.
- Review-gate logging: implemented load logging for `AVALON-PET-WOLF-001`, `BlockedUntilManualProof`, FOA-Diagnostic refs `26`, Avalon scene refs `0`, `safeSpawnCandidate=false`, and `rosterApproved=false`. Build/deploy validation after this change passed on 2026-06-14 with 0 warnings and 0 errors.
- Load validation: passed on 2026-06-14 after full game restart. BepInEx log showed `Review=AVALON-PET-WOLF-001`, `Status=BlockedUntilManualProof`, `FoaDiagnosticSpawnerRefs=26`, `AvalonSceneSpawnerRefs=0`, `safeSpawnCandidate=false`, `rosterApproved=false`, and `behavior=blocked`.
- Feature validation: not applicable; behavior is intentionally blocked.
- Save/load validation: not run because no save-affecting behavior exists.
