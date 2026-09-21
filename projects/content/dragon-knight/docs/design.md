# Dragon Knight Design Notes

## Scope

- Requested behavior: standalone Dragon Knight mod with a roaming boss, companion, weapon set, and armor set.
- Files expected to change in the first implementation pass: Dragon Knight loader source and gate documentation.
- Risk level: Medium for loader; High for visual/actor/item runtime.

## Architecture

Proven methods to adapt:

- `mods/avalon-companions`: companion command, lifecycle, not-saved actor, native assist, and validation methods.
- `mods/avalon-awakened`: external asset inventory, licence, isolated Unity authoring, cooked transport, hash, deployment, and missing-content methods.
- `mods/avalon-ai-runtime`: AI package/host, blackboard, planning, action-gateway, ownership lease, and fail-closed validation methods.

Dragon Knight remains standalone. The proven methods should be implemented in Dragon Knight's own lanes. These systems must not become compile-time references, runtime dependencies, package hosts, asset-pack owners, or shared actor owners until a later Dragon Knight-specific integration decision explicitly allows that exact boundary.

Planned gate order:

1. DK0 source, licence, and asset inventory.
2. DK1 standalone BepInEx loader build/load proof.
3. DK2 visual-only in-game asset transport proof for the Iron and Fire Dragon Knight weapon prefabs.
4. DK3 Dragon Knight AI package lane.
5. DK4A native actor/template proof profile and isolated template proof.
6. DK5 Dragon Knight-owned HOS placement observer and approved native `LocationSpawner` candidate placement after DK4A evidence exists.
7. DK6 bounded Dragon Knight AI goals/actions only after owned actor evidence exists.
8. DK7 follower mechanics.
9. DK8 usable weapon and armor item lane.
10. DK9 boss behavior, population, roaming, rewards, and release packaging only after the earlier gates pass.

## Feature Ownership

Boss:
- Must own its own proof profile and cleanup.
- Must not reuse Wyrd Hunt encounter budgets or Living Avalon route-patrol slots without a later explicit compatibility decision.
- Must use the approved DK5D native `LocationSpawner` candidate route for Ancient Cromlech production placement. The production route may hook the exact Cromlech-area native spawner candidate list, but must not call `LocationTemplate.SpawnLocation` directly, must not use DK4 temporary no-save actors, must not use Hollow Druid shrine activation, and must not use bear/resource slots as the Dragon Knight source anchor.

Ancient Cromlech encounter geometry:
- Boss title: `Sir Vaelor, the Ashen Dragon Knight`.
- Arena: Ancient Cromlech / Stonehenge in Horns of the South.
- Stage from a Dragon Knight-owned boss root separated from the Cromlech altar/center. `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844` is altar reference evidence only; the boss root is `-1795.209|80.243|-2915.928` with rotation `0|-140.441|0`. Do not use `AltarInteract`, `Spec_Discovery_Small_Stonecircle`, prompt visibility, discovery radius, or shrine activation as the trigger.
- The target scene is the existing Horns of the South scene `CampaignMap_HOS`; this is not a new map or region.
- The current placement record is `dragon-knight.vaelor.cromlech.center`, using `Spec_DragonKnight_DK4A` / `NPCTemplate_DragonKnight_DK4A` at boss root `-1795.209|80.243|-2915.928` with a `3m` placement tolerance.
- DK5D native source anchor: `CampaignMap_HOS_merged` `/SpawnerSingle_EnemyMonster_T1_Grindylow_01` at `-1860.15|73.81|-2958.34`, source template `Spec_EnemyMonster_T1_Grindylow` / `fa79aaa0bff59484dab2cf35c5ea805c`, evidence `template-diagnostics.20260803-035222.spawner_refs.csv:line341`. Dragon Knight is moved from the native source position to the boss root after native `BaseLocationSpawner` ownership is established.
- Outer wake radius: `25m`. This is presentation only: stand, turn, and ready/ignite the sword without health bar, attacks, movement pursuit, damage, AI goals, or phase behavior.
- Inner fight-start radius: `5m`. This is the deliberate central entry ring, equivalent to a 10m-wide circle.
- Arena containment: prefer a `22m` soft leash and `25m` hard leash so the boss stays inside the Stonehenge structure.
- Evidence: `docs/research/dk4-cromlech-encounter-geometry-2026-08-02.md`.

