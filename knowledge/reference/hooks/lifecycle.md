# Choosing a Hook by Lifecycle Timing

Use this page when you have found several possible patch targets and need to decide **which one runs at the right time**.

The most important question is not:

> What method can I patch?

It is:

> What is guaranteed to be true immediately before and after this method runs?

## Prefix, Postfix, and Transpiler

Harmony commonly gives you:

- **Prefix** — runs before the original method;
- **Postfix** — runs after the original method;
- **Transpiler** — rewrites the method body.

Prefer Prefix/Postfix when they can express the change. A Transpiler should normally be the last option because it couples the mod more tightly to implementation details.

## Useful FoA lifecycle boundaries

### Hero.OnFullyInitialized

Public Mono mods use this for Hero-dependent setup after basic Hero initialization.

~~~text
plugin Awake / PatchAll
→ game/world systems come online
→ Hero.OnFullyInitialized
→ Hero-dependent listener/UI/state setup
~~~

This is stronger than checking only `Hero.Current != null`, but it is not proof that every child Element, UI View, restored owner, or later scene system is ready.

### HeroRPGStats.AfterHeroFullyInitialized

Use this when the work specifically needs Hero RPG/stat infrastructure.

Exact Mono inspection shows that `HeroRPGStats.OnInitialize()` initializes its wrapper and registers this callback on the parent Hero's fully-initialized event.

~~~text
Hero exists
→ HeroRPGStats initializes
→ parent Hero reaches fully initialized
→ HeroRPGStats.AfterHeroFullyInitialized
→ stat-dependent work
~~~

### UI-specific readiness

Public mods use UI-owner callbacks such as:

- `HeroStorageUI.OnFullyInitialized`
- `PContainerUI.OnFullyInitialized`
- `MapUI.AfterViewSpawned`
- `VHeroHUD.AfterFullyInitialized`

These are useful when the change depends on concrete UI state. They are not general gameplay-ready events.

### Restore-specific readiness

A public IL2CPP-native recipe implementation uses `HeroItems.OnRestore` to obtain a valid restored `HeroItems` instance.

Restore callbacks answer a different question from normal initialization callbacks.

## Timing examples

### Damage

~~~text
HealthElement.OnDamage
→ incoming Damage can still be changed

HealthElement.TakeDamage / damage events
→ later observation/presentation work
~~~

Use the earlier point for mutation and the later point for observation when that matches the feature.

### UI snapshots

~~~text
underlying state
→ UI initializes
→ UI caches/builds a list or visual tree
→ rows refresh / SetData
~~~

If you patch after a UI has already taken its snapshot, changing the underlying data may not update what the player sees until you trigger the correct refresh.

## Before choosing a target

Record:

- fully qualified type;
- exact method/signature;
- assembly;
- Prefix/Postfix/other patch type;
- what is initialized before it runs;
- what becomes true after it runs;
- whether the original method must still execute;
- approximate call frequency;
- build/runtime evidence;
- cleanup/unpatch behavior.

## Prefer

- the narrowest method that owns the behavior;
- observation before mutation when you are still learning the system;
- exact overloads/signatures;
- fail-closed checks when required state is missing;
- normal original execution unless the feature intentionally replaces it.

## Common mistakes

- choosing a method because its name sounds relevant without checking when it runs;
- patching the wrong overload;
- suppressing the original unintentionally;
- assuming UI automatically refreshes after data changes;
- treating `Hero.Current != null` as full Hero readiness;
- moving code to later and later hooks when the real issue is wrong ownership;
- keeping a private/reflected target after a game update without rechecking it.

One public hand-regrow mod is a useful example: several plausible restore/init hooks were still too early for the specific Element-removal operation, while a later presentation callback was safe enough for that operation.

## How to verify a hook

Prove these separately:

1. the target resolves;
2. the patch installs;
3. the hook actually fires;
4. required state is valid at that moment;
5. the intended change happens exactly when expected;
6. downstream game code sees the result;
7. cleanup/unpatch restores normal behavior where required.

A hook firing is not the same as the feature working.

See [Hook Catalogue](catalogue.md) and [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
