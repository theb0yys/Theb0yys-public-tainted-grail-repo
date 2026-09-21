# Mod Project Example Catalogue

This catalogue maps the non-importer, non-protected mod projects from the development workspace into public examples.

**Inventory snapshot:** 101 projects included in this pass.

These entries are examples of real project structure, design decisions, integration patterns, diagnostics, and failure boundaries. They are **not** a claim that every project is release-ready, supported on every runtime, or safe to copy wholesale.

The public repository keeps small runnable examples under [Mono](../mono/README.md), [IL2CPP](../il2cpp/README.md), and [Hybrid](../hybrid/README.md). This catalogue answers a different question: **what larger real projects exist, and what can a mod author learn from each one?**

Importer projects are intentionally deferred from this catalogue. Their end-to-end processes are handled separately and are not modified in this pass.

Two protected overhaul projects are also excluded from this pass.

## Starter-template coverage

The **Public starter** column links the 31 selected cross-runtime starters that already exist under [Project Templates](../../templates/README.md). A blank entry means the project is represented here as an example but does not currently have a dedicated public starter.

## Foundations, frameworks, and tooling

| Project | What it demonstrates | Public starter |
| --- | --- | --- |
| `Tainted Interface` | Shared semantic UI assets, styles, catalogues, HUD helpers, and custom-UI integration. | [Starter](../../templates/mod-families/tainted-interface/) |
| `Tainted-Diagnostic Tool` | Read-only runtime evidence collection for templates, items, recipes, actors, routes, scenes, and shared-framework diagnostics. | [Starter](../../templates/mod-families/tainted-diagnostic-tool/) |
| `aetherborne` | Reusable aerial-traversal framework separating intent, deterministic movement, FoA adaptation, and presentation. |  |
| `avalon-ai-runtime` | Package-driven shared AI runtime with a single FoA host and explicit package/host boundaries. | [Starter](../../templates/mod-families/avalon-ai-foa-host/) |
| `avalon-contracts` | Cross-mod provider discovery, readback, validation, and explicitly supported lifecycle contracts. |  |
| `avalon-core` | Shared trust, capability discovery, evidence lookup, and bounded planning/authorization metadata. | [Starter](../../templates/mod-families/tainted-core/) |
| `avalon-core-dependency-probe` | Test-only example for BepInEx dependency order and Avalon Core discovery/compatibility checks. |  |
| `avalon-creature-companion-shared-source` | Shared source used by standalone one-session creature companion projects. |  |
| `avalon-exceptions` | The Last Menhir: local-first crash, freeze, exception, and support-reporting infrastructure. |  |
| `avalon-progression-core` | Provider registry and read-only progression definition/state contracts for shared progression systems. |  |
| `foa-mod-manager` | Shared in-game BepInEx settings, controller actions, status rows, cursor/input scope, and mod UI management. | [Starter](../../templates/mod-families/foa-mod-manager/) |
| `tainted-framework` | Cross-runtime abstractions and shared runtime-service host with deliberately limited public services. |  |
| `tainted-grail-extender` | Managed extension host and authenticated local SDK bridge for advanced integrations. |  |
| `template-diagnostics` | Development/runtime evidence dumper for templates, actors, routes, audio ownership, and other game-system research. |  |
| `tg-haf` | Source-only animation/action framework foundation with fail-closed contracts before later runtime work. |  |

## Gameplay, balance, progression, and quality of life

