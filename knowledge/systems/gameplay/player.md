# Player / Hero System

Canonical location for established knowledge about native player/hero ownership and execution.

## Publicly established hero access

Public source demonstrates more than one way to reach the hero:

- `Hero.Current` — dominant direct current-hero access pattern;
- `World.Any<Hero>()` — null-checkable world lookup used by a public Mono mod.

These are access paths, not interchangeable lifecycle guarantees.

## Publicly established hero-owned surfaces

Examples include:

- `HeroItems` for hero item ownership/state;
- `KnownItems` for known/discovered item state;
- `HeroRecipes.LearnRecipe` for learning existing/native recipes;
- `HeroStats` for gameplay stats such as encumbrance and summon limit;
- `HeroRPGStats` for RPG/progression stats;
- `CharacterStatuses` / `Hero.Statuses` for active hero statuses;
- `HeroStorage.Items` for hero-storage contents in the documented storage context;
- `Hero.TryGetElement<T>()` for hero-owned MVC elements such as `ArmorWeight`.

These surfaces belong to different responsibilities. Do not collapse them into a single generic "player object" API.

## Lifecycle

Public Mono mods establish two especially useful initialization boundaries.

### `Hero.OnFullyInitialized`

Used for:

- installing event listeners once world/HUD infrastructure is ready;
- clearing per-save caches;
- checking/removing hero statuses;
- hero-dependent initialization generally.

One public mod explicitly comments that `World.EventSystem` and the HUD are not initialized when the plugin itself loads, so it waits for this Postfix.

### `HeroRPGStats.AfterHeroFullyInitialized`

Used for stat-system changes requiring both hero stats and `TweakSystem`.

Public mods resolve:

~~~text
Hero.Current
→ HeroStats / HeroRPGStats
→ World.Services.Get<TweakSystem>()
→ TweakSystem.AddTweak(...)
~~~

at this stage.

### Restore-specific owners

A public IL2CPP-native implementation captures `HeroItems` at `HeroItems.OnRestore`, where the restored owner instance is valid, instead of relying on an unsafe generic lookup.

## Public stat/member examples

Public mods access:

- `hero.HeroStats.EncumbranceLimit`
- `hero.HeroStats.ArmorWeightMultiplier`
- `hero.HeroStats.SummonLimit`
- `hero.HeroRPGStats`
- `hero.AliveStats.Health`
- `hero.AliveStats.MaxHealth`
- `hero.Statuses`

These names are useful navigation/reference facts; their complete ownership and persistence behavior remains system-specific.

## Persistence boundary

Public evidence distinguishes hero/session state from saved native progression:

- `ProficiencyStats.TryAddXP` changes native proficiency progression.
- Mod-owned practice/session ledgers can remain non-saved until deliberately converted into native progression.
- Known-item reconstruction can run per loaded session without implying that the reconstruction mechanism owns persistence.

## Presentation is separate

Questline public source and public mods expose hero-related HUD/camera/presentation types, but access to those presentation objects does not establish ownership of hero gameplay state.

See [Runtime Access](../../reference/runtime-access/README.md), [Types](../../reference/types/README.md) and [Hooks](../../reference/hooks/README.md).

## Current proof boundary

This page currently records only publicly documented FoA behavior. Exact construction/destruction ordering, cross-scene identity, death/reload replacement semantics and complete model/view ownership remain unclaimed until stronger evidence is reviewed.
