# Native-Owner-First Modding

This is the core reasoning pattern behind the repository's working mods.

When you want to change one behaviour, find the FoA system that already owns that behaviour and intervene at the smallest useful seam.

```text
exact subject / identity
→ native owner
→ lifecycle and data contract
→ smallest justified intervention
→ downstream native behaviour
→ cleanup / restoration
→ verify the intended behaviour
```

The cross-case evidence is summarized in [Native-Owner-First Modding Pattern](../../research/case-studies/failures-and-fixes/native-owner-first.md).

## Why this matters

A mod usually becomes harder to maintain when it replaces a whole controller to change one scalar, presentation effect or decision.

Keeping native ownership gives you more of FoA's existing:

- lifecycle;
- compatibility behaviour;
- persistence rules;
- events;
- cleanup;
- downstream systems.

## Three common intervention shapes

### 1. Adjust a result

Let native code run, then adjust its returned value.

Use this when the desired change is a scalar/result.

Examples:

- [mount speed](../tasks/gameplay/change-mount-speed.md);
- [vendor prices](../tasks/gameplay/change-vendor-prices.md).

Shape:

```text
native calculation
→ postfix/result adapter
→ bounded result change
→ native consumer continues
```

### 2. Guard an action

Observe the native action and add a narrow condition.

Shape:

```text
native action requested
→ mod checks its additional rule
→ allow normal native action
  or
→ block only this action cleanly
```

Do not replace unrelated state merely because the action is patchable.

### 3. Attach a sidecar

Observe a native lifecycle and add mod-owned presentation, telemetry or UI.

Examples include:

- footstep/audio presentation;
- damage/death VFX;
- action receipts.

Shape:

```text
native lifecycle event
→ mod-owned side effect/presentation
→ mod-owned cleanup
→ native gameplay state remains authoritative
```

## A practical investigation loop

When starting a feature, write down:

1. **What exact thing am I changing?**
2. **What native object/system owns it?**
3. **When is that owner created/used/destroyed?**
4. **Can I change one input/result instead of replacing the owner?**
5. **What state does my mod itself own?**
6. **How is that state cleaned up?**
7. **What observation proves the feature worked?**
8. **What observation proves unrelated native behaviour still works?**

If you cannot answer ownership/lifecycle yet, move into [Research](../../research/README.md) instead of guessing.

## Examples from proven work

| Goal | Small owner-preserving seam |
| --- | --- |
| Give an existing item | normal `Item` → `World` → `HeroItems` |
| Change mount speed | `VMount` velocity getter result |
| Change vendor price | `TradeUtils.Price` result |
| Replace hero footsteps | exact `VHeroFootsteps` FMOD request |
| Add bonfire service routes | existing `FireplaceUI` service methods |
| Add character hit/death VFX | native damage/death lifecycle |
| Companion defend behaviour | `NpcHeroPetAlly.EnterCombat()` |
| Discover framework capability | public read-only discovery contract |

## How to know your seam is too broad

Warning signs:

- you are replacing input, AI, rendering and persistence for a scalar tweak;
- you patch a global audio method without proving the event owner;
- you create a second inventory, save system or transaction path;
- your mod writes internal lifecycle tags/counters to make state look complete;
- your cleanup requires scanning and deleting objects you cannot identify as yours;
- disabling the mod cannot return to native behaviour.

Broaden only when the requested feature genuinely requires broader ownership.

## Evidence discipline

A working hook proves only the hook.

Keep these claims separate:

- static/source identity;
- runtime activation;
- visible/audible output;
- repeated lifecycle;
- persistence;
- compatibility;
- release packaging.

A beginner guide should state which of those are actually established.

## Where to go next

For a first owner-first exercise, use one of these:

- [Grant an existing item](../tasks/items/grant-an-existing-item.md)
- [Change mount speed](../tasks/gameplay/change-mount-speed.md)
- [Change vendor prices](../tasks/gameplay/change-vendor-prices.md)
- [Replace hero footsteps](../tasks/audio/replace-hero-footsteps.md)
- [Build a read-only framework consumer](../tasks/interoperability/build-a-read-only-framework-consumer.md)

After you can explain why the chosen seam owns the behaviour you are changing, move into the more complex domain guides.