| Project | What it demonstrates | Public starter |
| --- | --- | --- |
| `Avalon Stash` | Stash-aware crafting information, native storage access, and stash-summary UI without replacing native storage. | [Starter](../../templates/mod-families/avalon-stash/) |
| `Tainted Combat` | Preset-driven combat director covering stamina pressure, enemy pressure, parry/block tuning, poise/stagger, and world scaling. | [Starter](../../templates/mod-families/tainted-combat/) |
| `TaintedEconomy` | Systemic economy, vendor, reward, crafting, harvest, and loot tuning with narrow live mutation gates. |  |
| `always-show-hud` | HUD visibility/persistence pattern for keeping selected native HUD information available. | [Starter](../../templates/mod-families/immersive-hud/) |
| `avalon-cheat-panel` | Configurable player tuning plus an in-game tools/action panel built on bounded native routes. | [Starter](../../templates/mod-families/avalon-cheat-panel/) |
| `avalon-difficulty-director` | One shared difficulty profile coordinating combat damage, fall damage, and kill-XP pacing. |  |
| `better-bonfire-menu` | Native-looking bonfire Services entry and submenu that reuses existing game service screens. | [Starter](../../templates/mod-families/better-bonfire-menu/) |
| `carry-weight-tweaks` | Runtime carry-capacity override through the researched encumbrance stat rather than item-weight edits. | [Starter](../../templates/mod-families/carry-weight-tweaks/) |
| `crime-and-consequences` | Extends native theft, bounty, guard pressure, fencing, stealth, and jail consequences without replacing the crime system. |  |
| `dungeon-exit-helper` | Lightweight interior exit marker that remembers the entrance without implementing a dungeon minimap. | [Starter](../../templates/mod-families/dungeon-exit-marker/) |
| `easy-avalon` | Small player-involved damage multiplier mod for calmer, longer fights. | [Starter](../../templates/mod-families/easy-avalon/) |
| `hold-to-steal` | Hold-interact safety layer for theft-prone pickup and take-all actions while preserving legal pickup. | [Starter](../../templates/mod-families/hold-to-steal/) |
| `immersive-backgrounds` | Rebalances and previews prologue background/playstyle rewards through fixed roleplay presets. | [Starter](../../templates/mod-families/immersive-backgrounds/) |
| `immersive-progression` | Progression observation/tuning scaffold for action-, rest-, training-, and context-driven advancement. | [Starter](../../templates/mod-families/immersive-progression/) |
| `jump-higher` | Extra-air-jump/high-jump example with native jump-height reuse and bounded runtime state. | [Starter](../../templates/mod-families/jump-higher/) |
| `lockpicking-reforged` | Configurable lockpicking presets including gated auto-unlock while preserving key/lockpick requirements. | [Starter](../../templates/mod-families/lockpicking-reforged/) |
| `magic-tweaks` | Runtime tuning for projectile movement, casting, mana costs, damage, cooldowns, area size, and status buildup. | [Starter](../../templates/mod-families/magic-tweaks/) |
| `merchant-stock-tweaks` | Restocks normal merchant inventory through the game's own restock path before the shop UI opens. | [Starter](../../templates/mod-families/merchant-stock-tweaks/) |
| `missing-pebbles` | Adds configurable Small Pebble drops to corpse/search and mining routes. |  |
| `multi-pin-map-notes` | Plugin-owned map pinbook with naming, management UI, HUD directions, and sidecar persistence. | [Starter](../../templates/mod-families/multi-pin-map-notes/) |
| `no-fall-damage` | Minimal native fall-damage interception for the player. | [Starter](../../templates/mod-families/no-fall-damage/) |
| `no-map-fog` | Presentation-only map fog removal without rewriting exploration/discovery save data. |  |
| `origins-of-avalon` | New-game origin selection, starting kits, burdens, heirlooms, and per-hero application records. | [Starter](../../templates/mod-families/origins-of-avalon/) |
| `rich-merchant` | Cross-runtime merchant gold-floor example using a shared framework dependency. | [Starter](../../templates/mod-families/rich-merchant/) |
| `smart-save-backups` | Copies existing save-slot data into plugin-owned backup archives without adding visible save slots. |  |
| `stamina-action-control` | Separately scales sprint and combat/action stamina drain. | [Starter](../../templates/mod-families/stamina-control/) |
| `tainted-bugfixes` | Conservative diagnostics/recovery toolkit with backups, previews, preflights, and repair planning. |  |
| `tainted-lockpick` | Custom runtime lockpick template plus conditional unbreakable-lockpick behavior. |  |
| `tainted-survival` | Survival-lite fatigue, preparation, wounds, weather/night, and Wyrd pressure with conservative safety gates. |  |
| `theB0yysSkillUncapper` | Per-skill configurable caps above vanilla limits while retaining conservative caps for risky skills. |  |
| `wyrdflight` | Player-flight plugin using the Aetherborne motor and native movement/collision checks. |  |

