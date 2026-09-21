# Human Recruitment/Capture Classifier

## Scope

Move toward a capture/recruitment system by extending the existing disabled actor scanner with evidence-only friendly/enemy research lanes.

This does not approve live recruitment, capture, conversion of existing NPCs, persistence, faction edits, hostility edits, crime edits, story edits, dialogue edits, or save data.

## Research basis

- `mods/avalon-human-companions/docs/research.md` keeps human recruitment and existing-NPC conversion blocked until actor identity, faction, interaction, transition, and save/load paths are researched and validated.
- `mods/avalon-human-companions/docs/design.md` requires a second review before any human NPC behavior and limits the scanner to plugin-owned evidence output.
- `mods/avalon-human-companions/docs/research/human-actor-scanner-2026-06-15.md` approves scanner CSV output only and explicitly blocks spawning, moving, recruiting, converting, commanding, dismissing, faction-editing, target-editing, interaction-editing, and persistence.

## Decision

Version 0.6.0 may add one scanner CSV:

- `human-recruitment-capture-research.csv`

The CSV may classify actor-like rows into conservative research lanes:

- `EnemyCaptureResearch` for enemy-like evidence.
- `FriendlyRecruitmentResearch` for friendly-like evidence.
- `DispositionAmbiguousResearch` when both friendly and enemy evidence are present.
- `DispositionUnknownResearch` when the scanner cannot infer a lane from current strings.
- `OneSessionProofOnly` for the current managed not-saved ally proof actor.

The classification is only a review aid. It is based on template names, display/debug names, and component/faction hint strings. It is not proof of actual hostility, friendliness, recruitment safety, capture safety, or persistence safety.

## Implementation boundary

The scanner may:

- write the new CSV under `BepInEx/config/kane.tgfoa.avalon-human-companions`,
- add dry-run rows for `recruit-friendly` and `capture-enemy`,
- mark every recruitment/capture row `blocked=true`,
- mark every recruitment/capture row `liveAction=false`,
- keep `recruitmentApproved=false`, `captureApproved=false`, and `persistenceApproved=false`.

The scanner must not:

- add dialogue choices or panel buttons for recruitment/capture,
- attach prompts to existing NPCs,
- move, teleport, command, dismiss, recruit, capture, convert, or discard existing NPCs,
- edit faction, hostility, targeting, ownership, crime, quest, story, dialogue, interaction, or save state,
- claim a safe capture or recruitment path.

## Required next proof

Before live capture/recruitment behavior, collect and review scanner output near known friendly guards/civilians and hostile enemies. The follow-up research must prove live disposition/hostility APIs, guard/civilian/quest/crime safety, plugin-owned UI flow, transition cleanup, and save/load behavior without affecting unrelated NPCs or the player.
