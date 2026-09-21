# Avalon Companions AI Package

This is the first source-only consumer package for Avalon AI Runtime.

- Package ID: `kane.tgfoa.avalon-companions.ai-intent-profile`
- Package version: `0.2.1`
- Required Avalon AI Runtime API: `1.1`
- Production dependencies: `AvalonAI.Contracts` only

The package extracts the existing evidence-approved `CompanionAiIntent` classification policy from Avalon Companions. It accepts caller-supplied observations through AIR-02 and may return one of two proposals: `CatchUpRecall` for `FollowCatchUpCandidate`, or `NativeDefendPrompt` for `NativeCombatObserved` only with live hero attackers, Defend mode or an already-approved native-assist gate, and caller-supplied confirmation that the existing native defend cooldown is ready. Every other state returns no proposal.

The package performs no observation collection, scheduling, command dispatch, Unity access, Blaze access, Photon access, FoA access, movement, targeting, combat, persistence, file I/O, or logging.

Its fixture now proves both proposal kinds and a no-proposal observation through Runtime's full host activation, collection, evaluation, dispatch, and release lifecycle. This host is a test double only. The package is not referenced by the game-loaded Avalon Companions plugin; live FoA observation, dispatch, scheduling, and consumer wiring remain blocked by `mods/avalon-ai-runtime/docs/gates/AIR-01-blaze-and-foa-host-intake.md`.

## Advanced companion package

- Package ID: `kane.tgfoa.avalon-companions.ai-advanced-companion`
- Package version: `0.1.0`
- Required Avalon AI Runtime API: `1.1`
- Production dependencies: `AvalonAI.Contracts` only

The advanced companion package accepts caller-supplied companion observations for managed animal companions first: wolf, bear, and existing roster animals. Wild animals are accepted only for tame-readiness classification. Avalon Companions remains the owner of tame, summon, dismiss, persistence, command UI, observation collection, native dispatch, and lifecycle. The AI package owns decision proposals only.

The advanced observation surface includes actor identity, template identity, scene, position, distance to player, health/alive/unconscious state, command mode, follow range, trust, loyalty, bond level, native idle/alert/combat/flee state, current target threat, visible attacker count, movement blocked/stuck state, managed/tame/hostile/undead/passive classification, cooldown gates, owner validity, and kill-switch state.

The package may propose `CatchUpRecall`, `NativeDefendPrompt`, `HoldAnchor`, `RegroupNearPlayer`, `RecoverStuck`, `RetreatOrStandDown`, `TameCandidateReady`, or `TameCandidateUnsafe`. It may also return no proposal. Every proposal records that it is proposal-only and does not call Unity, FoA, BepInEx, native game APIs, Rabbit, GOAP, Blaze, persistence, target overrides, faction overrides, movement overrides, tame/summon/dismiss ownership, or command UI ownership.

## Build and fixture gate

```powershell
dotnet build .\mods\avalon-companions\ai-package\tests\AvalonCompanions.AI.Package.Fixtures\AvalonCompanions.AI.Package.Fixtures.csproj -c Release
dotnet run --project .\mods\avalon-companions\ai-package\tests\AvalonCompanions.AI.Package.Fixtures\AvalonCompanions.AI.Package.Fixtures.csproj -c Release --no-build
```
