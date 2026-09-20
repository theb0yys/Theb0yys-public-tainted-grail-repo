# Beginner Guide Conversion Inventory

This file is the durable planning source for converting real mod work under `research/case-studies/` into beginner-facing public guides.

It covers every case study currently indexed by `research/case-studies/README.md` as of 2026-09-20: **55 entries across 31 domains**.

The inventory is evidence-driven. A classification describes only the scope actually supported by the linked case study. It does not promote adjacent, broader or unrun behaviour.

## Classification

| Classification | Meaning | Guide rule |
| --- | --- | --- |
| **PROVEN** | The case study records direct runtime, user, log, screenshot or equivalent proof for the declared narrow capability. | A beginner build guide may be authored for that proven scope. Preserve all stated limits. |
| **PARTIAL** | Useful implementation/static/runtime evidence exists, but important lifecycle, visual, compatibility, persistence or end-to-end validation remains incomplete. | A guide may teach the proven portion only. Unproven stages must be marked explicitly and must not be presented as completed. |
| **RESEARCH_ONLY** | The case establishes architecture, identity, ownership or a blocked boundary without a sufficiently proven implementation route. | Do not publish a beginner "build this" recipe until the missing evidence is promoted. Route readers to research instead. |
| **FAILURE_LESSON** | The primary value is a correction, negative result or ownership lesson rather than a build target. | Route into troubleshooting, investigation methods or golden rules instead of presenting it as a successful mod recipe. |

## Conversion state

- **NOT_STARTED** — eligible guide work has not started.
- **BLOCKED_BY_EVIDENCE** — beginner implementation guide is blocked until the case gains sufficient evidence.
- **ROUTE_TO_TROUBLESHOOTING** — convert the lesson into troubleshooting/golden-rule material rather than a build guide.
- **EXISTING_EXAMPLE** — an already-present runnable example can support the eventual guide; this does not by itself upgrade the case-study proof level.
- **GUIDE_CREATED** — the beginner guide exists at the recorded repository path; its evidence boundary remains the one recorded in this inventory.
- **TROUBLESHOOTING_CREATED** — the corrective/failure lesson has been converted into a troubleshooting page rather than a build recipe.

Paths prefixed with **planned:** are destinations only. Their presence in this inventory does not mean the file or example exists.

## Inventory

