# Status Effects and Buildup

Use this page when you want to **observe or change buffs, debuffs, diseases, curses, burn/bleed/poison buildup, or other status-driven effects**.

The key distinction is:

> Adding a Status and building up toward a Status are different native paths.

## Native status flow

A useful model is:

~~~text
StatusTemplate identity
→ direct add OR buildup
→ add/upgrade/replace/stack decision
→ Status instance
→ Status.OnInitialize
→ stat/skill/gameplay effect
→ duration/refresh/removal
~~~

## Main native owners

- `StatusTemplate` — status definition.
- `CharacterStatuses` — status collection and add/buildup entry points.
- `Status` — runtime status instance.
- `StatusSourceInfo` — source character/item context.
- buildup attachment/type data — threshold/application behavior.
- status-owned skill/stat logic — actual gameplay effect after initialization.

## Direct status addition

Useful surfaces include:

- `CharacterStatuses.AddStatus(StatusTemplate, StatusSourceInfo, ...)`
- `CharacterStatuses.AddResult`
- result kinds such as Add, Upgrade, AddAndProlong, AddAndRenew, Replace, and Stack
- `StatusSourceInfo.GetSourceCharacter`
- `StatusSourceInfo.GetSourceItemSafe`

A Postfix observer here can tell you:

- target;
- old/new Status;
- result type;
- positive/negative classification;
- source character;
- source item.

That is a good place to learn what actually reaches a character before attempting mutation.

## Buildup

Useful surfaces include:

- `CharacterStatuses.BuildupStatus(float, StatusTemplate, StatusSourceInfo)`
- `BuildupAttachment.BuildupStatusType`
- buildup threshold/unit relationships.

A typical buildup path is:

~~~text
source action
→ buildup amount
→ BuildupStatus
→ buildup category/threshold
→ threshold reached
→ actual Status applied
~~~

If the goal is "this buildup accumulates faster," changing the buildup input is usually narrower than rewriting every projectile/source that can contribute to it.

## Exact source matters

Broad status hooks can see:

- Hero self-effects;
- NPC-vs-NPC effects;
- spells;
- items;
- environmental effects;
- scripted effects.

If the mod is player-only, filter source and target explicitly.

Use native positive/negative classification where available instead of guessing from names.

## Example traced spell

One researched projectile route connects:

~~~text
Projectile_OnHit_ApplyStatus
→ Status_Fire1_Burn
~~~

Projectile entries can also override status/buildup values, so do not assume the SkillGraph default is always the final runtime value.

## Keep secondary mod state separate

A mod can react to a native negative status by adding its own session-only mechanic without changing the native Status itself.

That is a different operation and should be documented as such.

## Common mistakes

- treating `AddStatus` as if every status uses buildup;
- applying a global buildup/status multiplier without source/target filters;
- classifying positive/negative from names;
- treating a status observer as persistence proof;
- assuming a custom `StatusTemplate` is registered because a related prefab or effect exists.

## How to verify

Check:

1. exact target;
2. exact `StatusTemplate`;
3. source character/item;
4. direct-add or buildup route;
5. add/upgrade/replace/stack result;
6. duration/refresh behavior;
7. actual gameplay effect;
8. removal/expiry;
9. unrelated status routes remain unchanged;
10. save/load only if you intend to claim persistence.

For buildup tuning, also verify threshold progress before/after and the exact final Status applied.

## Evidence limits

`CharacterStatuses.AddStatus` and `BuildupStatus` are well-mapped native surfaces.

There is not yet a general public process for registering arbitrary durable custom statuses.
