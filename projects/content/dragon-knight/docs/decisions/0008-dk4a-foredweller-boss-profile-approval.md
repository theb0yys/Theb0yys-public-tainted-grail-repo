# Decision 0008: DK4A Foredweller Boss Profile Approval

Date: 2026-08-01

Status: accepted for proof-profile approval; source, template authoring, deployment, live spawning, and AI remain blocked.

## Context

Decision 0007 recorded the read-only serialized Foredweller T6 Knight native-template probe. That probe supplied exact NPC and location-template values but left the Dragon Knight baseline unselected.

The user then approved the Foredweller path with overrides and requested boss-level stats.

## Decision

Select the Foredweller T6 Knight pair as the disposable Dragon Knight DK4A proof profile:

- `NPCTemplate_EnemyForedweller_T6_Knight` GUID `439a2dc3cf64e4e4aa792be5b4034cf1`
- `Spec_EnemyForedweller_T6_Knight` GUID `24ee850d0ffdbf64ea2b07b6a718d63b`

Record the exact approval profile:

`mods/dragon-knight/docs/research/dk4a-foredweller-boss-profile-approval-2026-08-01.md`

The approved boss-level stats are the direct serialized Foredweller T6 Knight values from the DK4A probe. No higher or alternate numeric stat overrides are invented.

## Consequences

- The explicit DK4A proof-profile approval blocker is closed.
- Foredweller T6 Knight is now the selected source baseline for the next isolated Dragon Knight template proof.
- The future Dragon Knight display name is locked as `Dragon Knight`.
- The future template pack folder remains `DragonKnightDK4ATemplate`.
- The future address prefix remains `dragon-knight/boss/dk4a`.
- The Foredweller native visual address is approved only as the native bootstrap visual for proof initialization.
- Dragon Knight Iron/Fire visuals remain overlays and still do not prove attacks, damage, phase transitions, death, loot, rewards, roaming, companion behavior, or item behavior.
- DK4 live actor observation source remains blocked until DK4A produces an authored, load-proven Dragon Knight `NpcTemplate` / `LocationTemplate` pair or an exact existing live Dragon Knight `Location` source.

## Review Required

High-risk review is still required before template authoring because the next step creates native actor templates and may become the source for a later `LocationTemplate.SpawnLocation` call.
