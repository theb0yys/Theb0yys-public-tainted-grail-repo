# Avalon Taming Tool Set — 2026-08-19

## Document control

- Owner: `mods/tainted-weapons/`
- Canonical branch: `tainted-weapons`
- Scope: exact native item identity and registration for one Taming Bow and one Taming Arrow
- Source implementation: `IMPLEMENTED_SOURCE`
- Build validation: `NOT_RUN`
- Runtime registration: `NOT_RUN`
- Inventory/equip/fire validation: `NOT_RUN`
- Companion acquisition logic: `NOT_APPLICABLE`

## One system, one truth

Tainted Weapons owns the item and weapon truth for this pair:

- source-item selection;
- custom item GUIDs and names;
- native `ItemTemplate` cloning;
- native template registration;
- preservation of bow, ammunition, icon, visual and projectile components;
- immutable registration receipts.

Avalon Companions may request and consume the registered identities. It does not clone, rename, register, catalogue or redefine the items.

## Exact reviewed source pair

### Bow source

```text
source internal id: Weapon_Bow_Tier1_Light_Shortbow
source item:        Shortbow
required profile:   non-abstract, visible, droppable, IsShortBow=true
```

The resolver accepts only the exact reviewed internal ID or a deterministic native prefix variant ending in that exact ID. Ambiguous matches fail closed.

### Arrow source

```text
source internal id: Weapon_Ammo_Tier0_WoodenArrow
source item:        Wooden Arrow
required profile:   non-abstract, visible, droppable, IsArrow=true,
                    ItemProjectile component present
```

The resolver accepts only the exact reviewed internal ID or a deterministic native prefix variant ending in that exact ID. Ambiguous matches fail closed.

## Registered identities

### Taming Bow

```text
custom GUID:          b45ee5b5aa01517d885c9a7ec1e1adc3
custom template name: ItemTemplate_Weapon_Bow_Avalon_TamingBow
player-facing name:   Taming Bow
```

### Taming Arrow

```text
custom GUID:          f0b0b02d1f755c8f9d386dedb01d97b6
custom template name: ItemTemplate_Weapon_Ammo_Avalon_TamingArrow
player-facing name:   Taming Arrow
```

These GUIDs and names are Tainted Weapons-owned truth. Consumers should obtain them through `TaintedWeaponsTamingToolsApi` and its receipt rather than inventing or independently registering replacements.

## Public service

```text
TaintedWeaponsTamingToolsApi.EnsureRegistered(out receipt)
TaintedWeaponsTamingToolsApi.TryGetReceipt(out receipt)
```

The service is request-driven. Merely loading Tainted Weapons does not register the pair. A consumer requests the set from the Unity main thread. If FoA's `TemplatesProvider` is not ready, the service returns an accepted queued receipt and the caller retries later.

The receipt exposes:

- resolved source bow GUID/name;
- resolved source arrow GUID/name;
- registered custom GUIDs/names/display names;
- status and reason code;
- native mutation flag;
- `TemplatesLoader.AddToMap` invocation count;
- source/clone profile hashes.

## Registration path

```text
consumer request
  -> Tainted Weapons service
  -> current TemplatesProvider readiness
  -> exact Shortbow source resolution
  -> exact Wooden Arrow source resolution
  -> source category and ItemProjectile validation
  -> custom GUID/name collision preflight
  -> clone both source GameObjects in memory
  -> apply only custom identity/display fields
  -> compare component profiles
  -> compare attachment profiles
  -> compare nested TemplateReference profiles
  -> verify source templates unchanged
  -> verify stackability preserved
  -> register Taming Bow through TemplatesLoader.AddToMap
  -> register Taming Arrow through TemplatesLoader.AddToMap
  -> verify provider lookup returns both exact clone objects
  -> publish immutable receipt
```

The source icon, visual references, equip path and native projectile route are deliberately inherited. No new visual or projectile asset is introduced in this slice.

## Failure policy

Before native insertion, any source, category, collision, profile or structural failure destroys the temporary clones and returns a denial.

FoA exposes no researched unregister operation for `TemplatesLoader.AddToMap`. If an exception occurs after either insertion becomes visible, the service marks itself terminally poisoned for the process and requires a game restart. It never attempts a misleading rollback.

## Required runtime matrix

| Test | Required result |
|---|---|
| Request before provider readiness | accepted queued receipt; no mutation |
| Request after provider readiness | both exact custom templates registered |
| Repeat request | idempotent receipt; no additional insertion |
| Resolve Taming Bow | exact custom GUID/name and `IsShortBow=true` |
| Resolve Taming Arrow | exact custom GUID/name, `IsArrow=true`, `ItemProjectile` present |
| Grant through existing item consumer | both items enter inventory under new names |
| Equip Taming Bow | native Shortbow equip/draw path remains intact |
| Equip Taming Arrow | native quiver/in-hand projectile path remains intact |
| Fire exact pair | native projectile launches with Taming Bow as source weapon and Taming Arrow as ammunition/template |
| Fire vanilla Shortbow/Wooden Arrow | remains the original native pair and is not treated as the custom pair |
| Restart FoA | service can reproduce the same two identities without collision |

## Explicit non-effects

This item service does not:

- grant the items to the player;
- add merchant stock, recipes, quests or notice-board entries;
- change damage, poise, force, knockdown or subdual;
- implement unconsciousness;
- identify or acquire a creature;
- call Avalon AI;
- call the Dialogue Framework;
- persist companion state.

## Current status

```text
source pair selected = PASSED_DESIGN
custom identities fixed = PASSED
owner boundary = PASSED_SOURCE_REVIEW
registration service source = IMPLEMENTED_SOURCE
build = NOT_RUN
plugin load = NOT_RUN
source resolution = NOT_RUN
native registration = NOT_RUN
inventory grant = NOT_RUN
equip/draw/fire = NOT_RUN
exact-pair projectile attribution = NOT_RUN
```

## Next researched task

Build and deploy the `tainted-weapons` branch against the matching FoA Mono installation, call `TaintedWeaponsTamingToolsApi.EnsureRegistered` from a main-thread consumer, and prove the full request/register/grant/equip/draw/fire matrix for the exact Taming Bow and Taming Arrow identities before any companion-side subdual or unconscious logic is authorised.
