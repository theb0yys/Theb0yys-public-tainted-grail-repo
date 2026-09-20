# Build a Read-Only Inventory Projection

Build search, grouping and workspace UI over FoA's existing inventory objects. Do not create a second authoritative inventory.

Working lineage: [Inventory Suite: Presentation Without a Second Inventory](../../../research/case-studies/ui/inventory-truth-boundary.md).

## Native inventory authority

The relevant native chain is:

~~~text
CharacterSheetUI
→ InventoryUI
→ HeroItems
→ Item
→ HeroLoadout / native action owners
~~~

For a read-only projection, start from the current hero and the current `HeroItems` collection.

The existing implementation slice uses:

- loaded `TG.Main`;
- `Hero.Current`;
- `HeroItems`;
- `HeroItems.Inventory`;
- native `Item` members.

Hidden native items are excluded when `HiddenOnUI` is true.

## Projection shape

Convert each native item into a **temporary presentation record** containing only what your UI needs, for example:

~~~text
session key
display name
template identity
quantity
quality
equipped/read-only flags
search text
workspace flags
~~~

Do not store native `Item` objects as durable mod truth.

On refresh, rebuild/reconcile the projection from the current native inventory.

## Search and workspaces

Search should run over projected presentation fields:

~~~text
native inventory
→ projection rows
→ workspace predicate
→ text query
→ sort
→ selected row/details
~~~

The working source keeps workspace/search logic on the projection and does not mutate `HeroItems`.

## Character Sheet integration

The stronger native integration route keeps the Character Sheet lifecycle rather than opening a competing global inventory:

- preserve `CharacterSheetUI`;
- preserve native primary tabs;
- use `InventoryUI.AfterViewSpawned(VInventoryUI)` as the candidate attachment notification;
- use `InventoryUI.OnDiscard(bool)` and final Character Sheet teardown as restoration boundaries.

Do not assume `AfterViewSpawned` means every child hierarchy/focus target is already safe to replace; inspect the current build's view hierarchy before suppressing native presentation.

## Actions stay native

When you later add equip/use/transfer, delegate to the existing action owner.

Known native lanes include:

- `BagUI` / `Item.Use` / `ItemSkillsInvoker`;
- `LoadoutsUI` / `EquipmentChooseUI` / `HeroLoadout` / `CharacterInventoryExtension`;
- `HeroItems` quick-slot methods / `VHeroKeys.UseQuickSlotItem`;
- `ContainerUI` / `TransferItems` / `ItemUtils.MoveTo`.

The custom UI owns presentation and user intent; the native action path owns legality and mutation.
