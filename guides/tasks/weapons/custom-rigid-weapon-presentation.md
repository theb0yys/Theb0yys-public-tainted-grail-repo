# Build Custom Rigid Weapon Presentation Through Drake

**Evidence status: PARTIAL.** Runtime evidence proves identity/equip redirect, Drake prototype construction, and mesh/material resource serving for the tested path. Repeated lifecycle, simultaneous instances, scene restore, full UI isolation, and final visual acceptance remain incomplete.

Working lineage: [Evil Greatsword: Equip and Presentation Boundary](../../../research/case-studies/weapons/evil-greatsword-presentation.md).  
Canonical owners: [Native Weapon Integration](../../../knowledge/systems/gameplay/native-weapons/README.md) and [Drake](../../../knowledge/systems/presentation/drake/README.md).

## Ownership chain

```text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase
→ Drake authoring/resource lifecycle
→ rendered rigid presentation
```

Do not make the mesh the weapon's source of truth.

## Process

1. Start from a valid native weapon/equip profile.
2. Preserve native `Item`, equip and hand ownership.
3. Build a Drake-compatible presentation prototype.
4. Bind stable custom mesh/material resource identities.
5. Let native equip instantiate the presentation owner.
6. Let Drake own entity/resource acquisition and spawned state.
7. Observe the native lifecycle rather than manually publishing Drake state.

## Already established in the tested receipt

```text
custom identity accepted
→ native equip redirect observed
→ Drake prototype built
→ registered mesh key served
→ registered material key served
```

The prototype used Drake presentation rather than an ordinary Unity renderer fallback.

## Do not

- directly create synthetic Drake ECS entities;
- mutate private resource counters;
- publish lifecycle tags manually;
- suppress unload/release broadly;
- claim a complete weapon because the model appears.

## Required validation before calling it complete

Still prove:

- repeated equip/unequip;
- two simultaneous instances where supported;
- FPP;
- TPP;
- inventory preview;
- hide/show;
- scene transition restore;
- final owner/resource release;
- screenshot/video visual acceptance.

## Current proof boundary

Use this guide to understand and reproduce the **known-good entry architecture**, not as evidence that the generic custom weapon importer is release-complete.
