# Weapon Pipeline Validation Matrix

| Gate | Required proof |
| --- | --- |
| Source profile | Exact native archetype and build scope recorded |
| Custom identity | Unique GUID/name; native source unchanged |
| Registration | Custom ItemTemplate resolves normally |
| Runtime Item | Normal Item ownership established |
| Equip | ItemEquip → CharacterHandBase path observed |
| Unequip | Detach/discard/release completes |
| Drake presentation | Custom mesh/material served by correct owner |
| Resource lifetime | Repeated equip/unequip has no leak/stale ownership |
| Combat | Native attack/sweep/hit path works |
| Animation/audio | Required events remain intact |
| FPP | First-person consumer verified |
| TPP | Third-person consumer verified |
| Preview | Inventory/equipment preview verified |
| Scene transitions | Presentation/equip recovers correctly |
| Persistence | Cold save/load if claimed |
| Missing package | Explicit behaviour if claimed |
| Migration | Upgrade path if claimed |
| Compatibility | Claimed build/runtime combinations tested |

No row may be inferred from another row.
