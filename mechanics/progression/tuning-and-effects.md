<!-- Canonical Wave 5 mechanic page split from docs/reference/PROGRESSION_SKILLS.md. -->
# Skills, Progression, XP, Talents, and Reversible Stat Growth — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/progression/README.md) first for ownership, identities, lifecycle, and proof scope.

## How we interact with it

### Separate XP source from XP sink

Patch/observe `XPGainEvent` when you need to know *what action produced XP*.

Use or modify the multiplier/stat layer when the goal is broad rate tuning.

Treat `TryAddXP` as the save-affecting sink and patch it only when that exact responsibility is intended.

### Prefer non-saved StatTweaks for runtime perk effects

The working research mapped many progression/perk effects to existing native stats rather than writing saved fields:

- attack-speed stats;
- bow draw speed;
- fist attack speeds;
- one-/two-handed speeds;
- movement/sprint/dash/swim;
- block prepare/movement;
- stamina cost/recovery;
- parry stamina damage;
- critical/weak-spot stats;
- stealth multipliers;
- armour weight/penalty.

This keeps the gameplay effect reversible and lets the native gameplay consumer stay in charge.

### Preserve native spending gates until the full transaction is understood

The normal talent path has native availability/temporary/commit/refund/respec behavior.

A custom gate should remain default-off until:

- confirmation;
- cancellation;
- refund;
- respec;
- persistence;
- UI blockability;

are all proven.

## Why this route

The progression research deliberately avoids editing:

- saved XP dictionaries;
- hero XP;
- levels;
- talent points;
- base-stat points;
- talent levels;
- RPG stat levels;

when a reversible runtime multiplier/tweak can express the desired gameplay effect.

That reduces save risk and keeps native ownership intact.

## What goes wrong

### Patching the XP sink when only the multiplier should change

Can affect every source and saved progression behavior.

### Writing saved progression fields directly

Bypasses native validation, UI, refund/respec, and migration expectations.

### Runtime perk effect implemented by rewriting combat/movement code

Often unnecessary when a native stat already exists.

### Custom spend gate ignores temporary/confirm/refund lifecycle

Can charge twice, fail to refund, or desynchronize UI and saved progression.

### Sidecar balance treated as native progression state

A mod-owned sidecar may hold mod currency/insight, but it is not the same owner as native XP/talent state.

## How to verify

For XP changes:

1. exact action context observed;
2. native XP sink receives expected category/value;
3. multiplier applies exactly once;
4. unrelated XP categories remain unchanged;
5. save/reload preserves native state correctly.

For runtime perk/stat effects:

1. exact stat owner identified;
2. non-saved tweak added once;
3. gameplay consumer reads changed value;
4. rank/config change updates it;
5. disable/unload removes it;
6. no saved base/diff field changes.

For spending:

1. availability;
2. temporary reservation;
3. confirm;
4. cancel;
5. refund/respec;
6. UI state;
7. save/reload;
8. insufficient-currency rejection.

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/progression/README.md).
