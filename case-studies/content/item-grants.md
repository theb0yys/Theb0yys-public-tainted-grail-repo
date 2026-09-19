# Existing Item Grants

Use this pattern when you want to give the hero an **existing loaded FoA item** without inventing a second inventory system.

## Working implementation lineage

Avalon Cheat Panel uses this path for its item browser and reward actions. A live screenshot confirmed an item-row `Grant` action completed once and returned a success receipt.

## Native owners

- `TemplatesProvider` — loaded item-template lookup
- `ItemTemplate` — native item definition
- `World` — runtime model ownership
- `Hero.HeroItems` — hero inventory ownership

## Working sequence

~~~text
hero available
→ TemplatesProvider available and AllLoaded
→ resolve one existing ItemTemplate
→ reject abstract/hidden/non-droppable/unsafe templates
→ create native Item(template, quantity)
→ add the Item to World
→ add that Item through HeroItems
→ report success/failure to the caller
~~~

The important part is that the mod does not directly edit a private inventory collection. It creates a normal native `Item` and hands it to the normal hero inventory owner.

## Minimal shape

~~~csharp
TemplatesProvider provider = World.Services.Get<TemplatesProvider>();
if (!provider.AllLoaded)
    return;

ItemTemplate template = ResolveExistingTemplate(provider, templateGuid);
Item item = World.Add(new Item(template, quantity));
Item added = Hero.Current.HeroItems.Add(item);
~~~

In real code, validate the hero, template, quantity and `Add` result before reporting success.

## Useful guards

The working browser filters out templates that are:

- abstract;
- hidden from UI;
- non-droppable;
- obvious debug/test/tutorial/quest-only candidates.

It also re-resolves the selected GUID at execution time instead of trusting a stale UI row.

## Do not confuse this with new-item registration

This pattern grants an **existing loaded template**. Creating and registering a genuinely new `ItemTemplate` is a separate content-integration problem.

## Runtime lane

The working implementation is Mono / BepInEx 5.
