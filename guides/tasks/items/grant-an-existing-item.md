# Grant an Existing FoA Item

Use this guide when you want a Mono/BepInEx mod to give the player an item that **already exists in Tainted Grail: The Fall of Avalon**.

This is deliberately simpler than creating a new item. You will reuse FoA's own `ItemTemplate`, create a normal runtime `Item`, and hand that item to the native hero inventory owner.

## Runnable source

Start from the minimal public example: [Existing item grant example](../../../examples/mono/items/grant-existing-item/README.md). Build it unchanged first, confirm the documented log/result, then make one change at a time.

## What you will build

A small command/action that:

```text
waits for FoA item templates
→ resolves one exact existing ItemTemplate
→ creates a native Item
→ adds it to the World
→ adds it through HeroItems
→ reports success or failure
```

The proven case behind this guide is [Existing Item Grants](../../../research/case-studies/content/item-grants.md).

## Prerequisites

Before starting:

1. complete the [first Mono plug-in](../../getting-started/first-mono-plugin.md);
2. make sure your plug-in loads and logs successfully;
3. choose one **existing** item template identity;
4. keep this first test on a disposable save.

For identity rules, read [GUIDs, names and identity kinds](../../../knowledge/reference/identities/identity-guids-names.md).

## Understand the owners

The relevant native owners are:

- `TemplatesProvider` — resolves loaded templates;
- `ItemTemplate` — defines the item;
- `World` — owns the runtime model;
- `Hero.Current.HeroItems` — owns the hero inventory.

The key rule is:

> Do not edit a private inventory collection. Create a normal native Item and give it to the normal inventory owner.

See [Items](../../../knowledge/systems/gameplay/items.md) for the deeper item/template model.

## Step 1 — wait for a playable hero

Do not run the grant while the hero or world is unavailable.

Your action should first establish:

```text
Hero.Current exists
+ World exists
+ TemplatesProvider exists
+ TemplatesProvider.AllLoaded
```

If any prerequisite is missing, return a clear "not ready" result instead of guessing.

## Step 2 — resolve one exact ItemTemplate

Resolve the chosen template through `TemplatesProvider`.

Conceptually:

```csharp
TemplatesProvider provider = World.Services.Get<TemplatesProvider>();
if (provider == null || !provider.AllLoaded)
    return;

ItemTemplate template = ResolveExistingTemplate(provider, templateGuid);
if (template == null)
    return;
```

Do not select items by display-name substring for a release mod. Use a stable identity.

For a browser or generic grant tool, reject unsafe candidates such as obvious debug, tutorial, quest-only, hidden, abstract or non-droppable templates unless your feature specifically understands them.

## Step 3 — create the native Item

Once the template resolves, create the runtime item through the native model owner:

```csharp
Item item = World.Add(new Item(template, quantity));
```

Validate the requested quantity before this call.

At this point you have created a normal FoA item model. You have **not** yet given it to the hero.

## Step 4 — add it through HeroItems

Pass the item to the native inventory owner:

```csharp
Item added = Hero.Current.HeroItems.Add(item);
```

Treat the returned result as the operation result. Do not report success merely because the button or command was triggered.

If the add fails, report the failure and inspect the exact template/hero state.

## Step 5 — make the action idempotent where appropriate

A generic cheat/grant action may intentionally allow repeated grants. Other features should not.

Decide explicitly whether repeated invocation means:

- give another copy;
- increase a stack;
- reject because the reward was already claimed;
- allow once per session;
- allow once per save.

That policy belongs to your mod. Do not assume the inventory owner provides your feature's one-shot semantics.

## Step 6 — report a real result

For UI actions, use the pattern from [Action Receipts](../../../research/case-studies/ui/action-receipts.md):

```text
capture exact template GUID + quantity
→ execute grant
→ receive success/failure
→ display concise receipt
```

Useful diagnostics include:

- template GUID;
- requested quantity;
- whether templates were loaded;
- whether the template resolved;
- whether `World.Add` succeeded;
- whether `HeroItems.Add` succeeded.

## Verify in game

Minimum proof for this guide:

1. your plug-in loads;
2. hero and template provider are ready;
3. exact template GUID resolves;
4. one native `Item` is created;
5. `HeroItems.Add` returns a valid result;
6. the item appears in the player's inventory;
7. invoking the action once grants exactly the intended quantity;
8. no unrelated inventory items change;
9. no errors appear in the BepInEx log.

If your feature claims persistence, perform a separate cold save/load test. A successful current-session grant does not by itself prove custom persistence behaviour.

## Common mistakes

### Creating a Unity object instead of a FoA Item

A prefab or arbitrary object is not a native inventory model.

Use the native `Item` + `World` + `HeroItems` route.

### Running before templates are ready

If `TemplatesProvider.AllLoaded` is false, stop and try at a legitimate later lifecycle point.

### Trusting a stale UI row

For a browser, store the stable template GUID and re-resolve it when the user presses Grant.

### Treating this as new-item registration

This guide grants an item whose `ItemTemplate` already exists.

For genuinely new item identity, use [Custom item integration](custom-items.md) and the canonical [item system](../../../knowledge/systems/gameplay/items.md).

## Evidence boundary

**Proven:** existing loaded template → native Item → World → HeroItems grant path, including a live item-row Grant action and result receipt.

**Not claimed by this guide:** arbitrary new template registration, new models/icons, quest-item safety, universal persistence behaviour, or uninstall/orphan handling.

## Next steps

After this works, useful expansions are:

- build an item browser;
- add category filters;
- expose quantity configuration;
- add action receipts;
- learn the separate process for [custom item integration](custom-items.md).

Keep the first version narrow enough that a failed grant tells you exactly which stage failed.
