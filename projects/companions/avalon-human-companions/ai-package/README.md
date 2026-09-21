# Avalon Humans AI Package

This engine-neutral package owns the approved human-companion brain policy for Avalon AI Runtime.

It evaluates a managed human's Follow, Hold, or Defend state and can propose only:

- applying or releasing the reviewed runtime Hold movement lock;
- recalling through the reviewed role-aware Follow or Combat placement; or
- prompting FoA's native ally defend path.

It does not choose targets, pathfind, recruit or convert NPCs, touch quests/dialogue/crime, or persist actors. The single FoA host maps it through Avalon Human Companions' direct exclusive boundary; the package remains engine-neutral and never calls the game.

## Executor-grade package

The executor-grade package source is included in `src/AvalonHumans.AI.Package` as `AvalonHumanCompanionExecutorPackage`.

- PACK PATH: `mods/avalon-human-companions/ai-package/src/AvalonHumans.AI.Package`
- PACK VERSION: `0.1.0`
- PACKAGE ID: `kane.tgfoa.avalon-human-companions.ai-executor`
- TARGET ACTORS: assigned Avalon-managed one-session human proof actors; existing native actors require snapshot/suspend/restore proof before binding.
- UNITY VERSION: Unity 6 adapter target; core package is `netstandard2.1` and engine-neutral.
- SOURCE INCLUDED: yes.

The executor package provides a Contracts-only C# API for:

- binding and unbinding one exact actor;
- assigning, replacing, validating, and clearing one exact target;
- accepting data-only observation snapshots;
- dispatching actions and returning unique handles;
- polling, cancelling, stopping all actions, suspending, resuming, and disposing;
- returning `Accepted`, `Running`, `Succeeded`, `Failed`, `Interrupted`, `Rejected`, or `TimedOut` with stable reason codes;
- exposing runtime configuration, per-role profiles, and diagnostics.

The package does not call Unity, FoA, BepInEx, Rabbit, GOAP, Blaze, or PlayMaker. Actual movement, combat, animation, and native state restoration still require a separate Avalon/FoAHost bridge that owns the Unity-main-thread execution and maps approved requests into reviewed game calls.
