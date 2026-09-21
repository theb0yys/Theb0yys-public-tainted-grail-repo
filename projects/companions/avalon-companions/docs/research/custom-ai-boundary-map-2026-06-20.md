# Research: Custom AI Boundary Map

Date: 2026-06-20
Scope: inspect native follow, pathing, combat, and targeting boundaries before any Avalon Companions custom AI, attack command, target selector, taming, training, or loyalty feature.
Question: What can Avalon safely own for advanced companion behavior, and what must remain native or diagnostic-only?
Game version and branch: local Mono install; exact game version not revalidated in game. `TG.Main.dll` last modified 2026-06-17 02:27:09.
Tools used: `ilspycmd` 10.1.0.8386 against `<local-path>`; repo research review only. No game launch or runtime probe.

## Evidence read

- `codex/skills/tainted-grail-foa-system-research/SKILL.md`
- `codex/skills/tainted-grail-foa-system-research/references/system-research-packet.md`
- `codex/skills/tainted-grail-foa-modding/SKILL.md`
- `FORBIDDEN_FILES.md`
- `Research/Making Tainted Grail The Fall of Avalon Mods-deep-research-report.md`
- `docs/foa-modding-environment.md`
- `docs/research/README.md`
- `docs/mod-lifecycle.md`
- `docs/engineering-process.md`
- `docs/diagnostic-tool-development-policy.md`
- `mods/avalon-companions/docs/research/post-0.1.29-development-sequence-2026-06-20.md`
- `mods/avalon-companions/docs/research/panel-debug-native-dialogue-roadmap-2026-06-19.md`
- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/ActorControlDiagnostics.cs`

## Decompiled or inspected targets

- Assembly: `TG.Main.dll`
- Type: `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`
  - Notes: registers an `ICanMoveProvider`, builds a `Patrol` state around the ally, updates on a 2.5 second tick, stays within ally range using walk/trot/run patrol velocity changes, teleports through `AstarPath` and `NpcTeleporter` when too far, imports `Ally.PossibleAttackers` into the NPC possible-target relation, and lets native recalculation choose the actual target.
- Type: `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`
  - Notes: derives from `NpcAlly`, always updates, blocks extra movement while the hero has `IHeroInvolvement`, listens to portal, fast travel, and long-teleport events, removes search/pickpocket actions, prevents friendly fire in the summon path, and `EnterCombat()` removes summon invisibility before calling the inherited native target path.
- Type: `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`
  - Notes: derives from `NpcHeroSummon`, has `DestroyOnRest=false`, `CharacterLimitedLocationType.None`, and a one-per-character limit.
- Type: `Awaken.TG.Main.Locations.Pets.PetElement`
  - Notes: owns saved follow state and saved weak target reference; defaults to following `Hero.Current`, listens to target teleports, and exposes native `SetFollowing(bool)` and `Recall(Vector3)`.
- Type: `Awaken.TG.Main.Locations.Pets.VCPetController`
  - Notes: requires `RichAI`, uses `Seeker`, `AstarPath`, and `ARPetAnimancer`; controls follow distance, speed selection, turning, ground adjustment, and near-target teleport for native pets.
- Type: `Awaken.TG.Main.Character.ICharacter`
  - Notes: exposes `PossibleTargets` and `PossibleAttackers` two-way relation storage that native target utilities use.
- Type: `Awaken.TG.Main.Fights.AITargetingUtils`
  - Notes: owns current-target relations, possible target validation, path checks, target fit scoring, target recalculation, and `ForceEndCombat()`. Target validity checks faction intent, alive/discarded/unconscious state, invisibility/perception, radar range, path possibility, and hero visibility.
- Type: `Awaken.TG.Main.AI.NpcAI`
  - Notes: owns `InCombat`, `Working`, alert/visibility state, and `EnterCombatWith(ICharacter, bool)`. Combat entry calls native `ForceAddCombatTarget` and blocks unsafe cases such as self-targeting, unconscious targets, non-working AI, and hero-summon/hero friendly cases.
- Type: `Awaken.TG.Main.AI.TargetOverrideElement`
  - Notes: non-saved element that can provide a priority target ahead of normal target calculation and can force native combat entry when the override is active.
- Type: `Awaken.TG.Main.AI.HeroSummonTargetOverride`
  - Notes: non-saved summon-specific target override that only returns its target while active and within `NpcHeroSummon.InTripledAllyRange`; initialization can call `NpcHeroSummon.EnterCombat()`, and discard can try to exit combat.
- Type: `Awaken.TG.Main.AI.Movement.NpcMovement`
  - Notes: non-saved movement element with main and interrupt movement states; callers can change or reset the main movement state, but this is the same stack native AI uses.
- Type: `Awaken.TG.Main.Fights.NPCs.Providers.NpcCanMoveHandler`
  - Notes: non-saved provider list where any provider can block movement or destination override.
- Type: `Awaken.TG.Main.AI.Movement.States.Patrol`
  - Notes: movement state used by `NpcAlly`, with random destinations, `AstarPath` nearest-node snapping, and velocity scheme changes.
- Type: `Awaken.TG.Main.AI.Movement.States.FollowMovement`
  - Notes: native follow movement state exists, but it is not what the current `NpcHeroPetAlly` path uses for companions. It depends on `NpcCrimeReactions`, destination state, and movement-state lifecycle.

## Findings

### Documented

- Existing Avalon Companions research only approves the native-safe lane: plugin-owned one-session companions, native pet follow/recall where available, verified catch-up recall for creature candidates, native `NpcHeroPetAlly.EnterCombat()` when the hero already has live attackers, and audit diagnostics.
- The post-0.1.29 gate explicitly says custom AI remains research-only until native follow/pathing/combat/targeting boundaries are inspected and documented.
- Taming, training, and loyalty are downstream systems and still require command surface, actor ownership, persistence/state storage, and AI boundaries before implementation.

### Decompiled

- Native ally follow and combat are already coupled. `NpcAlly` owns patrol movement around the ally, distance-based teleport, possible-attacker import, and native target recalculation.
- Native hero summon behavior adds important safety conditions around hero dialogue, portal/fast-travel/long teleport, friendly fire, summon invisibility, and combat enter/exit state.
- Native target selection is relation-based and path-aware. `AITargetingUtils` filters by faction intent, perception, alive state, invisibility, range, path possibility, and hero visibility, then scores possible targets.
- `TargetOverrideElement` and `HeroSummonTargetOverride` are real native target override hooks, but they force native combat and therefore are not safe to use until runtime validation proves town, stealth, crime, quest, interior, and transition behavior.
- `NpcMovement.ChangeMainState(...)` and native movement states are available, but overriding movement directly would compete with native ally/pet logic and needs runtime evidence before any behavior change.
- `NpcCanMoveHandler` is a safer diagnostic boundary than direct movement override because it exposes whether native systems are already blocking movement or destination override.

### Runtime-observed

- No new runtime evidence was collected for this note.

### Diagnostic-confirmed

- No new diagnostic probe was run for this note.

### Inferred

- The first useful advanced step should be a read-only custom-AI boundary diagnostic, not custom AI behavior.
- A future diagnostic can sample the active managed companion's native AI state, current target, possible attacker/target counts, movement state name, native target override presence, `NpcCanMoveHandler` state, and scene/dialogue/combat context without changing behavior.

### Unknown

- Whether `HeroSummonTargetOverride` behaves safely for Avalon companions in towns, stealth, interiors, quests, scripted scenes, and transitions.
- Whether passive animal candidates have enough native combat setup to respond meaningfully even when target selection is valid.
- Whether direct `NpcMovement` state changes survive native AI state transitions without jitter, stuck actors, path churn, or command fights.
- Whether a future loyalty/training state should live in plugin config, a runtime-only memory model, Avalon Core diagnostic/profile records, or a later approved save-safe persistence layer.

## Implementation boundary

Allowed now:

- Document custom-AI boundaries.
- Design a default-off, read-only diagnostic probe that records native AI/movement/target state for active managed companions.
- Keep command execution inside the current native-safe lane.

Not allowed yet:

- Custom target selection.
- Attack command or target selector UI.
- Direct `HeroSummonTargetOverride` use.
- Direct `NpcMovement` overrides.
- Forced hostility or arbitrary nearby target scans.
- Taming, training, loyalty, healing, resurrection, squads, persistence, reload restoration, or actor re-adoption.

Requires more evidence:

- Runtime probe output during peaceful town state, combat, stealth, interior, transition, rest, quit/reload, and return.
- Throwaway-save lifecycle validation for one-session companions before any stateful progression system.
- A separate design for where progression state can live without claiming actor persistence.

## Decision

Research more before behavior. The next safe advanced slice should be a default-off custom-AI boundary diagnostic that writes evidence only. It should not dispatch commands, change movement, change targets, add progression state, or call Core-executed behavior.

Implementation note: 0.1.32 implements this default-off probe as `Diagnostics.WriteCompanionAiBoundaryDiagnostics`. Runtime evidence still has to be collected and reviewed before any custom AI, target override, taming, training, loyalty, persistence, or Core-executed behavior is allowed.

## Validation needed

- Build only if a diagnostic probe is implemented.
- BepInEx load validation only after code exists.
- In-game diagnostic validation on a throwaway save:
  - active managed companion idle in town,
  - active managed companion following outside town,
  - active managed companion while hero has attackers,
  - active managed companion during stealth or non-combat hostile proximity,
  - transition/rest/quit/reload/return with diagnostics enabled.
- Confirm diagnostic rows never report command execution, movement mutation, targeting mutation, persistence mutation, or Core-executed behavior.
