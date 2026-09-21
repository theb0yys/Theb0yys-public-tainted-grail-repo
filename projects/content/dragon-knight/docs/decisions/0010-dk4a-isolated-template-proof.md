# Decision 0010: DK4A Isolated Template Proof

Date: 2026-08-01

Status: accepted for isolated Unity authoring/build/finalized load-release proof; runtime source, deployment, live spawning, and AI remain blocked.

## Context

Decision 0009 prepared the isolated Dragon Knight template proof but left final Dragon Knight template GUIDs unselected.

The user requested the next step: add and run `DragonKnightDK4ATemplateAuthoring.cs` under the prepared packet.

## Decision

Record the DK4A isolated template proof closure:

`mods/dragon-knight/docs/research/dk4a-isolated-template-proof-2026-08-01.md`

Record generated machine evidence:

- `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-build-2026-08-01.json`
- `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-finalized-2026-08-01.json`

The generated Dragon Knight template GUIDs are:

- `NPCTemplate_DragonKnight_DK4A`: `00f608ee051b57748a6a9ed8dae28678`
- `Spec_DragonKnight_DK4A`: `d7b09116519f7564593be62781bee3db`

## Consequences

- DK4A now supplies a load-proven Dragon Knight native `NpcTemplate` / `LocationTemplate` pair.
- These GUIDs may become the actor-source input for a later DK4 live actor observation gate.
- DK4 source work is still blocked until the later DK4 source gate accounts for exact eligible target source, target `Location.ID`, host ownership, activation, cleanup, and no-save proof.
- This decision does not authorize deployment, live template registration, spawning, movement, attacks, damage, death, rewards, retained corpse behavior, roaming, companion protection, follower mechanics, weapon items, armor items, real phase combat, Rabbit, GOAP, PlayMaker, Blaze, FoAHost binding, or AI behavior.

## Review Required

The next gate must be a default-off live diagnostic source gate or target-source proof gate. It must not add combat, movement, AI actions, companion behavior, item behavior, or spawn paths beyond the explicitly reviewed diagnostic scope.
