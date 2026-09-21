# Weapon Stage 6 — FPP, TPP, and Inventory Preview

## Objective

Prove each presentation consumer separately.

## Why this is separate

A weapon can work in combat and still fail in first-person, third-person, or equipment/inventory preview because those consumers may use different representation selection and lifetime paths.

## Procedure

1. Identify which representation the source archetype uses for:
   - first-person;
   - third-person;
   - off-hand/handedness variants if applicable;
   - inventory/equipment preview.
2. Map the custom presentation to each required owner without mutating vanilla instances globally.
3. Verify each route independently.
4. Confirm camera/preview systems own their normal lifetime and the custom weapon only supplies the registered presentation.
5. Repeat after equip/unequip and scene/UI reopen to detect stale cached presentation.

## Validation gate

Record separate results for FPP, TPP, and preview. Do not collapse them into one pass/fail claim.

## Does not prove

Perspective/preview success does not establish persistence, missing-package behaviour, hot-unload, or another archetype.

## Next

Proceed to [persistence and release](../07-persistence-release/README.md).
