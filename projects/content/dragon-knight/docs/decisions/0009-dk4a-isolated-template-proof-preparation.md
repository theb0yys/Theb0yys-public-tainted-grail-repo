# Decision 0009: DK4A Isolated Template Proof Preparation

Date: 2026-08-01

Status: accepted for isolated template proof preparation; Unity authoring, catalogue build, finalized load/release, runtime source, deployment, live spawning, and AI remain blocked.

## Context

Decision 0008 selected Foredweller T6 Knight as the disposable Dragon Knight proof profile, but final Dragon Knight `NpcTemplate` and `LocationTemplate` GUIDs remained pending.

The user identified that blocker: final Dragon Knight template GUIDs are still pending the isolated template proof step.

## Decision

Record the DK4A isolated template proof preparation:

`mods/dragon-knight/docs/research/dk4a-isolated-template-proof-preparation-2026-08-01.md`

Record the exact source-gate packet:

`mods/dragon-knight/docs/gates/DK4A-isolated-template-proof-source-gate-packet.md`

Record the generated preparation JSON:

`mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-proof-preparation-2026-08-01.json`

The next execution gate may generate the final Dragon Knight template GUIDs only by running the isolated Unity authoring/build/two-cycle load-release proof described in those files.

## Consequences

- Dragon Knight now has an accepted preparation packet for final template GUID generation.
- Final Dragon Knight template GUIDs are still not selected by this decision.
- The next execution gate must use new pack-owned GUIDs and must not reuse native Foredweller GUIDs.
- DK4 live actor observation source remains blocked until the isolated template proof produces a load-proven Dragon Knight `NpcTemplate` / `LocationTemplate` pair or an exact existing live Dragon Knight `Location` source.

## Review Required

The next gate is high-risk because it writes an isolated Unity project, builds Addressables content, and records the generated Dragon Knight template GUIDs. It still must not modify the game install, access saves, construct actors, spawn actors, write runtime plugin source, or change AI behavior.
