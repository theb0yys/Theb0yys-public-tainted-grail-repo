# Human Actor Scanner Proof

## Scope

Implement the approved next code step after the safe proof spawn: a disabled-by-default scanner that collects evidence only.

This does not approve human companion behavior, recruitment, follow, combat, dialogue, persistence, or a command panel.

## Research basis

- `docs/research/human-npc-proof-step-2026-06-15.md` approves a disabled scanner that classifies live NPCs and writes plugin-owned CSV evidence only.
- `docs/research/added-research-review-2026-06-15.md` keeps runtime behavior blocked and says future interaction UI must be plugin-owned and runtime-only.
- `docs/research/safe-proof-spawn-target-2026-06-15.md` approves `Spec_Enemy_Generic_Tier1_Outlaw_1H` / `2bd34a05d1e1fb94f9770b9ee7f23be2` only as a throwaway-save proof target, not as a companion.

## Implementation

Version 0.1.1 adds `HumanActorScanner`.

Config gates:

- `ActorScanner.EnableDisabledActorScanner`, default `false`.
- `ActorScanner.ScannerHotkey`, default `None`.
- `ActorScanner.ScanRadius`, default `35`, range `5..120`.
- `ActorScanner.SelectedTarget`, default empty. It may hold a reviewed template GUID or live location ID for dry-run classification.

Runtime behavior:

- The scanner only runs when the mod is enabled, the scanner gate is enabled, and the scanner hotkey is pressed.
- It requires `Hero.Current`.
- It scans live `Location` rows within radius plus known pet/summon/ally marker parents.
- It writes plugin-owned CSV files under `BepInEx/config/kane.tgfoa.avalon-human-companions`.
- It catches scanner exceptions and logs warnings instead of breaking gameplay.

CSV outputs:

- `human-actor-candidates.csv`: actor-like location rows, template GUID/name, uniqueness surface, not-saved state, component hints, blocked reason, and explicit `behaviorApproved=false` / `panelApproved=false`.
- `human-actor-components.csv`: template, location, known model markers, known location elements, and interesting view components grouped by hint bucket.
- `human-actor-command-dry-run.csv`: follow, stay, recall, dismiss, defend, and panel rows. Every row is blocked and `liveAction=false`.
- `human-recruitment-capture-research.csv` in 0.6.0: scanner-only friendly/enemy research lanes for future capture/recruitment review. Every row is blocked and keeps `liveAction=false`, `recruitmentApproved=false`, `captureApproved=false`, and `persistenceApproved=false`.

## Safety rules

- No NPC is spawned by the scanner.
- No NPC is moved, teleported, recruited, converted, commanded, dismissed, faction-edited, target-edited, story-edited, crime-edited, interaction-edited, or persisted.
- The panel row is a dry-run evidence row only. A real panel remains blocked until scanner output and a separate plugin-owned runtime UI proof pass review.
- Existing live actors are blocked unless later research proves the actor identity, ownership, faction/targeting, interaction, transition, and save/load path.
- Friendly/enemy lane classification is string evidence only. It is not proof of live hostility, friendliness, recruitment safety, capture safety, or persistence safety.

## First validation state

- Release build passed on 2026-06-15 with 0 warnings and 0 errors.
- Runtime scanner CSV validation has not been captured yet.
- The configured scanner hotkey did not produce the expected output folder or a scanner log line during the latest live check.
- Version 0.1.6 adds a gated proof-panel `Scan Actors` button that calls the same scanner path because the proof panel hotkey is already working in live testing.
- The proof-panel scanner button wrote all three CSVs during live validation, but that scan happened before the one-session proof actor was created, so the reviewed outlaw target GUID was absent.
- The next runtime test should spawn the one-session proof actor first, then run `Scan Actors` near it on a throwaway save and review all three CSVs before implementing behavior beyond the current one-session proof.
