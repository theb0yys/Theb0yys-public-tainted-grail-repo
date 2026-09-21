# Decision 0004: DK4 Live Actor Observation And Host Ownership Proof Gate

Date: 2026-08-01

Status: accepted for a documentation-only proof gate; source implementation remains blocked.

## Context

Dragon Knight is standalone and must adapt proven companion, asset, and AI methods without becoming a runtime dependency of those existing mods.

DK2 proved only visual asset transport. The imported Dragon Knight controller has zero states and the only usable imported clip is `Pose_Check`, so attack animation and two-phase combat behavior are not proven.

DK3 added only a source-only `AvalonAI.Contracts.V2` package shell. It reserves Dragon Knight boss and companion roles but declares no goals, actions, capabilities, procedure requirements, persistent keys, host mapping, or live runtime registration.

AIR-49 and Decision 0060 record the same next blocker for Boss AI: exact actor source, actor `Location.ID`, target `Location.ID`, Rabbit/GOAP authority proof, capability sufficiency, and FoAHost activation evidence are missing.

The current user instruction confirms that the next step is a default-off live diagnostic/proof gate that creates or observes Dragon Knight as a real owned FoA actor, logs its `Location.ID`, proves the target `Location.ID`, and records FoAHost ownership/activation.

## Decision

Create the DK4 proof gate at `mods/dragon-knight/docs/gates/DK4-live-actor-observation-host-ownership-proof.md`.

DK4 is the next Dragon Knight AI gate. It is diagnostic-only and must stay default-off. The gate exists to collect live actor observation and ownership evidence before any Dragon Knight AI package behavior is allowed.

## Consequences

- Dragon Knight AI source work remains blocked until a later DK4 source-gate packet is accepted.
- Dragon Knight runtime behavior remains visual-only until that source gate passes and is validated live.
- The next source packet must define exact source files, activation trigger, fixture list, marker text, build command, deploy target, live log checks, cleanup checks, and stop conditions.
- No gameplay behavior is authorized by this decision.

## Review Required

The later source gate needs a high-risk review because it will touch live FoA actor observation, ownership leases, diagnostic activation, and cleanup behavior.
