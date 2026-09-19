<!-- Canonical Wave 5 native-system page split from docs/reference/STATUS_EFFECTS.md. -->
# Status Effects, Buildup, Sources, and Safe Observation

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/magic/status-effect-intervention.md).

## What this system is

FoA distinguishes at least three status concerns:

~~~text
status identity/template
→ status addition/replacement/stacking
→ optional buildup threshold/application
→ Status initialization / skill/stat effect
→ duration/refresh/removal
~~~

A buildup-capable effect is not the same thing as an immediately added status.

## Who owns it in FoA

Important owners include:

- `StatusTemplate`;
- `CharacterStatuses`;
- `Status`;
- `StatusSourceInfo`;
- buildup attachments/types;
- skill/stat logic initialized by the Status.

## Important identities, types, and methods

Researched surfaces include:

- `CharacterStatuses.AddStatus(StatusTemplate, StatusSourceInfo, ...)`;
- `CharacterStatuses.AddResult`;
- add-result types including Add, Upgrade, AddAndProlong, AddAndRenew, Replace and Stack;
- `StatusSourceInfo.GetSourceCharacter`;
- `StatusSourceInfo.GetSourceItemSafe`;
- positive/negative classification from Status/StatusTemplate type;
- `CharacterStatuses.BuildupStatus(float, StatusTemplate, StatusSourceInfo)`;
- `BuildupAttachment.BuildupStatusType`;
- `SetStatusBuildupUnit` / buildup-threshold relationships;
- `Status.OnInitialize()` initializing status skill/stat behavior.

A traced spell example connects:

~~~text
Projectile_OnHit_ApplyStatus
→ Status_Fire1_Burn
~~~

with projectile-entry overrides for status/buildup fields.

## Where it exists in the lifecycle

### Direct status addition

~~~text
source/action
→ CharacterStatuses.AddStatus
→ add/upgrade/replace/stack decision
→ Status instance
→ Status.OnInitialize
→ skill/stat effect
→ duration/renew/remove
~~~

### Buildup path

~~~text
source action
→ buildup strength
→ BuildupStatus
→ buildup type/threshold
→ threshold reached
→ status application
~~~

## Current proof boundary

`CharacterStatuses.AddStatus` and `BuildupStatus` are well-researched native surfaces.

The public handbook does not yet claim a generic durable custom-status registration process.
