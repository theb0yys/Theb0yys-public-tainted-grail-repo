# Avalon Bear Companion Research

## Evidence

- `mods/avalon-companions/docs/research.md` keeps non-Qrko pet/creature rows diagnostics-only until shortlist and behavior validation pass.
- `mods/avalon-companions/docs/research/pet-creature-shortlist-diagnostics-2026-06-14.md` defines the new CSV gate.
- `mods/avalon-companions/docs/research/wolf-bear-diagnostic-review-2026-06-14.md` records `Spec_AnimalBear` as a FOA-Diagnostic Tool review candidate with 6 route-context spawner refs, while Avalon's own scene scan still reports 0 refs.
- `mods/living-avalon/docs/research/hos-route-spawner-shortlist-2026-06-14.md` lists `Spec_AnimalBear` / `c45508309b84907429f83d1361918fc2` with 6 vanilla spawner references.

## Boundary

- Do not spawn `Spec_AnimalBear`.
- Do not add it to Avalon Companions roster.
- Do not add `NpcAlly`, `NpcHeroSummon`, `NpcHeroPetAlly`, faction overrides, save data, quest flags, or persistence logic.
- Do not implement defend behavior yet.

## Next Validation

- Keep the scaffold diagnostic-only and reference the bear review row on load if needed.
- Research actor control, faction behavior, command behavior, placement, and save/load before any behavior work.
