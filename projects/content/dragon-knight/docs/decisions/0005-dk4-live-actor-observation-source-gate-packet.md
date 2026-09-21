# Decision 0005: DK4 Live Actor Observation Source Gate Packet

Date: 2026-08-01

Status: blocked source-gate packet accepted for implementation shape only.

## Context

DK4 already requires a default-off live diagnostic/proof gate for Dragon Knight actor observation, `Location.ID`, target `Location.ID`, host ownership, activation, kill switch, and cleanup.

The user confirmed that the DK4 source packet still needs exact source files, activation trigger, fixture list, marker text, build/deploy plan, live log checks, and cleanup checks before code changes.

Existing Dragon Knight source is a single standalone BepInEx plugin project. DK2 remains visual-only and DK3 remains a source-only AI package with no goals, actions, capabilities, live host mapping, or runtime registration.

Existing proven-method evidence shows native creature proof lanes use default-off F-key activation, exact `LocationTemplate` resolution, `BaseLocationSpawner.VerifyPosition`, `LocationTemplate.SpawnLocation`, immediate save exclusion, one-request/session caps, and explicit teardown. Dragon Knight does not yet have an accepted `LocationTemplate` or `NpcTemplate`.

## Decision

Add the DK4 source-gate packet:

`mods/dragon-knight/docs/gates/DK4-live-actor-observation-host-ownership-source-gate-packet.md`

The packet locks the exact source files, config keys, activation trigger, owner id, actor id format, target contract, fixture count, marker text, build/deploy plan, live log checks, cleanup checks, and stop conditions.

Source implementation remains blocked until Dragon Knight supplies exact `LocationTemplate`/`NpcTemplate` or exact existing live `Location` actor source evidence, plus exact eligible target source evidence.

## Consequences

- DK4 has an implementation-ready shape but does not yet authorize code changes.
- No Dragon Knight source may call `LocationTemplate.SpawnLocation` until the Dragon Knight native actor/template proof exists.
- The next task is to supply the exact Dragon Knight native actor/template source and eligible target source, or explicitly narrow DK4 to an observe-only blocked-marker diagnostic.

## Review Required

High-risk review is required before DK4 source implementation because the later source touches live FoA actor identity, ownership/lease state, deployment, and live log validation.
