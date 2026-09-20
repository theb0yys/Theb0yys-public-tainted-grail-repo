# Proven Mechanics Index

| Working implementation | Native owner / seam | Public cookbook page |
| --- | --- | --- |
| Magic Tweaks 0.1.0 | native magic projectile configuration/homing | [Magic projectile example](../examples/mono/magic-projectile-speed/README.md) |
| Hold to Steal 0.2.1 | direct pickup, container transfer/take-all, readable steal | [Theft guard](../examples/mono/illegal-pickup-guard/README.md) |
| Avalon Cheat Panel item browser | `TemplatesProvider → Item → World → HeroItems` | [Existing item grants](content/item-grants.md) |
| Tainted Economy vendor lane | `TradeUtils.Price` return value | [Vendor pricing](gameplay/vendor-pricing.md) |
| Better Bonfire Menu 0.6.3 | `VFireplaceUI` + native `FireplaceUI` services | [Bonfire services](gameplay/bonfire-services.md) |
| Avalon Mounts 0.1.3 | `VMount.RunningVelocity`, `TurningVelocity` | [Mount velocity](gameplay/mount-velocity.md) |
| Avalon Companions | reviewed `LocationTemplate`, `MarkedNotSaved`, `NpcHeroPetAlly` | [Companion lifecycle](companions/native-companion-lifecycle.md) |
| Tainted Instincts | exact template gate + native aggression/sight/melee/cooldown/slot knobs | [NPC tuning](gameplay/npc-tuning.md) |
| Wyrd Hunt fixed first target | exact Wyrdspirit native spawn/combat/death lifecycle | [Wyrdspirit encounter](gameplay/wyrdspirit-encounter.md) |
| Views of Avalon 0.2.8 | active HDRP/FoA Volume fog components | [Fog control](rendering/fog-control.md) |
| Tainted Blood | character damage/death lifecycle + native VFX manager | [Damage/death VFX](rendering/damage-death-vfx.md) |
| Immersive Footsteps | `VHeroFootsteps` filtered FMOD one-shot | [Footsteps](audio/footsteps.md) |
| Avalon Cheat Panel UI | gameplay-result action receipts | [Action receipts](ui/action-receipts.md) |
| Wyrd Decoy → Wyrd Hunt | small public API discovered at runtime | [Cross-mod API](gameplay/cross-mod-api.md) |

The cookbook grows from working mechanisms, not from speculative hook lists.