## Presentation, audio, camera, and environment

| Project | What it demonstrates | Public starter |
| --- | --- | --- |
| `Immersive UI` | Inventory/equipment presentation overhaul design that preserves native inventory/equipment truth. |  |
| `avalon-spell-vfx` | Supplemental HDRP projectile and impact VFX while retaining native spell movement/collision/lifecycle. |  |
| `base-weapon-trails` | Read-only native weapon-trail ownership probe before any trail replacement work. |  |
| `brighter-torches` | Held-torch light range/intensity tuning with an optional bounded helper light. |  |
| `creature-combat-sfx-proof` | Exact actor/weapon-keyed audio sidecars that preserve native FMOD playback. |  |
| `first-person-plus` | Read-only body-aware first-person camera/weapon visibility research probe. |  |
| `immersive-footsteps` | Surface-aware hero footstep replacement using embedded audio and fail-open native playback. | [Starter](../../templates/mod-families/immersive-footsteps/) |
| `immersive-water` | Reversible HDRP WaterSurface tuning and optional exact-surface custom material binding. |  |
| `last-breath` | Bounded spectral death presentation triggered after native NPC death events. |  |
| `tainted-blood` | Native-first blood, splat, decal, volumetric-impact, and corpse-pool presentation. |  |
| `tainted-music` | IL2CPP contextual music layer with explicit arbitration against native music. | [Starter](../../templates/mod-families/tainted-music/) |
| `tainted-performance` | IL2CPP frame/performance monitoring dashboard and local report generation. | [Starter](../../templates/mod-families/tainted-performance/) |
| `tainted-skybox` | Consumes Tainted Weather sky plans and owns local HDRI/cubemap sky rendering and cleanup. |  |
| `tainted-weather` | Weather-state, Weather Maker integration, audio, visual-effects, scheduler, and public weather-plan API work. |  |
| `true-third-person` | Comfort-focused third-person camera, contextual profiles, traversal feel, and guarded directional movement. |  |
| `views-of-avalon` | Visibility, distance-culling, camera, terrain, and HDRP fog tuning plus bounded evidence probes. |  |
| `weapons-magic-sfx` | Weapon/magic audio sidecars and audition tooling while preserving native sound playback. |  |
| `world-action-sfx` | Exact world-action audio sidecars; current door route intentionally fails closed on stale build fingerprints. |  |
| `world-material-conversion` | Isolated Unity authoring pipeline for HDRP water and multi-layer terrain material conversion. |  |
| `world-texture-replacements` | Scoped world-material replacement and cooked PBR terrain texture/material authoring routes. |  |
| `wyrd-whispers` | Read-only Wyrd pressure presentation through generated murmurs, cues, pulses, and optional HUD feedback. |  |

## Creatures, companions, AI, and world actors

