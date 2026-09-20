# Lifecycle and Hooks

> **Reference page.** Use this before choosing a Harmony target. A method is useful only if its position in the native lifecycle matches what you need to change.

## What this system is

A hook is an intervention at a particular point in execution.

Harmony commonly provides:

- **Prefix** — runs before the original method;
- **Postfix** — runs after the original method;
- **Transpiler** — rewrites method instructions and should be reserved for cases that cannot be expressed safely with a narrower hook.

The important question is not only **what method can I patch?** but **what is true immediately before and after this method?**

## Publicly useful lifecycle boundaries

### Hero initialization

Public Mono mods use `Hero.OnFullyInitialized` as a practical boundary for hero-dependent setup.

One working damage-number mod explicitly uses its Postfix because the mod loads before `World.EventSystem` and the HUD are ready; it installs its damage listener only after this hero initialization point.

~~~text
plugin Awake / PatchAll
→ game/world infrastructure starts
→ Hero.OnFullyInitialized
→ hero-dependent listeners/UI/state setup
~~~

This is stronger evidence than merely checking whether `Hero.Current` is non-null.

### Hero RPG-stat initialization

Public mods use `HeroRPGStats.AfterHeroFullyInitialized` for stat-system changes that require the hero and `TweakSystem` to be ready.

~~~text
hero exists
→ hero RPG stats finish initialization
→ HeroRPGStats.AfterHeroFullyInitialized
→ resolve Hero.Current / TweakSystem
→ add stat tweaks
~~~

### UI initialization

Public mods use several `OnFullyInitialized` / view-created surfaces for modifications that depend on concrete UI state:

- `HeroStorageUI.OnFullyInitialized`
- `PContainerUI.OnFullyInitialized`
- `MapUI.AfterViewSpawned`

These should not be generalized into gameplay-ready hooks; they are specific UI-owner boundaries.

### Restore boundaries

A public IL2CPP-native recipe implementation uses `HeroItems.OnRestore` because that point supplies a valid restored `HeroItems` instance. That is a restore/availability boundary, not a generic "hero loaded" synonym.

## Existing inspected lifecycle examples

- `TemplatesLoader.set_FinishedLoading(bool)` — template readiness boundary;
- `Shop.OpenShop()` — merchant open/decompression lifecycle;
- `ShopUI.OnFullyInitialized()` — UI initialization boundary used before its item-list snapshot;
- `LockpickingInteraction.ConsumePickHP(float)` — lockpick durability consumption;
- `VCCharacterMagicVFX.CastingBegun` — spell-cast VFX observation;
- concrete cloud-service `EndSave(string)` — completed save-slot write observation.

## Why timing matters

The same semantic area can expose several different hook points.

For damage, public source shows both:

~~~text
HealthElement.OnDamage
→ mutation of incoming Damage is still possible

HealthElement.TakeDamage / emitted damage events
→ downstream observation/presentation use
~~~

For UI:

~~~text
owner/model state
→ UI initializes
→ cached visual tree/list snapshot
→ later row refreshes / SetData
~~~

Choosing a hook after a snapshot may require an explicit refresh even if the underlying data changed correctly.

## Hook-selection checklist

Record for every hook:

- assembly;
- fully qualified type;
- exact method/signature;
- Prefix/Postfix/other patch kind;
- lifecycle state before the method;
- lifecycle state after the method;
- whether the original should still run;
- call frequency;
- version/build evidence;
- cleanup/unpatch behavior.

Prefer:

- observation before mutation;
- Prefix/Postfix over transpiler;
- exact signatures over broad name matching;
- fail-closed checks when state/preconditions are missing;
- normal original execution unless the feature explicitly requires suppression.

## What goes wrong

Known failures:

- patching a method that is semantically related but too early/late;
- wrong overload/signature;
- suppressing original behavior unintentionally;
- assuming a UI refresh happens automatically after its snapshot;
- using a name heuristic instead of exact target identity;
- patching private internals and not revalidating after game updates;
- treating `Hero.Current != null` as proof that every hero-owned subsystem is initialized;
- moving downstream code when the real problem is an upstream lifecycle/ownership boundary.

## How to verify

For a hook, prove:

1. target resolves;
2. patch installs;
3. marker/log shows the hook actually fires;
4. preconditions are true at that point;
5. intended state changes exactly once;
6. downstream owner observes the change;
7. cleanup/unpatch restores expected state where applicable.

## Current proof boundary

The hook catalogue in this repository contains only surfaces backed by inspected examples or research. It is not an exhaustive list of FoA methods.

See [Hook Catalogue](catalogue.md).
