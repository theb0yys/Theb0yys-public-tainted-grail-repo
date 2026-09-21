# Research: Actor Control Proof Design

Date: 2026-06-14
Scope: define the smallest safe actor-control proof for follow, stay, and defend research without approving wolf or bear spawning.
Game version and branch: local runtime not rechecked in this pass; repo baseline targets FoA Mono branch with BepInEx v5 Mono.
Tools used: repo research review and source inspection only. No game launch, decompilation, or runtime actor dump was run for this note.

## Evidence read

- `Research/foa-universal-actor-companion-framework-2026-06-15.md`
- `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Report-deep-research-report.md`
- `Research/Making Tainted Grail The Fall of Avalon Mods-deep-research-report.md`
- `docs/engineering-process.md`
- `docs/research/README.md`
- `docs/mod-lifecycle.md`
- `docs/code-review-standard.md`
- `docs/in-game-ui-quality-standard.md`
- `docs/validation-matrix.md`
- `docs/compatibility-and-versioning.md`
- `docs/hotkey-registry.md`
- `codex/skills/tainted-grail-foa-modding/references/quality-gates.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/research/wolf-bear-diagnostic-review-2026-06-14.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PetSystemDiagnostics.cs`

## Research decision

The next development step is not wolf or bear behavior. The next step is an actor-control proof that discovers and classifies existing live actor systems before moving anything.

The universal companion framework research recommends a staged path:

- actor discovery,
- summon or pet based baseline,
- control harness,
- faction matrix,
- placement and transitions,
- save prototype.

The original wolf and bear review blocked behavior because both candidates still had `safeSpawnCandidate=false` and `rosterApproved=false`. Follow-up framework research supports a fresh-spawn wolf/bear prototype as the first non-summon actor step when scoped to one-session testing. The current allowed boundary is explicit command spawn, mark not saved, session-only tracking, recall/catch-up/dismiss only. Defend, faction, combat-command, persistence, and save/load behavior remain blocked until separate validation passes.

2026-06-15 update: `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md` supersedes only the faction/ally part of this boundary for plugin-owned, explicit-command, one-session wolf/bear candidates. The approved path is native `NpcHeroPetAlly` ownership only. Defend, custom combat-command, persistence, save/load behavior, wild conversion, and humanoid companions remain blocked.

2026-06-15 defend update: `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md` supersedes only the automatic attack-response part of this boundary for plugin-owned one-session wolf/bear candidates that already have `NpcHeroPetAlly`. The approved action is calling native `NpcHeroPetAlly.EnterCombat()` when the hero already has live attackers. Manual defend commands, custom target selection, persistence, save/load behavior, wild conversion, and humanoid companions remain blocked.

2026-06-15 command-mode update: `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md` also supersedes the manual panel-button block only for an explicit `Defend` mode on plugin-owned one-session wolf/bear candidates. That mode uses the same guarded native entry path and still requires `Hero.Current.PossibleAttackers` to contain a live attacker. `ActorControlProof.AllowDefendAction` remains diagnostic-only for arbitrary or unreviewed actors.

## Implementation boundary

This proof may:

- inspect loaded templates and live actor-like world objects,
- dump actor identity, component, location, template, pet, summon, ally, nav, and faction hints,
- log which follow, stay, recall, and defend command path would run,
- use a dry-run command harness first,
- run one-session wolf/bear candidate spawn, recall, catch-up, and dismiss after the review row is selected and documented.

This proof must not:

- add wolf or bear as persistent roster entries,
- spawn wolf or bear automatically or without explicit player command,
- convert a wild wolf or bear,
- modify global templates,
- modify global faction, relation, hostility, crime, quest, or save state,
- persist any new actor,
- recruit named, unique, boss, story, quest, challenge, tutorial, scene-critical, or protected NPCs,
- claim defend behavior works until faction and target-filter evidence exists.

## Smallest safe proof

### Phase 1: actor-control reconnaissance

Add a diagnostic-only actor control scanner under Avalon Companions. It should be disabled by default and write only to the plugin-owned BepInEx config folder.

Output files:

- `actor-control-candidates.csv`
- `actor-control-components.csv`
- `actor-control-command-dry-run.csv`

Minimum fields:

- runtime object name,
- scene name,
- location template name and GUID when present,
- location runtime identity when available,
- `MarkedNotSaved`,
- component type list,
- pet markers such as `PetElement` and `PetVariantBase`,
- summon or ally markers such as `NpcHeroSummon` and `NpcHeroPetAlly`,
- NPC attachment type and `IsUnique` when present,
- faction, owner, target, hostility, relation, or team type names when found,
- movement, controller, brain, AI, and nav component type names when found,
- safe-control status: `Blocked`, `DryRunOnly`, or `Candidate`.

The scanner must classify, not approve. Rows are evidence for manual review.

### Phase 2: dry-run command harness

Add a dry-run harness that can select one already-existing safe actor candidate and log what each command would do.