| # | Case study | Classification | Evidence boundary retained | Beginner-guide destination | Example destination | Conversion |
| ---: | --- | --- | --- | --- | --- | --- |
| 1 | [Stink and Burn: static identity is not a repair](../research/case-studies/bugfixes/stink-and-burn-static-boundary.md) | **RESEARCH_ONLY** | Several static identities resolved; embedded objective GUID/marker and stable world anchor unresolved; insufficient to write a repair. | Blocked; keep under quest-repair research until exact broken state/owner is proven. | N/A | **BLOCKED_BY_EVIDENCE** |
| 2 | [Patch notes can invalidate an old fix](../research/case-studies/bugfixes/current-build-before-fix.md) | **FAILURE_LESSON** | Historical fixes intentionally not implemented pending current-build reproduction. | `guides/troubleshooting/bugfixes/verify-current-build-first.md` | N/A | **TROUBLESHOOTING_CREATED** |
| 3 | [Discovery before mutation](../research/case-studies/frameworks/discovery-before-mutation.md) | **PROVEN** | Sample consumer proved dependency/load order, read-only trust/discovery/registration and fail-closed guardrails with zero mutation capability. | `guides/tasks/interoperability/build-a-read-only-framework-consumer.md` | existing: `examples/mono/infrastructure/core-readonly/` | **GUIDE_CREATED** |
| 4 | [Authenticated TGE live handshake](../research/case-studies/frameworks/tge-live-handshake.md) | **PROVEN** | Live installed-game probe proved authenticated transport/session/negative-control behaviour for the read-only identity service only. | `guides/tasks/interoperability/connect-to-tge-safely.md` | existing: `examples/hybrid/infrastructure/tge-authenticated-handshake/` | **GUIDE_CREATED** |
| 5 | [TGE owned encounter lifecycle](../research/case-studies/frameworks/tge-owned-encounter.md) | **PROVEN** | Live rerun proved allowlisted native spawn, initialized/living/visual state, exact owned cleanup, and two-actor composition expansion. | `guides/tasks/world/build-an-owned-native-encounter.md` | existing: `examples/hybrid/encounters/tge-owned-encounter/` | **GUIDE_CREATED** |
| 6 | [Combat pressure without raw damage multipliers](../research/case-studies/combat/pressure-not-damage.md) | **PARTIAL** | Design/implementation direction uses coordination and stamina levers; case study does not record an end-to-end runtime validation matrix. | `guides/tasks/gameplay/tune-combat-pressure.md` | planned: `examples/mono/combat/combat-pressure/` | **GUIDE_CREATED** |
| 7 | [Poise is not stagger](../research/case-studies/combat/poise-not-stagger.md) | **PARTIAL** | Decompilation establishes distinct poise/stagger semantics and the chosen poise-processing seam; runtime proof is not recorded here. | `guides/tasks/gameplay/tune-poise-safely.md` | planned: `examples/mono/combat/poise-damage-tuning/` | **GUIDE_CREATED** |
| 8 | [Crafting was already stash-aware](../research/case-studies/storage/crafting-already-stash-aware.md) | **FAILURE_LESSON** | Decompilation showed native crafting already combines inventory and stash; the actual opportunity is presentation clarity. | `guides/troubleshooting/gameplay/crafting-stash-counts.md` | N/A | **TROUBLESHOOTING_CREATED** |
| 9 | [Auto-unlock without bypassing every lock rule](../research/case-studies/lockpicking/auto-unlock-boundary.md) | **PARTIAL** | Safe ownership boundary is defined—skip only the active minigame and retain native eligibility/crime/unlock paths—but runtime proof is not recorded here. | `guides/tasks/gameplay/auto-unlock-with-native-gates.md` | planned: `examples/mono/gameplay/lockpicking-auto-unlock/` | **GUIDE_CREATED** |
| 10 | [Architecture before replacing native dialogue](../research/case-studies/dialogue/architecture-before-replacement.md) | **RESEARCH_ONLY** | Architecture was separated correctly, but implementation remained blocked by missing package fingerprints and native runtime hooks. | Blocked; retain under dialogue investigation until exact bindings/hooks are proven. | N/A | **BLOCKED_BY_EVIDENCE** |
| 11 | [Wyrd Route Patrol: three owners, one feature](../research/case-studies/encounters/wyrd-route-patrol-ownership.md) | **PARTIAL** | Ownership split between exposure, authorized route and owned actor is defined; case does not itself record a complete runtime route-patrol proof. | `guides/tasks/world/build-a-route-patrol-with-split-ownership.md` | planned: `examples/mono/gameplay/owned-route-patrol/` | **GUIDE_CREATED** |
| 12 | [Temporary native actor: prove cleanup, not only spawn](../research/case-studies/encounters/temporary-native-actor.md) | **PROVEN** | Accepted TGE lifecycle proof covers loading readiness, initialized/live/visual state and exact model/view destruction on removal. | `guides/tasks/world/spawn-and-clean-up-a-temporary-native-actor.md` | existing: `examples/hybrid/encounters/temporary-native-actor/` | **GUIDE_CREATED** |
| 13 | [Research before fast-travel mutation](../research/case-studies/travel/research-first.md) | **RESEARCH_ONLY** | Project deliberately remained a research scaffold; live mutation was blocked because ownership crosses many coupled systems. | Blocked; route to investigation methodology rather than a fast-travel recipe. | N/A | **BLOCKED_BY_EVIDENCE** |
| 14 | [Small exit helper instead of a dungeon map](../research/case-studies/travel/interior-exit-helper.md) | **PARTIAL** | Narrow sidecar architecture is defined from scene metadata, coordinates and passive overlay; runtime validation is not recorded here. | `guides/tasks/world/build-an-interior-exit-helper.md` | planned: `examples/mono/ui/interior-exit-helper/` | **GUIDE_CREATED** |
| 15 | [One-session native companion lifecycle](../research/case-studies/companions/native-companion-lifecycle.md) | **PROVEN** | In-game Qrko spawn path and managed creature lifecycle rows were proved with native ally ownership, not-saved posture and bounded cleanup. | `guides/tasks/creatures/build-a-one-session-native-companion.md` | existing: `examples/mono/gameplay/native-session-companion/` | **GUIDE_CREATED** |
| 16 | [Human native-ally proof](../research/case-studies/companions/native-human-ally-proof.md) | **PARTIAL** | One safe temporary human-ally class is scoped; persistence, arbitrary NPC conversion, recruitment, pathing and other systems remain explicitly blocked. | `guides/tasks/creatures/build-a-temporary-human-ally.md` | planned: `examples/mono/gameplay/temporary-human-ally/` | **GUIDE_CREATED** |
| 17 | [Carry tweak must follow the current stat instance](../research/case-studies/stats/carry-stale-tweak.md) | **PARTIAL** | Ownership bug and corrected reattachment strategy are established; case does not record a complete runtime/load-transition matrix. | `guides/tasks/gameplay/change-carry-capacity-safely.md` | planned: `examples/mono/gameplay/carry-capacity-tweak/` | **GUIDE_CREATED** |
| 18 | [Projectile route coverage and aim correction](../research/case-studies/magic/projectile-route-and-aim.md) | **PARTIAL** | Corrected later anchor, fallback and aim-component scaling are established; complete runtime route coverage is not asserted here. | `guides/tasks/gameplay/tune-projectile-speed-without-breaking-aim.md` | planned: `examples/mono/magic/projectile-speed-and-aim/` | **GUIDE_CREATED** |
| 19 | [Native bonfire services](../research/case-studies/gameplay/bonfire-services.md) | **PROVEN** | Better Bonfire Menu 0.6.3 was deployed and added/restored service entries and service grid were reported working in game. | `guides/tasks/ui/add-native-services-to-the-bonfire-menu.md` | existing: `examples/mono/ui/bonfire-native-services/` | **GUIDE_CREATED** |
| 20 | [Native mount velocity tuning](../research/case-studies/gameplay/mount-velocity.md) | **PROVEN** | Live 1.25 running/turning getter proof plus user smoke through mount, movement, transitions, save/load and quit/relaunch. | `guides/tasks/gameplay/change-mount-speed.md` | existing: `examples/mono/gameplay/mount-velocity/` | **GUIDE_CREATED** |
| 21 | [Exact-target NPC tuning](../research/case-studies/gameplay/npc-tuning.md) | **PROVEN** | Runtime-validated exact-enemy profiles; Drowner path included matching build, activation, user-confirmed behaviour and native-route preservation checks. | `guides/tasks/creatures/tune-one-native-enemy-profile.md` | existing: `examples/mono/gameplay/exact-npc-tuning/` | **GUIDE_CREATED** |
| 22 | [Vendor price tuning](../research/case-studies/gameplay/vendor-pricing.md) | **PROVEN** | Throwaway-save validation covered hero buy, hero sell, resale changes and disable/reload rollback while native transaction ownership remained intact. | `guides/tasks/gameplay/change-vendor-prices.md` | existing: `examples/mono/gameplay/vendor-price-postfix/` | **GUIDE_CREATED** |
| 23 | [Fixed native Wyrdspirit encounter](../research/case-studies/gameplay/wyrdspirit-encounter.md) | **PROVEN** | Live fixed-profile encounter validated fight/kill, leave/return and save/load for the tested path. | `guides/tasks/world/build-a-fixed-native-encounter.md` | existing: `examples/mono/gameplay/fixed-native-encounter/` | **GUIDE_CREATED** |
| 24 | [Fail-closed cross-mod API bridge](../research/case-studies/gameplay/cross-mod-api.md) | **PROVEN** | Optional Wyrd Decoy → Wyrd Hunt bridge confirmed by user report and log evidence, including public-contract discovery and transactional resource use. | `guides/tasks/interoperability/build-an-optional-cross-mod-api-bridge.md` | existing: `examples/mono/infrastructure/optional-cross-mod-api/` | **GUIDE_CREATED** |
| 25 | [Existing item grants](../research/case-studies/content/item-grants.md) | **PROVEN** | Avalon Cheat Panel uses native template/item/world/inventory ownership; live screenshot confirmed a single successful Grant receipt. | `guides/tasks/items/grant-an-existing-item.md` | existing: `examples/mono/items/grant-existing-item/` | **GUIDE_CREATED** |
| 26 | [Evil Greatsword: equip and presentation boundary](../research/case-studies/weapons/evil-greatsword-presentation.md) | **PARTIAL** | Runtime receipt proved identity/equip redirect/Drake prototype and resource serving; repeated lifecycle, simultaneous instances, scene restore and full visual acceptance remain unrun. | `guides/tasks/weapons/custom-rigid-weapon-presentation.md` | planned: `examples/mono/items/custom-rigid-weapon-presentation/` | **GUIDE_CREATED** |
| 27 | [Native clothes/Kandra static contract](../research/case-studies/armour/native-clothes-kandra-contract.md) | **RESEARCH_ONLY** | Static native Kandra clothes/stitching contract is mapped; custom armour runtime equip/deformation/unequip is explicitly unproven. | Blocked; no custom-armour beginner recipe until runtime lifecycle proof exists. | N/A | **BLOCKED_BY_EVIDENCE** |
| 28 | [Native horse velocity: Proof125](../research/case-studies/movement/native-horse-velocity-proof.md) | **PROVEN** | Live 1.25/1.25 getter proof and later smoke through movement, transition, save/load and relaunch; broader mount ownership excluded. | `guides/tasks/gameplay/change-mount-speed.md` | existing: `examples/mono/gameplay/mount-velocity/` | **GUIDE_CREATED** |
| 29 | [Perspective transition vs camera framing](../research/case-studies/camera/perspective-vs-framing.md) | **RESEARCH_ONLY** | Research distinguishes broad perspective transition from narrow Cinemachine framing, but this case records no runtime framing implementation proof. | Blocked for build recipe; use as camera ownership/reference material until a bounded framing path is proven. | N/A | **BLOCKED_BY_EVIDENCE** |
| 30 | [Sidecar pinbook instead of native map injection](../research/case-studies/map/mod-owned-pinbook.md) | **PARTIAL** | Sidecar ownership model is defined with native coordinates and plugin TSV persistence; case does not record complete runtime/UI/restore proof. | `guides/tasks/ui/build-a-sidecar-map-pinbook.md` | planned: `examples/mono/ui/sidecar-pinbook/` | **GUIDE_CREATED** |
| 31 | [Map fog: display without rewriting discovery memory](../research/case-studies/map/map-fog-display-only.md) | **PARTIAL** | Safe display-only boundary is established while native visited-pixel/save truth remains untouched; runtime proof is not recorded here. | `guides/tasks/ui/change-map-fog-display-only.md` | planned: `examples/mono/ui/map-fog-display/` | **GUIDE_CREATED** |
| 32 | [Visible talent group is not automatically a native proficiency](../research/case-studies/progression/visible-tree-vs-proficiency.md) | **PARTIAL** | Research rejects invented one-to-one native proficiencies; project uses a separate branch/overlay model, with no end-to-end runtime proof recorded here. | `guides/tasks/gameplay/build-a-separate-progression-overlay.md` | planned: `examples/mono/gameplay/progression-overlay/` | **GUIDE_CREATED** |
| 33 | [Session first, persistence later](../research/case-studies/survival/session-first.md) | **PARTIAL** | Session-only fatigue/overlay/non-saved-effects architecture is staged; persistence is deliberately deferred and runtime matrix is not recorded here. | `guides/tasks/gameplay/prototype-a-session-only-survival-system.md` | planned: `examples/mono/gameplay/session-survival/` | **GUIDE_CREATED** |
| 34 | [Merchant restock boundary](../research/case-studies/merchants/restock-boundary.md) | **PARTIAL** | Boundary/self-review exists, but in-game restock behaviour and unique-item non-duplication are explicitly unverified. | `guides/tasks/gameplay/tune-restockable-merchant-stock.md` | planned: `examples/mono/gameplay/merchant-restock/` | **GUIDE_CREATED** |
| 35 | [Vendor price: a narrow validated seam](../research/case-studies/economy/vendor-price-seam.md) | **PROVEN** | Core buy/sell throwaway-save validation and disable/reload rollback are recorded for the terminal price seam. | `guides/tasks/gameplay/change-vendor-prices.md` | existing: `examples/mono/gameplay/vendor-price-postfix/` | **GUIDE_CREATED** |
| 36 | [Container rules: post-roll and save-backed](../research/case-studies/economy/container-row-boundary.md) | **PARTIAL** | Save-backed ownership risk and bounded post-roll operations are defined; runtime/persistence validation is not recorded here. | `guides/tasks/gameplay/change-post-roll-container-contents.md` | planned: `examples/mono/gameplay/container-post-roll-rules/` | **GUIDE_CREATED** |
| 37 | [Native service reuse and submenu ownership](../research/case-studies/bonfire/native-service-reuse.md) | **PARTIAL** | Representative service flow and native Services entry worked; newest full native submenu still lacked input/layout/service-return validation. | `guides/tasks/ui/build-a-native-looking-bonfire-services-menu.md` | planned: `examples/mono/ui/bonfire-services-submenu/` | **GUIDE_CREATED** |
| 38 | [Preserve native bounty, extend semantic truth](../research/case-studies/crime/preserve-native-bounty.md) | **PARTIAL** | Native bounty/witness/reporting ownership is preserved; added incident/case semantics are still first-phase passive/session-only. | `guides/tasks/gameplay/extend-crime-semantics-without-replacing-bounty.md` | planned: `examples/mono/gameplay/crime-semantic-sidecar/` | **GUIDE_CREATED** |
| 39 | [Rain / Day owner-stack validation](../research/case-studies/weather/rain-day-owner-stack.md) | **PROVEN** | Accepted live run proved the bounded Weather → Skybox/Water ownership handshake; other weather families, restore paths and full visuals remain outside this proof. | `guides/tasks/rendering/build-a-bounded-weather-consumer-stack.md` | existing: `examples/mono/rendering/weather-owner-stack/` | **GUIDE_CREATED** |
| 40 | [Action receipts for mod UI](../research/case-studies/ui/action-receipts.md) | **PROVEN** | Live screenshot showed one item Grant completing through gameplay ownership and a completed UI receipt. | `guides/tasks/ui/report-action-results-with-receipts.md` | existing: `examples/mono/ui/action-receipts/` | **GUIDE_CREATED** |
| 41 | [Companion dialogue input ownership](../research/case-studies/ui/companion-dialogue-input.md) | **FAILURE_LESSON** | Case is primarily a correction of an input-dispatch mismatch between IMGUI/debug patterns and Unity UI button ownership. | `guides/troubleshooting/ui/modal-ui-opens-but-buttons-do-not-fire.md` | N/A | **TROUBLESHOOTING_CREATED** |
| 42 | [Inventory suite: presentation without a second inventory](../research/case-studies/ui/inventory-truth-boundary.md) | **PARTIAL** | Static ownership and first read-only source slice are established; mutation and runtime host/focus/cleanup proof remain separate. | `guides/tasks/ui/build-a-read-only-inventory-projection.md` | planned: `examples/mono/ui/inventory-readonly-projection/` | **GUIDE_CREATED** |
| 43 | [Native HUD ownership vs custom visual proof](../research/case-studies/ui/hud-owner-and-visual-proof.md) | **PARTIAL** | Native visibility patch route exists; multiple custom visual versions had build/deploy/resource proof but incomplete in-game visual acceptance. | `guides/tasks/ui/customize-the-hud-with-separate-system-and-visual-proof.md` | planned: `examples/mono/ui/hud-visibility-and-theme/` | **GUIDE_CREATED** |
| 44 | [Native hero footstep replacement](../research/case-studies/audio/footsteps.md) | **PROVEN** | Runtime path loaded custom audio, installed exact scoped patch and logged 57 custom plays while native footstep suppression was enabled. | `guides/tasks/audio/replace-hero-footsteps.md` | existing: `examples/mono/audio/footstep-replacement/` | **GUIDE_CREATED** |
| 45 | [Tainted Music: own your lane, not all audio](../research/case-studies/audio/tainted-music-lane-ownership.md) | **PARTIAL** | Ownership architecture is established, but the private matrix retained audible-validation gaps for overlap/transitions/exceptions. | `guides/tasks/audio/build-a-contextual-music-mod.md` | planned: `examples/mono/audio/contextual-music/` | **GUIDE_CREATED** |
| 46 | [Wrong lifecycle seam: target resolution regression](../research/case-studies/vfx/target-resolution-regression.md) | **FAILURE_LESSON** | User-reported regression traced to a semantically wrong target-resolution hook; stable correction moved presentation to damage/death owners. | `guides/troubleshooting/rendering/when-a-vfx-hook-breaks-gameplay.md` | N/A | **TROUBLESHOOTING_CREATED** |
| 47 | [Telemetry without blame](../research/case-studies/performance/telemetry-without-blame.md) | **PARTIAL** | Diagnostic epistemic model is defined—presence/correlation/controlled isolation/causality remain distinct—but runtime completeness is not asserted here. | `guides/tasks/diagnostics/build-non-causal-performance-telemetry.md` | planned: `examples/mono/infrastructure/performance-telemetry/` | **GUIDE_CREATED** |
| 48 | [HDRP / FoA fog control](../research/case-studies/rendering/fog-control.md) | **PROVEN** | Views of Avalon 0.2.8 was feature-tested; user screenshots showed major fog/visibility change through active FoA/HDRP owners. | `guides/tasks/rendering/control-world-fog-through-the-active-hdrp-owner.md` | existing: `examples/mono/rendering/hdrp-fog-control/` | **GUIDE_CREATED** |
| 49 | [World fog: correcting the owner](../research/case-studies/rendering/hdrp-fog-owner-correction.md) | **FAILURE_LESSON** | Sequence records failed/incomplete ownership assumptions and the eventual correction to active game-owned HDRP Volume/Fog owners. | `guides/troubleshooting/rendering/fog-setting-applies-but-screen-does-not-change.md` | N/A | **TROUBLESHOOTING_CREATED** |
| 50 | [Damage and death VFX sidecars](../research/case-studies/rendering/damage-death-vfx.md) | **PROVEN** | Working Tainted Blood stable path uses character damage and terminal death owners while removing the target-resolution regression from the stable route. | `guides/tasks/rendering/add-damage-and-death-vfx-sidecars.md` | existing: `examples/mono/rendering/damage-death-vfx/` | **GUIDE_CREATED** |
| 51 | [Combat VFX sidecars](../research/case-studies/rendering/combat-vfx.md) | **PARTIAL** | Complete owner-preserving pattern is documented, but this generic case does not itself record a separate runtime validation receipt. | `guides/tasks/rendering/build-a-bounded-combat-vfx-sidecar.md` | planned: `examples/mono/rendering/combat-vfx-sidecar/` | **GUIDE_CREATED** |
| 52 | [Native save-domain boundary](../research/case-studies/persistence/native-save-domain-boundary.md) | **FAILURE_LESSON** | Static evidence found no supported arbitrary mutable native save-domain registrar for the inspected build; architecture was corrected toward a sidecar. | `guides/troubleshooting/saving/do-not-invent-a-native-save-domain.md` | N/A | **TROUBLESHOOTING_CREATED** |
| 53 | [Native save completion observer](../research/case-studies/persistence/native-save-completion-observer.md) | **PARTIAL** | Concrete `CloudService.EndSave(string)` observation seam exists; durable-success semantics, arbitrary domains, restoration timing and custom serialization are not proven. | `guides/tasks/saving/observe-native-save-completion.md` | planned: `examples/mono/infrastructure/save-completion-observer/` | **GUIDE_CREATED** |
| 54 | [Smart backups without more save slots](../research/case-studies/persistence/smart-backup-boundary.md) | **PARTIAL** | Sidecar archive architecture is selected, but actual backup archive creation remained unproven in the cited private validation. | `guides/tasks/saving/build-sidecar-save-backups.md` | planned: `examples/mono/infrastructure/smart-save-backup/` | **GUIDE_CREATED** |
| 55 | [Native-owner-first modding pattern](../research/case-studies/failures-and-fixes/native-owner-first.md) | **PROVEN** | Cross-case synthesis is grounded in multiple working mods using result adjustment, guarded native actions and lifecycle sidecars. | `guides/learning-paths/native-owner-first.md` | N/A; examples should link to the specific proven cases instead of duplicating one generic project. | **GUIDE_CREATED** |

