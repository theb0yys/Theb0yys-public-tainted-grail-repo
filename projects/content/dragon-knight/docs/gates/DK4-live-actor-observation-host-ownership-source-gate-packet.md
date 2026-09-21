# DK4: Live Actor Observation And Host Ownership Source Gate Packet

Status: source implementation authorized for the default-off DK4 live actor observation diagnostic. Live pass remains pending.

Date: 2026-08-01

## Scope

This packet defines the exact DK4 diagnostic source lane requested after `DK4-live-actor-observation-host-ownership-proof.md`.

DK4 is not Boss AI behavior. It is a default-off diagnostic that must prove Dragon Knight can be observed as one exact FoA `Location`, derive `ActorId` from that exact `Location.ID`, acquire and release one Dragon Knight-owned observation lease, prove one eligible target `Location.ID`, and log cleanup.

## Blocker Closure Inputs

Source implementation was blocked until a later update supplied:

- Dragon Knight `LocationTemplate` source: supplied by DK4A isolated template proof, `Spec_DragonKnight_DK4A` GUID `d7b09116519f7564593be62781bee3db`.
- Dragon Knight `NpcTemplate` source: supplied by DK4A isolated template proof, `NPCTemplate_DragonKnight_DK4A` GUID `00f608ee051b57748a6a9ed8dae28678`.
- Exact Dragon Knight runtime actor source: DK4 diagnostic creates one no-save actor from the proved Dragon Knight `LocationTemplate` through `LocationTemplate.SpawnLocation` only after F7 and all runtime gates.
- Exact eligible target source: Ancient Cromlech `AltarInteract`, used only as location-backed target proof and scene/measurement evidence, not as activation trigger.
- Exact target `Location.ID` proof route: `CM_Stonehenge_0_2258891627046429048_1`, resolved at runtime from `World.All<Location>()`.

The Dragon Knight `LocationTemplate` and `NpcTemplate` proof now exists, so `LocationTemplate.SpawnLocation` is authorized only inside `DragonKnightDk4DiagnosticHost`, only after F7, `Enabled=true`, `KillSwitch=false`, `AllowNativeSpawn=true`, exact scene, exact live target, exact template resolution, and no-save readback.

## Authorized Source Files After Blocker Closure

Only these files may be created or edited for the DK4 diagnostic:

- `mods/dragon-knight/src/Plugin.cs`
- `mods/dragon-knight/src/DragonKnight.csproj`
- `mods/dragon-knight/src/DK4/DragonKnightDk4DiagnosticOptions.cs`
- `mods/dragon-knight/src/DK4/DragonKnightDk4DiagnosticHost.cs`
- `mods/dragon-knight/src/DK4/DragonKnightDk4ActorObserver.cs`
- `mods/dragon-knight/src/DK4/DragonKnightDk4OwnershipLease.cs`
- `mods/dragon-knight/src/DK4/DragonKnightDk4Marker.cs`
- `mods/dragon-knight/tests/DragonKnight.DK4Diagnostic.Fixtures/DragonKnight.DK4Diagnostic.Fixtures.csproj`
- `mods/dragon-knight/tests/DragonKnight.DK4Diagnostic.Fixtures/Program.cs`
- `mods/dragon-knight/docs/test-notes.md`
- `mods/dragon-knight/docs/validation-plan.md`

No `mods/avalon-ai-runtime`, `mods/avalon-companions`, or `mods/avalon-awakened` source files may be changed for this gate.

## Exact Runtime Configuration

The diagnostic must bind these config keys:

- `DK4Diagnostic.Enabled=false`
- `DK4Diagnostic.KillSwitch=true`
- `DK4Diagnostic.ActivationHotkey=F7`
- `DK4Diagnostic.ReleaseHotkey=F6`
- `DK4Diagnostic.MaximumObservationSeconds=10`
- `DK4Diagnostic.AllowNativeSpawn=false`

Validation may temporarily set:

- `DK4Diagnostic.Enabled=true`
- `DK4Diagnostic.KillSwitch=false`
- `DK4Diagnostic.AllowNativeSpawn=true`

`AllowNativeSpawn` remains `false` by default. It may be set `true` only for the DK4 live diagnostic now that DK4A supplies exact `LocationTemplate` and `NpcTemplate` evidence.

## Activation Trigger

The only selected activation trigger is explicit operator input:

- F7 starts one DK4 diagnostic attempt.
- F6 releases the active DK4 observation lease and clears the active candidate.
- `OnDestroy` must call the same release path as F6.

Startup, scene load, save load, visual proof spawn, and package registration must not start DK4 automatically.

## Actor Identity Contract

The selected owner id is:

`dragon-knight.dk4.diagnostic-host`

The selected actor role id is:

`dragon-knight.boss`

The selected `ActorId` format is:

`foa.location:<Location.ID>`

The diagnostic must reject:

- missing `Location`;
- discarded `Location`;
- missing `Location.ID`;
- empty or whitespace `Location.ID`;
- duplicate active actor;
- changed `Location.ID` after lease acquisition;
- destroyed or discarded actor before release;
- release from any owner id other than `dragon-knight.dk4.diagnostic-host`.

## Target Contract

The selected target kind is:

`LocationBackedInteractable`

The target id format is:

`foa.location:<Location.ID>`

Target proof uses the exact live Cromlech `AltarInteract` `Location.ID` `CM_Stonehenge_0_2258891627046429048_1`. The diagnostic may not substitute a prefab path, template GUID, GameObject name, world position, hero position, visual proof object, or manually typed string for a live target `Location.ID`.

