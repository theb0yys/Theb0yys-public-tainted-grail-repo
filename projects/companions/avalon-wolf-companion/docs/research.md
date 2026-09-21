# Avalon Wolf Companion Research

## Evidence

- `mods/avalon-companions/docs/research.md` keeps non-Qrko pet/creature rows diagnostics-only until shortlist and behavior validation pass.
- `mods/avalon-companions/docs/research/pet-creature-shortlist-diagnostics-2026-06-14.md` defines the new CSV gate.
- `mods/avalon-companions/docs/research/wolf-bear-diagnostic-review-2026-06-14.md` records `Spec_AnimalWolf` as a FOA-Diagnostic Tool review candidate with 26 route-context spawner refs, while Avalon's own scene scan still reports 0 refs.
- `mods/living-avalon/docs/research/hos-route-spawner-shortlist-2026-06-14.md` lists `Spec_AnimalWolf` / `9086dee514edc644b9b55890d885db3f` with 26 vanilla spawner references.

## Boundary

- Do not spawn `Spec_AnimalWolf`.
- Do not add it to Avalon Companions roster.
- Do not add `NpcAlly`, `NpcHeroSummon`, `NpcHeroPetAlly`, faction overrides, save data, quest flags, or persistence logic.
- Do not implement defend behavior yet.

## Next Validation

- Keep the scaffold diagnostic-only and reference the wolf review row on load if needed.
- Research actor control, faction behavior, command behavior, placement, and save/load before any behavior work.
