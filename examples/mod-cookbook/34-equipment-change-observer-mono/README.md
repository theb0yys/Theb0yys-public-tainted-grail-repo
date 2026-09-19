# 34 — Equipment Change Observer

**Category:** equipment / lifecycle observation  
**Source-path evidence:** SOURCE_CONFIRMED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes FoA's item-level equipment transition methods:

~~~text
Item.EquipInSlot(EquipmentSlotType)
Item.UnequipInSlot(EquipmentSlotType)
~~~

The postfixes log the current hero's item identity, the affected slot, and the item's post-transition `IsEquipped` state.

## Important ownership boundary

These methods are useful lifecycle observation seams. They are **not** the authoritative entry point for building your own equipment UI.

FoA's higher-level native equipment path also includes loadouts, slot acceptance, displacement, restrictions, inventory return behavior and visual equipment handling. A custom UI should not call `Item.EquipInSlot` directly merely because this observer patches it.

This example therefore never calls either method itself.

## Build

~~~powershell
dotnet build .\EquipmentChangeObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The observer does not equip, unequip, move inventory items, alter loadouts or write saves.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