Commands:

- follow,
- stay,
- recall,
- defend.

Dry-run output must include:

- selected actor identity,
- selected actor safety status,
- command requested,
- native method or component candidate that would be used,
- reason the command is blocked or allowed,
- whether the command would touch faction or targeting.

Default behavior must be `DryRunOnly=true`.

### Phase 3: one-session control proof

Only after Phase 1 and Phase 2 produce a reviewed candidate, enable one-session control on the safest actor family.

Preferred target order:

1. Existing Qrko or pet actor with `PetElement` or `PetVariantBase`.
2. Existing summon or pet ally with `NpcHeroSummon` or `NpcHeroPetAlly`.
3. Fresh not-saved Qrko roster actor already covered by the current validated Qrko GUID gate.

Rejected targets for this proof:

- wolf,
- bear,
- wild creature conversion,
- humanoid NPC,
- story or named NPC,
- any actor whose identity, faction, or persistence cannot be classified.

Follow and stay may use known pet methods first:

- `PetElement.SetFollowing(bool)`
- `PetVariantBase.SetFollowing(bool)`
- `PetElement.Recall(Vector3)`

If the target is not a pet actor, the proof must first document the native follow/controller method. Direct Unity NavMesh movement is a fallback only after native control methods are missing and the risk is recorded.

### Phase 4: defend proof remains diagnostic

Defend must start as logging only.

It may report:

- nearby hostile candidate count,
- current player target or combat target candidates if a native path is found,
- whether the selected actor exposes target-sharing, hostility, faction, relation, ally, or team components.

It must not force attack behavior until a faction matrix proves the actor can fight enemies without targeting civilians, guards, quest actors, or the player.

## Proposed config

Use config keys only after code implementation is approved.

| Section | Key | Default | Purpose |
| --- | --- | --- | --- |
| `ActorControlProof` | `Enabled` | `false` | Master gate for actor-control diagnostics. |
| `ActorControlProof` | `DryRunOnly` | `true` | Logs commands without moving, targeting, spawning, or changing faction state. |
| `ActorControlProof` | `DumpActorControlCandidates` | `false` | Writes actor-control candidate CSV files once per loaded save. |
| `ActorControlProof` | `ScanRadius` | `35` | Limits nearby actor scan radius around the hero. |
| `ActorControlProof` | `SelectedCandidateGuid` | empty | Optional reviewed candidate identity for a controlled throwaway-save test. |
| `ActorControlProof` | `AllowOneSessionPetControl` | `false` | Allows active commands only for a reviewed pet/summon candidate. |
| `ActorControlProof` | `AllowDefendAction` | `false` | Must stay false until faction matrix validation passes. |

Hotkeys should not use `F8`, `F10`, `F11`, or `F12`. The current Avalon Companions defaults already use the keypad block. A later code pass should either reuse the existing panel/buttons for dry-run status or choose unassigned non-function-key defaults and update `docs/hotkey-registry.md`.

## Acceptance criteria

Phase 1 passes when:

- a throwaway loaded save writes actor-control CSVs,
- at least one existing pet or summon-like actor can be classified,
- wolf and bear rows, if present, remain blocked for custom defend, faction, persistence, and save/load behavior outside the later approved native `NpcHeroPetAlly` path,
- logs show no spawn, faction, quest, or save mutation.

Phase 2 passes when:

- follow, stay, recall, and defend dry-run commands log a concrete command path or a concrete block reason,
- status text shows what command actually ran or why it was blocked,
- no live actor state changes.

Phase 3 passes when:

- one reviewed pet/summon-like actor or one-session wolf/bear candidate follows, stays, and recalls in a single throwaway session,
- wolf/bear candidates are spawned only by explicit command and marked not saved,
- no persistence is enabled,
- BepInEx logs show no repeating errors or command spam.

Phase 4 passes only when:

- faction and hostility evidence is documented,
- actor-control proof defend remains off by default for arbitrary or unreviewed actors,
- a town or civilian safety scenario is validated before any attack assist is enabled.

## Next code step

Implement only Phase 1 and Phase 2 first:

- add an `ActorControlDiagnostics` class beside `PetSystemDiagnostics`,
- add disabled-by-default config entries,
- write plugin-owned CSVs under `BepInEx/config/kane.tgfoa.avalon-companions`,
- log dry-run command decisions,
- do not add custom defend, faction, persistence, or save/load behavior,
- keep Qrko native pet behavior and wolf/bear one-session candidate behavior separated.

## Validation needed before behavior

- Build Avalon Companions.
- Launch a throwaway save on the Mono branch.
- Confirm plugin load and config generation in `BepInEx/LogOutput.log`.
- Run actor-control dump.
- Review CSV rows for pet, summon, wolf, bear, and nearby NPC classification.
- Run dry-run commands and confirm the status text/logs identify the command path.
- Do not run active control until the dry-run candidate is reviewed.
