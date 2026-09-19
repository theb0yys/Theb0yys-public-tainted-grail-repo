# Diagnose — Weapon Works but Is Invisible

Document type: **troubleshooting**.

## Symptom

The weapon exists and can attack, animate, and deal damage, but the equipped model is missing or wrong.

## First rule

Do **not** reopen every system.

List what has already succeeded.

If the item exists and combat is working, the earliest failed lane is usually later than registration/combat.

## Diagnostic chain

```text
template resolves?
→ item acquired?
→ equip state active?
→ native equipped View created?
→ representation selected/loaded?
→ visual redirect reached?
→ renderer owner received valid resources?
→ presentation visible in the expected perspective?
```

Stop at the **first unproven transition**.

## 1. Confirm upstream success

Record evidence for:

- custom/native template identity;
- item acquisition;
- equipped state;
- attack/animation;
- damage/hit behaviour.

If these are working, keep them unchanged while investigating presentation.

## 2. Confirm the native equip/View path

Use [Native Weapon Lifecycle](../../systems/weapons/native-lifecycle.md).

Check whether the weapon reaches:

- `ItemEquip`;
- a valid `CharacterHandBase` / `CharacterWeapon`;
- native View binding/attachment;
- the expected representation load path.

A valid combat state does not automatically prove the visual representation was created correctly.

## 3. Confirm the presentation handoff

For renderer-specific paths, ask:

- was the equipped visual redirect actually invoked?
- did the custom asset/prototype resolve?
- did the renderer owner accept the mesh/material/resource identity?
- did a runtime view/prototype get built?
- are FPP/TPP/preview owners looking at the same representation?

Missing presentation markers after successful registration are evidence to stay in the presentation lane.

## 4. Check resource/lifetime state

An asset may resolve but still fail presentation because:

- the wrong identity was rebound;
- the handle is invalid/stale;
- the View was discarded;
- renderer resources were not adopted;
- the object is hidden by perspective/visibility ownership;
- cleanup ran too early.

## 5. Do not use these as fixes without evidence

Avoid:

- re-registering the item;
- changing damage/hit logic;
- changing attack animations;
- adding a second hand-socket Unity renderer;
- bypassing the native equip/View owner.

Those actions change already-proven lanes and make the failure harder to isolate.

## Evidence to collect

Capture the smallest useful set:

```text
template identity
equip marker
CharacterHandBase/weapon View identity
presentation redirect marker
asset/prototype resolution
renderer-resource marker
visible result
unequip/cleanup marker
```

## Stop conditions

If no equipped View/representation load exists, debug the equip lifecycle first.

If the equipped View exists but the presentation redirect/resource path is absent, stay in presentation.

If all presentation markers pass but the object is still invisible, investigate renderer/perspective/material/visibility ownership before touching combat.

## Related case

[Weapon works in combat but the equipped render is invisible](../../examples/failures-and-corrections/weapon-combat-works-render-invisible.md)

## Canonical pages

- [Native weapon lifecycle](../../systems/weapons/native-lifecycle.md)
- [Custom weapon integration](../../mechanics/weapons/custom-weapon-integration.md)
