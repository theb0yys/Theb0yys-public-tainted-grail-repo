# Gameplay Cookbook

Gameplay covers rules and behaviours that change what the player, items, merchants, NPCs or world systems actually do.

## Player and traversal

| Pattern | Evidence ceiling | Public execution |
| --- | --- | --- |
| [01 Stamina drain](../01-stamina-drain-mono/README.md) | SOURCE_BUILD_EVIDENCED | NOT_RUN |
| [02 Carry capacity](../02-carry-capacity-mono/README.md) | LOAD_EVIDENCED | NOT_RUN |
| [03 No fall damage](../03-no-fall-damage-mono/README.md) | SOURCE_BUILD_EVIDENCED | NOT_RUN |
| [08 Extra airborne jump](../08-extra-air-jump-mono/README.md) | LOAD_EVIDENCED | NOT_RUN |
| [26 Move/sprint speed](../26-move-sprint-speed-mono/README.md) | LOAD_EVIDENCED | NOT_RUN |
| [Skill caps and progression](../recipes/01-skill-caps/README.md) | LOAD_EVIDENCED underlying path | NOT_RUN |

## Magic and combat

### Runtime-backed mechanisms

- [04 Magic projectile speed](../04-magic-projectile-speed-mono/README.md) — underlying Magic Tweaks projectile-speed/range/homing mechanism has feature-tested runtime precedent; exact public rewrite remains **NOT_RUN**.
- [23 Character damage observer](../23-character-damage-observer-mono/README.md) — the character damage seam has working runtime precedent in damage/VFX owners; exact public rewrite remains **NOT_RUN**.

### Source/build/load candidates

- [09 Mana cost](../09-mana-cost-mono/README.md)
- [10 Magic damage](../10-magic-damage-mono/README.md)
- [13 Status buildup](../13-status-buildup-mono/README.md)
- [37 Combat-state observer](../37-combat-state-observer-mono/README.md)
- [38 Guard/block/parry observer](../38-guard-block-parry-observer-mono/README.md)
- [39 Attack/cast action observer](../39-attack-cast-action-observer-mono/README.md)
- [40 Poise-break observer](../40-poise-break-observer-mono/README.md)
- [41 Stagger observer](../41-stagger-observer-mono/README.md)
- [42 Character death observer](../42-character-death-observer-mono/README.md) — the underlying `HealthElement.OnDeathEvents` seam has runtime precedent in a working death/VFX owner, but this public rewrite is **NOT_RUN**.

Poise and stagger remain research candidates. Do not promote them to working recipes until their exact behaviour is validated.

## Items, inventory and equipment

Current public examples are primarily observation/research examples:

- [31 Consumable-use observer](../31-consumable-use-observer-mono/README.md)
- [32 Healing/recovery observer](../32-healing-recovery-observer-mono/README.md)
- [33 Status cure/removal observer](../33-status-cure-observer-mono/README.md)
- [34 Equipment-change observer](../34-equipment-change-observer-mono/README.md)
- [35 Main/off-hand observer](../35-hand-item-observer-mono/README.md)
- [36 Weapon visibility/state observer](../36-weapon-visibility-state-observer-mono/README.md)
- [Consumable effect attribution](../recipes/11-consumable-effect-attribution/README.md)
- [Equipment lifecycle attribution](../recipes/12-equipment-lifecycle-attribution/README.md)

A **native existing-item grant** pattern has working runtime precedent in the maintainer corpus and is listed in the [proven mechanics index](../systems/PROVEN_MECHANICS.md). A clean-room public implementation has not yet been published.

## Statuses and character state

- [28 Status-application observer](../28-status-application-observer-mono/README.md)
- [29 Active-status observer](../29-active-status-observer-mono/README.md)
- [30 Character-state observer](../30-character-state-observer-mono/README.md)
- [Buff/debuff tuning evidence gate](../recipes/10-buff-debuff-tuning/README.md)

These remain research/source-build material unless a working owner or exact runtime validation proves the behaviour being taught.

## Economy and merchants

Existing examples:

- [11 Merchant gold floor](../11-merchant-gold-floor-mono/README.md)
- [12 Restock on shop open](../12-merchant-restock-on-open-mono/README.md)

The maintainer corpus also contains a **validated vendor-price lane** with buy-side, sell-side, common-gear resale and rollback evidence. That is a stronger future public pattern than the current merchant examples; see the [proven mechanics index](../systems/PROVEN_MECHANICS.md).

## Interaction and theft

- [06 Modifier-gated illegal pickup](../06-illegal-pickup-guard-mono/README.md) — underlying Hold to Steal 0.2.1 behaviour was feature-tested for direct, container, take-all and readable theft paths; exact public rewrite remains **NOT_RUN**.

A newer native hold-prompt implementation exists in the maintainer workspace but does not inherit the older feature proof automatically.

## Camp services

The maintainer corpus has working runtime feedback for native bonfire service routing. A clean-room public camp-service pattern has not yet been published.

## Companions, mounts and NPC AI

These are **advanced** cookbook domains because actor ownership, save exclusion, lifecycle cleanup, movement/AI authority and compatibility matter.

Runtime-backed owner mechanisms exist for:

- one-session companion spawning/lifecycle;
- native mount running/turning velocity tuning;
- exact-target NPC tuning that preserves unrelated native behaviour.

They are indexed in [Proven mechanics](../systems/PROVEN_MECHANICS.md). They should be published as advanced patterns, not compressed into beginner one-file snippets.

## Content authoring

Merlin Workshop authoring examples remain separate from BepInEx runtime code:

- [Item stats](../content/01-item-stats/README.md)
- [Weapon](../content/02-weapon/README.md)
- [Armour](../content/03-armour/README.md)
- [Creature/NPC](../content/04-creature/README.md)

Their current ceiling is **STATIC_CONFIRMED** and the exact public recipes remain **NOT_RUN**.
