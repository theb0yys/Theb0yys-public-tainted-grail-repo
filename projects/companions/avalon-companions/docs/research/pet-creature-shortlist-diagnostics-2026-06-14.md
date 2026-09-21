# Research: Pet/Creature Shortlist Diagnostics

Date: 2026-06-14
Scope: add diagnostics for additional pet or creature `LocationTemplate` candidates without adding roster behavior.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/docs/research/runtime-diagnostics-2026-06-14.md`
- `Research/Tainted Grail The Fall of Avalon Template and Safe Population Injection Research-deep-research-report.md`
- Existing `mods/template-diagnostics/src/Plugin.cs` spawner-reference dump implementation.
- Existing `mods/living-avalon/src/DryRun/PopulationDirector.cs` scene-object spawner scan pattern.

## Decision

Add a disabled-by-default diagnostic dump that joins loaded `LocationTemplate` rows to loaded scene spawner evidence from:

- `LocationSpawnerAttachment`
- `GroupSpawnerAttachment`
- `AutoGuardSpawningAttachment`

The dump classifies rows with template GUID/name/type, regular/non-abstract status, scene spawner reference count, matched pet/creature terms, `NpcAttachment.IsUnique` evidence when available, name-risk blocking, and a diagnostics-only shortlist flag.

After first in-game validation, the dump wrote 272 name-matched rows but 0 spawner-backed shortlist rows. Existing Template Diagnostics route dumps had already found wolf and bear spawner references, so the Avalon scanner was changed to match Template Diagnostics by scanning all loaded `Resources.FindObjectsOfTypeAll<TAttachment>()` spawner attachments instead of filtering attachments by `gameObject.scene`.

After a later in-game validation, Avalon Companions still did not join `Spec_AnimalWolf` or `Spec_AnimalBear` into `pet-creature-location-shortlist.csv`: both rows remained `sceneSpawnerRefCount=0`. The latest FOA-Diagnostic Tool dump at `<local-path>` did register both targets in the current HoS route context:

- `Spec_AnimalWolf`: 26 rows total; 18 `LocationSpawnerAttachment/locationsToSpawn` rows and 8 `GroupSpawnerAttachment/locationsWithPositions` rows in `CampaignMap_HOS_merged`.
- `Spec_AnimalBear`: 6 `LocationSpawnerAttachment/locationsToSpawn` rows in `CampaignMap_HOS_merged`.

Decision: keep Avalon Companions' own shortlist gate unchanged, but add a diagnostic-only FOA-Diagnostic Tool crosscheck. The crosscheck writes comparison CSVs so rows seen by FOA-Diagnostic Tool but missed by Avalon's once-per-session dump are explicit registration/timing mismatches, not behavior approvals.

The first crosscheck run proved that choosing the newest FOA-Diagnostic Tool folder is not enough: FOA-Diagnostic Tool also writes low-context startup dumps with only 20 spawner refs, and the newest startup dump hid the route-context evidence. Avalon now prefers the newest readable dump with at least `Diagnostics.DiagnosticToolMinimumSpawnerRefs` rows, default `100`, and only falls back to a smaller newest dump if no qualifying dump exists.

After the thresholded selector was deployed and the game was rerun, Avalon selected FOA-Diagnostic Tool dump `20260614-221744`, which had 556 spawner refs. `pet-creature-diagnostic-tool-review.csv` contained 38 rows, including:

- `Spec_AnimalWolf`: `diagnosticToolSpawnerRefCount=26`, `evidenceStatus=seen-by-foa-diagnostic-tool-only`, `diagnosticToolReviewCandidate=true`.
- `Spec_AnimalBear`: `diagnosticToolSpawnerRefCount=6`, `evidenceStatus=seen-by-foa-diagnostic-tool-only`, `diagnosticToolReviewCandidate=true`.

Avalon's own scene-spawner scan still reported `avalonSceneSpawnerRefCount=0` for both. Treat this as a confirmed Avalon registration/timing mismatch. It is not approval for roster, summon, combat, or save behavior.

## Boundary

- Do not add any dumped row to the active companion roster automatically.
- Keep `safeSpawnCandidate=false` and `rosterApproved=false` in CSV output.
- Treat `shortlistCandidate=true` only as "worth manual review next."
- Do not call `LocationTemplate.SpawnLocation(...)` from this diagnostic path.
- Do not add `NpcAlly`, `NpcHeroSummon`, `NpcHeroPetAlly`, faction overrides, save data, quest flags, or persistence logic.

2026-06-15 update: `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md` supersedes only the `NpcHeroPetAlly` and per-instance summon-faction part of this boundary for the two plugin-owned one-session wolf/bear candidates. The shortlist remains diagnostics-only and still must not approve additional roster entries, persistence, quest flags, or template expansion.

2026-06-15 animal expansion update: `mods/avalon-companions/docs/research/animal-roster-expansion-2026-06-15.md` supersedes only the roster block for four manually reviewed rows: `Spec_AnimalDeer`, `Spec_AnimalPig`, `Spec_AnimalCow`, and `Spec_EnemyMonster_T4_Bullrat`. The rest of the shortlist remains diagnostics-only and still must not approve roster entries, persistence, quest flags, or template expansion.

## Output

When `Diagnostics.WritePetCreatureShortlistDump=true`, the plugin writes under `BepInEx/config/kane.tgfoa.avalon-companions/`:

- `pet-creature-location-candidates.csv`
- `pet-creature-location-shortlist.csv`
- `pet-creature-diagnostic-tool-crosscheck.csv`
- `pet-creature-diagnostic-tool-review.csv`

The FOA-Diagnostic Tool files are only written when a latest `kane.tgfoa.template-diagnostics/<timestamp>/spawner_refs.csv` dump exists. They keep `safeSpawnCandidate=false` and `rosterApproved=false`.

## Next validation

Run on a throwaway save, inspect the CSV rows, and only then choose any candidate for manual review. A candidate still needs visual behavior, follow/stay, transition, rest, quit, reload, and return validation before it can become roster behavior.
