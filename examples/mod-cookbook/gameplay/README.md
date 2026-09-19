# Gameplay Cookbook

## Magic

- [Magic projectile tuning](../04-magic-projectile-speed-mono/README.md) — player-owned projectile speed/range/homing pattern from Magic Tweaks.

## Interactions

- [Illegal pickup / theft guard](../06-illegal-pickup-guard-mono/README.md) — native theft actions with an additional authorization gate.

## Items and inventory

- [Existing item grants](ITEM_GRANTS.md) — `TemplatesProvider → Item → World → HeroItems`.

## Economy

- [Vendor price tuning](VENDOR_PRICING.md) — postfix on `TradeUtils.Price` while native trading stays intact.

## Bonfire services

- [Native bonfire services](BONFIRE_SERVICES.md) — route custom service menus into `FireplaceUI` actions.

## Mounts

- [Native mount velocity](MOUNT_VELOCITY.md) — `VMount.RunningVelocity` and `TurningVelocity`.

## Companions

- [One-session companion lifecycle](COMPANION_LIFECYCLE.md) — reviewed template spawn, `MarkedNotSaved`, `NpcHeroPetAlly`, bounded cleanup.

## NPC AI

- [Exact-target NPC tuning](NPC_TUNING.md) — exact native identity + narrow aggression/sight/damage/cooldown/slot changes.

## Encounters

- [Fixed native Wyrdspirit encounter](WYRDSPIRIT_ENCOUNTER.md) — exact profile, native spawn/combat/death lifecycle and cooldown.

## Combat lifecycle

- [Character damage observer](../23-character-damage-observer-mono/README.md)
- [Character death observer](../42-character-death-observer-mono/README.md)
