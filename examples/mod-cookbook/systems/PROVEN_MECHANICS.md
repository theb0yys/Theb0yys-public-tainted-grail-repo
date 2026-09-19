# Proven Mechanics Index

| Working implementation | Native owner / seam | Public cookbook page |
| --- | --- | --- |
| Magic Tweaks 0.1.0 | native magic projectile configuration/homing | [Magic projectile example](../04-magic-projectile-speed-mono/README.md) |
| Hold to Steal 0.2.1 | direct pickup, container transfer/take-all, readable steal | [Theft guard](../06-illegal-pickup-guard-mono/README.md) |
| Avalon Cheat Panel item browser | `TemplatesProvider → Item → World → HeroItems` | [Existing item grants](../gameplay/ITEM_GRANTS.md) |
| Tainted Economy vendor lane | `TradeUtils.Price` return value | [Vendor pricing](../gameplay/VENDOR_PRICING.md) |
| Better Bonfire Menu 0.6.3 | `VFireplaceUI` + native `FireplaceUI` services | [Bonfire services](../gameplay/BONFIRE_SERVICES.md) |
| Avalon Mounts 0.1.3 | `VMount.RunningVelocity`, `TurningVelocity` | [Mount velocity](../gameplay/MOUNT_VELOCITY.md) |
| Avalon Companions | reviewed `LocationTemplate`, `MarkedNotSaved`, `NpcHeroPetAlly` | [Companion lifecycle](../gameplay/COMPANION_LIFECYCLE.md) |
| Tainted Instincts | exact template gate + native aggression/sight/melee/cooldown/slot knobs | [NPC tuning](../gameplay/NPC_TUNING.md) |
| Wyrd Hunt fixed first target | exact Wyrdspirit native spawn/combat/death lifecycle | [Wyrdspirit encounter](../gameplay/WYRDSPIRIT_ENCOUNTER.md) |
| Views of Avalon 0.2.8 | active HDRP/FoA Volume fog components | [Fog control](../graphics/FOG_CONTROL.md) |
| Tainted Blood | character damage/death lifecycle + native VFX manager | [Damage/death VFX](../visual-effects/DAMAGE_DEATH_VFX.md) |
| Immersive Footsteps | `VHeroFootsteps` filtered FMOD one-shot | [Footsteps](../audio/FOOTSTEPS.md) |
| Avalon Cheat Panel UI | gameplay-result action receipts | [Action receipts](../ui-hud/ACTION_RECEIPTS.md) |
| Wyrd Decoy → Wyrd Hunt | small public API discovered at runtime | [Cross-mod API](CROSS_MOD_API.md) |

The cookbook grows from working mechanisms, not from speculative hook lists.
