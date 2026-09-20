# Player / Hero

Use this page when your mod needs **player-owned state** such as inventory, recipes, stats, statuses, or storage.

The main mistake to avoid is treating `Hero` as one giant all-purpose player API. Different pieces of player state are owned by different Hero Elements and become ready at different times.

## Getting the current Hero

Common access patterns include:

- `Hero.Current` — the usual direct current-Hero reference;
- `World.Any<Hero>()` — a null-checkable world lookup used by public Mono mods.

A non-null Hero does not prove every Hero-owned Element has finished initializing.

## Common Hero-owned state

Useful objects include:

- `HeroItems` — Hero inventory/item state;
- `KnownItems` — known/discovered item state;
- `HeroRecipes` — learned recipe state;
- `HeroStats` — Hero-specific gameplay stats;
- `HeroRPGStats` — RPG/progression attributes;
- `CharacterStatuses` / `Hero.Statuses` — active statuses;
- `HeroStorage.Items` — Hero storage contents in the storage context;
- `Hero.TryGetElement<T>()` — optional Hero-owned Elements such as `ArmorWeight`.

Use the subsystem that actually owns the state you are changing.

## Useful initialization points

### Hero.OnFullyInitialized

Public Mono mods use this after the Hero's normal initialization has completed for tasks such as:

- installing event listeners;
- refreshing Hero-dependent state;
- clearing per-save caches;
- checking/removing statuses.

One public mod explicitly waits for this point because the EventSystem/HUD are not ready when the plugin first loads.

It is a useful Hero boundary, **not** a promise that every child Element, View, scene system, or restored object is ready.

### HeroRPGStats.AfterHeroFullyInitialized

Public stat mods use this when they need Hero RPG/stat infrastructure and `TweakSystem`.

Exact Mono inspection shows:

~~~text
HeroRPGStats.OnInitialize()
→ wrapper.Initialize(this)
→ ParentModel.AfterFullyInitialized(AfterHeroFullyInitialized)
→ AfterHeroFullyInitialized()
~~~

That gives the method a concrete lifecycle meaning rather than treating the name as a guess.

### HeroItems.OnRestore

A public IL2CPP-native implementation captures `HeroItems` at `HeroItems.OnRestore` because that point provides the restored owner directly.

That is a restore-time availability point, not a generic synonym for "the player is loaded."

## Stat access examples

Public mods use members such as:

- `hero.HeroStats.EncumbranceLimit`
- `hero.HeroStats.ArmorWeightMultiplier`
- `hero.HeroStats.SummonLimit`
- `hero.HeroRPGStats`
- `hero.AliveStats.Health`
- `hero.AliveStats.MaxHealth`
- `hero.Statuses`

For the larger stat/member inventory, see [Hero and Character stat surfaces](../../reference/types/hero-character-stats.md).

## Base stats and temporary tweaks are different

In the inspected Mono build:

- `Stat.BaseValue` is the mutable base state;
- `Stat.ModifiedValue` is the effective value after tweaks;
- `Stat.ValueForSave` returns `BaseValue`;
- `SetTo` and `IncreaseBy` change the base value;
- `StatTweak` changes the effective value through `TweakSystem`.

So "change the stat" can mean two very different things:

~~~text
change BaseValue
→ changes saved/native base state

apply StatTweak
→ changes effective runtime value through tweak ownership
~~~

Choose deliberately.

## Persistence

Examples of different persistence behavior:

- `ProficiencyStats.TryAddXP` changes native progression.
- A mod-owned session/practice counter can remain transient until deliberately converted into native progression.
- Known-item reconstruction can happen every loaded session without owning persistence itself.

Do not infer save behavior from the fact that a value changed successfully at runtime.

## Presentation is separate

Hero HUD, camera, body, and VFX objects are presentation layers.

Access to one of those objects does not make it the owner of inventory, stats, recipes, or other Hero gameplay state.

See [Runtime Access](../../reference/runtime-access/README.md), [Types](../../reference/types/README.md), and [Hooks](../../reference/hooks/README.md).

## Evidence limits

This page combines public source with exact-build Mono static evidence where stated.

Cross-scene Hero identity, death/reload replacement behavior, and every Model/View relationship are not claimed here unless a linked page proves them.
