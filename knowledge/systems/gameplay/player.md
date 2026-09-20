# Player / Hero System

Canonical location for established knowledge about native player/hero ownership and execution.

## Publicly established surfaces

Public FoA mod work consistently treats the hero as a runtime owner that is not guaranteed to be usable merely because a plugin has loaded.

Examples of publicly demonstrated hero-owned surfaces include:

- `HeroItems` for hero item state;
- `KnownItems` for known/discovered item state;
- `HeroRecipes.LearnRecipe` for learning existing/native recipes;
- `Hero.Current.ProficiencyStats.TryAddXP` for native proficiency XP;
- `HeroStorage.Items` for hero-storage contents in the documented storage UI context.

These surfaces belong to different responsibilities. Do not collapse them into a single generic "player object" API.

## Lifecycle

Public mod evidence establishes at least these practical boundaries:

~~~text
plugin initialization
→ player/hero becomes available
→ hero-owned state restores/initializes
→ gameplay use
~~~

A public recipe mod performs its `KnownItems` startup backfill only once the player is loaded. A public IL2CPP implementation captures `HeroItems` at `HeroItems.OnRestore`, where the instance is valid, instead of relying on an unsafe earlier/generic lookup.

A separate public progression mod reported that installing a broad `HeroItems.Add` Harmony patch during `Plugin.Awake()` / `Harmony.PatchAll` could disrupt `HeroItems` initialization and prevent saves from loading. Treat that as evidence for the specific lifecycle hazard, not as a claim that `HeroItems.Add` can never be patched safely.

## Persistence boundary

Public mod evidence also distinguishes hero/session state from saved native progression:

- `Hero.Current.ProficiencyStats.TryAddXP` is used to award native proficiency XP and therefore changes saved progression.
- Mod-owned practice/session ledgers can remain non-saved until deliberately converted into native progression.
- Known-item reconstruction can be performed once per loaded session without implying that the reconstruction mechanism itself owns persistence.

## Presentation is separate

Questline's public Merlin source and public mods expose hero-related HUD/camera/presentation types, but access to those presentation objects does not establish ownership of hero gameplay state.

See [Runtime Access](../../reference/runtime-access/README.md) for acquisition patterns and [Hooks](../../reference/hooks/README.md) for intervention points.

## Current proof boundary

This page currently records only publicly documented FoA behaviour. Exact construction/destruction ordering, cross-scene identity, death/reload replacement semantics and complete model/view ownership remain unclaimed until stronger evidence is reviewed.
