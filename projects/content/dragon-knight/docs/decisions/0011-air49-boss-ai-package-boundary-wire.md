# Decision 0011: AIR-49 Boss AI Package Boundary Wire

Date: 2026-08-02

Status: accepted for default-off package validation wiring only.

## Evidence

- The user supplied the AIR-49 requirement that Boss AI remains blocked until exact boss actor source, `ActorId` proof, native-assisted authority proof, target `Location.ID` proof, Rabbit/GOAP proof, capability sufficiency proof, and FoAHost activation trigger are supplied.
- The user then stated the AIR-49 Dragon Knight Boss AI package contract was implemented with package ID `dragon-knight.boss.ai.v2`, assembly `DragonKnight.AI.Package.V2`, role `dragon-knight.boss`, fail-closed policy checks, 15 offline fixtures, and marker-first output.
- The user explicitly directed the current agent to wire the package boundary, not author the package internals.
- `mods/avalon-ai-runtime/docs/architecture.md` states that third-party AI mods reference Avalon public contracts only, while Rabbit, GOAP, PlayMaker, Blaze/native execution, actor ownership, and safety remain inside the guarded Avalon runtime boundary.

## Decision

Dragon Knight may wire the implemented AIR-49 package into the standalone loader by:

- referencing `DragonKnight.AI.Package.V2`;
- validating package identity, template GUIDs, arena radii, default-off flags, kill switch, no-direct-native/no-bypass/no-save flags, manifest counts, no persistent keys, procedure requirement count, and fail-closed default/kill-switch/forbidden-trigger decisions;
- logging the package marker line and a Dragon Knight-specific package-wired line;
- keeping `DragonKnightBossAI.Enabled=false` and `DragonKnightBossAI.KillSwitch=true` by default;
- copying the package and contract DLLs with the Dragon Knight loader deployment.

## Limits

This decision does not authorize:

- live Avalon AI Runtime registration;
- live package host mapping;
- actor observation;
- actor spawning;
- target acquisition;
- Rabbit writes;
- GOAP execution;
- PlayMaker execution;
- Blaze execution;
- native FoA command dispatch;
- movement;
- attacks;
- phase combat;
- companion protection;
- items;
- save writes;
- roaming.

If `DragonKnightBossAI.Enabled=true` before DK4 live actor/target/host proof passes, the loader must still log a blocked runtime line and dispatch no behavior.
