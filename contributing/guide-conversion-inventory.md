# Beginner Guide Conversion Inventory

This file tracks where the repository's known Tainted Grail modding information is surfaced for readers.

It covers the 55 case studies currently indexed by research/case-studies/README.md as of 2026-09-20.

This is **not a grading or evidence-status system**. Its job is routing:

- working implementation information belongs in a beginner guide;
- corrections and failure patterns belong in troubleshooting;
- topics without a concrete implementation path stay in research/canonical system documentation;
- runnable examples are listed only when the example actually exists.

Do not add speculative "planned example" destinations here. Add an example path only after the source exists in examples/.

## Inventory

| # | Case study | Public destination | Existing runnable example | Surface |
| ---: | --- | --- | --- | --- |
| 1 | [Stink and Burn: static identity is not a repair](../research/case-studies/bugfixes/stink-and-burn-static-boundary.md) | No build guide; keep the known information in the linked research/canonical system pages until a concrete implementation path is known. | N/A | Research only |
| 2 | [Patch notes can invalidate an old fix](../research/case-studies/bugfixes/current-build-before-fix.md) | `guides/troubleshooting/bugfixes/verify-current-build-first.md` | N/A | Troubleshooting |
| 3 | [Discovery before mutation](../research/case-studies/frameworks/discovery-before-mutation.md) | `guides/tasks/interoperability/build-a-read-only-framework-consumer.md` | existing: `examples/mono/infrastructure/core-readonly/` | Guide |
| 4 | [Authenticated TGE live handshake](../research/case-studies/frameworks/tge-live-handshake.md) | `guides/tasks/interoperability/connect-to-tge-safely.md` | existing: `examples/hybrid/infrastructure/tge-authenticated-handshake/` | Guide |
| 5 | [TGE owned encounter lifecycle](../research/case-studies/frameworks/tge-owned-encounter.md) | `guides/tasks/world/build-an-owned-native-encounter.md` | existing: `examples/hybrid/encounters/tge-owned-encounter/` | Guide |
| 6 | [Combat pressure without raw damage multipliers](../research/case-studies/combat/pressure-not-damage.md) | `guides/tasks/gameplay/tune-combat-pressure.md` | existing: `examples/mono/combat/combat-pressure/` | Guide |
| 7 | [Poise is not stagger](../research/case-studies/combat/poise-not-stagger.md) | `guides/tasks/gameplay/tune-poise-safely.md` | existing: `examples/mono/combat/poise-damage-tuning/` | Guide |
| 8 | [Crafting was already stash-aware](../research/case-studies/storage/crafting-already-stash-aware.md) | `guides/troubleshooting/gameplay/crafting-stash-counts.md` | N/A | Troubleshooting |
| 9 | [Auto-unlock without bypassing every lock rule](../research/case-studies/lockpicking/auto-unlock-boundary.md) | `guides/tasks/gameplay/auto-unlock-with-native-gates.md` | existing: `examples/mono/gameplay/lockpicking-auto-unlock/` | Guide |
| 10 | [Architecture before replacing native dialogue](../research/case-studies/dialogue/architecture-before-replacement.md) | No build guide; keep the known information in the linked research/canonical system pages until a concrete implementation path is known. | N/A | Research only |
| 11 | [Wyrd Route Patrol: three owners, one feature](../research/case-studies/encounters/wyrd-route-patrol-ownership.md) | `guides/tasks/world/build-a-route-patrol-with-split-ownership.md` | existing: `examples/mono/gameplay/owned-route-patrol/` | Guide |
| 12 | [Temporary native actor: prove cleanup, not only spawn](../research/case-studies/encounters/temporary-native-actor.md) | `guides/tasks/world/spawn-and-clean-up-a-temporary-native-actor.md` | existing: `examples/hybrid/encounters/temporary-native-actor/` | Guide |
| 13 | [Research before fast-travel mutation](../research/case-studies/travel/research-first.md) | No build guide; keep the known information in the linked research/canonical system pages until a concrete implementation path is known. | N/A | Research only |
| 14 | [Small exit helper instead of a dungeon map](../research/case-studies/travel/interior-exit-helper.md) | `guides/tasks/world/build-an-interior-exit-helper.md` | existing: `examples/mono/ui/interior-exit-helper/` | Guide |
| 15 | [One-session native companion lifecycle](../research/case-studies/companions/native-companion-lifecycle.md) | `guides/tasks/creatures/build-a-one-session-native-companion.md` | existing: `examples/mono/gameplay/native-session-companion/` | Guide |
| 16 | [Human native-ally proof](../research/case-studies/companions/native-human-ally-proof.md) | `guides/tasks/creatures/build-a-temporary-human-ally.md` | existing: `examples/mono/gameplay/temporary-human-ally/` | Guide |
| 17 | [Carry tweak must follow the current stat instance](../research/case-studies/stats/carry-stale-tweak.md) | `guides/tasks/gameplay/change-carry-capacity-safely.md` | existing: `examples/mono/gameplay/carry-capacity-tweak/` | Guide |
| 18 | [Projectile route coverage and aim correction](../research/case-studies/magic/projectile-route-and-aim.md) | `guides/tasks/gameplay/tune-projectile-speed-without-breaking-aim.md` | existing: `examples/mono/magic/projectile-speed/` | Guide |
| 19 | [Native bonfire services](../research/case-studies/gameplay/bonfire-services.md) | `guides/tasks/ui/add-native-services-to-the-bonfire-menu.md` | existing: `examples/mono/ui/bonfire-native-services/` | Guide |
| 20 | [Native mount velocity tuning](../research/case-studies/gameplay/mount-velocity.md) | `guides/tasks/gameplay/change-mount-speed.md` | existing: `examples/mono/gameplay/mount-velocity/` | Guide |
| 21 | [Exact-target NPC tuning](../research/case-studies/gameplay/npc-tuning.md) | `guides/tasks/creatures/tune-one-native-enemy-profile.md` | existing: `examples/mono/gameplay/exact-npc-tuning/` | Guide |
| 22 | [Vendor price tuning](../research/case-studies/gameplay/vendor-pricing.md) | `guides/tasks/gameplay/change-vendor-prices.md` | existing: `examples/mono/gameplay/vendor-price-postfix/` | Guide |
| 23 | [Fixed native Wyrdspirit encounter](../research/case-studies/gameplay/wyrdspirit-encounter.md) | `guides/tasks/world/build-a-fixed-native-encounter.md` | existing: `examples/mono/gameplay/fixed-native-encounter/` | Guide |
| 24 | [Fail-closed cross-mod API bridge](../research/case-studies/gameplay/cross-mod-api.md) | `guides/tasks/interoperability/build-an-optional-cross-mod-api-bridge.md` | existing: `examples/mono/infrastructure/optional-cross-mod-api/` | Guide |
| 25 | [Existing item grants](../research/case-studies/content/item-grants.md) | `guides/tasks/items/grant-an-existing-item.md` | existing: `examples/mono/items/grant-existing-item/` | Guide |
| 26 | [Evil Greatsword: equip and presentation boundary](../research/case-studies/weapons/evil-greatsword-presentation.md) | `guides/tasks/weapons/custom-rigid-weapon-presentation.md` | existing: `examples/mono/items/custom-rigid-weapon-presentation/` | Guide |
| 27 | [Native clothes/Kandra static contract](../research/case-studies/armour/native-clothes-kandra-contract.md) | No build guide; keep the known information in the linked research/canonical system pages until a concrete implementation path is known. | N/A | Research only |
| 28 | [Native horse velocity: Proof125](../research/case-studies/movement/native-horse-velocity-proof.md) | `guides/tasks/gameplay/change-mount-speed.md` | existing: `examples/mono/gameplay/mount-velocity/` | Guide |
| 29 | [Perspective transition vs camera framing](../research/case-studies/camera/perspective-vs-framing.md) | No build guide; keep the known information in the linked research/canonical system pages until a concrete implementation path is known. | N/A | Research only |
| 30 | [Sidecar pinbook instead of native map injection](../research/case-studies/map/mod-owned-pinbook.md) | `guides/tasks/ui/build-a-sidecar-map-pinbook.md` | existing: `examples/mono/ui/sidecar-pinbook/` | Guide |
| 31 | [Map fog: display without rewriting discovery memory](../research/case-studies/map/map-fog-display-only.md) | `guides/tasks/ui/change-map-fog-display-only.md` | existing: `examples/mono/ui/map-fog-display/` | Guide |
| 32 | [Visible talent group is not automatically a native proficiency](../research/case-studies/progression/visible-tree-vs-proficiency.md) | `guides/tasks/gameplay/build-a-separate-progression-overlay.md` | existing: `examples/mono/gameplay/progression-overlay/` | Guide |
| 33 | [Session first, persistence later](../research/case-studies/survival/session-first.md) | `guides/tasks/gameplay/prototype-a-session-only-survival-system.md` | existing: `examples/mono/gameplay/session-survival/` | Guide |
| 34 | [Merchant restock boundary](../research/case-studies/merchants/restock-boundary.md) | `guides/tasks/gameplay/tune-restockable-merchant-stock.md` | existing: `examples/mono/gameplay/merchant-restock/` | Guide |
| 35 | [Vendor price: a narrow validated seam](../research/case-studies/economy/vendor-price-seam.md) | `guides/tasks/gameplay/change-vendor-prices.md` | existing: `examples/mono/gameplay/vendor-price-postfix/` | Guide |
| 36 | [Container rules: post-roll and save-backed](../research/case-studies/economy/container-row-boundary.md) | `guides/tasks/gameplay/change-post-roll-container-contents.md` | existing: `examples/mono/gameplay/economy-runtime-rules/` | Guide |
| 37 | [Native service reuse and submenu ownership](../research/case-studies/bonfire/native-service-reuse.md) | `guides/tasks/ui/build-a-native-looking-bonfire-services-menu.md` | existing: `examples/mono/ui/bonfire-native-services/` | Guide |
| 38 | [Preserve native bounty, extend semantic truth](../research/case-studies/crime/preserve-native-bounty.md) | `guides/tasks/gameplay/extend-crime-semantics-without-replacing-bounty.md` | existing: `examples/mono/gameplay/crime-semantic-sidecar/` | Guide |
| 39 | [Rain / Day owner-stack validation](../research/case-studies/weather/rain-day-owner-stack.md) | `guides/tasks/rendering/build-a-bounded-weather-consumer-stack.md` | existing: `examples/mono/rendering/weather-owner-stack/` | Guide |
| 40 | [Action receipts for mod UI](../research/case-studies/ui/action-receipts.md) | `guides/tasks/ui/report-action-results-with-receipts.md` | existing: `examples/mono/ui/action-receipts/` | Guide |
| 41 | [Companion dialogue input ownership](../research/case-studies/ui/companion-dialogue-input.md) | `guides/troubleshooting/ui/modal-ui-opens-but-buttons-do-not-fire.md` | N/A | Troubleshooting |
| 42 | [Inventory suite: presentation without a second inventory](../research/case-studies/ui/inventory-truth-boundary.md) | `guides/tasks/ui/build-a-read-only-inventory-projection.md` | existing: `examples/mono/ui/inventory-projection/` | Guide |
| 43 | [Native HUD ownership vs custom visual proof](../research/case-studies/ui/hud-owner-and-visual-proof.md) | `guides/tasks/ui/customize-the-hud-with-separate-system-and-visual-proof.md` | existing: `examples/mono/ui/hud-visibility/` | Guide |
| 44 | [Native hero footstep replacement](../research/case-studies/audio/footsteps.md) | `guides/tasks/audio/replace-hero-footsteps.md` | existing: `examples/mono/audio/footstep-replacement/` | Guide |
| 45 | [Tainted Music: own your lane, not all audio](../research/case-studies/audio/tainted-music-lane-ownership.md) | `guides/tasks/audio/build-a-contextual-music-mod.md` | existing: `examples/mono/audio/contextual-music/` | Guide |
| 46 | [Wrong lifecycle seam: target resolution regression](../research/case-studies/vfx/target-resolution-regression.md) | `guides/troubleshooting/rendering/when-a-vfx-hook-breaks-gameplay.md` | N/A | Troubleshooting |
| 47 | [Telemetry without blame](../research/case-studies/performance/telemetry-without-blame.md) | `guides/tasks/diagnostics/build-non-causal-performance-telemetry.md` | existing: `examples/mono/infrastructure/performance-telemetry/` | Guide |
| 48 | [HDRP / FoA fog control](../research/case-studies/rendering/fog-control.md) | `guides/tasks/rendering/control-world-fog-through-the-active-hdrp-owner.md` | existing: `examples/mono/rendering/hdrp-fog-control/` | Guide |
| 49 | [World fog: correcting the owner](../research/case-studies/rendering/hdrp-fog-owner-correction.md) | `guides/troubleshooting/rendering/fog-setting-applies-but-screen-does-not-change.md` | N/A | Troubleshooting |
| 50 | [Damage and death VFX sidecars](../research/case-studies/rendering/damage-death-vfx.md) | `guides/tasks/rendering/add-damage-and-death-vfx-sidecars.md` | existing: `examples/mono/rendering/damage-death-vfx/` | Guide |
| 51 | [Combat VFX sidecars](../research/case-studies/rendering/combat-vfx.md) | `guides/tasks/rendering/build-a-bounded-combat-vfx-sidecar.md` | existing: `examples/mono/rendering/damage-death-vfx/` | Guide |
| 52 | [Native save-domain boundary](../research/case-studies/persistence/native-save-domain-boundary.md) | `guides/troubleshooting/saving/do-not-invent-a-native-save-domain.md` | N/A | Troubleshooting |
| 53 | [Native save completion observer](../research/case-studies/persistence/native-save-completion-observer.md) | `guides/tasks/saving/observe-native-save-completion.md` | existing: `examples/mono/infrastructure/save-completion-observer/` | Guide |
| 54 | [Smart backups without more save slots](../research/case-studies/persistence/smart-backup-boundary.md) | `guides/tasks/saving/build-sidecar-save-backups.md` | existing: `examples/mono/infrastructure/smart-save-backup/` | Guide |
| 55 | [Native-owner-first modding pattern](../research/case-studies/failures-and-fixes/native-owner-first.md) | `guides/learning-paths/native-owner-first.md` | N/A; examples should link to the specific proven cases instead of duplicating one generic project. | Guide |

## Public authoring rule

When turning known modding information into public material:

1. state the exact thing the reader is building;
2. name the runtime lane and prerequisites;
3. name the real FoA/BepInEx owners, types, methods, fields, IDs and hooks that are known;
4. show the smallest working implementation path first;
5. separate required mechanics from optional expansion;
6. show cleanup, failure and rollback behavior where applicable;
7. state only real known limits or unknowns—do not invent missing steps;
8. include a concrete in-game test procedure when the task is runnable;
9. link to the originating case study/canonical system page for deeper history;
10. link to examples/ only when runnable source actually exists.

The intended reader path is:

~~~text
I want to build X
→ find the exact native owner/hook
→ implement the known working path
→ test it in game
→ add cleanup/persistence/compatibility only where the real implementation needs them
→ package it
~~~

The public repository should teach **known Tainted Grail modding processes**, not speculative designs or a dump of unfinished ideas.
