# Weapon Stage 2 — Custom ItemTemplate Registration

## Objective

Create and register a separate weapon ItemTemplate identity without replacing the native source.

## Procedure

1. Clone the complete reviewed native ItemTemplate GameObject.
2. Assign a stable mod-owned GUID and template name.
3. Preserve the source component and attachment topology needed by ItemEquipSpec and combat.
4. Keep the source archetype's combat and animation profile unchanged for the first proof.
5. Register through the shared/current weapon registrar where available.
6. If registration must wait for template readiness, retry only at the documented readiness boundary.
7. Resolve the custom GUID again through TemplatesProvider.
8. Record a receipt tying the custom identity to its source profile and package owner.

The public rigid-weapon example uses a shared registrar rather than treating the weapon mesh as the gameplay definition.

## Check

Confirm that the custom weapon ItemTemplate resolves through the normal provider and its expected equip/combat attachments remain intact.

## Failure conditions

- custom identity collides with native or another mod identity;
- clone drops required attachment/component state;
- registration occurs before template readiness;
- provider re-resolution fails;
- post-insertion failure can leave native registration state changed without a transactional rollback.

## Before continuing

Registered ItemTemplate does not cover acquisition, equip, visible presentation, combat, preview, or persistence.

## Next

Proceed to [native equip lifecycle](../03-equip-lifecycle/README.md).
