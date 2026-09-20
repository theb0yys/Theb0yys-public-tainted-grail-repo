# Skills, XP, Talents, and Reversible Growth

Use this page when you want to change **XP rates, proficiency progression, talent spending, or progression-driven stat effects**.

The first question is:

> Are you changing the event that grants progression, the saved progression value, a multiplier, the spend transaction, or only the gameplay effect?

Those are different layers.

## Main progression owners

Useful researched owners include:

- `ProficiencyEventListener` — maps gameplay actions/context into proficiency XP events;
- `ProficiencyStats` — saved proficiency XP/state;
- `HeroMultStats` — progression multipliers;
- `Talent` and talent-tree systems — perk spending;
- `HeroStats` / `CharacterStats` — gameplay stats affected by progression;
- `StatTweak` — reversible runtime modifiers.

## Proficiency XP flow

A useful model is:

~~~text
gameplay action
→ ProficiencyEventListener classifies it
→ multipliers
→ ProficiencyStats.TryAddXP
→ saved proficiency state
~~~

Useful methods include:

- `ProficiencyEventListener.XPGainEvent(...)`
- `ProficiencyStats.TryAddXP(ProfStatType, float)`
- `HeroMultStats.ProfMultiplier`
- `HeroMultStats.KillExpMultiplier`
- `HeroMultStats.ExpMultiplier`

## Change the right layer

If you need to know **why** XP was granted, observe the event/context layer.

If you want a broad XP-rate change, prefer the multiplier/stat layer where it already exists.

Treat `TryAddXP` as the save-affecting sink. Patch it only when you intentionally want to affect every matching call at that point.

## Reversible progression effects

Many perk-like effects can be expressed as temporary native stat tweaks instead of rewriting saved progression.

A common route is:

~~~text
HeroStats / CharacterStats initializes
→ add non-saved StatTweak
→ native gameplay code reads ModifiedValue
→ rank/config changes
→ update or remove tweak
~~~

Examples mapped in the research include:

- attack speeds;
- bow draw speed;
- movement/sprint/swim;
- block/parry timing or stamina effects;
- critical/weak-spot stats;
- stealth multipliers;
- armour weight/penalty.

This keeps the gameplay effect reversible while native systems continue consuming the same stats they already understand.

## Talent spending is a transaction

The talent path is more than "subtract a point and enable a perk."

Research surfaces include:

- `Talent.AcquireNextTemporaryLevel`
- `Talent.ApplyTemporaryLevels`
- native spend availability;
- confirmation/cancel behavior;
- refund/respec paths;
- persistence.

Do not replace the normal spending gate until the full transaction is understood.

## Fireplace/rest context

The ordinary talent-upgrade route is tied to native fireplace/rest availability.

Related researched surfaces include:

- `RestPopupUI.SkipWeatherTime(...)`
- Hero before/after-rest events

If your feature changes when spending is allowed, treat that as a spend-gate change, not merely a UI tweak.

## Common mistakes

### Patching the saved XP sink for a simple multiplier feature

This broadens the effect unnecessarily and can change every source feeding the sink.

### Writing saved XP/levels/talent points directly

That bypasses native validation, UI, refund/respec, and migration behavior.

### Rewriting combat/movement code for a perk that already maps to a native stat

Use the stat owner where possible.

### Implementing a custom spend screen without confirm/refund semantics

This can create double charges, failed refunds, or mismatched UI/save state.

### Treating mod currency as native progression

A sidecar/mod-owned balance can be useful, but it is not the same state as FoA XP/talent progression.

## How to verify XP changes

Check:

1. exact action/context;
2. expected proficiency/category;
3. value reaching the native sink;
4. multiplier applies once;
5. unrelated categories remain unchanged;
6. save/reload preserves native state correctly.

## How to verify runtime perk effects

Check:

1. exact native stat;
2. tweak added once;
3. effective value changes;
4. the real gameplay consumer reads it;
5. rank/config changes update it;
6. disable/unload removes it;
7. no saved base state changes unintentionally.

## How to verify spending changes

Check:

1. availability;
2. reservation/temporary state;
3. confirm;
4. cancel;
5. refund/respec;
6. UI state;
7. save/reload;
8. insufficient-currency rejection.

## Evidence limits

The XP flow, multiplier owners, talent transaction surfaces, and many stat mappings are source/decompilation-backed.

Exact balance and gameplay feel still need runtime validation per feature.