## Promotion rules

A row may be upgraded only when repository evidence supports the higher state.

### PARTIAL → PROVEN

Require evidence for the exact beginner-guide scope. Depending on the feature this may include:

- runtime load/activation;
- expected native owner/hook identity;
- intended behaviour observed in game;
- cleanup/release;
- repeated lifecycle where ownership is reference-counted or stateful;
- scene/load transition where relevant;
- persistence where the feature claims persistence;
- visual/audible acceptance where output is presentation;
- rollback/disable behaviour where mutation is reversible;
- compatibility/version scope.

Do not require irrelevant evidence merely to satisfy a generic checklist.

### RESEARCH_ONLY → PARTIAL or PROVEN

Resolve the explicitly blocked identity/owner/lifecycle question first. Static, runtime, save and visual/audio evidence remain separate lanes and must not substitute for each other.

### FAILURE_LESSON

Do not promote a negative/corrective lesson into a build recipe just because a nearby working mod exists. Link the correction to the working mechanic or guide that owns the successful path.

## Guide authoring contract

When conversion begins, each beginner guide should:

1. state exactly what the reader will build;
2. name prerequisites and runtime lane;
3. link to canonical `knowledge/` owners instead of duplicating system truth;
4. explain the minimum ownership/lifecycle model needed for the task;
5. build the smallest working implementation first;
6. distinguish required code from optional expansion;
7. preserve the case-study evidence boundary;
8. show cleanup, failure and rollback paths where relevant;
9. include a concrete in-game verification checklist;
10. link back to the originating case study for evidence/history;
11. link to runnable `examples/` source when one exists;
12. never present planned/unrun behaviour as proven.

The intended author journey is:

```text
I want to build X
→ understand the native owner
→ build the smallest proven X
→ verify it in game
→ understand cleanup/persistence/compatibility
→ package it
→ follow research links only when going beyond the proven route
```

This inventory is planning/governance state. It does not itself upgrade the evidence status of any system, mechanic, example or case study.
