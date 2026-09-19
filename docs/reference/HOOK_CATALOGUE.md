# Hook Catalogue

> **Reference page.** Use this to discover researched hook surfaces and the reason each hook is useful. Re-verify patch-sensitive targets against the game build you are actually modding.

## What this system is

This catalogue records selected hooks that have direct repository evidence.

It is **not** an exhaustive method list and it is not permission to patch every listed target.

## Who owns it in FoA

Each row is owned by the native type/method shown. Harmony only attaches mod code to that owner.

## Important identities, types, and methods

| Purpose | Type | Method | Patch | Lifecycle meaning | Evidence boundary |
|---|---|---|---|---|---|
| Template readiness retry | `Awaken.TG.Main.Templates.TemplatesLoader` | `set_FinishedLoading(bool)` | Postfix | Retry registration after native template loading reports ready | Source-inspected in multiple custom item/weapon consumers; private lifecycle, revalidate by build |
| Lockpick durability | `Awaken.TG.Main.Locations.Actions.Lockpicking.LockpickingInteraction` | `ConsumePickHP(float)` | Prefix | Intervene exactly when durability is consumed | Source-inspected; runtime behavior requires feature-specific validation |
| Merchant open | `Awaken.TG.Main.Locations.Shops.Shop` | `OpenShop()` | Prefix/observation depending mod | Shop lifecycle before/through stock open | Source-inspected; some merchant mutation variants remain runtime-unverified |
| Merchant UI snapshot boundary | `Awaken.TG.Main.Locations.Shops.UI.ShopUI` | `OnFullyInitialized()` | Prefix | Stock is decompressed and custom rows can be inserted before original UI list capture | Custom item shop path has bounded runtime-visible proof |
| Spell casting VFX | `Awaken.TG.Main.Heroes.Combat.VCCharacterMagicVFX` | `CastingBegun` | Postfix | Observe/overlay after casting begins | Source-inspected; template-name family heuristics are not native identity proof |
| Completed save-slot observation | concrete Steam/NoCloud/Debug/GOG cloud services | `EndSave(string)` | Postfix | Observe completed native slot write | Source-inspected/decompiled target research; not a custom serialization hook |

## Where it exists in the lifecycle

Never choose a row by method name alone.

For example, the merchant custom-item path depends on:

~~~text
Shop.OpenShop
→ stock decompressed
→ ShopUI.OnFullyInitialized Prefix
→ add Item to RestockableStock
→ original UI captures list
~~~

## How we interact with it

For every use:

- resolve exact overload/signature;
- record game build and relevant assembly hash when possible;
- log patch installation;
- log one bounded runtime marker;
- keep original behavior unless intentionally suppressing it;
- cleanly unpatch on unload where supported.

## Why this route

The useful property of a hook is the **state guaranteed at that point**.

`ShopUI.OnFullyInitialized` is useful for custom stock because it occurs at a point where the stock is available but the original item-list snapshot has not yet completed.

## What goes wrong

- wrong overload;
- method renamed/changed after update;
- hook fires in menus/scenes you did not expect;
- Prefix blocks original by returning the wrong value;
- late mutation is invisible because UI already cached state;
- a heuristic classification is mistaken for native identity;
- private hook is treated as stable public API.

## How to verify

For each hook, verify target resolution, invocation count, precondition state, intended result, downstream observation and compatibility after game updates.

## Current proof boundary

This catalogue intentionally includes only a small set of researched surfaces. Add new entries only when the owner, lifecycle meaning, evidence and limits are understood.
