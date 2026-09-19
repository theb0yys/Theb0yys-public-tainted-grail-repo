# Recipe — Consumable Effect Attribution

**Category:** consumables / healing / statuses  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED attribution shape  
**This public recipe:** NOT_RUN

The safe question is not:

> "What does this item's name suggest it should do?"

It is:

> "What changed across the native item-use boundary, and which observer owns each fact?"

This recipe connects examples 31–33 without turning a template flag into an unsupported gameplay claim.

## Stage 1 — classify the use attempt

Start at:

~~~text
Item.Use()
~~~

Require the item owner to be `Hero.Current`, then record only native item/template context needed for diagnostics.

Useful template flags include consumable, potion, food, dish, fish, alcohol, and the built-in health/mana/stamina consumable flags.

These fields are classification evidence. They are not effect proof.

## Stage 2 — snapshot relevant state before native use

Capture only the state needed for the outcome you want to attribute.

For healing:

~~~text
Hero.Health.ModifiedValue
~~~

For cure/buff deltas:

~~~text
Hero.Statuses.AllStatuses
~~~

If quantity matters to your diagnostic, snapshot `Item.Quantity` too.

Do not mutate anything in the prefix.

## Stage 3 — let native item use run

Preferred observer shape:

~~~text
prefix snapshot
    ↓
native Item.Use
    ↓
postfix snapshot
    ↓
delta report
~~~

Do not skip the original method merely to simplify attribution.

## Stage 4 — report observed deltas

Examples:

- health increased: synchronous healing observed;
- negative status instance disappeared: synchronous removal/cure candidate observed;
- positive status instance appeared: synchronous buff/status gain observed;
- quantity decreased: native item quantity changed during the observed call.

Each line states only what the delta proves.

A quantity decrease does not prove healing. A health-related template flag does not prove health increased.

## Stage 5 — keep observer ownership separate

Two useful surfaces can fire during one consumable action:

~~~text
Item.Use
CharacterStatuses.AddStatus
~~~

They answer different questions.

`Item.Use` owns the action-level attribution window: what changed around this hero-owned item use?

`CharacterStatuses.AddStatus` owns the native status-application event: what status add/upgrade/renew/replace/stack result occurred, and what source/target did native status handling expose?

Do not count the same positive status twice merely because both observers saw it.

If a larger system needs correlation, assign one action identifier at the `Item.Use` boundary and attach status events to that action instead of creating two independent rewards or effects.

## Stage 6 — respect timing limits

A postfix on `Item.Use` proves only synchronous state visible when the call returns.

Delayed animation effects, periodic healing, delayed status changes and other later scripts require a wider evidence window or a narrower later lifecycle hook.

Separate maintainer evidence has live diagnostic coverage for representative food, dish, potion and alcohol uses through `Item.PerformImmediate(ItemActionType)`. That does not automatically prove that every effect is final at that boundary, and it does not change the requested public examples' `Item.Use` ownership model.

## Validation ladder

1. Observe a hero-owned consumable use without changing it.
2. Verify a non-healing consumable reports no positive health delta.
3. Verify a known healing action reports a positive health delta when synchronous.
4. Verify a negative status removal is reported only when the instance disappears.
5. Verify a positive status gain is reported only when a new instance appears.
6. Compare example 33 with example 28 so one native status application is understandable across both surfaces.
7. Test a delayed effect separately; do not force it into the synchronous model.
8. Confirm no observer changes item quantity, status state, health or saves.

This progression keeps action attribution, status lifecycle and gameplay mutation as separate concerns.
