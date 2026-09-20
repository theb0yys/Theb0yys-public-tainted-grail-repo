# Hook Catalogue

Hooks are only one segment of a complete mechanic. A method target does not prove owner, lifecycle, downstream success, cleanup or compatibility.

## Evidence-scoped private-corpus hook candidates

| Native target | Patch | Evidence state | Compatibility risk | Public use |
| --- | --- | --- | --- | --- |
| `LockpickingInteraction.ConsumePickHP(float)` | Prefix | Source inspected | High | Narrow lockpick durability guard |
| `Shop.OpenShop` | Prefix | Source inspected; owner runtime-unverified | High | Restock normal `RestockableStock` before shop UI |
| `VCCharacterMagicVFX.CastingBegun` | Postfix | Source inspected | High | Player-owned mod VFX overlay |
| `TemplatesLoader.set_FinishedLoading(bool)` | Postfix | Source inspected across several consumers | **Critical** | Retry/readiness boundary for template registration |
| concrete `CloudService.EndSave(string)` providers | Postfix | Source inspected + decompiled target research | High | Observe slot IDs; **not** generic durable-success semantics |

## Existing public working hook families

| System | Native surface | Working use |
| --- | --- | --- |
| Magic projectiles | projectile configuration / base-damage projectile setup | scale player-owned projectile speed/range/homing without replacing projectile ownership |
| Direct theft | `PickItemAction.OnStart` | guard illegal loose-world pickup while preserving native transfer |
| Container theft | `ContainerUI.TakeItemFromContainer`, `ContainerUI.TakeAllItems` | guard native container theft/take-all |
| Readable theft | `VReadablePopupUI.OnSteal` | guard the native readable steal action |
| Hero footsteps | `FMODManager.PlayOneShot(...)` filtered to `VHeroFootsteps` | replace only hero footstep playback |
| Character damage | `HealthElement.TakeDamage(Damage)` | observe completed character damage for UI/VFX/audio sidecars |
| Character death | `HealthElement.OnDeathEvents` | attach terminal character presentation/cleanup sidecars |

## Rules

- Patch the narrow native owner for the behaviour being changed.
- Preserve the original path unless the mod intentionally owns that calculation/action.
- Revalidate private/reflection targets after game updates.
- A hook firing is not terminal success.
