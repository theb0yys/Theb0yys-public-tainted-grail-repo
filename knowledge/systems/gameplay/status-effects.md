# Status Effects, Buildup, Sources, and Safe Observation

> **Reference page.** Use this when working with buffs, debuffs, diseases, curses, burn/bleed/poison buildup, or status-driven gameplay.

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

## How we interact with it

### Observe `AddStatus` to learn what actually reaches a character

A postfix observer can inspect:

- target;
- old/new Status;
- result type;
- positive/negative class;
- source character;
- source item.

This is useful before deciding to mutate status behavior.

### Tune buildup at the buildup owner

If the goal is "burn builds 1.5x faster," a scoped `BuildupStatus` input multiplier is closer to the native owner than rewriting every projectile or source that can apply burn.

### Scope by source/target

Broad status hooks are global/high-value.

For player-only effects, verify source and target explicitly.

### Keep status mutation separate from secondary mod state

A mod can react to a negative status by adding its own session-only fatigue, for example, without changing the native disease/curse itself.

That is a different claim and can be safer.

## Why this route

The working research deliberately began with **diagnostic-only** status observation.

That allowed it to establish:

- which statuses are actually added;
- which are positive/negative;
- who sourced them;
- how add/upgrade/stack behavior appears;

before adding any mutation.

Magic research then found buildup has its own native owner, allowing buildup tuning without rewriting every effect source.

## What goes wrong

### AddStatus = buildup

False. Some statuses accumulate before being applied.

### Broad global multiplier without source/target filter

Can affect NPC-vs-NPC, environmental, self, item, spell and scripted status routes.

### Positive/negative name heuristic

Use native status classification where available.

### Status observer called persistence proof

Seeing a Status object added does not prove save/restore/duration across restart.

### Custom spell/status template confused with existing-status tuning

Registering a genuinely new StatusTemplate has separate identity/registration/persistence requirements.

## How to verify

For status observation/tuning:

1. exact target character;
2. exact StatusTemplate;
3. source character/item;
4. direct-add or buildup path;
5. result type;
6. stack/refresh behavior;
7. duration;
8. native gameplay effect;
9. removal/expiry;
10. unrelated status routes unchanged;
11. save/load if persistence is claimed.

For buildup tuning, also verify threshold progression before/after and the final applied status identity.

## Current proof boundary

`CharacterStatuses.AddStatus` and `BuildupStatus` are well-researched native surfaces.

The public handbook does not yet claim a generic durable custom-status registration process.
