# Case Packet — Weapon Works in Combat but Presentation Fails

## Reader symptom

> The custom weapon exists, attacks, animates and deals damage, but the equipped render is missing or wrong.

## Canonical owners

- item/template identity and registration;
- native equip lifecycle;
- combat owner;
- renderer/presentation owner;
- cleanup/resource lifetime.

## Evidence-backed case history

Reviewed private evidence establishes a visual-only Evil Greatsword failure:

- the item existed;
- attack animation, attack logic and damage were treated as working;
- template registration had succeeded;
- the equipped render remained invisible;
- the missing evidence was on the equipped visual redirect/runtime-view/presentation route;
- the repair scope was deliberately restricted to presentation identity/resource handling rather than registration, combat or animation.

Later evidence showed the framework-owned visual route could produce an equipped visual redirect and a Drake-backed runtime prototype.

## What this case proves

- a weapon can pass definition/acquisition/equip/combat checks while still failing presentation;
- the earliest failed lane should constrain the debugging scope;
- presentation failure is not evidence that item registration or damage logic is wrong;
- direct fallback rendering should not be reintroduced merely because the renderer owner has not been reached.

## What this case does not prove

- every invisible weapon has the same cause;
- the generic custom-weapon importer is complete;
- persistence, missing-package, migration or hot-unload are proven;
- the public rewrite itself has been runtime executed.

## Public-safe claims to preserve

```text
weapon exists
→ attacks/animation/damage work
→ equipped render missing
→ registration/combat are not the first failed lanes
→ inspect equipped representation / renderer owner
→ prove redirect/resource/runtime-view path
```

## Canonical dependencies

- [Native weapon lifecycle](../../../systems/gameplay/native-weapons/README.md)
- [Custom weapon integration](../../../mechanics/weapons/equipped-presentation.md)
- [Evidence status](../../../reference/evidence/README.md)

## Public outputs

- case study: `examples/failures-and-corrections/weapon-combat-works-render-invisible.md`
- diagnosis: `diagnose/presentation/weapon-works-but-is-invisible.md`

## Evidence status

- underlying case: runtime-observed and receipt-backed within its recorded environment;
- public rewrite: documentation-only; no new runtime execution;
- persistence/compatibility/release: not inherited.
