# Professional Spider Companion AI Design

Date: 2026-08-06

Status: design-only. This document does not authorize code edits, deployment, FoA launch, save access, direct forced jump playback, true native `VDialogue`, template edits, persistence, random spawning, or public packaging.

## Purpose

Define the intended professional behavior for the Broodmother companion as an intelligent spider predator:

- It should read the fight, not spam attacks.
- It should prefer pressure, angles, timing, and target commitment over constant melee contact.
- It should feel dangerous because it chooses when to close, feint, leap, retreat, and recover.
- It must stay inside the current companion and spider AI safety boundaries.

## Governing Evidence

- Broodmother companion rules require one active in-memory Broodmother, not-saved location handling, identity rejection, native ally conversion, and guarded defend behavior. See `mods/avalon-broodmother-companion/docs/design.md:18-24`.
- Current research records the spider animation boundary: ShortRange state `16`, two ShortRange attack clips, two jump-motion clips, no package direct native calls, and no save writes. See `mods/avalon-broodmother-companion/docs/research.md:16-18`.
- Current Broodmother implementation evidence says advanced spider companion AI targets native-recognized live combat candidates in priority order from hero attackers, Broodmother attackers, and Broodmother possible targets; it classifies ShortBite / LeapJump / Feint / Reposition / targeted native combat and uses native combat handoff plus `NpcHeroPetAlly.EnterCombat()`. See `mods/avalon-broodmother-companion/docs/research.md:47`.
- Live deployment, save access, live summon claims, direct forced jump playback, restoration, auto-respawn, and save-owned companion behavior remain blocked. See `mods/avalon-broodmother-companion/docs/research.md:39-48`.
- The spider package is session-only, default-off, kill-switch-default, and declares no direct native calls, native control, Rabbit writes, GOAP bypass, Blaze bypass, PlayMaker native calls, or save writes. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:9-34`.
- The package exposes observe, threat memory, role, positioning, pack pressure, telegraph, bite, leap, ShortRange 16, jump motion, hit, death, cleanup, and stop capabilities. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:75-96`.
- The host advertises the package boundary with `direct-native=0` and `save=0`. See `mods/avalon-awakened/host/src/AvalonAwakened.Spiders.Host.V1/AvalonAwakenedSpiderHostV1.cs:9-21`.
- The safe companion route is one-session, explicitly triggered, one active companion, duplicate-guarded, recallable, dismissible, and fail-closed. See `mods/avalon-awakened/docs/research/spider-broodmother-spell-companion-gate-packet-2026-08-06.md:45-53`.

## Personality Model

The companion should behave like a controlled apex predator:

- Patient: it holds when the release window is not ready.
- Opportunistic: it commits when the target is in a proven distance and angle envelope.
- Spatial: it avoids stacking and chooses flanking or ambush positions.
- Readable: it telegraphs before committable attacks.
- Defensive when hurt: it retreats after low health or repeated hits.
- Efficient: it finishes weak visible targets instead of wasting motion.

This is combat behavior only. It must not imply save ownership, autonomous world spawning, population behavior, route rows, scene integration, or persistent companion restoration.

## Runtime Boundaries

The AI design must obey these boundaries:

1. The package owns decision vocabulary only.
2. The host owns translation to bounded commands.
3. The Broodmother companion owns its one-session actor, ally conversion, recall, dismiss, and native-recognized live combat candidate handoff.
4. Native calls stay in the companion or host layer where already evidenced, not inside the AI package.
5. The package must remain session-only and save-free.
6. The direct `Spider_jump` and `Spider_jump_v2` clips stay motion-only until a native jump state is proven.
7. Any live validation must use the existing live-validation gate before making claims.

## Decision Loop

Each combat tick should follow this order:

1. Validate authority and lifecycle.
2. Observe the current native-recognized live combat candidate.
3. Update threat memory from sight, proximity, damage, noise, or last known position.
4. Choose role.
5. Select desired position and spacing.
6. Coordinate attack slots so the companion does not spam attacks.
7. Choose attack.
8. Telegraph before commit.
9. Commit only when release window, distance, angle, cooldown, and recovery checks pass.
10. Recover, reposition, retreat, or search if the target is not in a valid envelope.

## Roles

The package already defines the professional role set:

