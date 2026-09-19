# Lifecycle and Hooks

> **Reference page.** Use this before choosing a Harmony target. A method is useful only if its position in the native lifecycle matches what you need to change.

## What this system is

A hook is an intervention at a particular point in execution.

Harmony commonly provides:

- **Prefix** — runs before the original method;
- **Postfix** — runs after the original method;
- **Transpiler** — rewrites method instructions and should be reserved for cases that cannot be expressed safely with a narrower hook.

The important question is not only **what method can I patch?** but **what is true immediately before and after this method?**

## Who owns it in FoA

The native class/method owns the lifecycle being patched.

Examples from inspected working-repo paths:

- `TemplatesLoader.set_FinishedLoading(bool)` — template readiness boundary;
- `Shop.OpenShop()` — merchant open/decompression lifecycle;
- `ShopUI.OnFullyInitialized()` — UI initialization boundary used before its item-list snapshot;
- `LockpickingInteraction.ConsumePickHP(float)` — lockpick durability consumption;
- `VCCharacterMagicVFX.CastingBegun` — spell-cast VFX observation;
- concrete cloud-service `EndSave(string)` — completed save-slot write observation.

## Important identities, types, and methods

Record for every hook:

- assembly;
- fully qualified type;
- exact method/signature;
- Prefix/Postfix/other patch kind;
- lifecycle state before the method;
- lifecycle state after the method;
- whether the original should still run;
- version/build evidence;
- cleanup/unpatch behavior.

## Where it exists in the lifecycle

Examples:

### Template readiness

~~~text
templates loading
→ FinishedLoading becomes true
→ custom registration retry
~~~

### Proven merchant item insertion timing

~~~text
Shop.OpenShop
→ stock decompresses
→ ShopUI.OnFullyInitialized prefix
→ custom item inserted
→ original ShopUI builds/captures item list
~~~

The timing is the mechanism.

## How we interact with it

Choose the smallest hook that gives the required state without stealing ownership from the native system.

Prefer:

- observation before mutation;
- Prefix/Postfix over transpiler;
- exact signatures over broad name matching;
- fail-closed checks when state/preconditions are missing;
- normal original execution unless the feature explicitly requires suppression.

## Why this route

Hook placement determines what native state is available and whether downstream systems see the change.

The item/shop research showed that an apparently valid stock mutation can still fail at the UI if it happens after the UI captured a stale list.

## What goes wrong

Known failures:

- patching a method that is semantically related but too early/late;
- wrong overload/signature;
- suppressing original behavior unintentionally;
- assuming a UI refresh happens automatically after its snapshot;
- using a name heuristic instead of exact target identity;
- patching private internals and not revalidating after game updates;
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

See [Hook Catalogue](HOOK_CATALOGUE.md).
