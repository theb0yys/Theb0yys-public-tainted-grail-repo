# Hook Catalogue

This catalogue lists hook families used by working Tainted Grail mod paths in this handbook.

| System | Native surface | Working use |
| --- | --- | --- |
| Magic projectiles | projectile configuration / base-damage projectile setup | scale player-owned projectile speed/range/homing without replacing projectile ownership |
| Direct theft | `PickItemAction.OnStart` | guard illegal loose-world pickup while preserving native transfer |
| Container theft | `ContainerUI.TakeItemFromContainer`, `ContainerUI.TakeAllItems` | guard native container theft/take-all |
| Readable theft | `VReadablePopupUI.OnSteal` | guard the native readable steal action |
| Hero footsteps | `FMODManager.PlayOneShot(...)` filtered to `VHeroFootsteps` | replace only hero footstep playback |
| Character damage | `HealthElement.TakeDamage(Damage)` | observe completed character damage for UI/VFX/audio sidecars |
| Character death | `HealthElement.OnDeathEvents` | attach terminal character presentation/cleanup sidecars |

## Rule

Patch the narrow native owner for the behaviour you are changing. Preserve the original game path unless the mod intentionally owns that calculation or action.
