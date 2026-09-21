# Human Proof Candidate Roster

Date: 2026-06-21

## Scope

Register the human diagnostic review queue in the runtime panel using the same foundational pattern as Avalon Companions: a hardcoded roster entry contract, selected index config, Prev/Next controls, and a single explicit Spawn / Swap command.

This is roster foundation for proof testing only. It does not approve persistent human companions, recruitment, conversion of existing NPCs, dialogue, quest state, inventory, equipment, affinity, custom targeting, save ownership, or release-ready behavior.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/generated/human-npc-diagnostic-map-20260616-090615.csv`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Framework/PetRosterEntry.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Companion reference

Avalon Companions exposes a registered roster through:

- `SelectedPetIndex`
- a `PetRosterEntry[]` runtime registry
- `Prev`, `Summon / Swap`, and `Next` panel controls
- selected-index normalization
- explicit player command as the only actor creation path

The human panel was missing that foundation and only read a single manual `Target.TemplateGuid`, which left the panel blank unless config was edited manually.

## Decision

Avalon Human Companions may register the 42 `ReviewQueueNonUniqueSpawnerBacked` diagnostic rows as proof candidates.

Approved:

- Add `HumanCompanionRosterEntry`.
- Add a hardcoded `ProofCandidateRoster` from the generated diagnostic map.
- Add `HumanRoster.EnableProofCandidateRoster`, default `true`.
- Add `HumanRoster.SelectedCandidateIndex`, defaulting to the existing reviewed outlaw proof target.
- Show selected candidate count/name, template, GUID, review ID, and risk flags in the debug panel.
- Add `Prev`, `Spawn / Swap`, and `Next` controls.
- Route selected candidates through the existing one-session ally proof spawn path.
- Keep the existing runtime guards: research-mode gate, explicit proof gate, non-unique template checks, `NpcElement` checks, native summon-faction plus `NpcHeroPetAlly`, and `MarkedNotSaved=true`.
- Default scanner target to the selected roster candidate when `ActorScanner.SelectedTarget` is empty.

Not approved:

- Treating review candidates as save-backed companions.
- Converting existing NPCs.
- Bypassing unique/story/boss checks.
- Adding persistence, recruitment, dialogue graph edits, quest integration, global faction edits, custom targeting, or true Wait.

## Validation needed

- Release build.
- Throwaway-save runtime check with `Research.ResearchModeOnly=false` and `HumanAllyProof.EnableOneSessionAllyProof=true`.
- Open the debug panel and confirm it shows `Selected: n/42`.
- Confirm Prev/Next changes the selected proof candidate.
- Confirm Spawn / Swap creates the selected proof actor through the existing guarded path and marks it not saved.
- Confirm scanner rows use the selected candidate GUID when no scanner target override is configured.
