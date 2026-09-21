# Decision 0006: DK4A Native Actor/Template Proof Profile

Date: 2026-08-01

Status: accepted for documentation-only proof-profile gate; source and template authoring remain blocked.

## Context

DK4 source implementation is blocked because Dragon Knight still lacks exact `LocationTemplate` and `NpcTemplate` evidence, exact runtime actor source evidence, exact eligible target source evidence, and a target `Location.ID` proof route.

The user identified the native actor/template proof as the recommended next move because that gives Dragon Knight the `LocationTemplate` and `NpcTemplate` required before any Dragon Knight source may call `LocationTemplate.SpawnLocation`.

The DK2 asset work proves visual transport only. It does not prove a native FoA actor-controller root, native template registration, attacks, combat, phase transitions, companion behavior, items, armor, saves, or roaming.

## Decision

Add the DK4A gate:

`mods/dragon-knight/docs/gates/DK4A-native-actor-template-proof-profile.md`

Record the read-only candidate scan:

`mods/dragon-knight/docs/research/dk4a-native-actor-template-candidate-scan-2026-08-01.md`

DK4A is a prerequisite to DK4 source implementation. It defines how Dragon Knight will prove a native `NpcTemplate` and `LocationTemplate` before live actor observation, host ownership, AI, movement, attacks, companion protection, phase combat, items, armor, or roaming.

## Consequences

- No Dragon Knight native baseline is selected yet.
- The preferred next evidence path is a serialized native-template probe for Foredweller T6 Knight.
- The fastest fallback is explicit user approval to use the already serialized Highwayman 1H values as a disposable Dragon Knight proof profile.
- Actual template authoring remains blocked until a later proof-profile approval locks exact values.
- DK4 live observation source remains blocked until DK4A supplies a Dragon Knight `LocationTemplate` / `NpcTemplate` pair or an existing live `Location` actor source.

## Review Required

High-risk review is required before any DK4A template authoring because the later proof will create native actor templates and may become the source of later runtime `SpawnLocation` calls.
