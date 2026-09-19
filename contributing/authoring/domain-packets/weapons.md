# Domain Packet — Weapons

## Subject

Weapon definition, native item/equip ownership, combat ownership, rigid presentation/Drake integration, cleanup, persistence and importer boundaries.

## Reader questions

1. What is the durable gameplay object for a weapon?
2. Which owner selects and instantiates the equipped representation?
3. Which owner controls combat/sweep geometry?
4. Which owner controls rigid presentation and lifetime?
5. Why can a weapon work in combat while still be visually broken?
6. Which parts of a generic importer remain partial?

## Archetypes required

- **System hub:** end-to-end weapon ownership map.
- **System:** native equip/view/presentation lifecycle.
- **Mechanic:** custom weapon integration.
- **Case:** invisible-render/presentation-only failure.
- **Reference:** exact identities/types/evidence.
- **Learning journey:** custom weapon path after item fundamentals.

## Canonical native owners

- `ItemTemplate`
- `Item`
- `ItemEquipSpec`
- `ItemEquip`
- `CharacterHandBase`
- `CharacterWeapon`
- native renderer/Drake ownership where selected
- native inventory/equipment/save ownership.

## Required prerequisites

- [Items domain packet](items.md)
- [Native object ownership](../../../systems/core/native-object-ownership.md)
- [Templates and registries](../../../systems/core/templates-and-registries.md)
- [Saving and persistence](../../../systems/persistence/README.md)

## Public source set

- `docs/reference/WEAPONS.md`
- `docs/reference/WEAPONS_NATIVE_LIFECYCLE.md` (legacy shim only)
- `docs/reference/PROPRIETARY_RENDERING_SYSTEMS.md`
- item/template/persistence pages
- public examples where applicable.

## Private-evidence conclusions to preserve

- the native weapon route is item/equip/view owned rather than a separate universal Weapon model;
- `ItemEquip` owns representation selection/load/binding/attach/detach/release;
- `CharacterWeapon` owns native melee behaviour/sweep semantics;
- Drake/resource presentation is a separate owner from item/equip/combat;
- direct Unity renderer fallback can make something visible while bypassing production ownership;
- multiple private implementations support archetype-preserving item/template registration;
- a visual failure can occur after definition/acquisition/equip/combat have substantially succeeded;
- generic importer persistence, transactional registration, hot-unload and broad compatibility remain incomplete.

## Claims to teach

- item definition, acquisition, equip, combat and presentation are distinct lanes;
- a custom mesh does not define native hit geometry;
- presentation replacement must enter the renderer owner's lifecycle;
- equip cleanup is part of correctness;
- a successful custom `ItemTemplate` does not mean the custom weapon problem is solved;
- evidence must identify which lane actually failed.

## Failure/correction history to preserve

The public case-study plan should preserve the reasoning pattern demonstrated by the visual-only failure:

```text
item exists
→ acquisition/equip/combat substantially work
→ equipped render is wrong/invisible
→ first failed lane is presentation
→ inspect presentation identity/resource/Drake path
→ do not reopen registration/damage/animation without evidence
```

Also preserve:
- direct Unity renderer fallback is not the reusable production path;
- cloning a few fields does not prove semantic equivalence;
- late insertion/global visibility is not guaranteed;
- direct template-map insertion lacks a researched transactional unregister/rollback path.

## Required diagrams

1. **Ownership:** `ItemTemplate → Item → ItemEquipSpec/ItemEquip → CharacterHandBase/CharacterWeapon → renderer owner`.
2. **Lifecycle:** equip event → asset load → bind/attach → active → detach/discard/release.
3. **Diagnostic:** definition/acquisition/equip/combat/presentation as independent failure lanes.

## Worked-example candidate

A public-safe visual-only failure case should be authored from already public conclusions, without copying private implementation code.

## Evidence lanes

- native ownership: strong;
- custom registration: bounded/patch-sensitive;
- presentation framework: substantial but generic importer remains partial;
- persistence/missing-package/hot-unload: separate incomplete lanes.

## Canonical target map

- `systems/weapons/README.md` — domain ownership hub.
- `systems/weapons/native-lifecycle.md` — concise native equip/view/presentation model.
- `mechanics/weapons/custom-weapon-integration.md` — migrated process page.
- `examples/failures-and-corrections/weapon-visible-combat-invisible-render.md` — later case conversion.
- `learn/content-authoring/weapons/README.md` — guided route.

## Completion checks

- hub links Item and Drake/Kandra/etc rather than duplicating them;
- native system page explains ownership before process;
- mechanic preserves incomplete generic-importer boundary;
- presentation failure cannot be misread as registration failure;
- persistence remains separate.