- `Harasser`: default pressure role, offset around the target at readable melee distance.
- `Flanker`: side-pressure role that creates leap opportunities.
- `Ambusher`: wide-angle role for behind/side positioning.
- `Retreating`: self-preservation role when low health or recently hit.
- `Finisher`: close-in role when the target is visible and low health.

Role evidence:

- Low health or repeated hits return `Retreating`.
- Low visible target health returns `Finisher`.
- Pack index rotates Harasser, Flanker, Ambusher, and Harasser. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:530-558`.

## Positioning

Positioning should make the spider feel intelligent before it attacks:

- Flanker: side angle around 75 degrees, roughly 3.25 meters.
- Ambusher: wide angle around 145 degrees, roughly 4.5 meters.
- Retreating: behind/out angle around 180 degrees, roughly 7.5 meters.
- Finisher: narrow aggressive angle around 25 degrees, roughly 2 meters.
- Harasser: default angle around 45 degrees, roughly 3 meters.
- Camera-center avoidance can shift the chosen angle when camera data is available.

The package already marks selected positions as avoiding stacking and optionally avoiding camera center. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:560-610`.

## Attack Slot Discipline

The spider should not attack every tick.

Attack permission depends on:

- active attackers being below the allowed max slots,
- no active attack cooldown,
- no recovery state,
- no already committed attack.

When an attack slot is denied, the spider should hold or reposition depending on local density. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:612-633`.

## Attack Selection

Attack choice must follow the existing package thresholds:

- `ShortBite`: target distance at or under 2.25 meters and target angle at or under 55 degrees.
- `LeapJump`: flanker role at or under 5.5 meters and at or under 100 degrees.
- `LeapJump`: non-flanker mid-range target at or under 6 meters and at or under 75 degrees.
- `Feint`: target at or under 4 meters when a telegraph has not already been emitted.
- `Reposition`: dense local space or bad attack envelope.
- `Hold`: attack slot denied, cooldown active, or recovery active.

Evidence: `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:635-687`.

## Telegraph And Commit

Committable attacks are only `ShortBite` and `LeapJump`.

Commit must fail or hold when:

- the attack is not committable,
- recovery or cooldown is active,
- no telegraph has happened,
- the release window is closed,
- the target has moved outside 6 meters or outside 90 degrees.

Only a valid release becomes a `ShortRange16Request`. Evidence: `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:689-725`.

## Custom Attack Design

### Short Bite

Purpose: close, readable melee punishment.

Animation policy:

- Use ShortRange state `16`.
- Use `Spider_Attack1_ShortRange_CI4` unless alternate attack policy chooses otherwise.

### Leap Jump

Purpose: intelligent predator close-in attack from flank or mid-range.

Animation policy:

- The chosen attack may request jump motion through `Spider_jump` or `Spider_jump_v2`.
- The actual hit must resolve through ShortRange state `16`.
- Leap uses `Spider_Attack2_ShortRange_CI4` for the ShortRange handoff.
- Do not directly force `Spider_jump` or `Spider_jump_v2` as a proven native attack state.

Evidence:

- Jump clips are declared as `Spider_jump` and `Spider_jump_v2`. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:57-60`.
- Leap resolves to the secondary ShortRange clip. See `mods/avalon-awakened/ai-package/src/AvalonAwakened.Spiders.AI.Package.V1/AvalonAwakenedSpiderAiV1Contract.cs:841-845`.
- The host translates `LeapJump` attack selection into a `JumpMotionRequest`. See `mods/avalon-awakened/host/src/AvalonAwakened.Spiders.Host.V1/AvalonAwakenedSpiderHostV1.cs:408-428`.
- The host translates valid attack commit into `ShortRange16Request`. See `mods/avalon-awakened/host/src/AvalonAwakened.Spiders.Host.V1/AvalonAwakenedSpiderHostV1.cs:466-482`.

### Feint

Purpose: make the spider feel intelligent before it strikes.

Behavior:

- Use when close but not in a clean attack envelope.
- Telegraph pressure without committing damage.
- Follow with reposition, ShortBite, or LeapJump on later ticks.

### Reposition

Purpose: maintain professional spacing.

Behavior:

- Use when crowded, off-angle, outside release envelope, or attack slot is not available.
- Prefer flanking and ambush angles over straight-line pursuit.