| Project | What it demonstrates | Public starter |
| --- | --- | --- |
| `avalon-bear-companion` | Standalone Avalon Awakened bear companion example using the one-session companion family. |  |
| `avalon-broodmother-companion` | Spider/Broodmother one-session companion with native ally lifecycle, command UI, call items, and bounded combat integration. |  |
| `avalon-bull-companion` | Standalone Avalon Awakened bull companion using the shared one-session creature-companion source. |  |
| `avalon-companions` | Research-gated pet/creature companion foundation, roster, commands, lifecycle guards, and diagnostics. | [Starter](../../templates/mod-families/avalon-companions/) |
| `avalon-dragon-companion` | Standalone Avalon Awakened dragon companion using the shared one-session creature-companion source. |  |
| `avalon-elephant-companion` | Standalone Avalon Awakened elephant companion using the shared one-session creature-companion source. |  |
| `avalon-goblin-companion` | Standalone Avalon Awakened goblin companion using the shared one-session creature-companion source. |  |
| `avalon-human-companions` | One-session human companion baseline with native ally behavior, command surfaces, and recruitment/capture research diagnostics. | [Starter](../../templates/mod-families/avalon-human-companions/) |
| `avalon-mounts` | Diagnostics-first mount research plus tightly gated native velocity and wolf-seat prototype work. |  |
| `avalon-summons` | Summon-control and summon-progression work, including shared progression-provider integration. |  |
| `avalon-vampire-companion` | Standalone Avalon Awakened vampire companion using the shared one-session creature-companion source. |  |
| `avalon-wolf-companion` | Diagnostic-only wolf companion candidate project; no actor mutation in its baseline scope. |  |
| `bandit-outlaw` | Standalone outlaw actor/AI project with separate runtime and AI-package structure. |  |
| `dragon-knight` | Standalone boss/content project combining imported visuals, native template proof, placement research, AI package boundaries, weapons, and armour lanes. |  |
| `dragon-knight-companion` | One-session Dragon Knight companion that reuses Dragon Knight visual content and native ally mechanics. |  |
| `living-avalon` | Research-gated road-life system for route evidence, travellers, patrols, and bounded encounter lanes. |  |
| `tainted-instincts` | Research-gated hostile-creature awareness/AI profiles with exact-template ownership boundaries. |  |
| `tainted-npcs` | Custom conversation scaffold with plugin-owned text UI, backend contracts, skill checks, and read-only native dialogue observation. |  |
| `wyrd-decoy` | Wyrd Hunt companion mod demonstrating cross-mod APIs, scent suppression, optional item consumption, and guarded recipe work. |  |
| `wyrd-hunt` | Wyrd pressure/hunt system with bounded encounter profiles, reward evidence, and AI package integration. | [Starter](../../templates/mod-families/wyrd-hunt/) |

## Items, weapons, and authored content

| Project | What it demonstrates | Public starter |
| --- | --- | --- |
| `beautiful-tattoos` | Character-creator tattoo content/package pipeline. |  |
| `evil-greatsword-moveset` | Custom Greatsword consumer for the Tainted Weapons registrar/presentation path plus vendor acquisition. |  |
| `food-drink-asset-proof` | Scoped cooked-AssetBundle food/drink load proof plus bounded native/custom shop-stock experiments. |  |
| `realistic-longsword` | Custom Longsword item/package experiment using native template cloning and a cooked weapon bundle. |  |
| `tainted-gems` | Embedded gem AssetBundle, world placement, and shop-stock content example. |  |

## Research, prototypes, recovery, and planned projects

| Project | What it demonstrates | Public starter |
| --- | --- | --- |
| `avalon-coop-prototype` | Session-only local Player 2 research prototype with strict ownership and validation gates. |  |
| `avalon-flight` | Superseded flight experiment retained as a record of why Wyrdflight/Aetherborne replaced the earlier route. |  |
| `do-not-use-unless-you-want-pain` | Inert-by-default exception generator used only to validate Avalon Exceptions reporting. |  |
| `hello-avalon` | Minimal first BepInEx plugin: load, config, hotkey, and logging without game patches. |  |
| `oh-merry-men-fix` | Quest-specific recovery scaffold; current-build research required before mutation. |  |
| `over-my-dead-bodies-fix` | Quest-specific recovery scaffold retained separately from shared quest infrastructure. |  |
| `stink-and-burn-fix` | Research-documented Stink and Burn augmentation scaffold with implementation still blocked. |  |
| `tainted-coop` | Planned narrow online co-op project distinct from the larger local co-op prototype. |  |
| `tainted-travel` | Travel-system research/design project with boundaries, native-target mapping, and validation planning. |  |

## How to use this catalogue

Use a project row as a **pattern pointer**, not as authority to copy every implementation detail. Start from the public guide or runnable example for the mechanism you need, then use the project name to understand which larger mod proved or explored that pattern.

Projects marked as diagnostic, planned, superseded, research-gated, or protected should be read with that limitation intact.
