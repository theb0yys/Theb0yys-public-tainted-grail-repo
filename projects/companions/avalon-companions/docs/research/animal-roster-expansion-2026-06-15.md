# Research: Animal Roster Expansion

Date: 2026-06-15
Scope: add a small set of additional animal or animal-like one-session companion candidates before deeper behavior logic.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/docs/research/pet-creature-shortlist-diagnostics-2026-06-14.md`
- `mods/avalon-companions/docs/research/wolf-bear-diagnostic-review-2026-06-14.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `Research/foa-actor-interaction-feasibility-2026-06-15.md`
- `Research/foa-universal-actor-companion-framework-2026-06-15.md`
- `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Report-deep-research-report.md`
- Live Avalon Companions diagnostic CSVs under `BepInEx/config/kane.tgfoa.avalon-companions/`, generated 2026-06-15 03:38:44.

## Candidate review

| Candidate | Template | GUID | FOA-Diagnostic refs | Avalon scene refs | Actor proof | Role | Status |
| --- | --- | --- | ---: | ---: | --- | --- | --- |
| Deer | `Spec_AnimalDeer` | `467a9c9208854394cbb77032251288a1` | 14 | 0 | `RepetitiveNpcAttachment`, `npcIsUnique=false` | passive animal pet | `OneSessionCandidatePrototype` |
| Pig | `Spec_AnimalPig` | `3e24c74d7d86f6743b05d4c3587de57d` | 3 | 0 | `RepetitiveNpcAttachment`, `npcIsUnique=false` | passive animal pet | `OneSessionCandidatePrototype` |
| Cow | `Spec_AnimalCow` | `6217a5ecb1ff2914b854b9304d9bd54b` | 3 | 0 | `RepetitiveNpcAttachment`, `npcIsUnique=false` | passive animal pet | `OneSessionCandidatePrototype` |
| Bullrat | `Spec_EnemyMonster_T4_Bullrat` | `dc69c95f2c2930841aab6b7cfe48b4d5` | 1 | 0 | `RepetitiveNpcAttachment`, `npcIsUnique=false` | offensive creature pet | `OneSessionCandidatePrototype` |

## Decision

Add these four rows to the active Avalon Companions roster as one-session candidates only. They use the same guarded runtime path as wolf and bear:

1. Spawn only after explicit player command.
2. Mark the spawned `Location` not saved.
3. Require the spawned location to expose `NpcElement`.
4. Apply the native summon faction override and `NpcHeroPetAlly`.
5. Track only the current live `Location` in memory.
6. Attach only the runtime-only `AvalonCompanionCommandAction`.
7. Support recall, dismiss, catch-up, and the existing Follow/Stay/Defend mode state.

For deer, pig, and cow, Defend mode is allowed to use the same native hero-attacker entry path, but combat capability is not guaranteed. They are primarily passive animal companions until in-game behavior proves otherwise.

For bullrat, Defend mode uses only `NpcHeroPetAlly.EnterCombat()` when the hero already has live attackers. No custom target selection or forced attack logic is approved.

## Explicit exclusions

Do not add these rows yet:

- humanoid rows that matched `Rat` only because names such as `DesperateArcher`, `DesperatePeasant`, or `RadleighTheRat` contain the term,
- arena, trial, or stage variants such as drowned arena rows, golems, and Merlin trial monsters,
- swarm rows such as bees,
- special Wyrd rows such as `Spec_AnimalWyrdDeer_Summer_Special`,
- generic monster families such as corpse eater, redcap, flamegobbler, grindylow, wyrdspirit, or sharg.

Those require separate manual review because they may have scene, quest, encounter, animation, size, VFX, faction, or special-template risks not covered by this pass.

## Boundary

This does not approve:

- persistence or save/load support,
- custom combat target selection,
- custom attack commands,
- defend hotkeys,
- wild creature conversion,
- humanoid companions,
- quest/story actor edits,
- vanilla serialized interaction-list edits,
- multi-companion active squads.

## Validation needed

- Throwaway-save smoke test each added row: summon, confirm no immediate hostility, recall, dismiss, and inspect BepInEx log.
- For deer, pig, and cow, confirm whether native ally follow works and whether Defend mode does anything useful.
- For bullrat, confirm it enters combat only when the hero already has attackers.
- Confirm the native `Companion` prompt appears only on the active managed actor and disappears after dismiss.
- Confirm save/load is not used as a persistence claim.