### Retreat

Purpose: preserve the companion and avoid mindless tanking.

Behavior:

- Use when health is low or repeated hits are observed.
- Increase desired distance and reset the attack rhythm.

## Companion Integration

For the Broodmother companion slice:

- Target source should be native-recognized live combat candidates only, prioritized from `Hero.PossibleAttackers`, Broodmother `NpcElement.PossibleAttackers`, then Broodmother `NpcElement.PossibleTargets`.
- Combat handoff may use the evidenced native target combat route and `NpcHeroPetAlly.EnterCombat()`.
- Follow/defend behavior stays inside the existing native companion route.
- Spawn, recall, dismiss, cleanup, and failure handling must preserve one active not-saved companion.
- No random spawn, world population, route row, scene hook, save restoration, auto-respawn, or public packaging belongs in this design.

Evidence: `mods/avalon-broodmother-companion/docs/design.md:18-24`, `mods/avalon-broodmother-companion/docs/research.md:37-48`, and `mods/avalon-awakened/docs/research/spider-broodmother-spell-companion-gate-packet-2026-08-06.md:45-53`.

## Native Dialogue Process

This section records the known working process for later design alignment. It does not authorize implementation in this document.

Approved companion dialogue route:

1. Use one runtime-only native `Companion` `AbstractLocationAction` on managed one-session companion actors.
2. Mark it not saved.
3. Attach it only to tracked managed companions that satisfy native-safe gates.
4. Open a plugin-owned Unity UI command surface from that action.
5. Route visible choices through approved companion command methods.

Evidence: `mods/avalon-companions/docs/research/native-dialogue-decision-gate-2026-06-21.md:24-41`.

Blocked native dialogue route:

- fake `StoryBookmark` values,
- runtime-authored story graphs,
- `DialogueAttachment`,
- `PetTalkAttachment`,
- `StoryInteractAction`,
- template edits,
- vanilla serialized interaction-list writes,
- true native `VDialogue` without a separate proof.

Evidence: `mods/avalon-companions/docs/research/native-dialogue-decision-gate-2026-06-21.md:43-58`.

Optional interop pattern:

- Avalon Companions exposes `RegisterDialogueCommand(ownerId, commandId, targetTemplateGuid, label, canExecute, execute)` and `UnregisterDialogueCommands(ownerId)`. See `mods/avalon-companions/src/AvalonCompanionsInteropApi.cs:6-30`.
- Registered external dialogue commands are filtered by target template GUID. See `mods/avalon-companions/src/Patches/PetCompanionController.ExternalDialogue.cs:13-47` and `mods/avalon-companions/src/Patches/PetCompanionController.ExternalDialogue.cs:81-94`.
- The dialogue UI inserts external commands into the right-side choice list. See `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs:80-148`.
- Avalon Mounts demonstrates an optional reflection bridge that retries registration and unregisters when the gate is not available. See `mods/avalon-mounts/src/WolfMount/AvalonCompanionsDialogueBridge.cs:9-129`.

For Broodmother, this means any later native dialogue work must be a separate authorized slice, with either:

- a direct runtime-only `AbstractLocationAction` on the active Broodmother, or
- an optional Avalon Companions interop bridge by reflection,

and must not create story graphs, bookmarks, template edits, save writes, or a hard dependency unless separately researched and approved.

## Acceptance Criteria Before Implementation

Before code changes, the implementation packet should specify:

1. Exact config names and defaults.
2. Exact live target source.
3. Exact target identity checks.
4. Exact attack tick cadence.
5. Exact max attack slot policy for one companion.
6. Exact log marker for attack selection.
7. Exact fallback when spatial target data is unavailable.
8. Exact validation fixtures to run.
9. Exact live FoA validation gate, if any live proof is authorized.
10. Exact dialogue route, if native dialogue work is included in the same later slice.

## Explicit Non-Goals

- Do not implement in this document.
- Do not deploy.
- Do not launch FoA.
- Do not write or load saves.
- Do not force raw jump clips as native attacks.
- Do not add random spawns.
- Do not alter Spider or Broodmother templates.
- Do not add persistence, restoration, or auto-respawn.
- Do not add true native `VDialogue` or story graph content.
- Do not make public packaging claims.
