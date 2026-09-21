# DK4: Live Actor Observation And Host Ownership Proof Gate

Status: accepted documentation-only proof gate. No source implementation is authorized by this file.

Date: 2026-08-01

## Objective

DK4 is the next Dragon Knight AI gate after the source-only package scaffold. It must collect live proof that Dragon Knight can be observed as a real owned FoA actor before any goals, actions, movement, attacks, companion protection, phase behavior, or live AI registration are implemented.

The proof target is default-off, diagnostic-only, and temporary. It exists to answer the AIR-49 and Dragon Knight DK3 blockers, not to add gameplay.

## Required Proof

The later DK4 source-gate packet must prove all of these items before any live diagnostic source is written:

- Exact Dragon Knight boss actor source.
- Exact actor runtime identity source.
- Proof that the actor exposes `Location.ID` if `foa.location:<Location.ID>` is used.
- Exact `ActorId` formatting and rejection behavior for missing, empty, duplicate, stale, destroyed, or mismatched identities.
- Exact host owner identity and lease lifecycle.
- Exact FoAHost activation trigger and default-off configuration.
- Exact kill-switch behavior.
- Exact eligible target source.
- Exact target `Location.ID` proof.
- Exact cleanup and release behavior on scene unload, actor destruction, disabled config, kill switch, failed observation, and stop/dispose.

## Diagnostic Boundary

DK4 may only define or later implement a diagnostic that:

- is disabled by default;
- has an independent kill switch that fails closed;
- runs only after an explicit operator action or source-gate-approved activation trigger;
- observes or creates exactly one Dragon Knight boss candidate for proof collection;
- logs actor source, actor runtime identity, owner, lease, target source, target runtime identity, activation reason, and cleanup result;
- releases ownership before shutdown;
- records no persistent state;
- writes no Rabbit facts;
- registers no GOAP goals or actions;
- dispatches no native movement, interaction, attack, damage, death, loot, reward, phase, follower, or item commands.

## Required Marker

The later source-gate packet must define the exact marker text. The marker must include at least:

- gate id;
- package id;
- actor role id;
- actor source;
- `ActorId`;
- actor `Location.ID`;
- owner id;
- lease id;
- target source;
- target `Location.ID`;
- activation trigger;
- default-off state;
- kill-switch state;
- cleanup result;
- `goals=0`;
- `actions=0`;
- `movement=0`;
- `attacks=0`;
- `phase-combat=0`;
- `companion-protect=0`;
- `items=0`;
- `save=0`.

## Explicitly Not Authorized

DK4 does not authorize:

- Boss AI source implementation.
- Dragon Knight AI package live registration.
- Dragon Knight host mapping.
- Rabbit schema or write authority.
- GOAP sensors, scoring, goals, or actions.
- PlayMaker procedures.
- Blaze execution.
- Direct FoA/native command dispatch.
- Movement.
- Attacks.
- Companion follow, defend, or protect behavior.
- Real two-phase combat.
- Actor persistence.
- Save writes.
- Roaming, population, loot, rewards, corpse, death, or health-bar behavior.
- Weapon or armor item registration.
- Deployment or live game launch without a later source-gate packet and validation plan.

## Acceptance Criteria

DK4 can pass only when a later accepted source gate and live run produce evidence that:

- Dragon Knight actor observation succeeded for one exact candidate.
- `ActorId` is derived from an exact live `Location.ID`, or the source gate rejects `foa.location:<Location.ID>` and records the replacement identity contract.
- Host ownership is acquired and released exactly once for the observed actor.
- The eligible target is location-backed and has an exact live `Location.ID`.
- Default-off and kill-switch states are logged.
- All unsupported gameplay channels remain inactive.
- Cleanup is observed after stop, scene transition, and actor disposal.

## Next Step

The next useful work is a DK4 source-gate packet for the diagnostic only. That packet must provide the exact source files, activation trigger, fixture list, marker text, build command, deploy target, live log checks, cleanup checks, and stop conditions before any source code is changed.
