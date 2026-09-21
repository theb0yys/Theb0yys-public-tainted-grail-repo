# Added Research Review - 2026-06-15

## Scope

Review newly added or newly relevant research after the first safe proof spawn troubleshooting pass.

This review covers the human companion lane only. Animal/creature roster expansion and Wyrd Decoy inventory-cost research are noted as separate lanes.

## Research read

- `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Reportdeep-research-report.md`
- `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Report-deep-research-report.md`
- `Research/foa-actor-interaction-feasibility-2026-06-15.md`
- `Research/foa-universal-actor-companion-framework-2026-06-15.md`
- `Research/foa-readable-markers-crime-sleep-assets-2026-06-15.md`
- `mods/avalon-companions/docs/research/offensive-creature-roster-expansion-2026-06-15.md`
- `mods/wyrd-decoy/docs/research/item-template-dump-review-2026-06-15.md`

## Duplicate report finding

The untracked file `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Reportdeep-research-report.md` has the same SHA-256 hash as the existing tracked report with the hyphenated filename.

Hash: `BF0926750A82EEF5D341B66E5542BECC9AA44BF69F02FFD71CB0439D7C4B74D7`

Decision: treat it as a duplicate filename issue, not new evidence. Do not reference both files in future docs.

## Human companion impact

The added research reinforces the current conservative path.

- No research approves Skyrim-style recruitable human followers.
- No research approves named, story, quest, civilian, boss, or unique NPC recruitment.
- The strongest human-adjacent path remains a summon-like or pseudo-follower framework after actor identity, faction, interaction, transition, and save/load proofs.
- The safe proof spawn target remains only a throwaway-save spawn proof, not a companion proof.
- The next safe implementation step after the proof spawn smoke test is still a disabled actor scanner that writes plugin-owned evidence only. This step is implemented in version 0.1.1.

## Runtime interaction impact

`foa-actor-interaction-feasibility-2026-06-15.md` adds a useful later UI direction:

- Use a plugin-owned runtime prompt or command panel.
- Gate the prompt through a strict plugin roster registry.
- Do not write custom entries into vanilla serialized interaction lists for the first milestone.
- Keep command UI state runtime-only unless a separate save/load proof approves persistence.

Decision: this is not part of the current proof spawn step. It becomes relevant only after actor scanning, CSV review, and ownership/faction proof.

## Spawn and population impact

`foa-readable-markers-crime-sleep-assets-2026-06-15.md` reinforces these spawn rules:

- Prefer verified spawner systems or authored location-backed content for future patrols and bounty-style systems.
- Do not use unique NPC templates for generic population.
- Do not rely on vanilla spawner cooldowns for sleep-driven consequences.
- Direct low-level spawning requires duplicate prevention and persistence proof before it becomes a gameplay feature.

Decision: the current safe proof spawn remains acceptable only because it is explicit, default-off, one-session, non-unique, marked not saved, and used on a throwaway save.

## Separate lanes

`mods/avalon-companions/docs/research/offensive-creature-roster-expansion-2026-06-15.md` belongs to Avalon Companions. It approves a wider one-session creature roster boundary for explicit player-command spawns, but explicitly excludes humanoid companion rows and persistence.

`mods/wyrd-decoy/docs/research/item-template-dump-review-2026-06-15.md` belongs to Wyrd Decoy. It approves Wyrdstone as the first carried-item cost target and does not affect human companion behavior.

## Current next step

1. Run the disabled actor scanner near the proof-spawned outlaw on a throwaway save.
2. Confirm the scanner writes `human-actor-candidates.csv`, `human-actor-components.csv`, and `human-actor-command-dry-run.csv`.
3. Review the CSVs for actor identity, uniqueness, ownership/faction/targeting hints, and interaction/panel hints.
4. Only after that review, decide whether a plugin-owned runtime command panel proof is safe to implement. Human behavior remains blocked.
