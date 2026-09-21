# Decision 0007: DK4A Foredweller Serialized Native-Template Probe

Date: 2026-08-01

Status: accepted read-only probe result; Dragon Knight baseline and template authoring remain blocked.

## Context

Decision 0006 defined DK4A as the native actor/template proof-profile gate and identified Foredweller T6 Knight as the preferred next serialized native-template probe candidate.

The user confirmed that Foredweller T6 Knight still needed a serialized native-template probe before template authoring.

## Decision

Record the DK4A Foredweller serialized native-template probe:

`mods/dragon-knight/docs/research/dk4a-foredweller-serialized-native-template-probe-2026-08-01.md`

Record the generated deterministic JSON report:

`mods/dragon-knight/docs/generated/dragon-knight-dk4a-foredweller-template-probe-2026-08-01.json`

The probe establishes exact serialized Foredweller T6 Knight NPC and location-template values for a later Dragon Knight proof-profile approval decision.

## Consequences

- The missing serialized Foredweller probe evidence is now supplied.
- Foredweller T6 Knight is still not selected as the Dragon Knight baseline.
- Dragon Knight template authoring remains blocked until the proof profile is explicitly approved.
- DK4 live actor observation source remains blocked until DK4A produces an approved Dragon Knight `NpcTemplate` / `LocationTemplate` pair or an exact existing live Dragon Knight `Location` source.

## Review Required

High-risk review is still required before template authoring because the later step will create native actor templates and may become the source for later `LocationTemplate.SpawnLocation` calls.
