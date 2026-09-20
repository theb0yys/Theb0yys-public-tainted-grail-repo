# Tune Restockable Merchant Stock

Modify the shop's existing `RestockableStock` elements before the native shop UI opens. Leave `UniqueStock`, purchases, transfers and merchant wealth alone.

Working lineage: [Merchant Restock Boundary](../../../research/case-studies/merchants/restock-boundary.md).

## Patch target

The working implementation patches:

~~~text
Awaken.TG.Main.Locations.Shops.Shop.OpenShop()
~~~

with a Harmony prefix.

That gives the mod a point immediately before FoA performs the rest of its normal shop-open sequence.

## Iterate only RestockableStock

Use the model element collection:

~~~csharp
foreach (RestockableStock stock in shop.Elements<RestockableStock>())
{
    stock.Restock();
}
~~~

Do not enumerate `UniqueStock` and do not call purchase/removal code.

`Shop.OnInitialize()` creates separate stock owners, including:

- `UniqueStock`;
- `BoughtFromHeroStock`;
- `RestockableStock`.

Keep those lanes separate.

## Useful controls from the working implementation

The public process can safely add policy around the same native restock call:

- disabled / timed / below-threshold mode;
- 1–5 restock passes per shop open;
- per-shop cooldown using runtime shop identity;
- stock-count threshold;
- optional category filter.

The implementation stores last-restock time by the runtime `Shop` object identity so opening one merchant does not throttle every merchant.

## Capacity and compressed rows

For threshold/category work, the implementation reads the existing `RestockableStock` data:

- `Capacity` getter or `<Capacity>k__BackingField`;
- private `_compressedItems`;
- `ItemSpawningDataRuntime.ItemTemplate`;
- row quantity and level metadata.

Category filtering is implemented by capturing the quantities of **disallowed** rows before `stock.Restock()`, letting the native restock run, then removing/reducing only newly added disallowed rows back to their previous baseline.

That preserves native restock behavior for allowed categories rather than rebuilding the loot roll.

## Useful native item flags

The implementation classifies with existing `ItemTemplate` properties/attachments such as:

- `IsConsumable`;
- `IsCrafting`;
- `IsArrow`;
- `LockpickAttachment`;
- `IsPlainFood`, `IsDish`, `IsFish`;
- gear flags such as `IsArmor`, `IsWeapon`, `IsShield`, `IsJewelry`, `IsEquippable`.

## Keep native shop ownership

After the prefix returns, let `Shop.OpenShop()` continue normally.

Do not replace:

- stock UI;
- item transfer;
- price calculation;
- merchant wealth;
- purchase/removal;
- unique-item unlock state.
