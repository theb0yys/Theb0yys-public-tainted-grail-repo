# Research: Wolf/Bear Diagnostic Review

Date: 2026-06-14
Scope: turn the successful Avalon Companions FOA-Diagnostic Tool crosscheck into a two-row review table for wolf and bear companion planning.

## Evidence read

- `mods/avalon-companions/docs/research/pet-creature-shortlist-diagnostics-2026-06-14.md`
- `mods/avalon-wolf-companion/docs/research.md`
- `mods/avalon-bear-companion/docs/research.md`
- Runtime CSV: `<local-path>`
- FOA-Diagnostic Tool source dump: `<local-path>`

## Review table

| Candidate | Template | GUID | FOA-Diagnostic refs | Avalon scene refs | Actor proof | Status |
| --- | --- | --- | ---: | ---: | --- | --- |
| Wolf | `Spec_AnimalWolf` | `9086dee514edc644b9b55890d885db3f` | 26 | 0 | `RepetitiveNpcAttachment`, `npcIsUnique=false` | `OneSessionCandidatePrototype` |
| Bear | `Spec_AnimalBear` | `c45508309b84907429f83d1361918fc2` | 6 | 0 | `RepetitiveNpcAttachment`, `npcIsUnique=false` | `OneSessionCandidatePrototype` |

## Findings

- Both templates are regular, non-abstract `LocationTemplate` rows.
- Both have strict non-unique NPC evidence through `Awaken.TG.Main.Fights.NPCs.RepetitiveNpcAttachment`.
- FOA-Diagnostic Tool observed route-context spawner evidence for both in `CampaignMap_HOS_merged`.
- Avalon Companions' own scene scan still reports `0` refs for both, so the mismatch is confirmed and must be treated as a diagnostic registration/timing issue.
- Neither row is approved for defend, combat-command, faction, persistence, or save behavior.
- Follow-up framework research supports a fresh-spawn wolf/bear prototype as the first non-summon actor step when scoped to one-session testing.

## Gate decision

| Gate | Wolf | Bear |
| --- | --- | --- |
| Template identity | Pass | Pass |
| Regular/non-abstract | Pass | Pass |
| Non-unique NPC proof | Pass | Pass |
| FOA-Diagnostic spawner evidence | Pass | Pass |
| Avalon scene-scan evidence | Fail/mismatch | Fail/mismatch |
| Name-risk block | Pass | Pass |
| Behavior safety | One-session spawn/recall/dismiss only | One-session spawn/recall/dismiss only |
| Save/load safety | Not run | Not run |

## Implementation boundary

- Keep `safeSpawnCandidate=false` for general population injection.
- Keep `rosterApproved=false` for persistent companion behavior.
- Wolf and bear may be exposed as Avalon Companions one-session creature candidates.
- The one-session candidate prototype may call `LocationTemplate.SpawnLocation(...)` for the selected wolf or bear only after explicit player command, must mark the spawned location not saved, and must track it only for the current plugin session.
- Recall/dismiss may use direct session-owned `Location` operations. Follow may use a conservative catch-up recall only.
- Original 2026-06-14 boundary: do not enable defend, faction, ally, summon ownership, combat-command, persistence, or save/load behavior until later research supersedes the specific blocked path.

2026-06-15 update: `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md` supersedes only the faction/ally part of this boundary for the two plugin-owned one-session candidates. Wolf and bear may now receive the native `NpcHeroPetAlly` marker after explicit player-command spawn. Defend, custom combat-command, persistence, save/load behavior, wild conversion, and template expansion remain blocked.

2026-06-15 defend update: `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md` supersedes only the automatic defend-response part of this boundary. Wolf and bear may now call native `NpcHeroPetAlly.EnterCombat()` when the hero has live attackers. Manual defend commands, custom target selection, persistence, save/load behavior, wild conversion, and template expansion remain blocked.

2026-06-15 command-mode update: `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md` also supersedes the manual panel-button block only for explicit Follow/Stay/Defend modes on plugin-owned one-session wolf/bear candidates. Defend mode still uses the same native `NpcHeroPetAlly.EnterCombat()` path and still requires a live hero attacker. Custom target selection, attack commands, defend hotkeys, persistence, save/load behavior, wild conversion, and template expansion remain blocked.

## Next step

Validate the one-session wolf/bear candidate prototype on a throwaway save. Custom defend, combat-command, faction, and persistent behavior remains blocked until actor control, faction safety, command behavior, placement, and save/load validation are researched.