Companion:
- Must be a separate mode or actor contract from the boss.
- Must adapt Avalon Companions' proven lifecycle, command, native assist, and validation methods.
- Must not assume pet, summon, or humanoid companion behavior from other mods.

Weapons and armor:
- Must use a separate item/equipment lane.
- Must adapt Avalon Awakened's proven asset import and cooked-content validation methods.
- Must not treat visual prefabs as FoA equipment without native item/equipment template evidence.

Assets:
- Source archives remain read-only.
- No source FBX, Substance, Maya, Unity package, or publisher demo project should be shipped.
- Runtime packages, if later approved, must contain only reviewed compiled/cooked outputs and required metadata.

Visual phases:
- DK2 treats Iron as phase 1 visual and Fire as phase 2 visual.
- DK2 may instantiate these as visual-only prefabs from a Dragon Knight-owned AssetBundle.
- DK2 must disable visual-proof colliders at runtime and must not create a native actor.
- The base Dragon Knight package must not claim custom attacks or real phase combat because the imported controller has zero states and the only usable imported clip is `Pose_Check`.
- Custom attacks and phase-transition animation proof must use the supplied `MCB` boss move-set source, imported as Humanoid in an isolated Unity proof, then presented through a controller assigned to the Dragon Knight humanoid prefab.
- The one-handed Dragon Knight sword prefab remains the weapon source for this boss proof. `SM_Greatsword.fbx` from the move-set source is not part of the Dragon Knight one-handed proof.
- Phase 1 remains single sword. Phase 2 may present as dual-sword: Fire character-with-weapon as mainhand plus the Iron static Dragon Knight sword attached to the Fire phase `hand_l` bone as the offhand weapon.

AI:
- Must have a Dragon Knight AI package lane.
- Must adapt Avalon AI Runtime's package/host, blackboard, planning, action-gateway, lease, and fail-closed validation methods.
- Must not register a Dragon Knight AI package, host mapping, action gateway, or third-party AI runtime dependency without a Dragon Knight AI decision.
- AIR-49 authorizes and fixtures a source-only `AvalonAI.Contracts.V2` Boss AI package contract with Dragon Knight boss goals, actions, capabilities, session-only Rabbit schema, procedure requirements, and fail-closed authority checks.
- Dragon Knight `0.2.1` may validate the AIR-49 package boundary at loader startup, default-off, without live host mapping or runtime action dispatch.
- DK4A must prove or explicitly select the Dragon Knight native `NpcTemplate` and `LocationTemplate` source before any Dragon Knight source may call `LocationTemplate.SpawnLocation`.
- DK4 is documentation-only and requires DK4A plus a later default-off live diagnostic source gate to prove exact actor observation, `Location.ID`, target identity, host ownership, activation, and cleanup before any AI behavior is added.
- The production boss path must use the owned placement observer after the native candidate route creates the save-owned actor. The observer may prove `Location.ID` and actor ID for AI handoff, but it must not grant movement, attacks, phase combat, companion protection, items, or live AI dispatch by itself.

## Failure Behavior

Every future runtime slice must fail closed when:

- required local content is missing or hash mismatched;
- native template or item identity changes;
- game version or branch is unsupported;
- save-exclusion readback fails for temporary actors;
- cleanup or release cannot be proven;
- companion, boss, item, armor, or asset lane evidence is incomplete.

## Current Non-Goals

- No direct production `LocationTemplate.SpawnLocation` call from Dragon Knight plugin code.
- No claim of live production placement until the DK5D native candidate build/deploy/live marker is observed.
- No companion command UI.
- No Dragon Knight AI runtime binding.
- No item/equipment registration.
- No automatic Dragon Knight visual spawn.
- No runtime combat animation, hitbox, damage, or attack claim from the visual-only proof.
- No claim that dual-sword phase 2 is runtime combat-ready until a later runtime bridge proves weapon sockets, hitboxes, damage windows, animation events, and cleanup.
- No release package.
