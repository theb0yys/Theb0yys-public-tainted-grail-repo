# Add Native Services to the Bonfire Menu

Use this guide when you want to expose additional bonfire actions without reimplementing storage, cooking, alchemy, level-up or other native services.

Canonical owner: [Bonfire / Fireplace Native Services](../../../knowledge/systems/world/bonfire-services.md).  
Working lineage: [Native Bonfire Services](../../../research/case-studies/gameplay/bonfire-services.md).

## Runnable source

Start from the runnable public example: [Bonfire native services](../../../examples/mono/ui/bonfire-native-services/README.md). Build/run it unchanged first, confirm its documented result, then make one change at a time.

## What you will build

```text
VFireplaceUI initializes
→ attach one Services entry
→ show service choices using native-style controls
→ user selects service
→ call existing FireplaceUI/native action
→ native service screen owns gameplay transaction
→ return to bonfire UI
```

The menu owns **routing and presentation**. FoA still owns the actual service.

## Step 1 — start with one native service

Choose a single action such as:

- `FireplaceUI.OpenHeroStorage()`;
- `FireplaceUI.CookAction()`;
- `FireplaceUI.AlchemyAction()`;
- `FireplaceUI.LevelUpAction()`.

Do not build a full replacement menu first.

## Step 2 — attach at the bonfire UI lifecycle

The working path attaches from:

```text
VFireplaceUI.OnInitialize
```

Wait until the native UI/button style you need is actually initialized.

## Step 3 — reuse native presentation where practical

The working implementation cloned the initialized native Level Up button style for service rows.

That gives the added entries a native visual/control baseline without inventing an unrelated button system.

Keep your own object ownership clear so you can destroy added controls on teardown.

## Step 4 — call the native action

When the user selects your row, call the real service method.

Example:

```text
Storage row selected
→ FireplaceUI.OpenHeroStorage()
→ Hero storage/native screen owns everything after that
```

Do not recreate the stash transaction inside your menu.

## Step 5 — preserve native prerequisites

An unavailable service should remain unavailable.

Examples:

- pet recall requires an applicable left-behind pet;
- arrow crafting/native crafting route may require the vanilla prerequisite;
- save/fast-travel actions have native guards.

Your menu can display unavailable actions, but should not bypass their underlying rules.

## Step 6 — keep UI ownership separate from service ownership

If you build a custom submenu, it must separately handle:

- cursor/input scope;
- focus;
- Back/Cancel;
- disabled state;
- descriptions;
- close/return;
- teardown/restoration.

See [UI, Cursor, Focus, and Input Ownership](../../../knowledge/systems/presentation/ui-input.md).

A known service method does not automatically prove your replacement UI lifecycle.

## Verification checklist

For each added service:

1. bonfire UI initializes normally;
2. one added Services entry appears;
3. selection triggers exactly once;
4. native service opens;
5. native prerequisite/disabled state is preserved;
6. service completes using native gameplay logic;
7. returning to bonfire works;
8. closing/scene change removes owned UI;
9. original bonfire actions still work.

## Evidence boundary

**Proven:** Better Bonfire Menu 0.6.3 deployment with added/restored service entries and working service grid using native service owners.

**Separate/partial:** newer full replacement submenu input/layout/service-return lifecycle requires its own validation and should not inherit the service-call proof.
