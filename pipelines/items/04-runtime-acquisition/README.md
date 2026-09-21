# Item Stage 4 — Runtime Item Construction and Acquisition

## Objective

Create a normal FoA runtime Item from the registered template and hand it to one real native acquisition owner.

## Required state

- Stage 3 PASSED;
- custom GUID resolves through TemplatesProvider;
- one acquisition route is selected and its lifecycle is understood.

## Runtime construction

The proven runtime shape is:

    registered ItemTemplate
    → World.Add(new Item(customTemplate, quantity))
    → native acquisition owner

The Item must be owned by the game's runtime/MVC model rather than by a disconnected Unity object or custom POCO.

## First proven acquisition route: merchant stock

The bounded merchant sequence is:

    shop opens
    → stock is decompressed/ready
    → before the original UI captures its list
    → create World-owned Item
    → RestockableStock.AddItem
    → original ShopUI builds its list
    → separate custom item is visible

Timing is part of the mechanism. Injecting after the UI has captured a snapshot can leave a valid stock mutation invisible to the player.

## Procedure

1. Resolve the registered template.
2. Construct the Item through World.
3. Validate that the Item references the custom template identity.
4. Select exactly one acquisition owner for the first proof.
5. Insert before that owner's downstream presentation snapshot.
6. Confirm ownership using counts/identity, not only a screenshot.
7. Confirm the downstream UI/gameplay surface sees the same Item.

## Validation gate

PASSED when native runtime ownership, acquisition-owner membership, and downstream visibility all agree on the same custom identity.

## Does not prove

Merchant acquisition does not prove inventory grant, loot, crafting, quest reward, world pickup, persistence, or arbitrary item-family generalisation.

## Next

Proceed to [assets and presentation](../05-presentation/README.md) only after identity/registration/acquisition is stable.