## Fixture List

The later implementation must add exactly 36 DK4 fixtures:

1. default config disables DK4;
2. default kill switch blocks DK4;
3. F7 does nothing while disabled;
4. F7 logs blocked while kill switch is true;
5. F6 release is idempotent with no active lease;
6. `OnDestroy` release is idempotent with no active lease;
7. no source references Avalon AI Runtime;
8. no source references Avalon Companions;
9. no source references Avalon Awakened;
10. no source references Rabbit;
11. no source references GOAP;
12. no source references PlayMaker;
13. no source dispatches native movement;
14. no source dispatches native interaction;
15. no source dispatches attack, damage, death, loot, item, or phase behavior;
16. missing actor source logs blocked;
17. missing actor `Location.ID` logs blocked;
18. empty actor `Location.ID` logs blocked;
19. discarded actor logs blocked;
20. duplicate actor logs blocked;
21. actor id is formatted as `foa.location:<Location.ID>`;
22. ownership lease uses owner id `dragon-knight.dk4.diagnostic-host`;
23. wrong-owner release is rejected;
24. correct-owner release succeeds;
25. actor identity change after lease logs stale identity;
26. scene cleanup calls release;
27. plugin shutdown calls release;
28. missing target source logs blocked;
29. missing target `Location.ID` logs blocked;
30. target id is formatted as `foa.location:<Location.ID>`;
31. marker requires actor source;
32. marker requires actor id and lease id;
33. marker requires target id;
34. marker requires default-off and kill-switch fields;
35. marker requires all unsupported gameplay fields set to `0`;
36. fixture runner prints the exact DK4 blocked or pass marker first.

## Required Marker Text

When the native actor or target source is still missing, the first marker line must be:

```text
DRAGON_KNIGHT_DK4_SOURCE_GATE_BLOCKED fixtures=36 owner=dragon-knight.dk4.diagnostic-host actor-role=dragon-knight.boss actor-source=blocked actor-location-id=blocked actor-id=blocked target-source=blocked target-location-id=blocked activation=F7 release=F6 default-off=1 kill-switch-default=1 native-spawn=0 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0
```

After blocker closure and live validation, the first successful marker line must be:

```text
DRAGON_KNIGHT_DK4_LIVE_ACTOR_OBSERVATION_PASS fixtures=36 owner=dragon-knight.dk4.diagnostic-host actor-role=dragon-knight.boss actor-source=<source> actor-location-id=<Location.ID> actor-id=foa.location:<Location.ID> target-source=<source> target-location-id=<Location.ID> target-id=foa.location:<Location.ID> lease=<lease-id> activation=F7 release=F6 default-off=1 kill-switch=0 cleanup=pass native-spawn=<0-or-1> goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0
```

## Build Plan

After source implementation and fixture creation:

```powershell
dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"
dotnet run --project mods\dragon-knight\tests\DragonKnight.DK4Diagnostic.Fixtures\DragonKnight.DK4Diagnostic.Fixtures.csproj -c Release
```

The source build must pass with zero errors. The fixture run must print the required blocked marker or pass marker first.

## Deploy Plan

Deployment is allowed only after the source build and fixtures pass:

```powershell
dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>" -p:DeployOnBuild=true
```

Expected deployed DLL:

`<local-path>`

Expected config:

`<local-path>`

## Live Log Checks

The live check must inspect:

`<local-path>`

Required load lines:

- `Loading [Dragon Knight`
- `DK4Diagnostic.Enabled=False`
- `DK4Diagnostic.KillSwitch=True`
- `DK4Diagnostic.AllowNativeSpawn=False`

Required blocked-run line before actor/template proof:

- `DRAGON_KNIGHT_DK4_SOURCE_GATE_BLOCKED`

Required pass-run line after actor/template/target proof:

- `DRAGON_KNIGHT_DK4_LIVE_ACTOR_OBSERVATION_PASS`

Forbidden live lines:

- `goals=1`
- `actions=1`
- `movement=1`
- `attacks=1`
- `phase-combat=1`
- `companion-protect=1`
- `items=1`
- `save=1`

## Cleanup Checks

The live cleanup validation must prove:

- F6 release logs `cleanup=manual-release`;
- F6 can be pressed twice without stale ownership;
- plugin shutdown logs `cleanup=plugin-shutdown` when a lease is active;
- scene cleanup logs `cleanup=scene-change` when a lease is active;
- actor destruction or discard logs `cleanup=actor-gone`;
- after cleanup, no active lease id remains;
- no persistent state is written;
- `AllowNativeSpawn=false` remains true in the default generated config.

## Stop Conditions

Stop immediately and do not continue validation if:

- the Dragon Knight actor source is missing;
- the Dragon Knight target source is missing;
- `AllowNativeSpawn` is true before template proof;
- `Location.ID` is missing or changes;
- cleanup cannot release the lease;
- any Rabbit, GOAP, PlayMaker, Avalon runtime, companion, awakened, movement, attack, item, save, or phase behavior is invoked;
- the game crashes;
- BepInEx logs an unhandled DK4 exception;
- any protected overhaul path would be touched.

## Explicitly Not Authorized

This packet does not authorize:

- Dragon Knight native template authoring;
- Dragon Knight native actor spawning before template proof;
- Boss AI goals/actions;
- Rabbit writes;
- GOAP source or policy;
- PlayMaker procedures;
- Blaze execution;
- native movement or interaction dispatch;
- attacks, damage, death, loot, corpse, rewards, roaming, or phase combat;
- companion mechanics;
- weapon or armor item registration;
- save writes or persistence.
