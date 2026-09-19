# Case Study — Weapon Works in Combat but the Equipped Render Is Invisible

Document type: **case study**.

## Symptom

A custom weapon:

- exists as a distinct item;
- can be acquired/equipped;
- attacks;
- animates;
- deals damage;

but the equipped weapon render is missing.

## Why this matters

This is a clean example of **partial success across separate owners**.

The tempting response is to reopen everything:

- item registration;
- acquisition;
- animation;
- damage;
- hit logic;
- assets.

That would destroy useful evidence.

The observed behaviour already says several stages are working.

## What has definitely succeeded?

At minimum, the case established that the feature had progressed beyond:

```text
definition
→ registration
→ acquisition
→ enough equip/combat state for attacks and damage
```

Those lanes were therefore not the first place to debug.

## Earliest failed lane

The first clear failure was **equipped presentation**.

The relevant question became:

> Did the native equip/view path reach the framework-owned visual redirect and renderer/presentation owner?

The pre-fix evidence showed accepted registration but no corresponding visual-route/runtime-view activity for the failing weapon.

That narrowed the fault to the presentation path instead of combat.

## Corrected model

```text
ItemTemplate
→ Item
→ ItemEquip
→ CharacterHandBase / CharacterWeapon
→ presentation redirect
→ renderer owner / Drake-backed prototype
```

Combat and presentation are related by the equip lifecycle, but they are not the same owner.

A weapon can be valid enough to attack while its renderer path is still wrong.

## What changed

The repair was scoped to the framework-owned equipped visual identity/resource route.

It deliberately did **not**:

- re-register the item;
- rewrite damage;
- rewrite attack animation;
- restore an ad-hoc hand-socket Unity renderer fallback;
- mutate saves.

Later evidence showed the framework-owned visual redirect and Drake-backed prototype path being reached.

## Reusable lesson

When a feature is partially working:

```text
list what has definitely succeeded
→ identify the first unproven transition
→ debug that owner/lane
→ keep proven upstream systems unchanged
```

For weapons specifically:

```text
item exists + combat works + render missing
→ presentation is the first failed lane
→ inspect equipped View / redirect / renderer-resource path
```

Do not use “invisible” as evidence that template registration failed.

## Diagnostic handoff

Use [Weapon works but is invisible](../../diagnose/presentation/weapon-works-but-is-invisible.md).

## Canonical system/mechanic

- [Native weapon lifecycle](../../systems/weapons/native-lifecycle.md)
- [Custom weapon integration](../../mechanics/weapons/custom-weapon-integration.md)

## Evidence status

Underlying case: **runtime-observed and receipt-backed in its recorded environment**.

This public case-study rewrite: **documentation-only**.

This case does not establish:
- universal importer readiness;
- persistence;
- missing-package behaviour;
- hot-unload;
- cross-build compatibility.
