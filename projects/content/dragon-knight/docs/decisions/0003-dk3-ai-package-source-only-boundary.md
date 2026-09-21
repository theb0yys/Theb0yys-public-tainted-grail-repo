# Decision 0003: DK3 Source-Only AI Package Boundary

Date: 2026-08-01

Status: accepted for a source-only, observation-only Dragon Knight AI package scaffold.

## Evidence

- The user required Dragon Knight to remain a completely standalone mod and said the AI system should be used as a reference method, with AI needing its own AI package.
- `mods/dragon-knight/docs/design.md` requires a Dragon Knight AI package lane and says Dragon Knight must adapt Avalon AI Runtime's package/host, blackboard, planning, action-gateway, lease, and fail-closed validation methods.
- `mods/dragon-knight/docs/research.md` states Dragon Knight needs a Dragon Knight-owned AI package lane, not direct ad hoc AI inside boss or companion feature code.
- `mods/avalon-ai-runtime/docs/architecture.md` is the canonical architecture and states that third-party mods reference Avalon public contracts, while Rabbit, GOAP, PlayMaker, Blaze/native execution, actor ownership, and safety remain inside the guarded Avalon runtime boundary.
- `mods/avalon-ai-runtime/README.md` records that packages reference contracts only, and package-to-FoA mapping belongs in the single Mono host.
- DK2 import evidence found only the usable `Pose_Check` animation and zero controller states, so attack clips, phase state transitions, and combat animation mapping remain unproven.

## Decision

DK3 may add a Dragon Knight-owned `AvalonAI.Contracts.V2` package assembly that:

- targets `netstandard2.1`;
- references only `AvalonAI.Contracts.V2`;
- declares stable Dragon Knight package identity;
- reserves separate Dragon Knight boss and companion actor roles;
- declares no goals;
- declares no actions;
- declares no action capabilities;
- declares no procedure requirements;
- declares no persistent keys;
- ships no Unity, BepInEx, Awaken/FoA, Runtime, Rabbit, GOAP, PlayMaker, or Blaze references.

DK3 may add offline fixtures that reference `AvalonAI.Runtime.V2` only in the test project to prove the source-only package registers with Runtime V2 and remains contracts-only.

## Limits

This decision does not authorize:

- registering Dragon Knight with the live Avalon AI Runtime host;
- adding Avalon AI Runtime DLLs to the live Dragon Knight plugin folder;
- adding compile-time or runtime AI references to `DragonKnight.dll`;
- actor observation collection;
- native actor spawning;
- target selection;
- movement;
- attacks;
- two-phase combat;
- companion follow/defend/protect actions;
- PlayMaker procedures;
- Blaze execution;
- native FoA command dispatch;
- persistence or save writes.

Real Dragon Knight combat AI remains blocked until a later gate proves actor identity, observation source, host ownership, native/Blaze execution route, exact actions, target policy, movement policy, attack animation/combat mapping, phase-transition rules, and cleanup behavior.
