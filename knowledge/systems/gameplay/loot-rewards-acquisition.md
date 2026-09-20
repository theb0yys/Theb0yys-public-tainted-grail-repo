# Loot, Rewards, Containers, and Item Acquisition

Use this page when you are deciding **how an item should enter the player's possession**.

A valid `ItemTemplate` can exist without any way for the player to obtain it.

## Acquisition routes

Common routes include:

- direct inventory grant;
- world pickup;
- container transfer;
- corpse/search loot;
- vendor stock;
- quest/reward grant;
- crafting output;
- loot-table integration.

Choose the route that matches the gameplay meaning you want.

## Direct grant

The best-established controlled grant is:

~~~csharp
Item item = World.Add(new Item(template, quantity));
hero.HeroItems.Add(item);
~~~

Use this when you want to prove an existing loaded item can be acquired without also changing vendors, loot tables, quests, or crafting.

## Not every loaded template is safe to grant

A provider can contain:

- abstract definitions;
- hidden/system entries;
- cannot-drop items;
- debug/test/tutorial items;
- quest/story-sensitive items.

Resolve exact identities and filter out inappropriate templates.

"Provider can resolve it" does not mean "player should own it."

## World pickup

A world pickup path includes:

~~~text
world owner / Pickable
→ interaction
→ crime/legality checks if relevant
→ PickItemAction
→ Hero inventory
→ pickup/world cleanup
~~~

If you change interaction timing, preserve the native transfer and crime logic where possible.

## Containers

A container route typically includes:

~~~text
container owns Item
→ UI/prompt
→ theft/pickpocket classification if relevant
→ TakeItemFromContainer
→ Hero inventory
~~~

The UI transfer is downstream of whatever created the container contents.

## Corpse/search loot

Treat corpse loot as part of actor/death ownership.

Do not bolt arbitrary loot onto a custom creature before its:

- actor lifecycle;
- death;
- corpse;
- cleanup;

are already understood.

## Quest/reward items

Quest/story items are especially risky as generic rewards.

Granting one outside its intended story path can create impossible quest state even if the item itself is technically valid.

## Common mistakes

- loaded template = safe player reward;
- direct grant = loot-table integration;
- container UI hook = loot generation;
- corpse contents changed without mapping corpse/death lifecycle;
- quest item treated like ordinary loot;
- current-session custom item grant = save-safe custom item.

## How to verify acquisition

Check:

1. exact item template;
2. item is appropriate for this acquisition route;
3. native `Item` creation;
4. acquisition owner;
5. stack/duplicate behavior;
6. crime/legal state if relevant;
7. inventory/UI visibility;
8. use/drop behavior;
9. source world/container cleanup;
10. no story/quest bypass;
11. save/load for durable state;
12. missing-mod behavior for custom definitions.

For true loot-table work, also verify table identity, weights/counts, eligibility, repeat/respawn behavior, and economy impact.

## Evidence limits

Direct existing-item grants have strong source/runtime precedent.

World pickup and container ownership are well mapped for several routes.

A generic custom loot/reward-table injection framework is not yet established.
